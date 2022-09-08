using System.Linq;

using AutoMapper;

using DevExpress.Xpo;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using MudBlazor.Services;

using Westwind.AspNetCore.LiveReload;

using Xenial.AspNetIdentity.Xpo.Mappers;
using Xenial.AspNetIdentity.Xpo.Models;
using Xenial.AspNetIdentity.Xpo.Stores;
using Xenial.Identity.Areas.Admin.Pages.Clients;
using Xenial.Identity.Data;
using Xenial.Identity.Infrastructure;
using Xenial.Identity.Models;
using Xenial.Identity.Xpo.Storage;

namespace Xenial.Identity;

public class Startup
{
    public IWebHostEnvironment Environment { get; }
    public IConfiguration Configuration { get; }

    public Startup(IWebHostEnvironment environment, IConfiguration configuration)
    {
        Environment = environment;
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        if (Environment.IsDevelopment())
        {
            services.AddLiveReload();
        }

        var mvcBuilder = services.AddControllersWithViews();

        if (Environment.IsDevelopment())
        {
            mvcBuilder.AddRazorRuntimeCompilation();
        }

        var razorPagesBuilder = services.AddRazorPages(o =>
        {
            o.Conventions.AuthorizeAreaFolder("Admin", "/", "Administrator");
            o.Conventions.AuthorizePage("/_Host", "Administrator");
        });

        if (Environment.IsDevelopment())
        {
            razorPagesBuilder.AddRazorRuntimeCompilation();
        }

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

        var builder = services.AddIdentityServer(options =>
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

        builder.AddCertificate(Environment, Configuration, null);

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
                //policy.RequireRole("Administrator");
            });
        });

        services.AddServerSideBlazor();

        services.Configure<RouteOptions>(options =>
        {
            options.ConstraintMap.Add(nameof(ClientTypes), typeof(EnumRouteConstraint<ClientTypes>));
        });
    }

    public void Configure(IApplicationBuilder app)
    {
        if (Environment.IsDevelopment())
        {
            app.UseLiveReload();
            app.UseDeveloperExceptionPage();
        }

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
    }
}
