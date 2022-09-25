// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Xenial.Identity.Quickstart.Account
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult AccessDenied() => View();
    }
}
