using System.Security.Claims;
using System.Security.Principal;

using DevExpress.Data.Filtering.Helpers;
using DevExpress.Xpo;

using Duende.IdentityServer;
using Duende.IdentityServer.AspNetIdentity;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Validation;

using Microsoft.AspNetCore.Authentication;
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

    [Route("add")]
    [Authorize(AuthPolicies.UsersCreate)]
    [ProducesResponseType(typeof(XenialUser), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public Task<IActionResult> Create([FromBody] CreateXenialUserRequest req, [FromServices] UnitOfWork uow, CancellationToken cancellationToken)
    {
        return Task.FromResult<IActionResult>(Ok());
    }
}
