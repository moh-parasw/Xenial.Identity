using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using DevExpress.Xpo;

using FluentAssertions;

using Microsoft.AspNetCore.Identity;

using Xenial.AspNetIdentity.Xpo.Models;
using Xenial.AspNetIdentity.Xpo.Stores;

using static Xenial.Tasty;

namespace Xenial.AspNetIdentity.Xpo.Tests.Stores
{
    public static class UserStoreTests
    {
        public static void Tests(string dbName, string connectionString) => Describe($"{nameof(XPUserStore<IdentityUser, string, IdentityUserClaim<string>, IdentityUserLogin<string>, IdentityUserToken<string>, XpoIdentityUser, XpoIdentityUserClaim, XpoIdentityUserLogin, XpoIdentityUserToken>)} using {dbName}", () =>
        {
            var dataLayer = XpoDefault.GetDataLayer(connectionString, DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);

            UnitOfWork unitOfWorkFactory() => new UnitOfWork(dataLayer);

            (XPUserStore<IdentityUser, string, IdentityUserClaim<string>, IdentityUserLogin<string>, IdentityUserToken<string>, XpoIdentityUser, XpoIdentityUserClaim, XpoIdentityUserLogin, XpoIdentityUserToken> store, UnitOfWork uow) CreateStore()
            {
                var uow = new UnitOfWork(dataLayer);
                var store = new XPUserStore<
                    IdentityUser,
                    string,
                    IdentityUserClaim<string>,
                    IdentityUserLogin<string>,
                    IdentityUserToken<string>,
                    XpoIdentityUser,
                    XpoIdentityUserClaim,
                    XpoIdentityUserLogin,
                    XpoIdentityUserToken
                >(
                    uow,
                    new FakeLogger<XPUserStore<IdentityUser, string, IdentityUserClaim<string>, IdentityUserLogin<string>, IdentityUserToken<string>, XpoIdentityUser, XpoIdentityUserClaim, XpoIdentityUserLogin, XpoIdentityUserToken>>(),
                    new IdentityErrorDescriber()
                );
                return (store, uow);
            }
            XpoIdentityUser CreateUser(UnitOfWork uow, string id = null) => new XpoIdentityUser(uow)
            {
                Id = id ?? Guid.NewGuid().ToString(),
                UserName = Guid.NewGuid().ToString(),
                Email = Guid.NewGuid().ToString(),
            };

            It($"Can CreateAsync", async () =>
            {
                var (store, uow) = CreateStore();
                using (store)
                using (uow)
                {
                    var result = await store.CreateAsync(new IdentityUser { Id = Guid.NewGuid().ToString() }, CancellationToken.None);
                    return result.Succeeded;
                }
            });

            It($"Can DeleteAsync", async () =>
            {
                using var uow = unitOfWorkFactory();
                var id = Guid.NewGuid().ToString();
                var user = CreateUser(uow, id);

                await uow.SaveAsync(user);
                await uow.CommitChangesAsync();

                var (store, uow1) = CreateStore();
                using (store)
                using (uow1)
                {
                    var result = await store.DeleteAsync(new IdentityUser { Id = id }, CancellationToken.None);
                    return result.Succeeded;
                }
            });

            Describe($"Can FindByIdAsync", () =>
            {
                It($"with existing", async () =>
                {
                    using var uow = unitOfWorkFactory();
                    var id = Guid.NewGuid().ToString();
                    var user = CreateUser(uow, id);

                    await uow.SaveAsync(user);
                    await uow.CommitChangesAsync();

                    var (store, uow1) = CreateStore();
                    using (store)
                    using (uow1)
                    {
                        var result = await store.FindByIdAsync(id, CancellationToken.None);
                        return result.Id == id;
                    }
                });

                It($"with not existing", async () =>
                {
                    var id = Guid.NewGuid().ToString();
                    var (store, uow) = CreateStore();
                    using (store)
                    using (uow)
                    {
                        var result = await store.FindByIdAsync(id, CancellationToken.None);
                        return result == null;
                    }
                });
            });

            Describe($"Can FindByNameAsync", () =>
            {
                It($"with existing", async () =>
                {
                    using var uow = unitOfWorkFactory();
                    var name = Guid.NewGuid().ToString();
                    var user = CreateUser(uow);
                    user.NormalizedUserName = name;

                    await uow.SaveAsync(user);
                    await uow.CommitChangesAsync();

                    var (store, uow1) = CreateStore();
                    using (store)
                    using (uow1)
                    {
                        var result = await store.FindByNameAsync(name, CancellationToken.None);
                        return result.NormalizedUserName == name;
                    }
                });

                It($"with not existing", async () =>
                {
                    var id = Guid.NewGuid().ToString();
                    var (store, uow) = CreateStore();
                    using (store)
                    using (uow)
                    {
                        var result = await store.FindByNameAsync(id, CancellationToken.None);
                        return result == null;
                    }
                });
            });

            Describe($"Can UpdateAsync", () =>
            {
                It($"with existing", async () =>
                {
                    using var uow = unitOfWorkFactory();
                    var id = Guid.NewGuid().ToString();
                    var user = CreateUser(uow, id);
                    user.NormalizedUserName = dbName;

                    await uow.SaveAsync(user);
                    await uow.CommitChangesAsync();

                    var (store, uow1) = CreateStore();
                    using (store)
                    using (uow1)
                    {
                        var userFromStore = await store.FindByIdAsync(id, CancellationToken.None);
                        userFromStore.PhoneNumber = Guid.NewGuid().ToString();
                        var result = await store.UpdateAsync(userFromStore, CancellationToken.None);

                        using var uow2 = unitOfWorkFactory();
                        var userFromDb = uow2.GetObjectByKey<XpoIdentityUser>(id);

                        return result.Succeeded && userFromDb.PhoneNumber == userFromStore.PhoneNumber;
                    }
                });

                It($"with not existing", async () =>
                {
                    var id = Guid.NewGuid().ToString();

                    var (store, uow) = CreateStore();
                    using (store)
                    using (uow)
                    {
                        var result = await store.UpdateAsync(new IdentityUser(), CancellationToken.None);

                        return !result.Succeeded;
                    }
                });
            });

            Describe($"Can FindByEmailAsync", () =>
            {
                It($"with existing", async () =>
                {
                    using var uow = unitOfWorkFactory();
                    var name = Guid.NewGuid().ToString();
                    var user = CreateUser(uow);
                    user.NormalizedEmail = name;

                    await uow.SaveAsync(user);
                    await uow.CommitChangesAsync();

                    var (store, uow1) = CreateStore();
                    using (store)
                    using (uow1)
                    {
                        var result = await store.FindByEmailAsync(name, CancellationToken.None);
                        return result.NormalizedEmail == name;
                    }
                });

                It($"with not existing", async () =>
                {
                    var id = Guid.NewGuid().ToString();

                    var (store, uow) = CreateStore();
                    using (store)
                    using (uow)
                    {
                        var result = await store.FindByEmailAsync(id, CancellationToken.None);
                        return result == null;
                    }
                });
            });
            Describe("Tokens", () =>
            {
                Describe("GetTokenAsync", () =>
                {
                    It("non existing token", async () =>
                    {
                        var (store, uow) = CreateStore();
                        using (store)
                        using (uow)
                        {
                            var token = await store.GetTokenAsync(new IdentityUser(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), CancellationToken.None);

                            return token == null;
                        }
                    });

                    It("existing token", async () =>
                    {
                        using var uow = unitOfWorkFactory();
                        var name = Guid.NewGuid().ToString();
                        var user = CreateUser(uow);
                        var loginProvider = Guid.NewGuid().ToString();
                        var loginProviderName = Guid.NewGuid().ToString();
                        var expectedToken = Guid.NewGuid().ToString();
                        user.Tokens.Add(new XpoIdentityUserToken(uow)
                        {
                            Id = Guid.NewGuid().ToString(),
                            LoginProvider = loginProvider,
                            Name = loginProviderName,
                            Value = expectedToken
                        });

                        await uow.SaveAsync(user);
                        await uow.CommitChangesAsync();

                        var (store, uow1) = CreateStore();
                        using (store)
                        using (uow1)
                        {
                            var token = await store.GetTokenAsync(
                                new IdentityUser { Id = user.Id },
                                loginProvider,
                                loginProviderName,
                                CancellationToken.None
                            );

                            return expectedToken == token;
                        }
                    });
                });

                Describe("SetTokenAsync", () =>
                {
                    It("non existing user and token", async () =>
                    {
                        var loginProvider = Guid.NewGuid().ToString();
                        var loginProviderName = Guid.NewGuid().ToString();
                        var tokenValue = Guid.NewGuid().ToString();

                        var (store, uow) = CreateStore();
                        using (store)
                        using (uow)
                        {
                            var action = new Func<Task>(async () => await store.SetTokenAsync(new IdentityUser(), loginProvider, loginProviderName, tokenValue, CancellationToken.None));
                            await action.Should().ThrowAsync<ArgumentNullException>();
                        }
                    });

                    It("adds non existing token", async () =>
                    {
                        var loginProvider = Guid.NewGuid().ToString();
                        var loginProviderName = Guid.NewGuid().ToString();
                        var tokenValue = Guid.NewGuid().ToString();
                        using var uow = unitOfWorkFactory();
                        var user = CreateUser(uow);
                        await uow.SaveAsync(user);
                        await uow.CommitChangesAsync();

                        var (store, uow1) = CreateStore();
                        using (store)
                        using (uow1)
                        {
                            var identityUser = await store.FindByIdAsync(user.Id, CancellationToken.None);
                            await store.SetTokenAsync(identityUser, loginProvider, loginProviderName, tokenValue, CancellationToken.None);
                            await store.UpdateAsync(identityUser, CancellationToken.None);
                        }
                        using var uow2 = unitOfWorkFactory();
                        var userInDb = await uow.GetObjectByKeyAsync<XpoIdentityUser>(user.Id);

                        userInDb.Tokens.Should().NotBeEmpty();
                        userInDb.Tokens.First().Value.Should().Be(tokenValue);
                    });

                    It("updates existing token", async () =>
                    {
                        var loginProvider = Guid.NewGuid().ToString();
                        var loginProviderName = Guid.NewGuid().ToString();
                        var tokenValue = Guid.NewGuid().ToString();
                        using var uow = unitOfWorkFactory();
                        var user = CreateUser(uow);
                        user.Tokens.Add(new XpoIdentityUserToken(uow)
                        {
                            Id = Guid.NewGuid().ToString(),
                            LoginProvider = loginProvider,
                            Name = loginProviderName,
                            Value = tokenValue,
                        });
                        await uow.SaveAsync(user);
                        await uow.CommitChangesAsync();

                        var newTokenValue = Guid.NewGuid().ToString();

                        var (store, uow1) = CreateStore();
                        using (store)
                        using (uow1)
                        {
                            var identityUser = await store.FindByIdAsync(user.Id, CancellationToken.None);
                            await store.SetTokenAsync(identityUser, loginProvider, loginProviderName, newTokenValue, CancellationToken.None);
                            await store.UpdateAsync(identityUser, CancellationToken.None);
                        }
                        using var uow2 = unitOfWorkFactory();
                        var userInDb = await uow.GetObjectByKeyAsync<XpoIdentityUser>(user.Id);

                        userInDb.Tokens.Should().NotBeEmpty();
                        userInDb.Tokens.Count.Should().Be(1);
                        userInDb.Tokens.First().Value.Should().Be(newTokenValue);
                    });
                });

                Describe("RemoveTokenAsync", () =>
                {
                    It("non existing user and token", async () =>
                    {
                        var loginProvider = Guid.NewGuid().ToString();
                        var loginProviderName = Guid.NewGuid().ToString();

                        var (store, uow) = CreateStore();
                        using (store)
                        using (uow)
                        {
                            var action = new Func<Task>(async () => await store.RemoveTokenAsync(new IdentityUser(), loginProvider, loginProviderName, CancellationToken.None));
                            await action.Should().NotThrowAsync();
                        }
                    });

                    It("removed existing token", async () =>
                    {
                        var loginProvider = Guid.NewGuid().ToString();
                        var loginProviderName = Guid.NewGuid().ToString();
                        var tokenValue = Guid.NewGuid().ToString();
                        using var uow = unitOfWorkFactory();
                        var user = CreateUser(uow);
                        user.Tokens.Add(new XpoIdentityUserToken(uow)
                        {
                            Id = Guid.NewGuid().ToString(),
                            LoginProvider = loginProvider,
                            Name = loginProviderName,
                            Value = tokenValue,
                        });
                        await uow.SaveAsync(user);
                        await uow.CommitChangesAsync();

                        var newTokenValue = Guid.NewGuid().ToString();
                        var (store, uow1) = CreateStore();
                        using (store)
                        using (uow1)
                        {
                            var identityUser = await store.FindByIdAsync(user.Id, CancellationToken.None);
                            await store.RemoveTokenAsync(identityUser, loginProvider, loginProviderName, CancellationToken.None);
                            await store.UpdateAsync(identityUser, CancellationToken.None);
                        }
                        using var uow2 = unitOfWorkFactory();
                        var userInDb = await uow.GetObjectByKeyAsync<XpoIdentityUser>(user.Id);

                        userInDb.Tokens.Should().BeEmpty();
                    });
                });
            });

            Describe("Logins", () =>
            {
                Describe("AddLoginAsync", () =>
                {
                    It("adds login", async () =>
                    {
                        using var uow = unitOfWorkFactory();
                        var user = CreateUser(uow);
                        await uow.SaveAsync(user);
                        await uow.CommitChangesAsync();

                        var loginInfo = new UserLoginInfo(
                            Guid.NewGuid().ToString(),
                            Guid.NewGuid().ToString(),
                            Guid.NewGuid().ToString()
                        );

                        var (store, uow1) = CreateStore();
                        using (store)
                        using (uow1)
                        {
                            var identityUser = await store.FindByIdAsync(user.Id, CancellationToken.None);
                            await store.AddLoginAsync(identityUser, loginInfo, CancellationToken.None);
                            await store.UpdateAsync(identityUser, CancellationToken.None);
                        }

                        using var uow2 = unitOfWorkFactory();
                        var userInDb = await uow2.GetObjectByKeyAsync<XpoIdentityUser>(user.Id);
                        userInDb.Logins.Should().NotBeEmpty();
                        userInDb.Logins.First().ProviderDisplayName.Should().Be(loginInfo.ProviderDisplayName);
                        userInDb.Logins.First().ProviderKey.Should().Be(loginInfo.ProviderKey);
                        userInDb.Logins.First().LoginProvider.Should().Be(loginInfo.LoginProvider);
                    });
                });

                Describe("FindByLoginAsync", () =>
                {
                    It("with non existent user and login", async () =>
                    {
                        var loginProvider = Guid.NewGuid().ToString();
                        var providerKey = Guid.NewGuid().ToString();
                        var (store, uow) = CreateStore();
                        using (store)
                        using (uow)
                        {
                            var user = await store.FindByLoginAsync(loginProvider, providerKey, CancellationToken.None);
                            return user == null;
                        }
                    });

                    It("with existent user and login", async () =>
                    {
                        var loginProvider = Guid.NewGuid().ToString();
                        var providerKey = Guid.NewGuid().ToString();
                        using var uow = unitOfWorkFactory();
                        var user = CreateUser(uow);
                        user.Logins.Add(new XpoIdentityUserLogin(uow)
                        {
                            Id = Guid.NewGuid().ToString(),
                            LoginProvider = loginProvider,
                            ProviderKey = providerKey
                        });
                        await uow.SaveAsync(user);
                        await uow.CommitChangesAsync();

                        var (store, uow1) = CreateStore();
                        using (store)
                        using (uow1)
                        {
                            var userFromStore = await store.FindByLoginAsync(loginProvider, providerKey, CancellationToken.None);
                            userFromStore.Should().NotBeNull();
                        }
                        using var uow2 = unitOfWorkFactory();
                        var userInDb = await uow.GetObjectByKeyAsync<XpoIdentityUser>(user.Id);

                        userInDb.Logins.Should().NotBeEmpty();
                    });
                });

                Describe("RemoveLoginAsync", () =>
                {
                    It("with existent user and login", async () =>
                    {
                        var loginProvider = Guid.NewGuid().ToString();
                        var providerKey = Guid.NewGuid().ToString();
                        using var uow = unitOfWorkFactory();
                        var user = CreateUser(uow);
                        user.Logins.Add(new XpoIdentityUserLogin(uow)
                        {
                            Id = Guid.NewGuid().ToString(),
                            LoginProvider = loginProvider,
                            ProviderKey = providerKey
                        });
                        await uow.SaveAsync(user);
                        await uow.CommitChangesAsync();

                        var (store, uow1) = CreateStore();
                        using (store)
                        using (uow1)
                        {
                            var userFromStore = await store.FindByIdAsync(user.Id, CancellationToken.None);
                            await store.RemoveLoginAsync(userFromStore, loginProvider, providerKey, CancellationToken.None);
                            await store.UpdateAsync(userFromStore, CancellationToken.None);
                        }

                        using var uow2 = unitOfWorkFactory();
                        var userInDb = await uow.GetObjectByKeyAsync<XpoIdentityUser>(user.Id);

                        userInDb.Logins.Should().BeEmpty();
                    });
                });

                Describe("GetLoginsAsync", () =>
                {
                    It("With non existent user and login", async () =>
                    {
                        using var uow = unitOfWorkFactory();
                        var user = CreateUser(uow);
                        await uow.SaveAsync(user);
                        await uow.CommitChangesAsync();

                        var (store, uow1) = CreateStore();
                        using (store)
                        using (uow1)
                        {
                            var userFromStore = await store.FindByIdAsync(user.Id, CancellationToken.None);
                            var logins = await store.GetLoginsAsync(userFromStore, CancellationToken.None);
                            logins.Should().BeEmpty();
                        }
                    });

                    It("With existent user and login", async () =>
                    {
                        using var uow = unitOfWorkFactory();
                        var user = CreateUser(uow);
                        user.Logins.Add(new XpoIdentityUserLogin(uow)
                        {
                            Id = Guid.NewGuid().ToString(),
                            LoginProvider = Guid.NewGuid().ToString(),
                            ProviderKey = Guid.NewGuid().ToString()
                        });
                        user.Logins.Add(new XpoIdentityUserLogin(uow)
                        {
                            Id = Guid.NewGuid().ToString(),
                            LoginProvider = Guid.NewGuid().ToString(),
                            ProviderKey = Guid.NewGuid().ToString()
                        });
                        await uow.SaveAsync(user);
                        await uow.CommitChangesAsync();

                        var (store, uow1) = CreateStore();
                        using (store)
                        using (uow1)
                        {
                            var userFromStore = await store.FindByIdAsync(user.Id, CancellationToken.None);
                            var logins = await store.GetLoginsAsync(userFromStore, CancellationToken.None);
                            logins.Should().NotBeEmpty();
                            logins.Count.Should().Be(2);
                        }
                    });
                });
            });

            Describe("Claims", () =>
            {
                Describe("AddClaimsAsync", () =>
                {
                    It("adds claims", async () =>
                    {
                        var claimType = Guid.NewGuid().ToString();
                        var claimValue = Guid.NewGuid().ToString();
                        using var uow = unitOfWorkFactory();
                        var user = CreateUser(uow);
                        await uow.SaveAsync(user);
                        await uow.CommitChangesAsync();

                        var (store, uow1) = CreateStore();
                        using (store)
                        using (uow1)
                        {
                            var identityUser = await store.FindByIdAsync(user.Id, CancellationToken.None);
                            await store.AddClaimsAsync(identityUser, new[]
                            {
                                new Claim(claimType, claimValue)
                            }, CancellationToken.None);
                            await store.UpdateAsync(identityUser, CancellationToken.None);
                        }
                        using var uow2 = unitOfWorkFactory();
                        var userInDb = await uow.GetObjectByKeyAsync<XpoIdentityUser>(user.Id);

                        userInDb.Claims.Should().NotBeEmpty();
                        userInDb.Claims.First().Type.Should().Be(claimType);
                        userInDb.Claims.First().Value.Should().Be(claimValue);
                    });
                });
            });
        });
    }
}
