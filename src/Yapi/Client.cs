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

        public Config DefaultConfig;

        public Client(string baseUrl, Config config = null)
        {
            http.BaseAddress = new Uri(baseUrl);

            Configure(config ?? new Config());
        }

        public Client Configure(Config config)
        {
            DefaultConfig = config;

            var timeout = config.Timeout > 0 ? config.Timeout : 5000;

            http.Timeout = TimeSpan.FromMilliseconds(timeout);

            config.HeadersCommon.Add("User-Agent", new[] { config.UserAgent });
            return this;
        }

        public async Task<Response<T>> Send<T>(
            string method,
            string url = "",
            object query = null,
            object data = null,
            Dictionary<string, IEnumerable<string>> headers = null
        )
        {

            var request = new HttpRequestMessage();

            method = method.ToUpper();

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

            request.RequestUri = new Uri(http.BaseAddress, url);

            if (data != null)
            {
                request.Content = new StringContent(JsonConvert.SerializeObject(data, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                }));
            }

            // add common headers
            foreach (var header in DefaultConfig.HeadersCommon)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            // add request specific headers
            foreach (var header in DefaultConfig.HeadersFor(method))
            {
                request.Headers.Add(header.Key, header.Value);
            }

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    if (request.Headers.Contains(header.Key))
                    {
                        request.Headers.Remove(header.Key);
                    }

                    request.Headers.Add(header.Key, header.Value);
                }
            }

            var rawResponse = await http.SendAsync(request);

            var content = await rawResponse.Content.ReadAsStringAsync();

            var response = new Response<T>(content, (int)rawResponse.StatusCode);

            return response;

        }

        public async Task<Response> Send(string method,
            string url = "",
            object query = null,
            object data = null,
            Dictionary<string, IEnumerable<string>> headers = null
        )
        {
            var response = await Send<dynamic>(method, url, query, data, headers);

            return new Response(response.Raw(), response.StatusCode);
        }

        public Task<Response<T>> Get<T>(string url, object query = null, Config config = null, Dictionary<string, IEnumerable<string>> headers = null)
        {
            return Send<T>("GET", url, query, null, headers);
        }

        public Task<Response<T>> Post<T>(string url, object data = null, Config config = null, Dictionary<string, IEnumerable<string>> headers = null)
        {
            return Send<T>("POST", url, null, data, headers);
        }

        public Task<Response<T>> Delete<T>(string url, Config config = null, Dictionary<string, IEnumerable<string>> headers = null)
        {
            return Send<T>("DELETE", url, null, null, headers);
        }

        public Task<Response<T>> Put<T>(string url, object data = null, Config config = null, Dictionary<string, IEnumerable<string>> headers = null)
        {
            return Send<T>("PUT", url, null, data, headers);
        }

        public Task<Response> Get(string url, object query = null, Config config = null, Dictionary<string, IEnumerable<string>> headers = null)
        {
            return Send("GET", url, query, null, headers);
        }

        public Task<Response> Post(string url, object data = null, Config config = null, Dictionary<string, IEnumerable<string>> headers = null)
        {
            return Send("POST", url, null, data, headers);
        }

        public Task<Response> Delete(string url, Config config = null, Dictionary<string, IEnumerable<string>> headers = null)
        {
            return Send("DELETE", url, null, null, headers);
        }

        public Task<Response> Put(string url, object data = null, Config config = null, Dictionary<string, IEnumerable<string>> headers = null)
        {
            return Send("PUT", url, null, data, headers);
        }

    }
}
