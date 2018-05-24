using System;
using System.Collections.Generic;
using Paynow.Exceptions;

namespace Paynow.Core
{
    public class InitResponse : CanFail
    {
        protected IDictionary<string, string> Data { get; }
        
        protected bool WasSuccessful { get; set; }
        
        protected bool HasRedirect { get; set; }

        /// <summary>
        /// InitResponse constructor.
        /// </summary>
        /// <param name="response">Response data sent from Paynow</param>
        /// <exception cref="InvalidIntegrationException">If the error returned from paynow is</exception>
        public InitResponse(IDictionary<string, string> response)
        {
            this.Data = response;

            this.Load();
        }
        
        /// <summary>
        /// Reads through the response data sent from Paynow
        /// </summary>
        private void Load()
        {
            if (this.Data.ContainsKey("status"))
            {
                this.WasSuccessful = this.Data["status"].ToLower() == Constants.ResponseOk;
            }

            if (this.Data.ContainsKey("browserurl"))
            {
                this.HasRedirect = true;
            }

            if (this.WasSuccessful) return;
            
            if (this.Data.ContainsKey("error"))
            {
                this.Fail(this.Data["error"]);
            }
        }

        /// <summary>
        /// Returns the poll URL sent from Paynow
        /// </summary>
        /// <returns></returns>
        public string PollUrl()
        {
            return this.Data.ContainsKey("pollurl") ? this.Data["pollurl"] : "";
        }

        
        /// <summary>
        /// Gets a boolean indicating whether a request succeeded or failed
        /// </summary>
        /// <returns></returns>
        public bool Success()
        {
            return this.WasSuccessful;
        }

        /// <summary>
        /// Returns the url the user should be taken to so they can make a payment
        /// </summary>
        /// <returns></returns>
        public string RedirectLink()
        {
            return this.HasRedirect ? this.Data["browserurl"] : string.Empty;
        }

        /// <summary>
        /// Get the original data sent from Paynow
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> GetData()
        {
            return this.Data;
        }
    }
}