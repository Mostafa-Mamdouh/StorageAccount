using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FilesStorage.Services
{
   public interface IFileShare
    {
        Task<string> UploadFileToAzureSharedFile(IFormFile File);
        string GetFileLinkFromAzure(string FilePath);
    }
}
