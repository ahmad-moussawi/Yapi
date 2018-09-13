using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Yapi
{
    public class Client
    {
        protected HttpClient http = new HttpClient();

        public Config DefaultConfig;
        public Dictionary<string, IEnumerable<string>> HeadersCommon { get; set; } = new Dictionary<string, IEnumerable<string>>();
        public Dictionary<string, IEnumerable<string>> HeadersGet { get; set; } = new Dictionary<string, IEnumerable<string>>();
        public Dictionary<string, IEnumerable<string>> HeadersPost { get; set; } = new Dictionary<string, IEnumerable<string>>();
        public Dictionary<string, IEnumerable<string>> HeadersPut { get; set; } = new Dictionary<string, IEnumerable<string>>();
        public Dictionary<string, IEnumerable<string>> HeadersDelete { get; set; } = new Dictionary<string, IEnumerable<string>>();

        public Dictionary<string, IEnumerable<string>> HeadersFor(string method)
        {
            method = method.ToUpper();

            if (method == "GET") return HeadersGet;
            if (method == "POST") return HeadersPost;
            if (method == "PUT") return HeadersPut;
            if (method == "DELETE") return HeadersDelete;

            // empty
            return new Dictionary<string, IEnumerable<string>>();
        }

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

            HeadersCommon.Add("User-Agent", new[] { config.UserAgent });

            return this;
        }

        public async Task<Response<T>> Send<T>(
            string method,
            string url = "",
            Config config = null
        )
        {

            config = mergeConfig(config, DefaultConfig);

            method = method.ToUpper();

            var request = new HttpRequestMessage
            {
                Method = new HttpMethod(method),
                RequestUri = new Uri(http.BaseAddress, buildUrl(url, config.Query)),
            };

            if (config.Data != null)
            {
                var data = config.Data;

                var jsonSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                };

                var json = JsonConvert.SerializeObject(data, jsonSettings);

                request.Content = new StringContent(json);
            }

            // add common headers
            foreach (var header in HeadersCommon)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            // add request specific headers
            foreach (var header in HeadersFor(method))
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (config.Headers != null)
            {
                var headers = config.Headers;

                foreach (var header in headers)
                {
                    if (request.Headers.Contains(header.Key))
                    {
                        request.Headers.Remove(header.Key);
                    }

                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            if (config.OnBeforeSend != null)
            {
                config.OnBeforeSend(request, DefaultConfig);
            }

            var rawResponse = await http.SendAsync(request);

            var content = await rawResponse.Content.ReadAsStringAsync();

            foreach (var transformer in config.ResponseTransformers)
            {
                content = transformer(content);
            }

            var response = new Response<T>(content, (int)rawResponse.StatusCode);

            return response;

        }

        public async Task<Response<T>> Get<T>(string url = "", object query = null, Config config = null)
        {
            config = config ?? new Config { Query = query };

            return await Send<T>("GET", url, config);
        }

        public async Task<Response<T>> Post<T>(string url = "", object data = null, Config config = null)
        {
            config = config ?? new Config { Data = data };

            return await Send<T>("POST", url, config);
        }


        private Config mergeConfig(Config original, Config defaultConfig)
        {
            if (original == null)
            {
                return defaultConfig;
            }

            if (defaultConfig == null)
            {
                return original;
            }

            var config = new Config();

            config.Timeout = original.Timeout > 0 ? original.Timeout : defaultConfig.Timeout;

            config.Query = original.Query ?? defaultConfig.Query;
            config.Data = original.Data ?? defaultConfig.Data;

            foreach (var header in original.Headers)
            {
                config.Headers.Add(header.Key, header.Value);
            }

            foreach (var header in defaultConfig.Headers)
            {
                if (!config.Headers.ContainsKey(header.Key))
                {
                    config.Headers[header.Key] = header.Value;
                }
            }

            config.OnBeforeSend = original.OnBeforeSend ?? defaultConfig.OnBeforeSend;
            config.ResponseTransformers = original.ResponseTransformers ?? defaultConfig.ResponseTransformers;

            config.UserAgent = string.IsNullOrEmpty(original.UserAgent) ? defaultConfig.UserAgent : original.UserAgent;

            return config;
        }

        private string buildUrl(string url, object query)
        {
            if (query == null)
            {
                return url;
            }

            if (string.IsNullOrEmpty(url))
            {
                return url;
            }

            var arr = new List<string> { };

            foreach (var prop in query.GetType().GetProperties())
            {
                var key = System.Uri.EscapeDataString(prop.Name);
                var value = System.Uri.EscapeDataString(prop.GetValue(query).ToString());
                arr.Add($"{key}={value}");
            }

            var joiner = url.Contains("?") ? "" : "?";

            return url + joiner + string.Join("&", arr);
        }

    }
}
