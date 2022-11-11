using AutoMapper;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;

using Duende.IdentityServer;

using FluentValidation;
using FluentValidation.AspNetCore;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;

using MudBlazor.Services;

using Serilog;
using Serilog.Events;

using Westwind.AspNetCore.LiveReload;
using Xenial.AspNetIdentity.Xpo.Mappers;
using Xenial.AspNetIdentity.Xpo.Models;
using Xenial.AspNetIdentity.Xpo.Stores;
using Xenial.Identity;
using Xenial.Identity.Areas.Admin.Pages.Clients;
using Xenial.Identity.Channels;
using Xenial.Identity.Data;
using Xenial.Identity.Infrastructure;
using Xenial.Identity.Infrastructure.Localization;
using Xenial.Identity.Infrastructure.Logging.MemoryConsole.Themes;
using Xenial.Identity.Models;
using Xenial.Identity.Xpo;

using XLocalizer;

SQLiteConnectionProvider.Register();
MySqlConnectionProvider.Register();
PostgreSqlConnectionProvider.Register();

if (CreateLogger)
{
    DevExpress.Xpo.Logger.LogManager.SetTransport(new XpoConsoleLogger());
}

var loggerConfig = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
        .Enrich.FromLogContext()
        .WriteTo.Memory(out var inMemoryLogSink, outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code)
        ;

if (CreateLogger)
{
    loggerConfig = loggerConfig
        .WriteTo.File(
            @"C:\logs\identity.xenial.io\Xenial.Platform.Identity.Api.log",
            fileSizeLimitBytes: 1_000_000,
            rollOnFileSizeLimit: true,
            shared: true,
            flushToDiskInterval: TimeSpan.FromSeconds(1))
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code)
        ;
}

loggerConfig = loggerConfig
        .WriteTo.Sink(inMemoryLogSink);

Log.Logger = loggerConfig.CreateLogger();

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


    services.AddValidatorsFromAssemblyContaining<Program>();
    services.AddCommunicationChannels(o =>
    {
        o.AddMailKit();
        o.AddAnySms();
        o.AddWebSmsCom();
    });

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
    services.AddSingleton<DatabaseUpdateHandler>();
    services.AddXpoDefaultUnitOfWork();

    services.AddDefaultIdentity<XenialIdentityUser>(options =>
    {
        //TODO: Email Sender
        options.SignIn.RequireConfirmedAccount = false;
        options.ClaimsIdentity.UserNameClaimType = "name";
        options.ClaimsIdentity.UserIdClaimType = "sub";
        options.ClaimsIdentity.RoleClaimType = "role";

    }).AddClaimsPrincipalFactory<MyAppUserClaimsPrincipalFactory>()
      .AddXpoStores()
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
      .AddCustomTokenRequestValidator<ClientCredentialsTokenRequestValidator>()
      .AddRedirectUriValidator<RedirectValidator>()
      .AddAspNetIdentity<XenialIdentityUser>()
      .AddXpoIdentityStore();

    idsBuilder.AddCertificate(Environment, Configuration, null);

    services
        .AddAuthentication()
        .AddLocalApi(o =>
        {
            o.ExpectedScope = IdentityServerConstants.LocalApi.ScopeName;
        })
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
        AuthPolicies.Configure(o);
    });

    services.AddServerSideBlazor();

    services.Configure<RouteOptions>(options =>
    {
        options.ConstraintMap.Add(nameof(ClientTypes), typeof(EnumRouteConstraint<ClientTypes>));
    });

    services.AddHostedService<UpdateDatabaseBackgroundService>();

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
        endpoints.MapGet("/ping", () => DateTime.UtcNow);
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

public partial class Program
{
    public static bool CreateLogger { get; set; } = true;
}
