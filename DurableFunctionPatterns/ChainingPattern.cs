using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace AzureServicesDemo.DurableFunctionPatterns
{
    public static class ChainingPattern
    {
        //[FunctionName("ChainingPattern")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            outputs.Add(await context.CallActivityAsync<string>("ChainingPattern_Hello", "Tokyo"));
            outputs.Add(await context.CallActivityAsync<string>("ChainingPattern_Hello", "Seattle"));
            outputs.Add(await context.CallActivityAsync<string>("ChainingPattern_Hello", "London"));

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

        //[FunctionName("ChainingPattern_Hello")]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }

        //[FunctionName("ChainingPattern_HttpStart")]
        //public static async Task<HttpResponseMessage> HttpStart(
        //    [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
        //    [DurableClient] IDurableOrchestrationClient starter,
        //    ILogger log)
        //{
        //    // Function input comes from the request content.
        //    string instanceId = await starter.StartNewAsync("ChainingPattern", null);

        //    log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

        //    return starter.CreateCheckStatusResponse(req, instanceId);
        //}
    }
}