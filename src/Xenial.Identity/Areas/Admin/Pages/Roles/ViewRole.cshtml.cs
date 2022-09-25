using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Xenial.Identity.Data;

namespace Xenial.Identity.Areas.Admin.Pages.Roles
{
    public class ViewRoleModel : PageModel
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<XenialIdentityUser> userManager;

        public ViewRoleModel(
            RoleManager<IdentityRole> roleManager,
            UserManager<XenialIdentityUser> userManager
        )
            => (this.roleManager, this.userManager) = (roleManager, userManager);

        public class RoleOutputModel
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public List<ClaimModel> Claims { get; set; } = new List<ClaimModel>();
            public List<UserModel> Users { get; set; } = new List<UserModel>();

            public class ClaimModel
            {
                public string Type { get; set; }
                public string Value { get; set; }
                public string Issuer { get; set; }
            }

            public class UserModel
            {
                public string Id { get; set; }
                public string UserName { get; set; }
                public string ImageTag { get; set; }
            }
        }

        public RoleOutputModel Output { get; set; } = new RoleOutputModel();

        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGet([FromRoute] string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role == null)
            {
                StatusMessage = "Error: Cannot find role";
                return Page();
            }

            var claims = await roleManager.GetClaimsAsync(role);
            var users = await userManager.GetUsersInRoleAsync(role.NormalizedName);

            Output = new RoleOutputModel
            {
                Name = role.Name,
                Id = role.Id,
                Claims = claims.Select(c => new RoleOutputModel.ClaimModel
                {
                    Type = c.Type,
                    Value = c.Value,
                    Issuer = c.Issuer,
                }).ToList(),
                Users = users.Select(u => new RoleOutputModel.UserModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    ImageTag = UsersModel.UserImageTag(u)
                }).ToList()
            };

            return Page();
        }
    }
}
