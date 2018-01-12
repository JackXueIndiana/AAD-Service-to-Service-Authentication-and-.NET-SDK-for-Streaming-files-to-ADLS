using System;
using System.IO;
using System.Threading;
using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;
using Microsoft.Azure.DataLake.Store;

namespace JackDataLakeStoreUploading
{
    class Program
    {
        private static ServiceClientCredentials GetCreds_SPI_SecretKey(
   string tenant,
   Uri tokenAudience,
   string clientId,
   string secretKey)
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

            var serviceSettings = ActiveDirectoryServiceSettings.Azure;
            serviceSettings.TokenAudience = tokenAudience;

            var creds = ApplicationTokenProvider.LoginSilentAsync(
             tenant,
             clientId,
             secretKey,
             serviceSettings).GetAwaiter().GetResult();
            return creds;
        }

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Starting...");

                // Service principal / appplication authentication with client secret / key
                // Use the client ID of an existing AAD "Web App" application.
                string TENANT = "72f988bf-86f1-41af-91ab-2d7cd011db47";
                string CLIENTID = "354d9d70-4993-4be6-aef0-8fae7dbf700a";
                System.Uri ARM_TOKEN_AUDIENCE = new System.Uri(@"https://management.core.windows.net/");
                System.Uri ADL_TOKEN_AUDIENCE = new System.Uri(@"https://datalake.azure.net/");
                string secret_key = "9S5JUuKBm4xlL6aSer1paFoGikO3Gn374xz2fzbqBmw=";
                var armCreds = GetCreds_SPI_SecretKey(TENANT, ARM_TOKEN_AUDIENCE, CLIENTID, secret_key);
                var adlCreds = GetCreds_SPI_SecretKey(TENANT, ADL_TOKEN_AUDIENCE, CLIENTID, secret_key);

                var _adlsAccountName = "jackdatlakestore.azuredatalakestore.net";

                var client = AdlsClient.CreateClient(_adlsAccountName, adlCreds);

                // Get file properties
                var destPath = "/mytempdir/test.txt";
                var directoryEntry = client.GetDirectoryEntry(destPath);
                Console.WriteLine(directoryEntry.ToString());

                // Create a file - automatically creates any parent directories that don't exist

                string fileName = "/mytempdir/test1.txt";
                using (var streamWriter = new StreamWriter(client.CreateFile(fileName, IfExists.Overwrite)))
                {}

                // Append to existing file
                var srcPath = @"c:\tmp\test.txt";
                using (var streamWriter = new StreamWriter(client.GetAppendStream(fileName)))
                {
                    streamWriter.Write(File.ReadAllText(srcPath));
                }
                
                Console.WriteLine("Ending...");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Exception caught.", e);
            }
        }
    }
}
