using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DurableTask.Core.Exceptions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace AzureServicesDemo.DurableFunctionPatterns
{
    public static class AsyncHttpApiPattern
    {
        #region Client Function for Deterministic API
        [FunctionName("AsyncHttpApiPattern_HttpTrigger")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("AsyncHttpApiPattern_OrchestrationTrigger", null);

            log.LogInformation($"HttpTrigger Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
        #endregion

        #region Orchestration Function
        [FunctionName("AsyncHttpApiPattern_OrchestrationTrigger")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            log.LogInformation($"OrchestrationTrigger Started.");
            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            outputs.Add(await context.CallActivityAsync<string>("AsyncHttpApiPattern_ActivityTrigger", "Tokyo"));
            outputs.Add(await context.CallActivityAsync<string>("AsyncHttpApiPattern_ActivityTrigger", "Delhi"));
            outputs.Add(await context.CallActivityAsync<string>("AsyncHttpApiPattern_ActivityTrigger", "London"));                        

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
        [FunctionName("AsyncHttpApiPattern_ActivityTrigger")]
        public static string SayHelloActivity([ActivityTrigger] IDurableActivityContext activityContext, ILogger log)
        {
            log.LogInformation($"ActivityTrigger Started.");
            string name = activityContext.GetInput<string>();
            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }
        #endregion

        #region Activity Function Version 3
        //[FunctionName("AsyncHttpApiPattern_ActivityTrigger")]
        //public static async Task<string> SayHelloActivity([ActivityTrigger] IDurableActivityContext activityContext, ILogger log)
        //{
        //    log.LogInformation($"ActivityTrigger Started.");
        //    string name = activityContext.GetInput<string>();
        //    log.LogInformation($"Saying hello to {name}.");
        //    return $"Hello {name}!";
        //}
        #endregion
    }
}