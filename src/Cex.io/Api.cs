﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Nextmethod.Cex
{
    public sealed class Api
    {

        #region Static Helpers

        private static readonly Func<IEnumerable<KeyValuePair<string, string>>> EmptyRequestParams = () => Enumerable.Empty<KeyValuePair<string, string>>();

        static Api()
        {
            if (HttpClientFactory.ConnectionLimit != null && HttpClientFactory.ConnectionLimit.Value != Constants.DefaultConnectionLimit)
            {
                ServicePointManager.DefaultConnectionLimit = HttpClientFactory.ConnectionLimit.Value;
            }
            else if (ServicePointManager.DefaultConnectionLimit == Constants.DefaultConnectionLimit)
            {
                ServicePointManager.DefaultConnectionLimit = Constants.DesiredConnectionLimit;
            }
        }

        #endregion


        public Api(string username, string apiKey, string apiSecret)
            : this(new ApiCredentials(username, apiKey, apiSecret)) {}

        public Api(ApiCredentials credentials)
        {
            Credentials = credentials;
        }

        public TimeSpan? Timeout { get; set; }

        public ApiCredentials Credentials { get; private set; }


        public async Task<Ticker> Ticker(SymbolPair pair, CancellationTokenSource tokenSource = null)
        {
            const string basePath = "/ticker";
            var path = string.Format("{0}/{1}/{2}", basePath, pair.From, pair.To);

            try
            {
                return await this.GetFromService(
                    path,
                    Cex.Ticker.FromDynamic,
                    tokenSource
                    );
            }
            catch (AggregateException ae)
            {
                throw ae.Flatten();
            }
        }

        public async Task<OrderBook> OrderBook(SymbolPair pair, CancellationTokenSource tokenSource = null)
        {
            const string basePath = "/order_book";
            var path = string.Format("{0}/{1}/{2}", basePath, pair.From, pair.To);

            try
            {
                return await this.GetFromService(
                    path,
                    Cex.OrderBook.FromDynamic,
                    tokenSource
                    );
            }
            catch (AggregateException ae)
            {
                throw ae.Flatten();
            }
        }

        public async Task<IEnumerable<Trade>> TradeHistory(SymbolPair pair, CancellationTokenSource tokenSource = null)
        {
            const string basePath = "/trade_history";
            var path = string.Format("{0}/{1}/{2}", basePath, pair.From, pair.To);

            try
            {
                return await this.GetFromService(
                    path,
                    Trade.FromDynamic,
                    tokenSource
                    );
            }
            catch (AggregateException ae)
            {
                throw ae.Flatten();
            }
        }

        public async Task<Balance> AccountBalance(CancellationTokenSource tokenSource = null)
        {
            const string basePath = "/balance/";

            try
            {
                return await this.PostToService(
                    basePath,
                    EmptyRequestParams,
                    Balance.FromDynamic,
                    tokenSource
                    );
            }
            catch (AggregateException ae)
            {
                throw ae.Flatten();
            }
        }

        public async Task<IEnumerable<OpenOrder>> OpenOrders(SymbolPair pair, CancellationTokenSource tokenSource = null)
        {
            const string basePath = "/open_orders";
            var path = string.Format("{0}/{1}/{2}", basePath, pair.From, pair.To);

            try
            {
                return await this.PostToService(
                    path,
                    EmptyRequestParams,
                    x =>
                    {
                        var ja = x as JsonArray;
                        return ja == null
                            ? Enumerable.Empty<OpenOrder>()
                            : ja.Select(OpenOrder.FromDynamic).ToArray();
                    },
                    tokenSource
                    );
            }
            catch (AggregateException ae)
            {
                throw ae.Flatten();
            }
        }

        public async Task<OpenOrder> PlaceOrder(SymbolPair pair, Order order, CancellationTokenSource tokenSource = null)
        {
            const string basePath = "/place_order";
            var path = string.Format("{0}/{1}/{2}", basePath, pair.From, pair.To);

            try
            {
                return await this.PostToService(
                    path,
                    () => new[]
                    {
                        this.NewRequestParam("type", order.Type == OrderType.Sell ? "sell" : "buy"),
                        this.NewRequestParam("price", order.Price.ToString()),
                        this.NewRequestParam("amount", order.Amount.ToString())
                    },
                    OpenOrder.FromDynamic,
                    tokenSource
                    );
            }
            catch (AggregateException ae)
            {
                throw ae.Flatten();
            }
        }

        public async Task<bool> CancelOrder(TradeId tradeId, CancellationTokenSource tokenSource = null)
        {
            const string basePath = "/cancel_order/";

            try
            {
                return await this.PostToService(
                    basePath,
                    () => new[] {this.NewRequestParam("id", tradeId.ToString())},
                    x => (bool) x,
                    tokenSource
                    );
            }
            catch (AggregateException ae)
            {
                throw ae.Flatten();
            }
        }


    }
}