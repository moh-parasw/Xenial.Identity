using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using DevExpress.Xpo;
using DevExpress.Xpo.DB.Exceptions;

using FluentAssertions;

using IdentityModel;

using IdentityServer4.Models;
using IdentityServer4.Stores.Serialization;

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

            It("StoreDeviceAuthorizationAsync when successful should store DeviceCode and UserCode", async () =>
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

                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    var foundDeviceFlowCodes = await uow1.Query<XpoDeviceFlowCodes>().FirstOrDefaultAsync(x => x.DeviceCode == deviceCode);

                    foundDeviceFlowCodes.Should().NotBeNull();
                    foundDeviceFlowCodes?.DeviceCode.Should().Be(deviceCode);
                    foundDeviceFlowCodes?.UserCode.Should().Be(userCode);
                }
            });

            It("StoreDeviceAuthorizationAsync when successful should store Data", async () =>
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

                using (var uow1 = new UnitOfWork(dataLayer))
                {
                    var foundDeviceFlowCodes = await uow1.Query<XpoDeviceFlowCodes>().FirstOrDefaultAsync(x => x.DeviceCode == deviceCode);

                    foundDeviceFlowCodes.Should().NotBeNull();
                    var deserializedData = new PersistentGrantSerializer().Deserialize<DeviceCode>(foundDeviceFlowCodes?.Data);

                    deserializedData.CreationTime.Should().BeCloseTo(data.CreationTime);
                    deserializedData.ClientId.Should().Be(data.ClientId);
                    deserializedData.Lifetime.Should().Be(data.Lifetime);
                }
            });

            It("StoreDeviceAuthorizationAsync_WhenUserCodeAlreadyExists_ExpectException", async () =>
            {
                var existingUserCode = $"user_{Guid.NewGuid().ToString()}";
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
                    new XpoDeviceFlowCodes(uow1)
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

                    await action.Should().ThrowAsync<ConstraintViolationException>();
                }
            });
        });
    }
}
