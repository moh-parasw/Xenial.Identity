using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Serilog.Extensions.Logging;

namespace Xenial.Identity.Infrastructure
{
    public static class IdentityServerExtentions
    {
        public static void AddCertificate(this IIdentityServerBuilder builder, IHostEnvironment environment, IConfiguration configuration, ILogger logger = null)
        {
            if (logger == null)
            {
                var factory = new SerilogLoggerFactory(Serilog.Log.Logger);
                logger = factory.CreateLogger(typeof(IdentityServerExtentions));
            }

            if (environment.IsDevelopment())
            {
                logger.LogInformation("IDS: AddDeveloperSigningCredential");
                builder.AddDeveloperSigningCredential(true);
            }
            else
            {
                // http://amilspage.com/signing-certificates-idsv4/
                AddCertificateFromStore(builder, configuration, logger);
            }
        }

        private static void AddCertificateFromStore(IIdentityServerBuilder builder, IConfiguration options, ILogger logger)
        {
            var certificate = FindCert(options, logger);

            if (certificate != null)
            {
                builder.AddSigningCredential(certificate);
            }
            else
            {
                logger.LogError("IDS: A certificate key not be found");
            }
        }

        private static X509Certificate2 FindCert(IConfiguration options, ILogger logger)
        {
            var keyIssuer = options.GetValue<string>("Identity:Cert:Issuer");
            logger.LogDebug($"IDS: SigninCredentialExtension adding key from store by {keyIssuer}");

            var store = new X509Store(StoreName.CertificateAuthority, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);

            var certificates = store.Certificates.Find(X509FindType.FindByIssuerName, keyIssuer, true);

            if (certificates.Count > 0)
            {
                logger.LogInformation($"IDS: Found certificate in store {certificates[0].Thumbprint}");
                return certificates[0];
            }

            logger.LogError("IDS: A matching key couldn't be found in the store {store}", StoreLocation.LocalMachine);

            store = new X509Store(StoreName.CertificateAuthority, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);

            certificates = store.Certificates.Find(X509FindType.FindByIssuerName, keyIssuer, true);

            if (certificates.Count > 0)
            {
                logger.LogInformation($"IDS: Found certificate in store {certificates[0].Thumbprint}");
                return certificates[0];
            }

            logger.LogError("IDS: A matching key couldn't be found in the store {store}", StoreLocation.CurrentUser);

            logger.LogInformation("IDS: Fallback to load certificate from local file");

            var certPath = options.GetValue<string>("Identity:Cert:Path");
            var certSecret = options.GetValue<string>("Identity:Cert:Secret");
            try
            {
                logger.LogInformation("IDS: File {certPath} exists? {exists}", certPath, File.Exists(certPath));
                var certificate = new X509Certificate2(certPath, certSecret, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.MachineKeySet);
                logger.LogInformation($"IDS: Found from file {certificate.Thumbprint}");
                return certificate;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"IDS: Certificate file error {certPath}");
                return null;
            }
        }
    }
}
