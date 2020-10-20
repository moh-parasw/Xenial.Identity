using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using DevExpress.Xpo;

using FluentAssertions;

using Microsoft.AspNetCore.Identity;

using Xenial.AspNetIdentity.Xpo.Models;
using Xenial.AspNetIdentity.Xpo.Stores;

using static Xenial.Tasty;

namespace Xenial.AspNetIdentity.Xpo.Tests.Stores
{
    public static class RoleStoreTests
    {
        public static void Tests(string dbName, string connectionString) => Describe($"{nameof(XPRoleStore<IdentityRole, string, IdentityUserRole<string>, IdentityRoleClaim<string>, XpoIdentityRole, XpoIdentityUser, XpoIdentityRoleClaim>)} using {dbName}", () =>
        {
            var dataLayer = XpoDefault.GetDataLayer(connectionString, DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);

            UnitOfWork unitOfWorkFactory() => new UnitOfWork(dataLayer);

            (XPRoleStore<IdentityRole, string, IdentityUserRole<string>, IdentityRoleClaim<string>, XpoIdentityRole, XpoIdentityUser, XpoIdentityRoleClaim> store, UnitOfWork uow) CreateStore()
            {
                var uow = new UnitOfWork(dataLayer);
                var store = new XPRoleStore<
                    IdentityRole,
                    string,
                    IdentityUserRole<string>,
                    IdentityRoleClaim<string>,
                    XpoIdentityRole,
                    XpoIdentityUser,
                    XpoIdentityRoleClaim
                >(
                    uow,
                    new FakeLogger<XPRoleStore<IdentityRole, string, IdentityUserRole<string>, IdentityRoleClaim<string>, XpoIdentityRole, XpoIdentityUser, XpoIdentityRoleClaim>>(),
                    new IdentityErrorDescriber()
                );
                return (store, uow);
            }

            XpoIdentityRole CreateRole(UnitOfWork uow, string id = null) => new XpoIdentityRole(uow)
            {
                Id = id ?? Guid.NewGuid().ToString(),
                Name = Guid.NewGuid().ToString(),
                NormalizedName = Guid.NewGuid().ToString(),
            };

            It("CreateAsync", async () =>
            {
                var id = Guid.NewGuid().ToString();
                var name = Guid.NewGuid().ToString();
                var normalizedName = Guid.NewGuid().ToString();

                var (store, uow) = CreateStore();
                using (uow)
                using (store)
                {
                    var result = await store.CreateAsync(new IdentityRole
                    {
                        Id = id,
                        Name = name,
                        NormalizedName = normalizedName
                    }, CancellationToken.None);
                    return result == IdentityResult.Success;
                }
            });

            It("DeleteAsync", async () =>
            {
                using var uow1 = unitOfWorkFactory();
                var role = CreateRole(uow1);

                var (store, uow) = CreateStore();
                using (uow)
                using (store)
                {
                    var result = await store.DeleteAsync(new IdentityRole
                    {
                        Id = role.Id
                    }, CancellationToken.None);
                    result.Should().Be(IdentityResult.Success);
                }

                using var uow2 = unitOfWorkFactory();
                var roleInDb = await uow2.GetObjectByKeyAsync<XpoIdentityRole>(role.Id);
                return roleInDb == null;
            });
        });
    }
}
