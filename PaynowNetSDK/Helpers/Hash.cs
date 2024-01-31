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
        /// Hash the values in the given dictonary
        /// </summary>
        /// <param name="values">Values to value</param>
        /// <param name="integrationKey">Paynow integration key</param>
        /// <returns></returns>
        public static string Make(IDictionary<string, string> values, Guid integrationKey)
        {
            string concat = string.Join("",
                values
                    .Where(c => c.Key.ToLowerInvariant() != "hash")
                    .Select(c => c.Value?.Trim() ?? "")
                    .ToArray()
            );

            byte[] hash;
            using (var sha = SHA512.Create())
            {
                hash = sha.ComputeHash(Encoding.UTF8.GetBytes(concat + integrationKey));
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