using System;
using System.IO;
using System.Net.Http;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Azureish.Santa.Mailroom.Api
{
    public static class ExtractText
    {
        [FunctionName("ExtractText")]
        public static void Run([BlobTrigger("letters/{name}", Connection = "santamailroomstor_STORAGE")]Stream myBlob, 
            string name, 
            ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            string _apiKey = Environment.GetEnvironmentVariable("cognitiveKey");
            string _apiEndpoint = Environment.GetEnvironmentVariable("cognitiveEndpoint");
            string _apiUrlBase = _apiEndpoint + "/vision/v3.1/read/analyze";

            using (var httpClient = new HttpClient()) {
                httpClient.BaseAddress = new Uri(_apiUrlBase);
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _apiKey);

                using(HttpContent content = new StreamContent(myBlob)) {
                    content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

                    var cognitiveServicesUri = $"{_apiUrlBase}?handwriting=true";
                    var response = httpClient.PostAsync(cognitiveServicesUri, content).Result;

                    string operationLocation = string.Empty;

                    if (response.IsSuccessStatusCode) {
                        log.LogInformation("ExtractText successful");
                        operationLocation = response.Headers.GetValues("Operation-Location").FirstOrDefault();
                    }
                    else {
                        log.LogError("Extract text failed");
                    }

                    string contentString;
                    int i=0;
                    do {
                        response = httpClient.GetAsync(operationLocation).Result;
                        contentString = response.Content.ReadAsStringAsync().Result;
                        ++i;
                        System.Threading.Thread.Sleep(1000);
                    } while (i < 10 && contentString.IndexOf("\"status\":\"Succeeded\"") == -1);
                    
                }
            }

        }
    }
}
