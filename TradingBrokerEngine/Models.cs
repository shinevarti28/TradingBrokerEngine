using Binance.Net.Objects.Models.Futures;
using CryptoExchange.Net.Objects;
using System;

namespace TradingBrokerEngine
{
    internal class Position
    {
        public string Symbol { get; set; }
        public string Action { get; set; }
        public string Price { get; set; }
       // public string SL { get; set; }
      //  public string TP { get; set; }
    }

    public class OrderResult
    {
        public WebCallResult<BinanceFuturesPlacedOrder> BinanceOrder { get; set; } = null;
        public Exception Excptn { get; set; } = null;
    }
}
