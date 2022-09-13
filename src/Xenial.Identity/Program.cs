using DevExpress.Data.Linq.Helpers;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;

using Duende.IdentityServer.Models;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

using System;

using Xenial.Identity;
using Xenial.Identity.Infrastructure;
using Xenial.Identity.Models;
using Xenial.Identity.Xpo.Storage.Models;

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
    //#if !DEBUG
    .WriteTo.File(
        @"C:\logs\identity.xenial.io\Xenial.Platform.Identity.Api.log",
        fileSizeLimitBytes: 1_000_000,
        rollOnFileSizeLimit: true,
        shared: true,
        flushToDiskInterval: TimeSpan.FromSeconds(1))
    //#endif
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code)
    .CreateLogger();

try
{
    Log.Information("Starting host...");

    var host = CreateHostBuilder(args).Build();

    Log.Information("Update Database");

    var serviceCollection = new ServiceCollection();
    serviceCollection
        .AddXpo(host.Services.GetRequiredService<IConfiguration>(), AutoCreateOption.DatabaseAndSchema)
        .AddXpoDefaultUnitOfWork();

    using (var provider = serviceCollection.BuildServiceProvider())
    using (var unitOfWork = provider.GetRequiredService<UnitOfWork>())
    {
        unitOfWork.UpdateSchema();
        SeedDatabase(unitOfWork);
    }

    Log.Information("Update Done");

    host.Run();
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

static IHostBuilder CreateHostBuilder(string[] args)
    => Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });

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
