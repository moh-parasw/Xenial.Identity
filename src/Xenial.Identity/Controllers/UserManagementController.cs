﻿using System.Collections.Immutable;
using System.Security.Claims;

using DevExpress.Xpo;

using FluentValidation;
using FluentValidation.AspNetCore;

using IdentityModel;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Xenial.AspNetIdentity.Xpo.Models;
using Xenial.Identity.Client;
using Xenial.Identity.Data;
using Xenial.Identity.Infrastructure;

namespace Xenial.Identity.Controllers;

[Route("api/management/users")]
[ApiController]
[Authorize]
public sealed class UserManagementController : ControllerBase
{
    [Route("currentUserId")]
    [HttpGet]
    [Authorize(AuthPolicies.LocalApiPolicyName)]
    [ProducesResponseType(typeof(IEnumerable<XenialIdResponse>), 200)]
    [ProducesResponseType(typeof(IEnumerable<ProblemDetails>), 200)]
    public async Task<IActionResult> GetCurrentUserId(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var sub = User.Claims.FirstOrDefault(m => m.Type == "sub")?.Value;

        if (string.IsNullOrEmpty(sub))
        {
            return BadRequest(new ProblemDetails
            {
                Detail = "Current user has no sub claim"
            });
        }

        return Ok(new XenialIdResponse(sub));
    }

    [Route("")]
    [HttpGet]
    [Authorize(AuthPolicies.UsersRead)]
    [ProducesResponseType(typeof(IEnumerable<XenialUser>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromServices] UnitOfWork uow, CancellationToken cancellationToken)
    {
        var users = await uow
            .Query<XpoIdentityUser>()
            .ToListAsync(cancellationToken);

        var userModel = users.Select(m => new XenialUser(m.Id, m.UserName));

        return Ok(userModel);
    }

    public sealed class CreateXenialUserRequestValidator : AbstractValidator<CreateXenialUserRequest>
    {
        public CreateXenialUserRequestValidator()
            => RuleFor(m => m.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(250);
    }

    [Route("create")]
    [HttpPost]
    [Authorize(AuthPolicies.UsersCreate)]
    [ProducesResponseType(typeof(XenialUser), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateXenialUserRequest req,
        [FromServices] IValidator<CreateXenialUserRequest> validator,
        [FromServices] UserManager<XenialIdentityUser> userManager,
        CancellationToken cancellationToken
    )
    {
        var validationResult = await validator.ValidateAsync(req, cancellationToken);
        if (!validationResult.IsValid)
        {
            validationResult.AddToModelState(ModelState);
            var problemDetails = new ValidationProblemDetails(ModelState);
            return UnprocessableEntity(problemDetails);
        }

        var id = CryptoRandom.CreateUniqueId();
        var userReq = new XenialIdentityUser
        {
            Id = id,
            UserName = req.Username,
            Email = req.Email
        };

        var result = string.IsNullOrEmpty(req.Password)
            ? await userManager.CreateAsync(userReq)
            : await userManager.CreateAsync(userReq, req.Password);

        if (!result.Succeeded)
        {
            var problemDetails = new ValidationProblemDetails(
                result.Errors.ToDictionary(
                    m => m.Code,
                    m => new[]
                    {
                        m.Description
                    })
                );

            return UnprocessableEntity(problemDetails);
        }

        var user = await userManager.FindByIdAsync(id);
        return Ok(await MapAsync(user, userManager));
    }

    private XenialUser Map(XenialIdentityUser user, IEnumerable<Claim> claims)
        => new XenialUser(user.Id, user.UserName)
        {
            Claims = claims.Select(x => new XenialClaim(x.Type, x.Value)).ToImmutableArray()
        };

    private async Task<XenialUser> MapAsync(XenialIdentityUser user, UserManager<XenialIdentityUser> userManager)
    {
        var claims = await userManager.GetClaimsAsync(user);
        var roles = await userManager.GetRolesAsync(user);
        return Map(user, claims.Concat(roles.Select(m => new Claim("role", m))));
    }

    [Route("{userId}")]
    [Authorize(AuthPolicies.UsersDelete)]
    [ProducesResponseType(typeof(XenialIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [HttpDelete]
    public async Task<IActionResult> Delete([FromRoute] string userId, [FromServices] UserManager<XenialIdentityUser> userManager, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(new ProblemDetails
            {
                Detail = "UserId must not be empty"
            });
        }

        var sub = User.Claims.FirstOrDefault(m => m.Type == "sub")?.Value;

        if (string.IsNullOrEmpty(sub))
        {
            return BadRequest(new ProblemDetails
            {
                Detail = "Current user has no sub claim"
            });
        }

        if (sub == userId)
        {
            return BadRequest(new ProblemDetails
            {
                Detail = "Can not delete current user"
            });
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound(new ProblemDetails
            {
                Detail = $"Can not find user with id {userId}"
            });
        }

        var result = await userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            var problemDetails = new ValidationProblemDetails(
                result.Errors.ToDictionary(
                    m => m.Code,
                    m => new[]
                    {
                        m.Description
                    })
                );

            return UnprocessableEntity(problemDetails);
        }

        return Ok(new XenialIdResponse(userId));
    }

    public sealed class AddToXenialRoleRequestValidator : AbstractValidator<AddToXenialRoleRequest>
    {
        public AddToXenialRoleRequestValidator()
        {
            RuleFor(m => m.UserId)
                .MaximumLength(SizeAttribute.DefaultStringMappingFieldSize)
                .NotEmpty();

            RuleFor(m => m.RoleName)
                .MaximumLength(50)
                .NotEmpty();
        }
    }

    [Route("roles/add")]
    [Authorize(AuthPolicies.UsersManage)]
    [HttpPost]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(XenialUser), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddToRoleAsync(
        [FromBody] AddToXenialRoleRequest req,
        [FromServices] UserManager<XenialIdentityUser> userManager,
        [FromServices] RoleManager<IdentityRole> roleManager,
        CancellationToken cancellationToken
    )
    {
        var userId = req.UserId;

        var sub = User.Claims.FirstOrDefault(m => m.Type == "sub")?.Value;

        if (string.IsNullOrEmpty(sub))
        {
            return BadRequest(new ProblemDetails
            {
                Detail = "Current user has no sub claim"
            });
        }

        if (req.UserId == sub)
        {
            return BadRequest(new ProblemDetails
            {
                Detail = "You cannot add roles to the current user"
            });
        }

        if (AuthPolicies.IsAllowedToAdd(User, req.RoleName) || User.IsInRole(req.RoleName) || User.IsInRole(DatabaseUpdateHandler.AdminRoleName))
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return NotFound(new ProblemDetails
                {
                    Detail = $"Can not find user with id {userId}"
                });
            }
            var role = await roleManager.FindByNameAsync(req.RoleName);

            if (role is null)
            {
                return NotFound(new ProblemDetails
                {
                    Detail = $"Can not find role {req.RoleName}"
                });
            }

            if (!(await userManager.IsInRoleAsync(user, req.RoleName)))
            {
                var result = await userManager.AddToRoleAsync(user, req.RoleName);
                if (!result.Succeeded)
                {
                    var problemDetails = new ValidationProblemDetails(
                        result.Errors.ToDictionary(
                            m => m.Code,
                            m => new[]
                            {
                            m.Description
                            })
                        );

                    return UnprocessableEntity(problemDetails);
                }
            }

            user = await userManager.FindByIdAsync(userId);
            return Ok(await MapAsync(user, userManager));
        }

        return BadRequest(new ProblemDetails
        {
            Detail = $"You are not allowed to add the role {req.RoleName}"
        });
    }

    [Route("roles/remove")]
    [Authorize(AuthPolicies.UsersManage)]
    [HttpPost]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(XenialUser), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveFromRoleAsync(
        [FromBody] AddToXenialRoleRequest req,
        [FromServices] UserManager<XenialIdentityUser> userManager,
        [FromServices] RoleManager<IdentityRole> roleManager,
        CancellationToken cancellationToken
    )
    {
        var userId = req.UserId;

        var sub = User.Claims.FirstOrDefault(m => m.Type == "sub")?.Value;

        if (string.IsNullOrEmpty(sub))
        {
            return BadRequest(new ProblemDetails
            {
                Detail = "Current user has no sub claim"
            });
        }

        if (req.UserId == sub)
        {
            return BadRequest(new ProblemDetails
            {
                Detail = "You cannot add roles to the current user"
            });
        }

        if (AuthPolicies.IsAllowedToAdd(User, req.RoleName) || User.IsInRole(req.RoleName) || User.IsInRole(DatabaseUpdateHandler.AdminRoleName))
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return NotFound(new ProblemDetails
                {
                    Detail = $"Can not find user with id {userId}"
                });
            }

            var role = await roleManager.FindByNameAsync(req.RoleName);

            if (role is null)
            {
                return NotFound(new ProblemDetails
                {
                    Detail = $"Can not find role {req.RoleName}"
                });
            }

            var result = await userManager.RemoveFromRoleAsync(user, req.RoleName);
            if (!result.Succeeded)
            {
                var problemDetails = new ValidationProblemDetails(
                    result.Errors.ToDictionary(
                        m => m.Code,
                        m => new[]
                        {
                            m.Description
                        })
                    );

                return UnprocessableEntity(problemDetails);
            }

            user = await userManager.FindByIdAsync(userId);
            return Ok(await MapAsync(user, userManager));
        }

        return BadRequest(new ProblemDetails
        {
            Detail = $"You are not allowed to remove the role {req.RoleName}"
        });
    }


    public sealed class AddXenialClaimRequestValidator : AbstractValidator<AddXenialClaimRequest>
    {
        public AddXenialClaimRequestValidator()
        {
            RuleFor(m => m.UserId)
                .MaximumLength(SizeAttribute.DefaultStringMappingFieldSize)
                .NotEmpty();

            RuleFor(m => m.Claim)
                .NotEmpty();

            RuleFor(m => m.Claim.Type)
                .MaximumLength(250)
                .NotEmpty();

            RuleFor(m => m.Claim.Value)
                .MaximumLength(250)
                .NotEmpty();
        }
    }

    [Route("claims/add")]
    [Authorize(AuthPolicies.UsersManage)]
    [HttpPost]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(XenialUser), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddClaimAsync(
        [FromBody] AddXenialClaimRequest req,
        [FromServices] UserManager<XenialIdentityUser> userManager,
        [FromServices] IValidator<AddXenialClaimRequest> validator,
        CancellationToken cancellationToken
    )
    {
        var validationResult = await validator.ValidateAsync(req, cancellationToken);
        if (!validationResult.IsValid)
        {
            validationResult.AddToModelState(ModelState);
            var problemDetails = new ValidationProblemDetails(ModelState);
            return UnprocessableEntity(problemDetails);
        }

        var userId = req.UserId;

        var sub = User.Claims.FirstOrDefault(m => m.Type == "sub")?.Value;

        if (string.IsNullOrEmpty(sub))
        {
            return BadRequest(new ProblemDetails
            {
                Detail = "Current user has no sub claim"
            });
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound(new ProblemDetails
            {
                Detail = $"Can not find user with id {userId}"
            });
        }

        var result = await userManager.AddClaimAsync(user, new Claim(req.Claim.Type, req.Claim.Value));
        if (!result.Succeeded)
        {
            var problemDetails = new ValidationProblemDetails(
                result.Errors.ToDictionary(
                    m => m.Code,
                    m => new[]
                    {
                            m.Description
                    })
                );

            return UnprocessableEntity(problemDetails);
        }

        user = await userManager.FindByIdAsync(userId);
        return Ok(await MapAsync(user, userManager));

    }

    public sealed class RemoveXenialClaimRequestValidator : AbstractValidator<RemoveXenialClaimRequest>
    {
        public RemoveXenialClaimRequestValidator()
        {
            RuleFor(m => m.UserId)
                .MaximumLength(SizeAttribute.DefaultStringMappingFieldSize)
                .NotEmpty();

            RuleFor(m => m.ClaimType)
                .MaximumLength(250)
                .NotEmpty();
        }
    }

    [Route("claims/remove")]
    [Authorize(AuthPolicies.UsersManage)]
    [HttpPost]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(XenialUser), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveClaimAsync(
        [FromBody] RemoveXenialClaimRequest req,
        [FromServices] UserManager<XenialIdentityUser> userManager,
        [FromServices] IValidator<RemoveXenialClaimRequest> validator,
        CancellationToken cancellationToken
    )
    {
        var validationResult = await validator.ValidateAsync(req, cancellationToken);
        if (!validationResult.IsValid)
        {
            validationResult.AddToModelState(ModelState);
            var problemDetails = new ValidationProblemDetails(ModelState);
            return UnprocessableEntity(problemDetails);
        }

        var userId = req.UserId;

        var sub = User.Claims.FirstOrDefault(m => m.Type == "sub")?.Value;

        if (string.IsNullOrEmpty(sub))
        {
            return BadRequest(new ProblemDetails
            {
                Detail = "Current user has no sub claim"
            });
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound(new ProblemDetails
            {
                Detail = $"Can not find user with id {userId}"
            });
        }

        var claims = await userManager.GetClaimsAsync(user);
        foreach (var claim in claims.Where(c => c.Type.Equals(req.ClaimType, StringComparison.InvariantCultureIgnoreCase)))
        {
            var result = await userManager.RemoveClaimAsync(user, claim);
            if (!result.Succeeded)
            {
                var problemDetails = new ValidationProblemDetails(
                    result.Errors.ToDictionary(
                        m => m.Code,
                        m => new[]
                        {
                            m.Description
                        })
                    );

                return UnprocessableEntity(problemDetails);
            }
        }

        user = await userManager.FindByIdAsync(userId);
        return Ok(await MapAsync(user, userManager));
    }

    public sealed class ResetXenialUserPasswordRequestValidator : AbstractValidator<ResetXenialUserPasswordRequest>
    {
        public ResetXenialUserPasswordRequestValidator()
            => RuleFor(m => m.UserId)
                .MaximumLength(SizeAttribute.DefaultStringMappingFieldSize)
                .NotEmpty();
    }

    [Route("password/reset")]
    [Authorize(AuthPolicies.UsersManage)]
    [HttpPost]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ResetXenialUserPasswordResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetPasswordAsync(
        [FromBody] ResetXenialUserPasswordRequest req,
        [FromServices] UserManager<XenialIdentityUser> userManager,
        [FromServices] IValidator<ResetXenialUserPasswordRequest> validator,
        CancellationToken cancellationToken
    )
    {
        var validationResult = await validator.ValidateAsync(req, cancellationToken);
        if (!validationResult.IsValid)
        {
            validationResult.AddToModelState(ModelState);
            var problemDetails = new ValidationProblemDetails(ModelState);
            return UnprocessableEntity(problemDetails);
        }

        var userId = req.UserId;

        var sub = User.Claims.FirstOrDefault(m => m.Type == "sub")?.Value;

        if (string.IsNullOrEmpty(sub))
        {
            return BadRequest(new ProblemDetails
            {
                Detail = "Current user has no sub claim"
            });
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound(new ProblemDetails
            {
                Detail = $"Can not find user with id {userId}"
            });
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        return Ok(new ResetXenialUserPasswordResponse(req.UserId, token));
    }


    public sealed class SetXenialUserPasswordRequestValidator : AbstractValidator<SetXenialUserPasswordRequest>
    {
        public SetXenialUserPasswordRequestValidator()

        {
            RuleFor(m => m.UserId)
                .MaximumLength(SizeAttribute.DefaultStringMappingFieldSize)
                .NotEmpty();

            RuleFor(m => m.Token)
                .NotEmpty();

            RuleFor(m => m.Password)
                .NotEmpty();
        }
    }

    [Route("password/set")]
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(XenialUser), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetPasswordAsync(
        [FromBody] SetXenialUserPasswordRequest req,
        [FromServices] UserManager<XenialIdentityUser> userManager,
        [FromServices] IValidator<SetXenialUserPasswordRequest> validator,
        CancellationToken cancellationToken
    )
    {
        var validationResult = await validator.ValidateAsync(req, cancellationToken);
        if (!validationResult.IsValid)
        {
            validationResult.AddToModelState(ModelState);
            var problemDetails = new ValidationProblemDetails(ModelState);
            return UnprocessableEntity(problemDetails);
        }

        var userId = req.UserId;

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound(new ProblemDetails
            {
                Detail = $"Can not find user with id {userId}"
            });
        }

        foreach (var passwordValidator in userManager.PasswordValidators)
        {
            var result = await passwordValidator.ValidateAsync(userManager, user, req.Password);

            if (!result.Succeeded)
            {
                var problemDetails = new ValidationProblemDetails(
                    result.Errors.ToDictionary(
                        m => m.Code,
                        m => new[]
                        {
                            m.Description
                        })
                    );

                return UnprocessableEntity(problemDetails);
            }
        }

        var setPasswordResult = await userManager.ResetPasswordAsync(user, req.Token, req.Password);

        if (!setPasswordResult.Succeeded)
        {
            var problemDetails = new ValidationProblemDetails(
                setPasswordResult.Errors.ToDictionary(
                    m => m.Code,
                    m => new[]
                    {
                        m.Description
                    })
                );

            return UnprocessableEntity(problemDetails);
        }

        return Ok(await MapAsync(user, userManager));
    }
}
