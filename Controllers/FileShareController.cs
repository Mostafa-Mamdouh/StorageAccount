using FilesStorage.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace FilesStorage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileShareController : ControllerBase
    {
        private readonly IFileShare fileShareService;
        public FileShareController(IFileShare fileShareService)
        {
            this.fileShareService = fileShareService ?? throw new ArgumentNullException(nameof(fileShareService));
        }
        [HttpPost("upload"), DisableRequestSizeLimit]
        public async Task<ActionResult> UploadAsync(IFormFile file)
        {
             var filePath=await  fileShareService.UploadFileToAzureSharedFile(file);
            return Ok(filePath);
        }


        [HttpGet("download")]
        public ActionResult Download([FromQuery] string filePath)
        {
            var fileUrl = fileShareService.GetFileLinkFromAzure(filePath);
            return Ok(fileUrl);
        }

    }
}
