using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Web.Http;
using Binance.Net.Enums;
using Microsoft.Extensions.Configuration;

namespace TradingBrokerEngine
{
    public static class BrokerEngine
    {
        [FunctionName("BrokerEngine")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Trading Broker Engine HTTP trigger function processed a request.");
            try
            {
                string responseMessage = await ExecuteOrder(req, log);
                return new OkObjectResult(responseMessage);
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                log.LogError(message);
                return new InternalServerErrorResult();
            }
        }

        public static async Task<string> ExecuteOrder(HttpRequest req, ILogger log)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                log.LogInformation("request body : " + requestBody);
                Position data = JsonConvert.DeserializeObject<Position>(requestBody.TrimEnd('"').TrimStart('"'));
                BinanceConnector binanceConnector = new BinanceConnector();
                OrderResult orderResult = await binanceConnector.PlaceMarketOrder(data);
                string responseMessage = orderResult.Excptn != null ? $"{orderResult.Excptn.Message} && {orderResult.Excptn.StackTrace}"
                                                                    : $"{JsonConvert.SerializeObject(orderResult.BinanceOrder.Data)} " +
                                                                    $"&& SUCCESS {orderResult.BinanceOrder.Success} && ERROR {orderResult.BinanceOrder.Error}.";
                log.LogInformation(responseMessage);
                return responseMessage;
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                log.LogError(message);
                return message;
            }
        }
    }
}