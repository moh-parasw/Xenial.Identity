using System.Collections.Immutable;
using System.Diagnostics;
using System.Security.Claims;

using DevExpress.Xpo;

using Duende.IdentityServer.Services;

using FluentValidation;
using FluentValidation.AspNetCore;

using IdentityModel;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Xenial.AspNetIdentity.Xpo.Models;
using Xenial.Identity.Client;
using Xenial.Identity.Components.Admin;
using Xenial.Identity.Data;
using Xenial.Identity.Infrastructure;

namespace Xenial.Identity.Controllers;

[Route("api/management/users")]
[ApiController]
[Authorize]
public sealed class UserManagementController : ControllerBase
{
    [Route("")]
    [HttpGet]
    [Authorize(AuthPolicies.UsersRead)]
    [ProducesResponseType(typeof(IEnumerable<XenialUser>), 200)]
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
            UserName = req.Email,
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
                .NotEmpty();

            RuleFor(m => m.RoleName)
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

        if (User.IsInRole(DatabaseUpdateHandler.AdminRoleName))
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

            user = await userManager.FindByIdAsync(userId);
            return Ok(await MapAsync(user, userManager));
        }

        return BadRequest(new ProblemDetails
        {
            Detail = $"You are not allowed to add the role {req.RoleName}"
        });
    }

}
