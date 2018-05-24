using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Paynow.Http
{
    public class Client
    {
        private readonly HttpClient _client;

        public Client()
        {
            _client = new HttpClient();
        }

        /// <summary>
        ///     /
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<string> PostAsync(string url, Dictionary<string, string> data = null)
        {
            var content = new FormUrlEncodedContent(data ?? new Dictionary<string, string>());

            var response = await _client.PostAsync(url, content);

            return await response.Content.ReadAsStringAsync();
        }
    }
}