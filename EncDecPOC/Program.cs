// See https://aka.ms/new-console-template for more information
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using EncDecPOC.Helpers;

namespace EncDecPOC
{
    public class EncDecPOC
    {
        public static void Main(string[] args)
        {
            var jsonFilePath = Path.Combine(PathHelper.FindProjectPath("EncDecPOC"), "appsettings.json");
            IConfiguration _configuration = new ConfigurationBuilder()
                            .AddJsonFile(jsonFilePath, optional: false, reloadOnChange: true)
                            .Build();
            // Replace "YourCertificateThumbprint" with the actual thumbprint of your certificate
            string thumbPrint = _configuration.GetSection("ThumbPrint").Value;

            // Retrieve the certificate from the Windows certificate store
            X509Certificate2 certificate = GetCertificateByThumbprint(thumbPrint);

            if (certificate == null)
            {
                Console.WriteLine("Certificate not found.");
                return;
            }
           

            if (_configuration["IsSettingEncrypted"].Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                SaveConfiguration(_configuration, jsonFilePath, true);
            }
            else
            {
                SaveConfiguration(_configuration, jsonFilePath, false);
            }
        }            
        static void SaveConfiguration(IConfiguration configuration, string filePath, bool isEncrypted)
        {
            var json = new JObject();
            foreach (var section in configuration.GetChildren())
            {
                if (isEncrypted)
                {
                    DeccryptSection(json, section);
                }
                else
                {
                    EncryptSection(json, section);
                }
            }
            File.WriteAllText(filePath, json.ToString());
        }
        static void EncryptSection(JObject json, IConfigurationSection section)
        {
            var nestedJson = new JObject();
            if (section.GetChildren().Count() == 0)
            {
                if (section.Key.Equals("IsSettingEncrypted"))
                {
                    json[section.Key] = "true";
                }
                else if (section.Key.Equals("ThumbPrint"))
                {
                    json[section.Key] = section.Value;
                }
                else
                {
                    json[section.Key] = Convert.ToBase64String(EncryptionDecryptionHelper.EncryptString(section.Value));
                }
                return;
            }
            foreach (var child in section.GetChildren())
            {
                if (child.Value != null)
                {
                    // Add flat properties to the nested JObject
                    nestedJson[child.Key] = Convert.ToBase64String(EncryptionDecryptionHelper.EncryptString(child.Value));
                }
                else
                {
                    // Recursively add nested properties
                    EncryptSection(nestedJson, child);
                }
            }

            json[section.Key] = nestedJson;

        }
        static void DeccryptSection(JObject json, IConfigurationSection section)
        {
            var nestedJson = new JObject();
            if (section.GetChildren().Count() == 0)
            {
                if (section.Key.Equals("IsSettingEncrypted"))
                {
                    json[section.Key] = "false";
                }
                else if (section.Key.Equals("ThumbPrint"))
                {
                    json[section.Key] = section.Value;
                }
                else
                {
                    json[section.Key] = EncryptionDecryptionHelper.DecryptString(section.Value);
                }
                return;
            }
            foreach (var child in section.GetChildren())
            {
                if (child.Value != null)
                {
                    // Add flat properties to the nested JObject
                    nestedJson[child.Key] = EncryptionDecryptionHelper.DecryptString(child.Value);
                }
                else
                {
                    // Recursively add nested properties
                    DeccryptSection(nestedJson, child);
                }
            }

            json[section.Key] = nestedJson;

        }
        private static X509Certificate2 GetCertificateByThumbprint(string thumbprint)
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
            store.Close();

            return certificates.Count > 0 ? certificates[0] : null;
        }
        private X509Certificate2 GetAllStoredCertificates()
        {
            // Specify the subject name of the certificate you're looking for
            string subjectNameToFind = "YourCertificateSubjectName";
            X509Certificate2 storedCertificate = null;
            // Specify the certificate store and location
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);

            try
            {
                store.Open(OpenFlags.ReadOnly);

                // Iterate through each certificate in the store
                foreach (X509Certificate2 cert in store.Certificates)
                {
                    // Compare the subject name with the one you're looking for
                    if (string.Equals(cert.Subject, subjectNameToFind, StringComparison.OrdinalIgnoreCase))
                    {
                        storedCertificate = cert;
                    }
                }
                return storedCertificate;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                store.Close(); // Make sure to close the store after use
            }
        }
    }
}