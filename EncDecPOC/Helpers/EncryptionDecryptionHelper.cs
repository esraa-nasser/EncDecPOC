using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace EncDecPOC.Helpers
{
    public static class EncryptionDecryptionHelper
    {
        private static IConfiguration _configuration;
        static EncryptionDecryptionHelper()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            _configuration = builder.Build();
        }
        public static byte[] EncryptString(string input)
        {
            var certificate = GetCertificateByThumbprint();
            using (RSA rsa = certificate.GetRSAPublicKey())
            {
                return rsa.Encrypt(Encoding.UTF8.GetBytes(input), RSAEncryptionPadding.OaepSHA256);
            }
        }

        public static string DecryptString(string encryptedString)
        {
            var encryptedBytes = Convert.FromBase64String(encryptedString);
            var certificate = GetCertificateByThumbprint();
            RSA rsa = certificate.GetRSAPrivateKey();
            byte[] decryptedData = rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.OaepSHA256);

            return Encoding.UTF8.GetString(decryptedData);        
        }
        private static X509Certificate2 GetCertificateByThumbprint()
        {
            string thumbPrint = _configuration.GetSection("ThumbPrint").Value;
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbPrint, false);
            store.Close();

            return certificates.Count > 0 ? certificates[0] : null;
        }
    }
}
