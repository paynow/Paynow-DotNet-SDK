using System;
using System.Collections.Generic;
using Paynow.Exceptions;

namespace Paynow.Core
{
    public class StatusResponse : CanFail, IResponse
    {
        protected IDictionary<string, string> Data { get; }

        protected bool WasSuccessful { get; set; }

        public string Reference { get; set; }

        public decimal Amount { get; set; }

        public bool WasPaid { get; set; }

        /// <summary>
        /// InitResponse constructor.
        /// </summary>
        /// <param name="response">Response data sent from Paynow</param>
        /// <exception cref="InvalidIntegrationException">If the error returned from paynow is</exception>
        public StatusResponse(IDictionary<string, string> response)
        {
            Data = response;

            Load();
        }

        /// <summary>
        /// Reads through the response data sent from Paynow
        /// </summary>
        private void Load()
        {
            if (!Data.ContainsKey("error"))
                WasSuccessful = true;

            if (Data.ContainsKey("status"))
            {
                WasPaid = Data["status"].ToLower() == Constants.ResponsePaid;
            }

            if (Data.ContainsKey("amount"))
            {
                Amount = Convert.ToDecimal(Data["amount"]);
            }

            if (Data.ContainsKey("reference"))
            {
                Reference = (Data["reference"]);
            }

            if (WasSuccessful) return;

            if (Data.ContainsKey("error"))
            {
                Fail(Data["error"]);
            }
        }

        /// <summary>
        /// Returns the poll URL sent from Paynow
        /// </summary>
        /// <returns></returns>
        public string PollUrl()
        {
            return Data.ContainsKey("pollurl") ? Data["pollurl"] : "";
        }


        /// <summary>
        /// Gets a boolean indicating whether a request succeeded or failed
        /// </summary>
        /// <returns></returns>
        public bool Success()
        {
            return WasSuccessful;
        }

        public bool Paid()
        {
            return WasPaid;
        }


    /// <summary>
        /// Get the original data sent from Paynow
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> GetData()
        {
            return Data;
        }
    }
}