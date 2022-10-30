using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace AzureServicesDemo.DurableFunctionPatterns
{
    public static class AsyncHttpApiPattern
    {
        #region Client Function for Deterministic API
        [FunctionName("AsyncHttpApiPattern_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("AsyncHttpApiPattern", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
        #endregion

        #region Orchestration Function
        [FunctionName("AsyncHttpApiPattern")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            outputs.Add(await context.CallActivityAsync<string>("AsyncHttpApiPattern_Hello", "Tokyo"));
            outputs.Add(await context.CallActivityAsync<string>("AsyncHttpApiPattern_Hello", "Delhi"));
            outputs.Add(await context.CallActivityAsync<string>("AsyncHttpApiPattern_Hello", "London"));

            //Correct way of using DateTime
            var currentUtcDateTime = context.CurrentUtcDateTime;

            //Correct way of using Guid
            var newGuid = context.NewGuid();

            // returns ["Hello Tokyo!", "Hello Delhi!", "Hello London!"]
            return outputs;
        }
        #endregion

        #region Activity Function Version 1
        //[FunctionName("AsyncHttpApiPattern_Hello")]
        //public static string SayHello([ActivityTrigger] string name, ILogger log)
        //{
        //    log.LogInformation($"Saying hello to {name}.");
        //    return $"Hello {name}!";
        //}
        #endregion

        #region Activity Function Version 2
        [FunctionName("AsyncHttpApiPattern_Hello")]
        public static string SayHello([ActivityTrigger] IDurableActivityContext activityContext, ILogger log)
        {
            string name = activityContext.GetInput<string>();
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }
        #endregion
    }
}