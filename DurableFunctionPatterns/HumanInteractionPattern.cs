using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace AzureServicesDemo.DurableFunctionPatterns
{
    public class HumanInteractionPattern
    {
        [FunctionName("HumanInteractionPattern")]
        public async Task<IActionResult> HumanInteractionPattern_HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            string instanceId = await starter.StartNewAsync("ApprovalWorkflow", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        [FunctionName("ApprovalWorkflow")]
        public static async Task Run([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            // you can pass some data that you want to send in your request approval
            await context.CallActivityAsync("RequestApproval", "Email");
            using (var timeoutCts = new CancellationTokenSource())
            {
                DateTime dueTime = context.CurrentUtcDateTime.AddHours(72);
                // create the durable timer
                Task durableTimeout = context.CreateTimer(dueTime, timeoutCts.Token);

                // notification is received by
                Task<bool> approvalEvent = context.WaitForExternalEvent<bool>("ApprovalEvent");

                // to decide whether to escalate
                if (approvalEvent == await Task.WhenAny(approvalEvent, durableTimeout))
                {
                    timeoutCts.Cancel();
                    await context.CallActivityAsync("ProcessApproval", approvalEvent.Result);
                }
                else
                {
                    await context.CallActivityAsync("Escalate", "Escalated!");
                }
            }
        }

        [FunctionName("RequestApproval")]
        public static void RequestApproval([ActivityTrigger]string message, ILogger log)
        {
            // TODO: Send an email or some notification
            log.LogInformation(string.Format("{0} Send to Manager!", message));
            return;
        }               

        [FunctionName("ProcessApproval")]
        public static void ProcessApproval([ActivityTrigger] bool isApproved, ILogger log)
        {
            // TODO: Check Decision
            if (isApproved)
            {
                log.LogInformation("Approved!");
            }
            else
            {
                log.LogInformation("Rejected!");
            }            
        }

        [FunctionName("Escalate")]
        public static void Escalate([ActivityTrigger]string message, ILogger log)
        {
            // TODO: Send an email Sr Manager or NEXT Level
            log.LogInformation(message);
        }

        [FunctionName("ApprovalEvent")]
        public static async Task ApprovalEvent(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            string instanceId = await StreamToStringAsync(req);

            log.LogInformation($"To raise event, Orchestration ID = '{instanceId}'.");

            await starter.RaiseEventAsync(instanceId, "ApprovalEvent", false);
        }

        private static async Task<string> StreamToStringAsync(HttpRequest request)
        {
            using (var sr = new StreamReader(request.Body))
            {
                return await sr.ReadToEndAsync();
            }
        }
    }
}