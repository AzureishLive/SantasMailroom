using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Azureish.Santa.Mailroom.Api
{
    public static class ExtractText
    {
        [FunctionName("ExtractText")]
        public static void Run([BlobTrigger("letters/{name}", Connection = "santamailroomstor_STORAGE")]Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
        }
    }
}
