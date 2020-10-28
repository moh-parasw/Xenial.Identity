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

using Xenial.Identity.Areas.Identity.Pages.Account.Manage;
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
            public string UserImageTag { get; set; }
        }

        public async Task OnGet(CancellationToken cancellationToken = default)
            => Users = (await userManager.Users.ToListAsync(cancellationToken)).Select(r => new UserOutputModel
            {
                Id = r.Id,
                UserName = r.UserName,
                FirstName = r.FirstName,
                LastName = r.LastName,
                UserImageTag = UserImageTag(r)
            }).AsQueryable();

        public static string UserImageTag(XenialIdentityUser user)
        {
            var model = new ProfilePictureModel(user);
            var userImageTag = @$"<div class=""profile-card__image"" style=""--profile-card-height: 2.5rem;"">";

            if (model != null && string.IsNullOrEmpty(model.ImageUri) && string.IsNullOrEmpty(model.Inititals))
            {
                userImageTag += @$"<i class=""fas fa-user profile-card__image-item""></i>";
            }
            else if (model != null && !string.IsNullOrEmpty(model.ImageUri))
            {
                userImageTag += @$"<img src=""{model.ImageUri}"" class=""profile-card__image-item"" style="" cursor: auto;"" />";
            }
            else
            {
                userImageTag += @$"<span class=""profile-card__image-initials profile-card__image-item"" style=""--data-forecolor: {model.ForeColor}; --data-backcolor: {model.BackColor};"">{model.Inititals}</span>";
            }
            userImageTag += @$"</div>";
            return userImageTag;
        }
    }
}
