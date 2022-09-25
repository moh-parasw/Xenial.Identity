﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;

using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores.Serialization;

using FluentAssertions;

using IdentityModel;

using Xenial.Identity.Xpo.Storage.Models;
using Xenial.Identity.Xpo.Storage.Stores;

using static Xenial.Tasty;

namespace Xenial.Identity.Xpo.Storage.Tests.IntegrationTests
{
    public static class DeviceFlowStoreTests
    {
        public static void Tests(string name, string connectionString) => Describe($"{nameof(DeviceFlowStore)} using {name}", () =>
        {
            var dataLayer = XpoDefault.GetDataLayer(connectionString, DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);
            var serializer = new PersistentGrantSerializer();

            using var unitOfWork = new UnitOfWork(dataLayer);
            unitOfWork.UpdateSchema();

            (DeviceFlowStore, UnitOfWork) CreateStore()
            {
                var uow = new UnitOfWork(dataLayer);
                return (new DeviceFlowStore(uow, new PersistentGrantSerializer(), new FakeLogger<DeviceFlowStore>()), uow);
            }

            _ = It("StoreDeviceAuthorizationAsync when successful should store DeviceCode and UserCode", async () =>
            {
                var deviceCode = Guid.NewGuid().ToString();
                var userCode = Guid.NewGuid().ToString();
                var data = new DeviceCode
                {
                    ClientId = Guid.NewGuid().ToString(),
                    CreationTime = DateTime.UtcNow,
                    Lifetime = 300
                };

                var (store, uow) = CreateStore();
                using (uow)
                {
                    await store.StoreDeviceAuthorizationAsync(deviceCode, userCode, data);
                }

                using var uow1 = new UnitOfWork(dataLayer);
                var foundDeviceFlowCodes = await uow1.Query<XpoDeviceFlowCodes>().FirstOrDefaultAsync(x => x.DeviceCode == deviceCode);

                _ = foundDeviceFlowCodes.Should().NotBeNull();
                _ = (foundDeviceFlowCodes?.DeviceCode.Should().Be(deviceCode));
                _ = (foundDeviceFlowCodes?.UserCode.Should().Be(userCode));
            });

            _ = It("StoreDeviceAuthorizationAsync when successful should store Data", async () =>
            {
                var deviceCode = Guid.NewGuid().ToString();
                var userCode = Guid.NewGuid().ToString();
                var data = new DeviceCode
                {
                    ClientId = Guid.NewGuid().ToString(),
                    CreationTime = DateTime.UtcNow,
                    Lifetime = 300
                };

                var (store, uow) = CreateStore();
                using (uow)
                {
                    await store.StoreDeviceAuthorizationAsync(deviceCode, userCode, data);
                }

                using var uow1 = new UnitOfWork(dataLayer);
                var foundDeviceFlowCodes = await uow1.Query<XpoDeviceFlowCodes>().FirstOrDefaultAsync(x => x.DeviceCode == deviceCode);

                _ = foundDeviceFlowCodes.Should().NotBeNull();
                var deserializedData = new PersistentGrantSerializer().Deserialize<DeviceCode>(foundDeviceFlowCodes?.Data);

                _ = deserializedData.CreationTime.Should().BeCloseTo(data.CreationTime, TimeSpan.FromMilliseconds(100));
                _ = deserializedData.ClientId.Should().Be(data.ClientId);
                _ = deserializedData.Lifetime.Should().Be(data.Lifetime);
            });

            _ = It("StoreDeviceAuthorizationAsync throws ConstraintViolationException when UserCode already", async () =>
            {
                var existingUserCode = $"user_{Guid.NewGuid()}";
                var deviceCodeData = new DeviceCode
                {
                    ClientId = "device_flow",
                    RequestedScopes = new[] { "openid", "api1" },
                    CreationTime = new DateTime(2018, 10, 19, 16, 14, 29),
                    Lifetime = 300,
                    IsOpenId = true,
                    Subject = new ClaimsPrincipal(new ClaimsIdentity(
                        new List<Claim> { new Claim(JwtClaimTypes.Subject, $"sub_{Guid.NewGuid()}") }))
                };

                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    _ = new XpoDeviceFlowCodes(uow1)
                    {
                        DeviceCode = $"device_{Guid.NewGuid()}",
                        UserCode = existingUserCode,
                        ClientId = deviceCodeData.ClientId,
                        SubjectId = deviceCodeData.Subject.FindFirst(JwtClaimTypes.Subject).Value,
                        CreationTime = deviceCodeData.CreationTime,
                        Expiration = deviceCodeData.CreationTime.AddSeconds(deviceCodeData.Lifetime),
                        Data = serializer.Serialize(deviceCodeData)
                    };

                    await uow1.CommitChangesAsync();
                }

                var (store, uow) = CreateStore();
                using (uow)
                {
                    var action = new Func<Task>(async () => await store.StoreDeviceAuthorizationAsync($"device_{Guid.NewGuid()}", existingUserCode, deviceCodeData));

                    _ = await action.Should().ThrowAsync<ConstraintViolationException>();
                }
            });

            _ = It("StoreDeviceAuthorizationAsync throws ConstraintViolationException when DeviceCode already exists", async () =>
            {
                var existingDeviceCode = $"device_{Guid.NewGuid()}";
                var deviceCodeData = new DeviceCode
                {
                    ClientId = "device_flow",
                    RequestedScopes = new[] { "openid", "api1" },
                    CreationTime = new DateTime(2018, 10, 19, 16, 14, 29),
                    Lifetime = 300,
                    IsOpenId = true,
                    Subject = new ClaimsPrincipal(new ClaimsIdentity(
                        new List<Claim> { new Claim(JwtClaimTypes.Subject, $"sub_{Guid.NewGuid()}") }))
                };

                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    _ = new XpoDeviceFlowCodes(uow1)
                    {
                        DeviceCode = existingDeviceCode,
                        UserCode = $"user_{Guid.NewGuid()}",
                        ClientId = deviceCodeData.ClientId,
                        SubjectId = deviceCodeData.Subject.FindFirst(JwtClaimTypes.Subject).Value,
                        CreationTime = deviceCodeData.CreationTime,
                        Expiration = deviceCodeData.CreationTime.AddSeconds(deviceCodeData.Lifetime),
                        Data = serializer.Serialize(deviceCodeData)
                    };
                    await uow1.CommitChangesAsync();
                }

                var (store, uow) = CreateStore();
                using (uow)
                {
                    var action = new Func<Task>(async () => await store.StoreDeviceAuthorizationAsync(existingDeviceCode, $"user_{Guid.NewGuid()}", deviceCodeData));

                    _ = await action.Should().ThrowAsync<ConstraintViolationException>();
                }
            });

            _ = It("FindByUserCodeAsync when UserCode exists retrieves Data correctly", async () =>
            {
                var testDeviceCode = $"device_{Guid.NewGuid()}";
                var testUserCode = $"user_{Guid.NewGuid()}";

                var expectedSubject = $"sub_{Guid.NewGuid()}";
                var expectedDeviceCodeData = new DeviceCode
                {
                    ClientId = "device_flow",
                    RequestedScopes = new[] { "openid", "api1" },
                    CreationTime = new DateTime(2018, 10, 19, 16, 14, 29),
                    Lifetime = 300,
                    IsOpenId = true,
                    Subject = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { new Claim(JwtClaimTypes.Subject, expectedSubject) }))
                };

                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    _ = new XpoDeviceFlowCodes(uow1)
                    {
                        DeviceCode = testDeviceCode,
                        UserCode = testUserCode,
                        ClientId = expectedDeviceCodeData.ClientId,
                        SubjectId = expectedDeviceCodeData.Subject.FindFirst(JwtClaimTypes.Subject).Value,
                        CreationTime = expectedDeviceCodeData.CreationTime,
                        Expiration = expectedDeviceCodeData.CreationTime.AddSeconds(expectedDeviceCodeData.Lifetime),
                        Data = serializer.Serialize(expectedDeviceCodeData)
                    };
                    await uow1.CommitChangesAsync();
                }

                var (store, uow) = CreateStore();
                using (uow)
                {
                    var code = await store.FindByUserCodeAsync(testUserCode);

                    _ = code.Should().BeEquivalentTo(expectedDeviceCodeData,
                        assertionOptions => assertionOptions.Excluding(x => x.Subject));

                    _ = code.Subject.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Subject && x.Value == expectedSubject).Should().NotBeNull();
                }
            });

            _ = It("FindByUserCodeAsync when UserCode does not exist returns null", async () =>
            {
                var (store, uow) = CreateStore();
                using (uow)
                {
                    var code = await store.FindByUserCodeAsync($"user_{Guid.NewGuid()}");
                    _ = code.Should().BeNull();
                }
            });

            _ = It("FindByDeviceCodeAsync when DeviceCode exists retrieves Data correctly", async () =>
            {
                var testDeviceCode = $"device_{Guid.NewGuid()}";
                var testUserCode = $"user_{Guid.NewGuid()}";

                var expectedSubject = $"sub_{Guid.NewGuid()}";
                var expectedDeviceCodeData = new DeviceCode
                {
                    ClientId = "device_flow",
                    RequestedScopes = new[] { "openid", "api1" },
                    CreationTime = new DateTime(2018, 10, 19, 16, 14, 29),
                    Lifetime = 300,
                    IsOpenId = true,
                    Subject = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { new Claim(JwtClaimTypes.Subject, expectedSubject) }))
                };

                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    _ = new XpoDeviceFlowCodes(uow1)
                    {
                        DeviceCode = testDeviceCode,
                        UserCode = testUserCode,
                        ClientId = expectedDeviceCodeData.ClientId,
                        SubjectId = expectedDeviceCodeData.Subject.FindFirst(JwtClaimTypes.Subject).Value,
                        CreationTime = expectedDeviceCodeData.CreationTime,
                        Expiration = expectedDeviceCodeData.CreationTime.AddSeconds(expectedDeviceCodeData.Lifetime),
                        Data = serializer.Serialize(expectedDeviceCodeData)
                    };
                    await uow1.CommitChangesAsync();
                }

                var (store, uow) = CreateStore();
                using (uow)
                {
                    var code = await store.FindByDeviceCodeAsync(testDeviceCode);
                    _ = code.Should().BeEquivalentTo(expectedDeviceCodeData,
                        assertionOptions => assertionOptions.Excluding(x => x.Subject));

                    _ = code.Subject.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Subject && x.Value == expectedSubject).Should().NotBeNull();
                }
            });

            _ = It("FindByDeviceCodeAsync when DeviceCode does not exist returns null", async () =>
            {
                var (store, uow) = CreateStore();
                using (uow)
                {
                    var code = await store.FindByDeviceCodeAsync($"device_{Guid.NewGuid()}");
                    _ = code.Should().BeNull();
                }
            });

            _ = It("UpdateByUserCodeAsync when DeviceCode is authorized should update Subject and Data", async () =>
            {
                var testDeviceCode = $"device_{Guid.NewGuid()}";
                var testUserCode = $"user_{Guid.NewGuid()}";

                var expectedSubject = $"sub_{Guid.NewGuid()}";
                var unauthorizedDeviceCode = new DeviceCode
                {
                    ClientId = "device_flow",
                    RequestedScopes = new[] { "openid", "api1" },
                    CreationTime = new DateTime(2018, 10, 19, 16, 14, 29),
                    Lifetime = 300,
                    IsOpenId = true
                };

                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    _ = new XpoDeviceFlowCodes(uow1)
                    {
                        DeviceCode = testDeviceCode,
                        UserCode = testUserCode,
                        ClientId = unauthorizedDeviceCode.ClientId,
                        CreationTime = unauthorizedDeviceCode.CreationTime,
                        Expiration = unauthorizedDeviceCode.CreationTime.AddSeconds(unauthorizedDeviceCode.Lifetime),
                        Data = serializer.Serialize(unauthorizedDeviceCode)
                    };
                    await uow1.CommitChangesAsync();
                }

                var authorizedDeviceCode = new DeviceCode
                {
                    ClientId = unauthorizedDeviceCode.ClientId,
                    RequestedScopes = unauthorizedDeviceCode.RequestedScopes,
                    AuthorizedScopes = unauthorizedDeviceCode.RequestedScopes,
                    Subject = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { new Claim(JwtClaimTypes.Subject, expectedSubject) })),
                    IsAuthorized = true,
                    IsOpenId = true,
                    CreationTime = new DateTime(2018, 10, 19, 16, 14, 29),
                    Lifetime = 300
                };

                var (store, uow) = CreateStore();
                using (uow)
                {
                    await store.UpdateByUserCodeAsync(testUserCode, authorizedDeviceCode);
                }

                using var uow2 = new UnitOfWork(dataLayer);
                var updatedCodes = await uow2.Query<XpoDeviceFlowCodes>().SingleAsync(x => x.UserCode == testUserCode);

                // should be unchanged
                _ = updatedCodes.DeviceCode.Should().Be(testDeviceCode);
                _ = updatedCodes.ClientId.Should().Be(unauthorizedDeviceCode.ClientId);
                _ = updatedCodes.CreationTime.Should().Be(unauthorizedDeviceCode.CreationTime);
                _ = updatedCodes.Expiration.Should().Be(unauthorizedDeviceCode.CreationTime.AddSeconds(authorizedDeviceCode.Lifetime));

                // should be changed
                var parsedCode = serializer.Deserialize<DeviceCode>(updatedCodes.Data);
                _ = parsedCode.Should().BeEquivalentTo(authorizedDeviceCode, assertionOptions => assertionOptions.Excluding(x => x.Subject));
                _ = parsedCode.Subject.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Subject && x.Value == expectedSubject).Should().NotBeNull();
            });

            _ = It("RemoveByDeviceCodeAsync when DeviceCodeExists should delete DeviceCode", async () =>
            {
                var testDeviceCode = $"device_{Guid.NewGuid()}";
                var testUserCode = $"user_{Guid.NewGuid()}";

                var existingDeviceCode = new DeviceCode
                {
                    ClientId = "device_flow",
                    RequestedScopes = new[] { "openid", "api1" },
                    CreationTime = new DateTime(2018, 10, 19, 16, 14, 29),
                    Lifetime = 300,
                    IsOpenId = true
                };

                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    _ = new XpoDeviceFlowCodes(uow1)
                    {
                        DeviceCode = testDeviceCode,
                        UserCode = testUserCode,
                        ClientId = existingDeviceCode.ClientId,
                        CreationTime = existingDeviceCode.CreationTime,
                        Expiration = existingDeviceCode.CreationTime.AddSeconds(existingDeviceCode.Lifetime),
                        Data = serializer.Serialize(existingDeviceCode)
                    };
                    await uow1.CommitChangesAsync();
                }

                var (store, uow) = CreateStore();
                using (uow)
                {
                    await store.RemoveByDeviceCodeAsync(testDeviceCode);
                }

                using var uow2 = new UnitOfWork(dataLayer);
                var code = await uow2.Query<XpoDeviceFlowCodes>().FirstOrDefaultAsync(x => x.UserCode == testUserCode);
                _ = code.Should().BeNull();
            });

            _ = It("RemoveByDeviceCodeAsync when DeviceCode does not exists should succeed", async () =>
            {
                var (store, uow) = CreateStore();
                using (uow)
                {
                    await store.RemoveByDeviceCodeAsync($"device_{Guid.NewGuid()}");
                }
            });
        });
    }
}
