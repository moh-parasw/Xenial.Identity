using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper.QueryableExtensions;

using DevExpress.Xpo;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Xenial.Identity.Data;

namespace Xenial.Identity.Areas.Admin.Pages
{
    public class UsersModel : PageModel
    {
        private readonly UserManager<XenialIdentityUser> userManager;

        public UsersModel(UserManager<XenialIdentityUser> userManager)
            => this.userManager = userManager;

        public IQueryable<UserOutputModel> Users { get; private set; } = new UserOutputModel[0].AsQueryable();
        public class UserOutputModel
        {
            public string Id { get; set; }
            public string UserName { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        public async Task OnGet(CancellationToken cancellationToken = default)
            => Users = (await userManager.Users.ToListAsync(cancellationToken)).Select(r => new UserOutputModel
            {
                Id = r.Id,
                UserName = r.UserName,
                FirstName = r.FirstName,
                LastName = r.LastName,
            }).AsQueryable();
    }
}
