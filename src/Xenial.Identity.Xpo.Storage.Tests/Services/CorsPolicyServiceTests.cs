using System;
using System.Collections.Generic;
using System.Text;

using DevExpress.Xpo;

using Duende.IdentityServer.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Xenial.Identity.Xpo.Services;
using Xenial.Identity.Xpo.Storage.Mappers;
using Xenial.Identity.Xpo.Storage.Models;

using static Xenial.Tasty;

namespace Xenial.Identity.Xpo.Storage.Tests.Services
{
    public static class CorsPolicyServiceTests
    {
        public static void Tests(string name, string connectionString) => Describe($"{nameof(CorsPolicyService)} using {name}", () =>
        {
            var dataLayer = XpoDefault.GetDataLayer(connectionString, DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);

            using var unitOfWork = new UnitOfWork(dataLayer);
            unitOfWork.UpdateSchema();

            (CorsPolicyService, ServiceProvider) CreateService()
            {
                var ctx = new DefaultHttpContext();
                var svcs = new ServiceCollection();
                svcs.AddTransient(_ => new UnitOfWork(dataLayer));
                var serviceProvider = svcs.BuildServiceProvider();
                ctx.RequestServices = serviceProvider;
                var ctxAccessor = new HttpContextAccessor();
                ctxAccessor.HttpContext = ctx;

                var service = new CorsPolicyService(ctxAccessor, FakeLogger<CorsPolicyService>.Create());
                return (service, serviceProvider);
            }

            It("IsOriginAllowedAsync when origin is allowed", async () =>
            {
                const string testCorsOrigin = "https://identityserver.io/";

                using (var uow = new UnitOfWork(dataLayer))
                {
                    await uow.SaveAsync(new Client
                    {
                        ClientId = Guid.NewGuid().ToString(),
                        ClientName = Guid.NewGuid().ToString(),
                        AllowedCorsOrigins = new List<string> { "https://www.identityserver.com" }
                    }.ToEntity(uow));

                    await uow.SaveAsync(new Client
                    {
                        ClientId = "2",
                        ClientName = "2",
                        AllowedCorsOrigins = new List<string> { "https://www.identityserver.com", testCorsOrigin }
                    }.ToEntity(uow));
                    await uow.CommitChangesAsync();
                }


                var (service, disposable) = CreateService();
                using (disposable)
                {
                    return await service.IsOriginAllowedAsync(testCorsOrigin);
                }
            });

            It("IsOriginAllowedAsync when origin is not allowed", async () =>
            {
                using (var uow = new UnitOfWork(dataLayer))
                {
                    await uow.SaveAsync(new Client
                    {
                        ClientId = Guid.NewGuid().ToString(),
                        ClientName = Guid.NewGuid().ToString(),
                        AllowedCorsOrigins = new List<string> { "https://www.identityserver.com" }
                    }.ToEntity(uow));

                    await uow.CommitChangesAsync();
                }

                var (service, disposable) = CreateService();
                using (disposable)
                {
                    var result = await service.IsOriginAllowedAsync("InvalidOrigin");
                    return !result;
                }
            });
        });
    }
}
