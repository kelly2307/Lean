﻿/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); 
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections.Generic;

namespace QuantConnect.Orders
{
    /// <summary>
    /// Order struct for placing new trade
    /// </summary>
    public abstract class Order 
    {
        /// <summary>
        /// Order ID.
        /// </summary>
        public int Id;

        /// <summary>
        /// Order id to process before processing this order.
        /// </summary>
        public int ContingentId;

        /// <summary>
        /// Brokerage Id for this order for when the brokerage splits orders into multiple pieces
        /// </summary>
        public List<long> BrokerId;

        /// <summary>
        /// Symbol of the Asset
        /// </summary>
        public string Symbol;
        
        /// <summary>
        /// Price of the Order.
        /// </summary>
        public decimal Price;

        /// <summary>
        /// Time the order was created.
        /// </summary>
        public DateTime Time;

        /// <summary>
        /// Number of shares to execute.
        /// </summary>
        public int Quantity;

        /// <summary>
        /// Order Type
        /// </summary>
        public OrderType Type { get; private set; }

        /// <summary>
        /// Status of the Order
        /// </summary>
        public OrderStatus Status;

        /// <summary>
        /// Order duration - GTC or Day. Day not supported in backtests.
        /// </summary>
        public OrderDuration Duration = OrderDuration.GTC;

        /// <summary>
        /// Tag the order with some custom data
        /// </summary>
        public string Tag;

        /// <summary>
        /// The symbol's security type
        /// </summary>
        public SecurityType SecurityType = SecurityType.Equity;

        /// <summary>
        /// Order Direction Property based off Quantity.
        /// </summary>
        public OrderDirection Direction 
        {
            get 
            {
                if (Quantity > 0) 
                {
                    return OrderDirection.Buy;
                } 
                if (Quantity < 0) 
                {
                    return OrderDirection.Sell;
                }
                return OrderDirection.Hold;
            }
        }

        /// <summary>
        /// Get the absolute quantity for this order
        /// </summary>
        public decimal AbsoluteQuantity
        {
            get { return Math.Abs(Quantity); }
        }

        /// <summary>
        /// Value of the order at limit price if a limit order, or market price if a market order.
        /// </summary>
        public abstract decimal Value 
        { 
            get; 
        }

        /// <summary>
        /// Added a default constructor for JSON Deserialization:
        /// </summary>
        protected Order(OrderType type)
        {
            Time = new DateTime();
            Price = 0;
            Type = type;
            Quantity = 0;
            Symbol = "";
            Status = OrderStatus.None;
            Tag = "";
            SecurityType = SecurityType.Base;
            Duration = OrderDuration.GTC;
            BrokerId = new List<long>();
            ContingentId = 0;
        }

        /// <summary>
        /// New order constructor
        /// </summary>
        /// <param name="symbol">Symbol asset we're seeking to trade</param>
        /// <param name="type">Type of the security order</param>
        /// <param name="quantity">Quantity of the asset we're seeking to trade</param>
        /// <param name="order">Order type (market, limit or stoploss order)</param>
        /// <param name="time">Time the order was placed</param>
        /// <param name="tag">User defined data tag for this order</param>
        protected Order(string symbol, int quantity, OrderType order, DateTime time, string tag = "", SecurityType type = SecurityType.Base)
        {
            Time = time;
            Price = 0;
            Type = order;
            Quantity = quantity;
            Symbol = symbol;
            Status = OrderStatus.None;
            Tag = tag;
            SecurityType = type;
            Duration = OrderDuration.GTC;
            BrokerId = new List<long>();
            ContingentId = 0;
        }

        /// <summary>
        /// New order constructor
        /// </summary>
        /// <param name="symbol">Symbol asset we're seeking to trade</param>
        /// <param name="type"></param>
        /// <param name="quantity">Quantity of the asset we're seeking to trade</param>
        /// <param name="order">Order type (market, limit or stoploss order)</param>
        /// <param name="time">Time the order was placed</param>
        /// <param name="tag">User defined data tag for this order</param>
        protected Order(string symbol, SecurityType type, int quantity, OrderType order, DateTime time, string tag = "") 
        {
            Time = time;
            Price = 0;
            Type = order;
            Quantity = quantity;
            Symbol = symbol;
            Status = OrderStatus.None;
            Tag = tag;
            SecurityType = type;
            Duration = OrderDuration.GTC;
            BrokerId = new List<long>();
            ContingentId = 0;
        }

        /// <summary>
        /// Modifies the state of this order to match the update request
        /// </summary>
        /// <param name="request">The request to update this order object</param>
        public virtual void ApplyUpdateOrderRequest(UpdateOrderRequest request)
        {
            if (request.OrderId != Id)
            {
                throw new ArgumentException("Attempted to apply updates to the incorrect order!");
            }
            if (request.Quantity.HasValue)
            {
                Quantity = request.Quantity.Value;
            }
            if (request.Tag != null)
            {
                Tag = request.Tag;
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return string.Format("{0} order for {1} unit{3} of {2}", Type, Quantity, Symbol, Quantity == 1 ? "" : "s");
        }

        /// <summary>
        /// Creates an <see cref="Order"/> to match the specified <paramref name="request"/>
        /// </summary>
        /// <param name="request">The <see cref="SubmitOrderRequest"/> to create an order for</param>
        /// <returns>The <see cref="Order"/> that matches the request</returns>
        public static Order CreateOrder(SubmitOrderRequest request)
        {
            Order order;
            switch (request.OrderType)
            {
                case OrderType.Market:
                    order =  new MarketOrder(request.Symbol, request.Quantity, request.Time, request.Tag, request.SecurityType);
                    break;
                case OrderType.Limit:
                    order =  new LimitOrder(request.Symbol, request.Quantity, request.LimitPrice, request.Time, request.Tag, request.SecurityType);
                    break;
                case OrderType.StopMarket:
                    order =  new StopMarketOrder(request.Symbol, request.Quantity, request.StopPrice, request.Time, request.Tag, request.SecurityType);
                    break;
                case OrderType.StopLimit:
                    order =  new StopLimitOrder(request.Symbol, request.Quantity, request.StopPrice, request.LimitPrice, request.Time, request.Tag, request.SecurityType);
                    break;
                case OrderType.MarketOnOpen:
                    order =  new MarketOnOpenOrder(request.Symbol, request.SecurityType, request.Quantity, request.Time, request.Tag);
                    break;
                case OrderType.MarketOnClose:
                    order =  new MarketOnCloseOrder(request.Symbol, request.SecurityType, request.Quantity, request.Time, request.Tag);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            order.Status = OrderStatus.New;
            order.Id = request.OrderId;
            if (request.Tag != null)
            {
                order.Tag = request.Tag;
            }
            return order;
        }
    }
}
