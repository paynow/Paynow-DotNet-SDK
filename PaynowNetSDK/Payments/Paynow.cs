using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Paynow.Core;
using Paynow.Exceptions;
using Paynow.Helpers;
using Paynow.Http;

namespace Paynow.Payments
{
    public class Paynow
    {
        /// <summary>
        /// Merchant's return url
        /// </summary>
        public string ResultUrl { get; set; } = "http://localhost";

        /// <summary>
        /// Merchant's result url
        /// </summary>
        public string ReturnUrl { get; set; } = "http://localhost";

        /// <summary>
        /// Merchant's integration id
        /// </summary>
        public Guid IntegrationKey { get; set; }

        /// <summary>
        /// Client for making http requests
        /// </summary>
        public Client Client { get; set; }

        /// <summary>
        /// Merchant's integration key
        /// </summary>
        public string IntegrationId { get; set; }

        /// <summary>
        /// Paynow constructor
        /// </summary>
        /// <param name="integrationId"></param>
        /// <param name="integrationKey"></param>
        /// <param name="resultUrl"></param>
        /// <exception cref="ArgumentException"></exception>
        public Paynow(string integrationId, string integrationKey, string resultUrl = null)
        {
            if (string.IsNullOrEmpty(integrationId))
                throw new ArgumentException("Integration id cannot be empty", nameof(integrationId));
            if (string.IsNullOrEmpty(integrationKey))
                throw new ArgumentException("Integration key cannot be empty", nameof(integrationKey));


            IntegrationId = integrationId;
            IntegrationKey = Guid.Parse(integrationKey);

            if (resultUrl != null)
                ResultUrl = resultUrl;


            Client = new Client();
        }

        /// <summary>
        /// Creates a new transaction
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public Payment CreatePayment(string reference, Dictionary<string, decimal> values = null)
        {
            return values != null ? new Payment(reference, values) : new Payment(reference);
        }

        /// <summary>
        /// Sends a payment to paynow
        /// </summary>
        /// <param name="payment"></param>
        /// <returns></returns>
        /// <exception cref="InvalidReferenceException"></exception>
        /// <exception cref="EmptyCartException"></exception>
        public InitResponse Send(Payment payment)
        {
            if (string.IsNullOrEmpty(payment.Reference))
                throw new InvalidReferenceException();

            if (payment.Total <= 0)
                throw new EmptyCartException();

            return Init(payment);
        }

        public StatusResponse PollTransaction(string url)
        {
            var response = Client.PostAsync(url, null);
            var data = HttpUtility.ParseQueryString(response.Result).ToDictionary();
            
            if (!data.ContainsKey("hash") || Hash.Verify(data, IntegrationKey))
            {
                throw new HashMismatchException();
            }
            
            return new StatusResponse(data);
        }
        
        /// <summary>
        /// Process a status update from Paynow
        /// </summary>
        /// <param name="response">Raw POST string sent from Paynow</param>
        /// <returns></returns>
        /// <exception cref="HashMismatchException"></exception>
        public StatusResponse ProcessStatusUpdate(string response)
        {
            var data = HttpUtility.ParseQueryString(response).ToDictionary();
            
            if (!data.ContainsKey("hash") || Hash.Verify(data, IntegrationKey))
            {
                throw new HashMismatchException();
            }
            
            return new StatusResponse(data);
        }
        
        
        /// <summary>
        /// Process a status update from Paynow
        /// </summary>
        /// <param name="response">Key-value pairs of data sent from Paynow</param>
        /// <returns></returns>
        /// <exception cref="HashMismatchException"></exception>
        public StatusResponse ProcessStatusUpdate(Dictionary<string, string> response)
        {            
            if (!response.ContainsKey("hash") || Hash.Verify(response, IntegrationKey))
            {
                throw new HashMismatchException();
            }
            
            return new StatusResponse(response);
        }
        
        /// <summary>
        /// Send a mobile transaction to paynow
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="phone"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        /// <exception cref="InvalidReferenceException"></exception>
        /// <exception cref="EmptyCartException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public InitResponse SendMobile(Payment payment, string phone,
            MobileMoneyMethod method = MobileMoneyMethod.Ecocash)
        {
            if (string.IsNullOrEmpty(payment.Reference))
                throw new InvalidReferenceException();

            if (payment.Total <= 0)
                throw new EmptyCartException();

            if (!Regex.IsMatch(phone, "07([7,8])((\\1=7)[1-9]|[2-5])\\d{6}"))
                throw new ArgumentException("Invalid phone number", nameof(phone));

            return InitMobile(payment, phone, method);
        }

        /// <summary>
        /// Initiate a new Paynow mobile transaction
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="phone"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private InitResponse InitMobile(Payment payment, string phone, MobileMoneyMethod method)
        {
            var data = FormatMobileInitRequest(payment, phone, method);

            var response = Client.PostAsync(Constants.UrlInitiateTransaction, data).Result;

            if (!data.ContainsKey("hash") || Hash.Verify(data, IntegrationKey))
            {
                throw new HashMismatchException();
            }
            
            return new InitResponse(HttpUtility.ParseQueryString(response).ToDictionary());
        }


        /// <summary>
        /// Initiate a new Paynow transaction
        /// </summary>
        /// <param name="payment"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private InitResponse Init(Payment payment)
        {
            var data = FormatInitRequest(payment);

            var response = Client.PostAsync(Constants.UrlInitiateTransaction, data).Result;

            if (!data.ContainsKey("hash") || Hash.Verify(data, IntegrationKey))
            {
                throw new HashMismatchException();
            }
            
            return new InitResponse(HttpUtility.ParseQueryString(response).ToDictionary());
        }

        /// <summary>
        /// Formats an init request before its sent to Paynow
        /// </summary>
        /// <param name="payment"></param>
        /// <returns></returns>
        private Dictionary<string, string> FormatInitRequest(Payment payment)
        {
            var items = payment.ToDictionary();

            items["returnurl"] = ReturnUrl.Trim();
            items["resulturl"] = ResultUrl.Trim();
            items["id"] = IntegrationId;

            items.Add("hash", Hash.Make(items, IntegrationKey));

            return items;
        }

        /// <summary>
        /// Initiate a new Paynow transaction
        /// </summary>
        /// <remarks>
        /// Currently, only eccocash is supported
        /// </remarks>
        /// <param name="payment">The transaction to be sent to Paynow</param>
        /// <param name="phone">The user's phone number</param>
        /// <param name="method">The mobile transaction method i.e ecocash, telecash</param>
        /// <returns></returns>
        private Dictionary<string, string> FormatMobileInitRequest(Payment payment, string phone,
            MobileMoneyMethod method)
        {
            var items = payment.ToDictionary();

            items["returnurl"] = ReturnUrl.Trim();
            items["resulturl"] = ResultUrl.Trim();
            items["id"] = IntegrationId;
            items["phone"] = phone;
            items["method"] = method.GetString();

            items.Add("hash", Hash.Make(items, IntegrationKey));

            return items;
        }
    }
}