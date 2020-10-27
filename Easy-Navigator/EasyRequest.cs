using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;

namespace Easy.Navigator
{
    public class EasyRequest
    {
        public string Token { get; set; } = string.Empty;
        public string SystemSource { get; set; } = string.Empty;

        private void Configure(HttpClient httpClient)
        {
            if (!string.IsNullOrWhiteSpace(this.Token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.Token);
            }

            if (!string.IsNullOrWhiteSpace(this.SystemSource))
            {
                httpClient.DefaultRequestHeaders.Add("SystemSource", this.SystemSource);
            }
        }

        public HttpResponseMessage Get(string url)
        {
            using (var httpClient = new HttpClient())
            {
                this.Configure(httpClient);

                return httpClient.GetAsync(url).Result;
            }
        }

        public HttpResponseMessage Send(object entity, string url)
        {
            using (var httpClient = new HttpClient())
            {
                this.Configure(httpClient);

                StringContent content = null;

                if (entity.GetType().Equals(typeof(string)))
                {
                    content = new StringContent(Convert.ToString(entity), Encoding.UTF8, "application/json");
                }
                else
                {
                    content = new StringContent(JsonConvert.SerializeObject(entity), Encoding.UTF8, "application/json");
                }

                return httpClient.PostAsync(url, content).Result;
            }
        }
    }
}
