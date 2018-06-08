using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Webdev.Http
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
        public string PostAsync(string url, Dictionary<string, string> data = null)
        {
            var content = new FormUrlEncodedContent(data ?? new Dictionary<string, string>());

            var response = _client.PostAsync(url, content).Result;

            return response.Content.ReadAsStringAsync().Result;
        }
    }
}