using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Binance.Net;
using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Objects;
using Binance.Net.Objects.Models.Futures;
using CryptoExchange.Net.Objects;

namespace TradingBrokerEngine
{
    internal class BinanceConnector
    {
        BinanceClient binanceClient;
        internal BinanceConnector()
        {
            var spot_api_base_address = Environment.GetEnvironmentVariable("SPOT_API_BASE_ADDRESS");
            var futures_api_base_address = Environment.GetEnvironmentVariable("FUTURES_API_BASE_ADDRESS");
            var spot_api_key = Environment.GetEnvironmentVariable("SPOT_API_KEY");
            var spot_api_secret = Environment.GetEnvironmentVariable("SPOT_API_SECRET");
            var futures_api_key = Environment.GetEnvironmentVariable("FUTURES_API_KEY");
            var futures_api_secret = Environment.GetEnvironmentVariable("FUTURES_API_SECRET");

            binanceClient = new BinanceClient(new BinanceClientOptions
            {
                ApiCredentials = new BinanceApiCredentials(spot_api_key, spot_api_secret, ApiCredentialsType.Hmac),
                SpotApiOptions = new BinanceApiClientOptions
                {
                    BaseAddress = spot_api_base_address,
                    RateLimitingBehaviour = RateLimitingBehaviour.Fail
                },
                UsdFuturesApiOptions = new BinanceApiClientOptions
                {
                    ApiCredentials = new BinanceApiCredentials(futures_api_key, futures_api_secret, ApiCredentialsType.Hmac),
                    BaseAddress = futures_api_base_address,
                }
            });
        }

        internal async Task<OrderResult> PlaceMarketOrder(Position data)
        {
            OrderResult orderResult = new OrderResult();
            try
            {
                string action = data?.Action;
                OrderSide orderSide = new OrderSide();
                OrderSide orderSideTP_SL = new OrderSide();
                if (action.Equals("Open Long") || action.Equals("Close Short"))
                {
                    orderSide = OrderSide.Buy;
                    orderSideTP_SL = OrderSide.Sell;
                }
                else if (action.Equals("Close Long") || action.Equals("Open Short"))
                {
                    orderSide = OrderSide.Sell;
                    orderSideTP_SL = OrderSide.Buy;
                }
                WebCallResult<BinanceFuturesPlacedOrder> openPositionResult = await binanceClient.UsdFuturesApi.Trading.PlaceOrderAsync(
                    symbol: "BTCUSDT",
                    side: orderSide,
                    type: FuturesOrderType.Market,
                    quantity: 1m,
                   // price: BinanceHelpers.FloorPrice(0.5m, Convert.ToDecimal(data?.Price)),
                   // timeInForce: TimeInForce.GoodTillCanceled,
                    orderResponseType: OrderResponseType.Result);
                orderResult.BinanceOrder = openPositionResult;
                if (openPositionResult.Success)
                {
                    decimal stopPrice = 0;
                    if (orderSideTP_SL == OrderSide.Sell)
                        stopPrice = Math.Floor(openPositionResult.Data.AveragePrice) + Constants.PROFIT_THRESHOLD;
                    else if (orderSideTP_SL == OrderSide.Buy)
                        stopPrice = Math.Floor(openPositionResult.Data.AveragePrice) - Constants.PROFIT_THRESHOLD;
                    // take profit
                    //var tpOrderResult = await binanceClient.UsdFuturesApi.Trading.PlaceOrderAsync(
                    //                            symbol: "BTCUSDT",
                    //                            side: orderSideTP_SL,
                    //                            type: FuturesOrderType.TakeProfitMarket,
                    //                            stopPrice: Convert.ToDecimal(data?.TP),
                    //                            quantity: null,
                    //                            orderResponseType: OrderResponseType.Result,
                    //                            closePosition: true
                    //                );
                    //var stopLossResult = await binanceClient.UsdFuturesApi.Trading.PlaceOrderAsync(
                    //                            symbol: "BTCUSDT",
                    //                            side: orderSideTP_SL,
                    //                            type: FuturesOrderType.StopMarket,
                    //                            quantity: null,
                    //                            orderResponseType: OrderResponseType.Result,
                    //                            closePosition: true,
                    //                            stopPrice: Convert.ToDecimal(data?.SL));
                }
            }
            catch (Exception ex)
            {
                orderResult.Excptn = ex;
            }
            return orderResult;
        }
    }
}
