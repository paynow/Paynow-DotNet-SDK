using System.Collections.Generic;
using System.Linq;
using Paynow.Exceptions;

namespace Paynow.Core
{
    public abstract class CanFail
    {
        private List<string> _errors = new List<string>();

        /// <summary>
        /// Throws an exception for critical errors and stores other non-critical errors
        /// </summary>
        /// <param name="error"></param>
        /// <exception cref="InvalidIntegrationException"></exception>
        public void Fail(string error)
        {
            switch (error)
            {
                case Constants.ResponseInvalidId:
                    throw new InvalidIntegrationException();
                default:
                    this._errors.Add(error);
                    break;
            }
        }

        /// <summary>
        /// Get the errors sent by Paynow
        /// </summary>
        /// <param name="separator"></param>
        /// <returns></returns>
        public string Errors(char separator = ',')
        {
            return this._errors
                .Aggregate(string.Empty,
                    (accumulator, value) => accumulator += string.Format("{0}{1} ", value, separator)).Trim();
        }

    }
}