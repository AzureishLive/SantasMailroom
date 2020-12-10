using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq; 
using Microsoft.Azure.Storage.Blob;  

namespace Azureish.Santa.Mailroom.Api
{
    public static class UploadLetter
    {
        
        [FunctionName("UploadLetter")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [Blob("letters", Connection = "santamailroomstor_STORAGE")] CloudBlobContainer outputContainer,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            
            await outputContainer.CreateIfNotExistsAsync();

            var fileForUpload = req.Form.Files[0];            

            if(!IsImage(fileForUpload)) {
                log.LogError($"An error occured during upload: Invalid File Type.");
                return new BadRequestResult();
            }

            var blobName = $"{Guid.NewGuid().ToString()}{fileForUpload.FileName.Substring(fileForUpload.FileName.LastIndexOf('.'))}";
            var fileStream = fileForUpload.OpenReadStream();          

            var cloudBlockBlob = outputContainer.GetBlockBlobReference(blobName);

            try {
                await cloudBlockBlob.UploadFromStreamAsync(fileStream);
            } catch (Exception ex) {
                log.LogError($"An error occured during upload: {ex.Message}");
                return new BadRequestResult();
            }

            return new OkObjectResult(blobName);
        }

        public static bool IsImage(IFormFile file)
        {
            if (file.ContentType.Contains("image"))
            {
                return true;
            }

            string[] formats = new string[] { ".jpg", ".png", ".gif", ".jpeg" };

            return formats.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }

    }
}
