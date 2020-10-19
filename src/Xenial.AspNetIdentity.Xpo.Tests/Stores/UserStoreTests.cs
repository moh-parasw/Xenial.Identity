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

            XpoIdentityUser CreateUser(UnitOfWork uow, string id = null) => new XpoIdentityUser(uow)
            {
                Id = id ?? Guid.NewGuid().ToString(),
                UserName = Guid.NewGuid().ToString(),
                Email = Guid.NewGuid().ToString(),
            };

            It($"Can {nameof(store.CreateAsync)}", async () =>
            {
                var result = await store.CreateAsync(new IdentityUser { Id = Guid.NewGuid().ToString() }, CancellationToken.None);
                return result.Succeeded;
            });

            It($"Can {nameof(store.DeleteAsync)}", async () =>
            {
                using var uow = unitOfWorkFactory();
                var id = Guid.NewGuid().ToString();
                var user = CreateUser(uow, id);

                await uow.SaveAsync(user);
                await uow.CommitChangesAsync();

                var result = await store.DeleteAsync(new IdentityUser { Id = id }, CancellationToken.None);
                return result.Succeeded;
            });

            Describe($"Can {nameof(store.FindByIdAsync)}", () =>
            {
                It($"with existing", async () =>
                {
                    using var uow = unitOfWorkFactory();
                    var id = Guid.NewGuid().ToString();
                    var user = CreateUser(uow, id);

                    await uow.SaveAsync(user);
                    await uow.CommitChangesAsync();

                    var result = await store.FindByIdAsync(id, CancellationToken.None);
                    return result.Id == id;
                });

                It($"with not existing", async () =>
                {
                    var id = Guid.NewGuid().ToString();

                    var result = await store.FindByIdAsync(id, CancellationToken.None);
                    return result == null;
                });
            });

            Describe($"Can {nameof(store.FindByNameAsync)}", () =>
            {
                It($"with existing", async () =>
                {
                    using var uow = unitOfWorkFactory();
                    var name = Guid.NewGuid().ToString();
                    var user = CreateUser(uow);
                    user.NormalizedUserName = name;

                    await uow.SaveAsync(user);
                    await uow.CommitChangesAsync();

                    var result = await store.FindByNameAsync(name, CancellationToken.None);
                    return result.NormalizedUserName == name;
                });

                It($"with not existing", async () =>
                {
                    var id = Guid.NewGuid().ToString();

                    var result = await store.FindByNameAsync(id, CancellationToken.None);
                    return result == null;
                });
            });
        });
    }
}
