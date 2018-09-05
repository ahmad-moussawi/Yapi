using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;

namespace Yapi
{
    public class Client
    {
        protected HttpClient http = new HttpClient();

        public Client(string baseUrl)
        {
            http.BaseAddress = new Uri(baseUrl);
        }

        public async Task<Response> Send(
            string method,
            string url,
            object query,
            object data,
            Dictionary<string, IEnumerable<string>> headers
        )
        {

            var request = new HttpRequestMessage();

            request.Method = new HttpMethod(method);

            if (query != null)
            {
                var dict = new Dictionary<string, string>();

                foreach (var prop in query.GetType().GetProperties())
                {
                    dict.Add(prop.Name, prop.GetValue(query).ToString());
                }

                url = QueryHelpers.AddQueryString(url, dict);

            }

            request.RequestUri = new Uri(url);

            if (data != null)
            {
                request.Content = new StringContent(JsonConvert.SerializeObject(data, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                }));
            }

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }


            var rawResponse = await http.SendAsync(request);

            var content = await rawResponse.Content.ReadAsStringAsync();

            var response = new Response(content, (int)rawResponse.StatusCode);

            return response;

        }

    }
}
