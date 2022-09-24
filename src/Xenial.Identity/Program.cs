﻿using AutoMapper;

using DevExpress.Data.Linq.Helpers;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

using MudBlazor.Services;

using Serilog;
using Serilog.Events;

using Westwind.AspNetCore.LiveReload;

using Xenial.AspNetIdentity.Xpo.Mappers;
using Xenial.AspNetIdentity.Xpo.Models;
using Xenial.AspNetIdentity.Xpo.Stores;
using Xenial.Identity.Areas.Admin.Pages.Clients;
using Xenial.Identity.Channels.Mail.MailKit;
using Xenial.Identity.Data;
using Xenial.Identity.Infrastructure;
using Xenial.Identity.Infrastructure.Channels;
using Xenial.Identity.Infrastructure.Localization;
using Xenial.Identity.Infrastructure.Logging.MemoryConsole.Themes;
using Xenial.Identity.Models;
using Xenial.Identity.Xpo.Storage.Models;

using XLocalizer;

SQLiteConnectionProvider.Register();
MySqlConnectionProvider.Register();

DevExpress.Xpo.Logger.LogManager.SetTransport(new XpoConsoleLogger());

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Memory(out var inMemoryLogSink, outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code)
    //#if !DEBUG
    .WriteTo.File(
        @"C:\logs\identity.xenial.io\Xenial.Platform.Identity.Api.log",
        fileSizeLimitBytes: 1_000_000,
        rollOnFileSizeLimit: true,
        shared: true,
        flushToDiskInterval: TimeSpan.FromSeconds(1))
    //#endif
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code)
    .WriteTo.Sink(inMemoryLogSink)
.CreateLogger();

try
{
    Log.Information("Starting host...");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    var Environment = builder.Environment;
    var Configuration = builder.Configuration;
    var services = builder.Services;

    services.AddSingleton(inMemoryLogSink);
    var localizer = new XpoStringLocalizer();
    services.AddSingleton(localizer);
    services.AddSingleton<IStringLocalizer>(localizer);
    services.AddSingleton<IHtmlLocalizer>(localizer);
    services.AddScoped<XpoStringLocalizerService>();

    services.AddCommunicationChannels(o =>
    {
        o.AddMailKit();
    });

    await UpdateDatabase(Configuration, localizer);

    if (Environment.IsDevelopment())
    {
        services.AddLiveReload();
    }

    var mvcBuilder = services.AddControllersWithViews();

    var razorPagesBuilder = services.AddRazorPages(o =>
    {
        o.Conventions.AuthorizeAreaFolder("Admin", "/", "Administrator");
        o.Conventions.AuthorizePage("/_Host", "Administrator");
    });

    razorPagesBuilder.AddXLocalizer<Program>(ops =>
    {
        localizer.SetOptions(ops);
    });
    services.AddSingleton<IStringLocalizer>(localizer);
    services.AddSingleton<IHtmlLocalizer>(localizer);

    services.AddMudServices();
    services.AddXpo(Configuration);
    services.AddXpoDefaultUnitOfWork();

    services.AddDefaultIdentity<XenialIdentityUser>(options =>
    {
        //TODO: Email Sender
        options.SignIn.RequireConfirmedAccount = false;
    }).AddXpoStores()
      .AddDefaultTokenProviders();

    services
        .AddScoped<IUserStore<XenialIdentityUser>>(s => new XPUserStore<XenialIdentityUser, XpoXeniaIIdentityUser>(
               s.GetService<UnitOfWork>(),
               s.GetService<ILogger<XPUserStore<XenialIdentityUser, XpoXeniaIIdentityUser>>>(),
               new IdentityErrorDescriber(),
               new MapperConfiguration(cfg => cfg.AddProfile<XPIdentityMapperProfile<XenialIdentityUser, IdentityRole, XpoXeniaIIdentityUser, XpoIdentityRole>>())
       ))
        .AddScoped<IRoleStore<IdentityRole>>(s => new XPRoleStore(
                s.GetService<UnitOfWork>(),
                s.GetService<ILogger<XPRoleStore>>(),
                new IdentityErrorDescriber()
        ))
        .AddScoped<UserManager<XenialIdentityUser>>()
        .AddScoped<RoleManager<IdentityRole>>()
        .AddScoped<IUserClaimsPrincipalFactory<XenialIdentityUser>, UserClaimsPrincipalFactory<XenialIdentityUser, IdentityRole>>()
    ;

    var idsBuilder = services.AddIdentityServer(options =>
    {
        options.Events.RaiseErrorEvents = true;
        options.Events.RaiseInformationEvents = true;
        options.Events.RaiseFailureEvents = true;
        options.Events.RaiseSuccessEvents = true;

        // see https://Duende.IdentityServer.readthedocs.io/en/latest/topics/resources.html
        options.EmitStaticAudienceClaim = true;
    })
      .AddRedirectUriValidator<RedirectValidator>()
      .AddAspNetIdentity<XenialIdentityUser>()
      .AddXpoIdentityStore();

    idsBuilder.AddCertificate(Environment, Configuration, null);

    services.AddAuthentication()
        .AddGitHub(options =>
        {
            options.ClientId = Configuration["GitHub:ClientId"];
            options.ClientSecret = Configuration["GitHub:ClientSecret"];
        });

    services.AddAuthorization(o =>
    {
        o.AddPolicy("Administrator", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole("Administrator");
        });
    });

    services.AddServerSideBlazor();

    services.Configure<RouteOptions>(options =>
    {
        options.ConstraintMap.Add(nameof(ClientTypes), typeof(EnumRouteConstraint<ClientTypes>));
    });

    Log.Information("Creating Application");

    var app = builder.Build();

    if (Environment.IsDevelopment())
    {
        app.UseLiveReload();
        app.UseDeveloperExceptionPage();
    }

    app.UseHttpsRedirection();

    app.UseStaticFiles();

    app.UseRouting();
    app.UseIdentityServer();
    app.UseAuthorization();
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapBlazorHub();
        endpoints.MapRazorPages();
        endpoints.MapControllers();
        endpoints.MapDefaultControllerRoute();
        endpoints.MapFallbackToPage("/_Host");
    });

    Log.Information("Run Application");
    app.Run();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly.");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

static void SeedDatabase(UnitOfWork unitOfWork)
{
    if (unitOfWork.FindObject<XpoThemeSettings>(null) is null)
    {
        unitOfWork.Save(new XpoThemeSettings(unitOfWork));
        unitOfWork.CommitChanges();
    }
    if (unitOfWork.FindObject<XpoApplicationSettings>(null) is null)
    {
        unitOfWork.Save(new XpoApplicationSettings(unitOfWork));
        unitOfWork.CommitChanges();
    }
    if (unitOfWork.Query<XpoIdentityResource>().Count() <= 0)
    {
        AddResource("profile", "Profile", new[] {
            "profile",
            "name",
            "given_name",
            "family_name",
            "middle_name",
            "nickname",
            "preferred_username"
        });

        AddResource("email", "E-mail", new[] {
            "email",
            "email_verified",
        });

        AddResource("openid", "User-Id", new[] {
            "openid",
            "sub",
        });

        AddResource("phone", "Phonenumber", new[] {
            "phone",
            "phone_verified",
        });

        AddResource("role", "Role", new[] {
            "role",
        });

        AddResource("xenial", "Avatar", new[] {
            "xenial",
            "xenial_backcolor",
            "xenial_forecolor",
            "xenial_initials",
        });

        unitOfWork.CommitChanges();
    }

    void AddResource(
        string name,
        string displayName,
        string[] resources,
        bool enabled = true,
        bool showInDiscoveryDocument = true
    )
    {
        var xpoResources = resources.Select(
            type => new XpoIdentityResourceClaim(unitOfWork)
            {
                Type = type
            }).ToArray();

        var resource = new XpoIdentityResource(unitOfWork)
        {
            Name = name,
            DisplayName = displayName,
            Enabled = true,
            ShowInDiscoveryDocument = true
        };
        resource.UserClaims.AddRange(xpoResources);
        unitOfWork.Save(resource);
    }
}

static async Task UpdateDatabase(ConfigurationManager Configuration, XpoStringLocalizer localizer)
{
    Log.Information("Update Database");

    var serviceCollection = new ServiceCollection();

    serviceCollection
        .AddXpo(Configuration, AutoCreateOption.DatabaseAndSchema)
        .AddSingleton(localizer)
        .AddScoped<XpoStringLocalizerService>()
        .AddXpoDefaultUnitOfWork();

    using (var provider = serviceCollection.BuildServiceProvider())
    using (var unitOfWork = provider.GetRequiredService<UnitOfWork>())
    {
        unitOfWork.UpdateSchema();
        SeedDatabase(unitOfWork);
        await provider.GetRequiredService<XpoStringLocalizerService>().Refresh();
    }

    Log.Information("Update Done");
}
