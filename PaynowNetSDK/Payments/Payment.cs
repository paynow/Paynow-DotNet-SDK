﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Webdev.Payments
{
    /// <summary>
    ///     Represents a single transaction to be sent to Paynow
    /// </summary>
    public class Payment
    {
        public Payment(string reference, string authEmail)
        {
            Reference = reference;
            Items = new Dictionary<string, decimal>();
            AuthEmail = authEmail;
        }

        public Payment(string reference, Dictionary<string, decimal> values, string authEmail)
        {
            Reference = reference;
            Items = values;
            AuthEmail = authEmail;
        }

        /// <summary>
        ///     This is the reference for the transaction (like an id in the database)
        /// </summary>
        public string Reference { get; set; }

        /// <summary>
        ///     List of the items in the transaction
        /// </summary>
        private Dictionary<string, decimal> Items { get; }

        /// <summary>
        ///     Get the total of the items in the transaction
        /// </summary>
        public decimal Total => GetTotal();

        /// <summary>
        /// Email to be sent to Paynow with the transaction 
        /// </summary>
        public string AuthEmail = "";


        /// <summary>
        ///     Add a new item to the transaction
        /// </summary>
        /// <param name="title">The name of the item</param>
        /// <param name="amount">The cost of the item</param>
        public Payment Add(string title, decimal amount)
        {
            Items.Add(title, amount);

            return this;
        }

        /// <summary>
        ///     Add a new item to the transaction
        /// </summary>
        /// <param name="title">The name of the item</param>
        /// <param name="amount">The cost of the item</param>
        public Payment Add(string title, float amount)
        {
            Items.Add(title, Convert.ToDecimal(amount));

            return this;
        }

        /// <summary>
        ///     Add a new item to the transaction
        /// </summary>
        /// <param name="title">The name of the item</param>
        /// <param name="amount">The cost of the item</param>
        public Payment Add(string title, int amount)
        {
            Items.Add(title, Convert.ToDecimal(amount));

            return this;
        }

        /// <summary>
        ///     Remove an item from the transaction
        /// </summary>
        /// <param name="title"></param>
        public Payment Remove(string title)
        {
            var exists = Items.Any(item => item.Key == title);
            if (exists)
                Items.Remove(title);

            return this;
        }

        /// <summary>
        ///     Get the string representation of the items in the transaction
        /// </summary>
        public string ItemsDescription()
        {
            return string.Join(", ", Items.Keys);
        }

        /// <summary>
        ///     Get the total cost of the items in the transaction
        /// </summary>
        private decimal GetTotal()
        {
            return Items.Aggregate(0m,
                (current, next) => current += next.Value);
        }

        /// <summary>
        ///     Get the items in the transaction as a dictionary
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>
            {
                {"resulturl", ""},
                {"returnurl", ""},
                {"reference", Reference},
                {"amount", Total.ToString(CultureInfo.CurrentCulture)},
                {"id", ""},
                {"additionalinfo", ItemsDescription()},
                {"authemail", AuthEmail},
                {"status", "Message"}
            };
        }
    }
}