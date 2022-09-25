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
        public static void Tests(string dbName, string connectionString) => Describe($"{nameof(XPUserStore<IdentityUser, string, IdentityUserClaim<string>, IdentityUserLogin<string>, IdentityUserToken<string>, XpoIdentityUser, XpoIdentityRole, XpoIdentityUserClaim, XpoIdentityUserLogin, XpoIdentityUserToken>)} using {dbName}", () =>
        {
            var dataLayer = XpoDefault.GetDataLayer(connectionString, DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);

            UnitOfWork unitOfWorkFactory()
            {
                return new UnitOfWork(dataLayer);
            }

            (XPUserStore<IdentityUser, string, IdentityUserClaim<string>, IdentityUserLogin<string>, IdentityUserToken<string>, XpoIdentityUser, XpoIdentityRole, XpoIdentityUserClaim, XpoIdentityUserLogin, XpoIdentityUserToken> store, UnitOfWork uow) CreateStore()
            {
                var uow = new UnitOfWork(dataLayer);
                var store = new XPUserStore<
                    IdentityUser,
                    string,
                    IdentityUserClaim<string>,
                    IdentityUserLogin<string>,
                    IdentityUserToken<string>,
                    XpoIdentityUser,
                    XpoIdentityRole,
                    XpoIdentityUserClaim,
                    XpoIdentityUserLogin,
                    XpoIdentityUserToken
                >(
                    uow,
                    new FakeLogger<XPUserStore<IdentityUser, string, IdentityUserClaim<string>, IdentityUserLogin<string>, IdentityUserToken<string>, XpoIdentityUser, XpoIdentityRole, XpoIdentityUserClaim, XpoIdentityUserLogin, XpoIdentityUserToken>>(),
                    new IdentityErrorDescriber()
                );
                return (store, uow);
            }
            XpoIdentityUser CreateUser(UnitOfWork uow, string id = null)
            {
                return new XpoIdentityUser(uow)
                {
                    Id = id ?? Guid.NewGuid().ToString(),
                    UserName = Guid.NewGuid().ToString(),
                    Email = Guid.NewGuid().ToString(),
                };
            }

            _ = It($"Can CreateAsync", async () =>
            {
                var (store, uow) = CreateStore();
                using (store)
                using (uow)
                {
                    var result = await store.CreateAsync(new IdentityUser { Id = Guid.NewGuid().ToString() }, CancellationToken.None);
                    return result.Succeeded;
                }
            });

            _ = It($"Can DeleteAsync", async () =>
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

            _ = Describe($"Can FindByIdAsync", () =>
            {
                _ = It($"with existing", async () =>
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

                _ = It($"with not existing", async () =>
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

            _ = Describe($"Can FindByNameAsync", () =>
            {
                _ = It($"with existing", async () =>
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

                _ = It($"with not existing", async () =>
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

            _ = Describe($"Can UpdateAsync", () =>
            {
                _ = It($"with existing", async () =>
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

                _ = It($"with not existing", async () =>
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

            _ = Describe($"Can FindByEmailAsync", () =>
            {
                _ = It($"with existing", async () =>
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

                _ = It($"with not existing", async () =>
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
            _ = Describe("Tokens", () =>
            {
                _ = Describe("GetTokenAsync", () =>
                {
                    _ = It("non existing token", async () =>
                    {
                        var (store, uow) = CreateStore();
                        using (store)
                        using (uow)
                        {
                            var token = await store.GetTokenAsync(new IdentityUser(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), CancellationToken.None);

                            return token == null;
                        }
                    });

                    _ = It("existing token", async () =>
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

                _ = Describe("SetTokenAsync", () =>
                {
                    _ = It("non existing user and token", async () =>
                    {
                        var loginProvider = Guid.NewGuid().ToString();
                        var loginProviderName = Guid.NewGuid().ToString();
                        var tokenValue = Guid.NewGuid().ToString();

                        var (store, uow) = CreateStore();
                        using (store)
                        using (uow)
                        {
                            var action = new Func<Task>(async () => await store.SetTokenAsync(new IdentityUser(), loginProvider, loginProviderName, tokenValue, CancellationToken.None));
                            _ = await action.Should().ThrowAsync<ArgumentNullException>();
                        }
                    });

                    _ = It("adds non existing token", async () =>
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
                            _ = await store.UpdateAsync(identityUser, CancellationToken.None);
                        }
                        using var uow2 = unitOfWorkFactory();
                        var userInDb = await uow2.GetObjectByKeyAsync<XpoIdentityUser>(user.Id);

                        _ = userInDb.Tokens.Should().NotBeEmpty();
                        _ = userInDb.Tokens.First().Value.Should().Be(tokenValue);
                    });

                    _ = It("updates existing token", async () =>
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
                            _ = await store.UpdateAsync(identityUser, CancellationToken.None);
                        }
                        using var uow2 = unitOfWorkFactory();
                        var userInDb = await uow2.GetObjectByKeyAsync<XpoIdentityUser>(user.Id);

                        _ = userInDb.Tokens.Should().NotBeEmpty();
                        _ = userInDb.Tokens.Count.Should().Be(1);
                        _ = userInDb.Tokens.First().Value.Should().Be(newTokenValue);
                    });
                });

                _ = Describe("RemoveTokenAsync", () =>
                {
                    _ = It("non existing user and token", async () =>
                    {
                        var loginProvider = Guid.NewGuid().ToString();
                        var loginProviderName = Guid.NewGuid().ToString();

                        var (store, uow) = CreateStore();
                        using (store)
                        using (uow)
                        {
                            var action = new Func<Task>(async () => await store.RemoveTokenAsync(new IdentityUser(), loginProvider, loginProviderName, CancellationToken.None));
                            _ = await action.Should().NotThrowAsync();
                        }
                    });

                    _ = It("removed existing token", async () =>
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
                            _ = await store.UpdateAsync(identityUser, CancellationToken.None);
                        }
                        using var uow2 = unitOfWorkFactory();
                        var userInDb = await uow2.GetObjectByKeyAsync<XpoIdentityUser>(user.Id);

                        _ = userInDb.Tokens.Should().BeEmpty();
                    });
                });
            });

            _ = Describe("Logins", () =>
            {
                _ = Describe("AddLoginAsync", () =>
                {
                    _ = It("adds login", async () =>
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
                            _ = await store.UpdateAsync(identityUser, CancellationToken.None);
                        }

                        using var uow2 = unitOfWorkFactory();
                        var userInDb = await uow2.GetObjectByKeyAsync<XpoIdentityUser>(user.Id);
                        _ = userInDb.Logins.Should().NotBeEmpty();
                        _ = userInDb.Logins.First().ProviderDisplayName.Should().Be(loginInfo.ProviderDisplayName);
                        _ = userInDb.Logins.First().ProviderKey.Should().Be(loginInfo.ProviderKey);
                        _ = userInDb.Logins.First().LoginProvider.Should().Be(loginInfo.LoginProvider);
                    });
                });

                _ = Describe("FindByLoginAsync", () =>
                {
                    _ = It("with non existent user and login", async () =>
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

                    _ = It("with existent user and login", async () =>
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
                            _ = userFromStore.Should().NotBeNull();
                        }
                        using var uow2 = unitOfWorkFactory();
                        var userInDb = await uow2.GetObjectByKeyAsync<XpoIdentityUser>(user.Id);

                        _ = userInDb.Logins.Should().NotBeEmpty();
                    });
                });

                _ = Describe("RemoveLoginAsync", () =>
                {
                    _ = It("with existent user and login", async () =>
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
                            _ = await store.UpdateAsync(userFromStore, CancellationToken.None);
                        }

                        using var uow2 = unitOfWorkFactory();
                        var userInDb = await uow2.GetObjectByKeyAsync<XpoIdentityUser>(user.Id);

                        _ = userInDb.Logins.Should().BeEmpty();
                    });
                });

                _ = Describe("GetLoginsAsync", () =>
                {
                    _ = It("With non existent user and login", async () =>
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
                            _ = logins.Should().BeEmpty();
                        }
                    });

                    _ = It("With existent user and login", async () =>
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
                            _ = logins.Should().NotBeEmpty();
                            _ = logins.Count.Should().Be(2);
                        }
                    });
                });
            });

            _ = Describe("Claims", () =>
            {
                _ = Describe("AddClaimsAsync", () =>
                {
                    _ = It("adds claims", async () =>
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
                            _ = await store.UpdateAsync(identityUser, CancellationToken.None);
                        }
                        using var uow2 = unitOfWorkFactory();
                        var userInDb = await uow2.GetObjectByKeyAsync<XpoIdentityUser>(user.Id);

                        _ = userInDb.Claims.Should().NotBeEmpty();
                        _ = userInDb.Claims.First().Type.Should().Be(claimType);
                        _ = userInDb.Claims.First().Value.Should().Be(claimValue);
                    });
                });

                _ = Describe("ReplaceClaimAsync", () =>
                {
                    _ = It("replaces claims", async () =>
                    {
                        var claimType = Guid.NewGuid().ToString();
                        var claimValue = Guid.NewGuid().ToString();
                        using var uow = unitOfWorkFactory();
                        var user = CreateUser(uow);
                        user.Claims.Add(new XpoIdentityUserClaim(uow)
                        {
                            Id = Guid.NewGuid().ToString(),
                            Value = claimValue,
                            Type = claimType
                        });

                        await uow.SaveAsync(user);
                        await uow.CommitChangesAsync();

                        var (store, uow1) = CreateStore();
                        using (store)
                        using (uow1)
                        {
                            var identityUser = await store.FindByIdAsync(user.Id, CancellationToken.None);
                            var newClaimType = Guid.NewGuid().ToString();
                            var newClaimValue = Guid.NewGuid().ToString();
                            await store.ReplaceClaimAsync(
                                identityUser,
                                new Claim(claimType, claimValue),
                                new Claim(newClaimType, newClaimValue),
                                CancellationToken.None
                            );
                            _ = await store.UpdateAsync(identityUser, CancellationToken.None);
                            using var uow2 = unitOfWorkFactory();
                            var userInDb = await uow2.GetObjectByKeyAsync<XpoIdentityUser>(user.Id);

                            _ = userInDb.Claims.Should().NotBeEmpty();
                            _ = userInDb.Claims.First().Type.Should().Be(newClaimType);
                            _ = userInDb.Claims.First().Value.Should().Be(newClaimValue);
                        }
                    });
                });

                _ = Describe("RemoveClaimsAsync", () =>
                {
                    _ = It("removes claims", async () =>
                    {
                        var claimType = Guid.NewGuid().ToString();
                        var claimValue = Guid.NewGuid().ToString();
                        using var uow = unitOfWorkFactory();
                        var user = CreateUser(uow);
                        user.Claims.Add(new XpoIdentityUserClaim(uow)
                        {
                            Id = Guid.NewGuid().ToString(),
                            Value = claimValue,
                            Type = claimType
                        });

                        await uow.SaveAsync(user);
                        await uow.CommitChangesAsync();

                        var (store, uow1) = CreateStore();
                        using (store)
                        using (uow1)
                        {
                            var identityUser = await store.FindByIdAsync(user.Id, CancellationToken.None);
                            var newClaimType = Guid.NewGuid().ToString();
                            var newClaimValue = Guid.NewGuid().ToString();
                            await store.RemoveClaimsAsync(
                                identityUser,
                                new[] { new Claim(claimType, claimValue) },
                                CancellationToken.None
                            );
                            _ = await store.UpdateAsync(identityUser, CancellationToken.None);
                            using var uow2 = unitOfWorkFactory();
                            var userInDb = await uow2.GetObjectByKeyAsync<XpoIdentityUser>(user.Id);

                            _ = userInDb.Claims.Should().BeEmpty();
                        }
                    });
                });

                _ = Describe("GetClaimsAsync", () =>
                {
                    _ = It("lists claims", async () =>
                    {
                        var claimType = Guid.NewGuid().ToString();
                        var claimValue = Guid.NewGuid().ToString();
                        using var uow = unitOfWorkFactory();
                        var user = CreateUser(uow);
                        user.Claims.Add(new XpoIdentityUserClaim(uow)
                        {
                            Id = Guid.NewGuid().ToString(),
                            Value = claimValue,
                            Type = claimType
                        });

                        await uow.SaveAsync(user);
                        await uow.CommitChangesAsync();

                        var (store, uow1) = CreateStore();
                        using (store)
                        using (uow1)
                        {
                            var identityUser = await store.FindByIdAsync(user.Id, CancellationToken.None);

                            var claims = await store.GetClaimsAsync(
                                identityUser,
                                CancellationToken.None
                            );
                            _ = await store.UpdateAsync(identityUser, CancellationToken.None);

                            _ = claims.Should().NotBeEmpty();
                            _ = claims.First().Type.Should().Be(claimType);
                            _ = claims.First().Value.Should().Be(claimValue);
                        }
                    });
                });

                _ = Describe("GetUsersForClaimAsync", () =>
                {
                    _ = It("lists users", async () =>
                    {
                        var claimType = Guid.NewGuid().ToString();
                        var claimValue = Guid.NewGuid().ToString();
                        using var uow = unitOfWorkFactory();
                        var user = CreateUser(uow);
                        user.Claims.Add(new XpoIdentityUserClaim(uow)
                        {
                            Id = Guid.NewGuid().ToString(),
                            Value = claimValue,
                            Type = claimType
                        });

                        await uow.SaveAsync(user);
                        await uow.CommitChangesAsync();

                        var (store, uow1) = CreateStore();
                        using (store)
                        using (uow1)
                        {
                            var users = await store.GetUsersForClaimAsync(
                                new Claim(claimType, claimValue),
                                CancellationToken.None
                            );

                            _ = users.Should().NotBeEmpty();
                            _ = users.First().UserName.Should().Be(user.UserName);
                        }
                    });
                });

                _ = Describe("Roles", () =>
                {
                    _ = It("Add to role throws if not existing", async () =>
                    {
                        using var uow = unitOfWorkFactory();
                        var user = CreateUser(uow);
                        await uow.SaveAsync(user);
                        await uow.CommitChangesAsync();

                        var (store, uow1) = CreateStore();
                        using (store)
                        using (uow1)
                        {
                            var identityUser = await store.FindByIdAsync(user.Id, CancellationToken.None);

                            Func<Task> action = async () => await store.AddToRoleAsync(identityUser, "foo", CancellationToken.None);
                            _ = await action.Should().ThrowAsync<InvalidOperationException>();
                        }
                    });

                    _ = It("Add to role", async () =>
                    {
                        using var uow = unitOfWorkFactory();
                        var user = CreateUser(uow);
                        var role = new XpoIdentityRole(uow)
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = Guid.NewGuid().ToString(),
                            NormalizedName = Guid.NewGuid().ToString(),
                        };
                        await uow.SaveAsync(user);
                        await uow.CommitChangesAsync();

                        var (store, uow1) = CreateStore();
                        using (store)
                        using (uow1)
                        {
                            var identityUser = await store.FindByIdAsync(user.Id, CancellationToken.None);

                            await store.AddToRoleAsync(identityUser, role.NormalizedName, CancellationToken.None);
                            _ = await store.UpdateAsync(identityUser, CancellationToken.None);
                        }

                        using var uow2 = unitOfWorkFactory();
                        var userInDb = await uow2.GetObjectByKeyAsync<XpoIdentityUser>(user.Id);

                        _ = userInDb.Roles.Should().NotBeEmpty();
                        _ = userInDb.Roles.First().Name.Should().Be(role.Name);
                    });

                    _ = It("Remove from role", async () =>
                    {
                        using var uow = unitOfWorkFactory();
                        var user = CreateUser(uow);
                        var role = new XpoIdentityRole(uow)
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = Guid.NewGuid().ToString(),
                            NormalizedName = Guid.NewGuid().ToString(),
                        };
                        user.Roles.Add(role);
                        await uow.SaveAsync(user);
                        await uow.CommitChangesAsync();

                        var (store, uow1) = CreateStore();
                        using (store)
                        using (uow1)
                        {
                            var identityUser = await store.FindByIdAsync(user.Id, CancellationToken.None);

                            await store.RemoveFromRoleAsync(identityUser, role.NormalizedName, CancellationToken.None);
                            _ = await store.UpdateAsync(identityUser, CancellationToken.None);
                        }

                        using var uow2 = unitOfWorkFactory();
                        var userInDb = await uow2.GetObjectByKeyAsync<XpoIdentityUser>(user.Id);

                        _ = userInDb.Roles.Should().BeEmpty();
                    });

                    _ = It("GetRoles", async () =>
                    {
                        using var uow = unitOfWorkFactory();
                        var user = CreateUser(uow);
                        var role = new XpoIdentityRole(uow)
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = Guid.NewGuid().ToString(),
                            NormalizedName = Guid.NewGuid().ToString(),
                        };
                        user.Roles.Add(role);
                        await uow.SaveAsync(user);
                        await uow.CommitChangesAsync();

                        var (store, uow1) = CreateStore();
                        using (store)
                        using (uow1)
                        {
                            var identityUser = await store.FindByIdAsync(user.Id, CancellationToken.None);

                            var roles = await store.GetRolesAsync(identityUser, CancellationToken.None);

                            _ = roles.Should().NotBeEmpty();
                            _ = roles.Should().Contain(role.Name);
                        }
                    });

                    _ = It("GetUsersInRoleAsync", async () =>
                    {
                        using var uow = unitOfWorkFactory();
                        var user = CreateUser(uow);
                        var role = new XpoIdentityRole(uow)
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = Guid.NewGuid().ToString(),
                            NormalizedName = Guid.NewGuid().ToString(),
                        };
                        user.Roles.Add(role);
                        await uow.SaveAsync(user);
                        await uow.CommitChangesAsync();

                        var (store, uow1) = CreateStore();
                        using (store)
                        using (uow1)
                        {
                            var users = await store.GetUsersInRoleAsync(role.NormalizedName, CancellationToken.None);

                            _ = users.Should().NotBeEmpty();
                            _ = users.First().UserName.Should().Be(user.UserName);
                        }
                    });

                    _ = It("IsInRoleAsync", async () =>
                    {
                        using var uow = unitOfWorkFactory();
                        var user = CreateUser(uow);
                        var role = new XpoIdentityRole(uow)
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = Guid.NewGuid().ToString(),
                            NormalizedName = Guid.NewGuid().ToString(),
                        };
                        user.Roles.Add(role);
                        await uow.SaveAsync(user);
                        await uow.CommitChangesAsync();

                        var (store, uow1) = CreateStore();
                        using (store)
                        using (uow1)
                        {
                            var identityUser = await store.FindByIdAsync(user.Id, CancellationToken.None);
                            return await store.IsInRoleAsync(identityUser, role.NormalizedName, CancellationToken.None);
                        }
                    });
                });
            });
        });
    }
}
