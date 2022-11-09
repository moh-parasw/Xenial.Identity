using DevExpress.Xpo;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Xenial.AspNetIdentity.Xpo.Models;
using Xenial.Identity.Client;

namespace Xenial.Identity.Controllers;

[Route("api/management/users")]
[ApiController]
public sealed class UserManagementController : ControllerBase
{
    [Route("")]
    public async Task<IActionResult> Get([FromServices] UnitOfWork uow, CancellationToken cancellationToken)
    {
        var users = await uow
            .Query<XpoIdentityUser>()
            .ToListAsync(cancellationToken);

        var userModel = users.Select(m => new XenialUser(m.Id, m.UserName));

        return Ok(userModel);
    }
}
