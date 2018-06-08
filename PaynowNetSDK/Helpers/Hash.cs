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
            // TODO: Use StringBuilder for improved efficiency
            var concat = values.Aggregate("",
                (accumulator, pair) => accumulator += pair.Key.ToLower() != "hash" ? pair.Value : "");

            concat += integrationKey.ToString();

            var hash = new byte[0];
            using (var sha = SHA512.Create())
            {
                hash = sha.ComputeHash(Encoding.UTF8.GetBytes(concat));
            }

            return GetStringFromHash(hash);
        }


        private static string GetStringFromHash(byte[] hash)
        {
            var result = new StringBuilder();
            foreach (var t in hash) result.Append(t.ToString("X2"));
            return result.ToString();
        }

        public static bool Verify(IDictionary<string, string> data, Guid integrationKey)
        {
            return Make(data, integrationKey) == data["hash"];
        }
    }
}