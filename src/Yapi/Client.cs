using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Yapi
{
    public class Client
    {
        protected HttpClient http = new HttpClient();
        public Config DefaultConfig;
        public bool Debug { get; set; } = false;
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

            HeadersCommon["User-Agent"] = new[] { config.UserAgent };

            return this;
        }

        public async Task<Response<T>> Send<T>(
            string method,
            string url = "",
            Config config = null,
            bool isJson = true
        )
        {

            config = mergeConfig(config, DefaultConfig);
            method = method.ToUpper();

            // all headers
            var headers = new Dictionary<string, string>();
            var addedHeaders = new HashSet<string>();

            var request = new HttpRequestMessage
            {
                Method = new HttpMethod(method),
                RequestUri = new Uri(http.BaseAddress, buildUrl(url, config.Query)),
            };

            if (config.Data != null)
            {
                var data = config.Data;

                if (data is string d)
                {
                    // if the developer has provided a string so pass it as is
                    request.Content = new StringContent(d);
                }
                else if (isJson)
                {

                    var jsonSettings = new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    };

                    var json = JsonConvert.SerializeObject(data, jsonSettings);

                    request.Content = new StringContent(json);
                }
                else
                {
                    // here we assume that the data is of type application/x-www-form-urlencoded

                    var body = buildUrl("", data);

                    request.Content = new StringContent(body);
                }
            }
            else
            {
                // always set a content to allow Content Headers even if no real content was provided
                // i.e. in GET requests
                request.Content = new StringContent("");
            }

            // add common headers first
            foreach (var header in HeadersCommon)
            {
                headers[header.Key.ToLower()] = string.Join(",", header.Value);
            }

            // add method specific headers
            foreach (var header in HeadersFor(method))
            {
                headers[header.Key.ToLower()] = string.Join(",", header.Value);
            }

            // add request specific headers
            if (config.Headers != null)
            {
                foreach (var header in config.Headers)
                {
                    headers[header.Key.ToLower()] = string.Join(",", header.Value);
                }
            }

            if (!headers.ContainsKey("content-type"))
            {
                headers["content-type"] = isJson ? "application/json" : "application/x-www-form-urlencoded";
            }

            foreach (var header in headers)
            {
                // try to add the header for the request first
                var added = request.Headers.TryAddWithoutValidation(header.Key, header.Value);

                // if not available for the request, then add it for the content
                if (!added)
                {
                    request.Content.Headers.Remove(header.Key);
                    request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            if (config.OnBeforeSend != null)
            {
                config.OnBeforeSend(request, headers, config);
            }


            if (Debug)
            {
                Console.WriteLine($"{request.Method} {request.RequestUri}");

                foreach (var item in headers)
                {
                    Console.WriteLine($"{item.Key}: {string.Join(", ", item.Value)}");
                }

                Console.WriteLine("\n" + (await request.Content.ReadAsStringAsync()));
            }

            var rawResponse = await http.SendAsync(request);

            var content = await rawResponse.Content.ReadAsStringAsync();

            if (Debug)
            {
                Console.WriteLine();
                foreach (var item in rawResponse.Headers)
                {
                    Console.WriteLine($"{item.Key}: {string.Join(", ", item.Value)}");
                }

                Console.WriteLine("\n" + content + "\n");
            }

            foreach (var transformer in config.ResponseTransformers)
            {
                content = transformer(content);
            }

            var response = new Response<T>(content, (int)rawResponse.StatusCode);

            return response;

        }

        public async Task<Response<T>> Get<T>(string url = "", object query = null, Config config = null)
        {
            config = config ?? new Config();
            config.Query = query;

            return await Send<T>("GET", url, config);
        }

        public async Task<Response<T>> Post<T>(string url = "", object data = null, Config config = null)
        {
            config = config ?? new Config();
            config.Data = data;

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

            url = url ?? "";

            var arr = new List<string> { };

            foreach (var prop in query.GetType().GetRuntimeProperties())
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
