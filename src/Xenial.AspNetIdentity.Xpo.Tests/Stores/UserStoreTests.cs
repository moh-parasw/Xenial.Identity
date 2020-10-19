using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using DevExpress.Xpo;

using Microsoft.AspNetCore.Identity;

using Xenial.AspNetIdentity.Xpo.Models;
using Xenial.AspNetIdentity.Xpo.Stores;

using static Xenial.Tasty;

namespace Xenial.AspNetIdentity.Xpo.Tests.Stores
{
    public static class UserStoreTests
    {
        public static void Tests(string name, string connectionString) => Describe($"{nameof(XPUserStore<IdentityUser, string, XpoIdentityUser>)} using {name}", () =>
        {
            var dataLayer = XpoDefault.GetDataLayer(connectionString, DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);
            Func<UnitOfWork> unitOfWorkFactory = () => new UnitOfWork(dataLayer);
            var store = new XPUserStore<IdentityUser, string, XpoIdentityUser>(unitOfWorkFactory, new FakeLogger<XPUserStore<IdentityUser, string, XpoIdentityUser>>());

            It($"Can {nameof(store.CreateAsync)}", async () =>
            {
                var result = await store.CreateAsync(new IdentityUser { Id = Guid.NewGuid().ToString() }, CancellationToken.None);
                return result.Succeeded;
            });

            It($"Can {nameof(store.DeleteAsync)}", async () =>
            {
                using var uow = unitOfWorkFactory();
                var id = Guid.NewGuid().ToString();
                var user = new XpoIdentityUser(uow)
                {
                    Id = id,
                    UserName = Guid.NewGuid().ToString(),
                    Email = Guid.NewGuid().ToString(),
                };

                await uow.SaveAsync(user);
                await uow.CommitChangesAsync();

                var result = await store.DeleteAsync(new IdentityUser { Id = id }, CancellationToken.None);
                return result.Succeeded;
            });
        });
    }
}
