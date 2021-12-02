using Azure;
using Azure.Storage;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace FilesStorage.Services
{
    public class FileShare : IFileShare
    {

        private readonly IConfiguration _config;
        public FileShare(IConfiguration config)
        {
            _config = config;
        }

        public string GetFileLinkFromAzure(string FilePath)
        {
            string fileName = "";
            FilePath = Uri.UnescapeDataString(FilePath);

            // Get a reference from our share 
            ShareClient share = new ShareClient(_config.GetConnectionString("AzureStorage"), _config["AzureSettings:ShareName"]);
            ShareDirectoryClient rootDir = share.GetRootDirectoryClient();
            // Get a reference to our file 
            ShareFileClient file = rootDir.GetFileClient(fileName);
            MemoryStream memStream = new MemoryStream();
            AccountSasBuilder sas = new AccountSasBuilder
            {
                // Allow access to blobs
                Services = AccountSasServices.Files,
                Protocol = SasProtocol.Https,
                // Allow access to the service level APIs
                ResourceTypes = AccountSasResourceTypes.All,
                StartsOn = DateTime.UtcNow.AddMinutes(-1),
                // Access expires in 1 hour!
                ExpiresOn = DateTime.UtcNow.AddMinutes(2),
            };
            // Allow read access
            sas.SetPermissions(AccountSasPermissions.Read);

            // Create a SharedKeyCredential that we can use to sign the SAS token
            StorageSharedKeyCredential credential = new StorageSharedKeyCredential(_config["AzureSettings:AzureStorageAccountName"], _config["AzureSettings:AzureStorageAccountKey"]);
            // Use the key to get the SAS token.
            var sasToken = sas.ToSasQueryParameters(credential).ToString();
            var fileUrl = _config["AzureSettings:AzureStorageAccountPath"] + FilePath + "?" + sasToken;
            return fileUrl;
        }

        public async Task<string> UploadFileToAzureSharedFile(IFormFile File)
        {
            string imageFullPath = null;
            // Get a reference from our share 
            ShareClient share = new ShareClient(_config.GetConnectionString("AzureStorage"), _config["AzureSettings:ShareName"]);

            // Get a reference from our directory - directory located at root level
            // folders hirarchy  Root => Uploads => Year => Month 
            ShareDirectoryClient rootDir = share.GetRootDirectoryClient();
            ShareDirectoryClient uploadfolder = rootDir.GetSubdirectoryClient("Uploads");
            await uploadfolder.CreateIfNotExistsAsync();
            ShareDirectoryClient yearDir = uploadfolder.GetSubdirectoryClient(DateTime.Now.Year.ToString());
            await yearDir.CreateIfNotExistsAsync();
            ShareDirectoryClient monthDir = yearDir.GetSubdirectoryClient(DateTime.Now.Month.ToString());
            await monthDir.CreateIfNotExistsAsync();

            string strExtension = Path.GetExtension(File.FileName).ToLower();
            string NewFileName = string.Empty;

            var RandomNo = new Random();

            NewFileName = RandomNo.Next(DateTime.Now.DayOfYear, DateTime.Now.DayOfYear + 100).ToString() + "-" + String.Format("Attachment-" + "{0:yyyy-MM-dd_hh-mm-ssss-fff-tt}", DateTime.Now);
            NewFileName = NewFileName.Replace("ص", "AM");
            NewFileName = NewFileName.Replace("م", "PM");

            // Get a reference to our file
            ShareFileClient fileShare = monthDir.GetFileClient(NewFileName + strExtension);
            using (Stream stream = File.OpenReadStream())
            {
                fileShare.Create(stream.Length);
                fileShare.UploadRange(
                    new HttpRange(0, stream.Length),
                    stream);
            }
            // File Path to save in DB so we can retreive the file by this path.
            imageFullPath = fileShare.Uri.ToString().Replace(_config["AzureSettings:AzureStorageAccountPath"], "");
            return imageFullPath;
        }
    }
}
