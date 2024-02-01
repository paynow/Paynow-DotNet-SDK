using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Webdev.Core;
using Webdev.Exceptions;
using Webdev.Helpers;
using Webdev.Http;
// ReSharper disable UnusedMember.Global

namespace Webdev.Payments
{
    public class Paynow
    {
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
        /// The base URL of the Paynow server we're communicating with. Do NOT include the trailing slash (/)
        /// </summary>
        public string PaynowBaseUrl { get; set; } = "https://www.paynow.co.zw";

        /// <summary>
        /// Creates a new transaction
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="values"></param>
        /// <param name="authEmail"></param>
        /// <returns></returns>
        public Payment CreatePayment(string reference, Dictionary<string, decimal> values = null, string authEmail = "")
        {
            return values != null ? new Payment(reference, values, authEmail) : new Payment(reference, authEmail);
        }

        /// <summary>
        /// Creates a new transaction (this overload is used for mobile payments where email is required)
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="authEmail"></param>
        /// <returns></returns>
        public Payment CreatePayment(string reference, string authEmail)
        {
            return new Payment(reference, authEmail);
        }

        /// <summary>
        /// Sends a payment to Paynow
        /// </summary>
        /// <param name="payment"></param>
        /// <returns></returns>
        /// <exception cref="InvalidReferenceException"></exception>
        /// <exception cref="EmptyCartException"></exception>
        public InitResponse Send(Payment payment)
        {
            return SendAsync(payment).Result;
        }

        /// <summary>
        /// Sends a payment to Paynow
        /// </summary>
        /// <param name="payment"></param>
        /// <returns></returns>
        /// <exception cref="InvalidReferenceException"></exception>
        /// <exception cref="EmptyCartException"></exception>
        public async Task<InitResponse> SendAsync(Payment payment)
        {
            if (string.IsNullOrEmpty(payment.Reference))
                throw new InvalidReferenceException();

            if (payment.Total <= 0)
                throw new EmptyCartException();

            return await InitAsync(payment);
        }

        /// <summary>
        /// Get a transaction status from Paynow
        /// </summary>
        /// <param name="url">The pollurl supplied in the response from Paynow after the transaction was initiated</param>
        /// <exception cref="HashMismatchException"></exception>
        public StatusResponse PollTransaction(string url)
        {
            return PollTransactionAsync(url).Result;
        }

        /// <summary>
        /// Get a transaction status from Paynow
        /// </summary>
        /// <param name="url">The pollurl supplied in the response from Paynow after the transaction was initiated</param>
        /// <exception cref="HashMismatchException"></exception>
        public async Task<StatusResponse> PollTransactionAsync(string url)
        {
            var response = await Client.PostAsync(url);
            var data = HttpUtility.ParseQueryString(response).ToDictionary();

            if (!data.ContainsKey("hash") || !Hash.Verify(data, IntegrationKey)) throw new HashMismatchException();

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

            if (!data.ContainsKey("hash") || !Hash.Verify(data, IntegrationKey)) throw new HashMismatchException();

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
            if (!response.ContainsKey("hash") || !Hash.Verify(response, IntegrationKey))
                throw new HashMismatchException();

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
        public async Task<InitResponse> SendMobileAsync(Payment payment, string phone, string method)
        {
            if (string.IsNullOrEmpty(payment.Reference))
            {
                throw new InvalidReferenceException();
            }

            if (payment.Total <= decimal.Zero)
            {
                throw new EmptyCartException();
            }

            if (string.IsNullOrEmpty(payment.AuthEmail))
            {
                throw new ArgumentException(
                    "When creating a mobile payment, please make sure you pass the auth email as the second parameter to the CreatePayment method",
                    nameof(payment));
            }

            return await InitMobileAsync(payment, phone, method);
        }

        /// <summary>
        /// Initiate a new Paynow mobile transaction
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="phone"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private InitResponse InitMobile(Payment payment, string phone, string method)
        {
            return InitMobileAsync(payment, phone, method).Result;
        }

        /// <summary>
        /// Initiate a new Paynow mobile transaction
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="phone"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private async Task<InitResponse> InitMobileAsync(Payment payment, string phone, string method)
        {
            var data = FormatMobileInitRequest(payment, phone, method);

            var response = await Client.PostAsync(PaynowBaseUrl + Constants.UrlInitiateMobileTransaction, data);

            var parsedResponse = HttpUtility.ParseQueryString(response).ToDictionary();

            if (!parsedResponse.ContainsKey("status"))
                throw new Exception("An unknown error occured while querying Paynow API");


            if (parsedResponse["status"].ToLower() != "error" &&
                (!parsedResponse.ContainsKey("hash") || !Hash.Verify(parsedResponse, IntegrationKey)))
                throw new HashMismatchException();

            return new InitResponse(parsedResponse);
        }

        /// <summary>
        /// Initiate a new Paynow transaction
        /// </summary>
        /// <param name="payment"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private InitResponse Init(Payment payment)
        {
            return InitAsync(payment).Result;
        }

        /// <summary>
        /// Initiate a new Paynow transaction
        /// </summary>
        /// <param name="payment"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private async Task<InitResponse> InitAsync(Payment payment)
        {
            var data = FormatInitRequest(payment);

            var response = await Client.PostAsync(PaynowBaseUrl + Constants.UrlInitiateTransaction, data);

            var parsedResponse = HttpUtility.ParseQueryString(response).ToDictionary();

            if (!parsedResponse.ContainsKey("status"))
                throw new Exception("An unknown error occured while querying Paynow API");

            if (parsedResponse["status"].ToLower() != "error" &&
                (!parsedResponse.ContainsKey("hash") || !Hash.Verify(parsedResponse, IntegrationKey)))
                throw new HashMismatchException();

            return new InitResponse(parsedResponse);
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
        private Dictionary<string, string> FormatMobileInitRequest(Payment payment, string phone, string method)
        {
            var items = payment.ToDictionary();

            items["returnurl"] = ReturnUrl.Trim();
            items["resulturl"] = ResultUrl.Trim();
            items["id"] = IntegrationId;
            items["phone"] = phone;
            items["method"] = method;

            items.Add("hash", Hash.Make(items, IntegrationKey));

            return items;
        }
    }
}