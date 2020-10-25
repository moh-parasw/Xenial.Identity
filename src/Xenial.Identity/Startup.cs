using System.Linq;

using AutoMapper;

using DevExpress.Xpo;


using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Westwind.AspNetCore.LiveReload;

using Xenial.AspNetIdentity.Xpo.Mappers;
using Xenial.AspNetIdentity.Xpo.Models;
using Xenial.AspNetIdentity.Xpo.Stores;
using Xenial.Identity.Data;
using Xenial.Identity.Models;
using Xenial.Identity.Xpo.Storage;

namespace Xenial.Identity
{
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

            var razorPagesBuilder = services.AddRazorPages();

            if (Environment.IsDevelopment())
            {
                razorPagesBuilder.AddRazorRuntimeCompilation();
            }

            services.AddXpoDefaultDataLayer(ServiceLifetime.Singleton, dl => dl
                .UseConnectionString(Configuration.GetConnectionString("DefaultConnection"))
                .UseThreadSafeDataLayer(true)
                .UseConnectionPool(false) // Remove this line if you use a database server like SQL Server, Oracle, PostgreSql, etc.
                .UseAutoCreationOption(DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema)
                .UseEntityTypes(
                    IdentityXpoTypes.PersistentTypes
                        .Concat(IdentityModelTypeList.ModelTypes)
                        .Concat(XenialIdentityModelTypeList.ModelTypes)
                        .ToArray()
                )
            );

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

                // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
                options.EmitStaticAudienceClaim = true;
            })
                .AddAspNetIdentity<XenialIdentityUser>()
                .AddXpoIdentityStore();

            // in-memory, code config
            builder.AddInMemoryIdentityResources(Config.IdentityResources);
            builder.AddInMemoryApiScopes(Config.ApiScopes);
            builder.AddInMemoryClients(Config.Clients);

            // not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential();

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

            //.AddOAuth("github", "Github", options =>
            //{
            //    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

            //    options.CallbackPath = new PathString("/signin-github");
            //    options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
            //    options.TokenEndpoint = "https://github.com/login/oauth/access_token";
            //    options.UserInformationEndpoint = "https://api.github.com/user";
            //    options.ClaimsIssuer = "OAuth2-Github";
            //    options.SaveTokens = true;

            //    options.Events = new OAuthEvents
            //    {
            //        OnCreatingTicket = async context => { await CreatingGitHubAuthTicket(context); }
            //    };

            //    static async Task CreatingGitHubAuthTicket(OAuthCreatingTicketContext context)
            //    {
            //        // Get the GitHub user
            //        var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            //        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
            //        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //        var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
            //        response.EnsureSuccessStatusCode();

            //        var user = JObject.Parse(await response.Content.ReadAsStringAsync());

            //        AddClaims(context, user);
            //    }

            //    static void AddClaims(OAuthCreatingTicketContext context, JObject user)
            //    {
            //        var identifier = user.Value<string>("id");
            //        if (!string.IsNullOrEmpty(identifier))
            //        {
            //            context.Identity.AddClaim(new Claim(
            //                ClaimTypes.NameIdentifier, identifier,
            //                ClaimValueTypes.String, context.Options.ClaimsIssuer));
            //        }

            //        var userName = user.Value<string>("login");
            //        if (!string.IsNullOrEmpty(userName))
            //        {
            //            context.Identity.AddClaim(new Claim(
            //                ClaimsIdentity.DefaultNameClaimType, userName,
            //                ClaimValueTypes.String, context.Options.ClaimsIssuer));
            //        }

            //        var name = user.Value<string>("name");
            //        if (!string.IsNullOrEmpty(name))
            //        {
            //            context.Identity.AddClaim(new Claim(
            //                "urn:github:name", name,
            //                ClaimValueTypes.String, context.Options.ClaimsIssuer));
            //        }

            //        var link = user.Value<string>("url");
            //        if (!string.IsNullOrEmpty(link))
            //        {
            //            context.Identity.AddClaim(new Claim(
            //                "urn:github:url", link,
            //                ClaimValueTypes.String, context.Options.ClaimsIssuer));
            //        }

            //        var avatarUrl = user.Value<string>("avatar_url");
            //        if (!string.IsNullOrEmpty(avatarUrl))
            //        {
            //            context.Identity.AddClaim(new Claim(
            //                "urn:github:avatar_url", avatarUrl,
            //                ClaimValueTypes.String, context.Options.ClaimsIssuer));
            //        }
            //    }
            //});

            services.AddServerSideBlazor();
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
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
