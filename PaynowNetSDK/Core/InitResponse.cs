using System.Collections.Generic;
using Webdev.Exceptions;

namespace Webdev.Core
{
    public class InitResponse : CanFail
    {
        /// <summary>
        /// InitResponse constructor.
        /// </summary>
        /// <param name="response">Response data sent from Paynow</param>
        /// <exception cref="InvalidIntegrationException">If the error returned from paynow is</exception>
        public InitResponse(IDictionary<string, string> response)
        {
            Data = response;

            Load();
        }

        protected IDictionary<string, string> Data { get; }

        protected bool WasSuccessful { get; set; }

        protected bool HasRedirect { get; set; }

        /// <summary>
        /// Reads through the response data sent from Paynow
        /// </summary>
        private void Load()
        {
            if (Data.ContainsKey("status")) WasSuccessful = Data["status"].ToLower() == Constants.ResponseOk;

            if (Data.ContainsKey("browserurl")) HasRedirect = true;

            if (WasSuccessful) return;

            if (Data.ContainsKey("error")) Fail(Data["error"]);
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

        /// <summary>
        /// Returns the url the user should be taken to so they can make a payment
        /// </summary>
        /// <returns></returns>
        public string RedirectLink()
        {
            return HasRedirect ? Data["browserurl"] : string.Empty;
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