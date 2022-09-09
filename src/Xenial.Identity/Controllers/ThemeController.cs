using System;

using DevExpress.Xpo;

using Microsoft.AspNetCore.Mvc;

using Xenial.Identity.Components.Admin;
using Xenial.Identity.Models;

namespace Xenial.Identity.Controllers;

[ApiController]
public class ThemeController : Controller
{
    private readonly UnitOfWork unitOfWork;
    private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment environment;

    public ThemeController(UnitOfWork unitOfWork, Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment)
    {
        environment = _environment;
        this.unitOfWork = unitOfWork;
    }

    [Route("/themes/logo")]
    public async Task<IActionResult> GetLogo()
    {
        var settings = await unitOfWork.FindObjectAsync<XpoThemeSettings>(null);
        if (settings.CustomLogo.Length > 0)
        {
            return File(settings.CustomLogo, settings.CustomLogoMimeType, false);
        }
        var logo = environment.WebRootFileProvider.GetFileInfo("img/logo.svg");
        var fs = logo.CreateReadStream();
        return File(fs, "image/svg+xml", false);
    }

    [Route("/themes/styles")]
    public async Task<IActionResult> GetStyleSheet()
    {
        var settings = await unitOfWork.FindObjectAsync<XpoThemeSettings>(null);
        var ms = GenerateStreamFromString(settings.CustomCss);
        return File(ms, "text/css", false);
    }

    [Route("/themes/favicon")]
    public async Task<IActionResult> GetFacivon()
    {
        var settings = await unitOfWork.FindObjectAsync<XpoThemeSettings>(null);
        if (settings.CustomFacivon.Length > 0)
        {
            return File(settings.CustomFacivon, "image/x-icon", false);
        }
        var logo = environment.WebRootFileProvider.GetFileInfo("img/favicon.ico");
        var fs = logo.CreateReadStream();
        return File(fs, "image/x-icon", false);
    }

    private static Stream GenerateStreamFromString(string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}
