using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AzureServicesDemo.DurableFunctionPatterns
{
    public class Counter
    {
        // class-based syntax
        [JsonProperty("value")]
        public int CurrentValue { get; set; }

        public void Add(int amount) => this.CurrentValue += amount;

        public void Reset() => this.CurrentValue = 0;

        public int Get() => this.CurrentValue;

        public void Delete() => Entity.Current.DeleteState();

        // Entity Function
        //[FunctionName(nameof(Counter))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx)
            => ctx.DispatchAsync<Counter>();
    }
    public static class AggregatorPattern
    {
        //[FunctionName("GetCounter")]
        public static async Task<HttpResponseMessage> GetCounter(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Counter/{entityKey}")] HttpRequestMessage req,
        [DurableClient] IDurableEntityClient client,
        string entityKey)
        {
            // Example: client reads entity state
            var counterDurableentityId = new EntityId(nameof(Counter), entityKey);
            await client.SignalEntityAsync(counterDurableentityId, "Add",2);
            EntityStateResponse<JObject> stateResponse = await client.ReadEntityStateAsync<JObject>(counterDurableentityId);
            return req.CreateResponse(HttpStatusCode.OK, stateResponse.EntityState);
        }

        //[FunctionName("Counter")]
        //public static void Counter([EntityTrigger] IDurableEntityContext ctx)
        //{
        //    // Function-based syntax
        //    int currentValue = ctx.GetState<int>();
        //    switch (ctx.OperationName.ToLowerInvariant())
        //    {
        //        case "add":
        //            int amount = ctx.GetInput<int>();
        //            ctx.SetState(currentValue + amount);
        //            break;
        //        case "reset":
        //            ctx.SetState(0);
        //            break;
        //        case "get":
        //            ctx.Return(currentValue);
        //            break;
        //        case "delete":
        //            ctx.DeleteState();
        //            break;
        //    }
        //}

        //[FunctionName("DeleteCounter")]
        //public static async Task<HttpResponseMessage> DeleteCounter(
        //[HttpTrigger(AuthorizationLevel.Function, "delete", Route = "Counter/{entityKey}")] HttpRequestMessage req,
        //[DurableClient] IDurableEntityClient client,
        //string entityKey)
        //{
        //    // Example: client signals entity
        //    var entityId = new EntityId("Counter", entityKey);
        //    await client.SignalEntityAsync(entityId, "Delete");
        //    return req.CreateResponse(HttpStatusCode.Accepted);
        //}      

        //    [FunctionName("IncrementThenGet")]
        //    public static async Task<int> Run(
        //    [OrchestrationTrigger] IDurableOrchestrationContext context)
        //    {
        //        // Example: orchestration first signals, then calls entity
        //        var entityId = new EntityId("Counter", "myCounter");

        //        // One-way signal to the entity - does not await a response
        //        context.SignalEntity(entityId, "Add", 1);

        //        // Two-way call to the entity which returns a value - awaits the response
        //        int currentValue = await context.CallEntityAsync<int>(entityId, "Get");

        //        return currentValue;
        //    }
        //
    }
}