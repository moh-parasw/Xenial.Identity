// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Duende.IdentityServer.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Xenial.Identity.Data;

namespace Xenial.Identity.Quickstart.Home
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly IIdentityServerInteractionService interaction;
        private readonly IWebHostEnvironment environment;
        private readonly ILogger logger;
        private readonly SignInManager<XenialIdentityUser> signInManager;

        public HomeController(
            IIdentityServerInteractionService interaction,
            IWebHostEnvironment environment,
            ILogger<HomeController> logger,
            SignInManager<XenialIdentityUser> signInManager)
        {
            this.interaction = interaction;
            this.environment = environment;
            this.logger = logger;
            this.signInManager = signInManager;
        }

        public IActionResult Index() => signInManager.IsSignedIn(User) ? Redirect("~/Identity/Account/Manage") : (IActionResult)Redirect("~/Identity/Account/Login");

        /// <summary>
        /// Shows the error page
        /// </summary>
        public async Task<IActionResult> Error(string errorId)
        {
            var vm = new ErrorViewModel();

            // retrieve error details from identityserver
            var message = await interaction.GetErrorContextAsync(errorId);
            if (message != null)
            {
                vm.Error = message;

                if (!environment.IsDevelopment())
                {
                    // only show in development
                    message.ErrorDescription = null;
                }
            }

            return View("Error", vm);
        }
    }
}
