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
    [Route("")]
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

    [Route("create")]
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

        return Ok(user);
    }

    public sealed class CreateXenialUserRequestValidator : AbstractValidator<CreateXenialUserRequest>
    {
        public CreateXenialUserRequestValidator()
            => RuleFor(m => m.Email)
                .NotEmpty()
                .EmailAddress();
    }
}
