using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Webdev.Helpers
{
    public static class Hash
    {
        /// <summary>
        ///     Hash the values in the given dictonary
        /// </summary>
        /// <param name="values">Values to value</param>
        /// <param name="integrationKey">Paynow integration key</param>
        /// <returns></returns>
        public static string Make(IDictionary<string, string> values, Guid integrationKey)
        {
            var concat = new StringBuilder();

            // add the value from each key/value pair to get a string we'll use to generate the hash
            foreach (var pair in values)
            {
                // ignore the 'hash' key/value pair if its included in the list because its not used for hash generation
                if (!pair.Key.Equals("hash", StringComparison.CurrentCultureIgnoreCase))
                {
                    concat.Append(pair.Value);
                }
            }

            // append the paynow integration key
            concat.Append(integrationKey.ToString());

            byte[] hash;
            using (var sha = SHA512.Create())
            {
                hash = sha.ComputeHash(Encoding.UTF8.GetBytes(concat.ToString()));
            }

            return GetStringFromHash(hash);
        }


        private static string GetStringFromHash(byte[] hash)
        {
            var result = new StringBuilder();

            // convert the byte array to a string by concatenating the hex value of each byte
            foreach (var t in hash)
            {
                result.Append(t.ToString("X2"));
            }

            return result.ToString();
        }

        public static bool Verify(IDictionary<string, string> data, Guid integrationKey)
        {
            return Make(data, integrationKey) == data["hash"];
        }
    }
}