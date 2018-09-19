using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Yapi
{
    public class Config
    {
        public string UserAgent { get; set; } = "Yapi CSharp Client";
        public int Timeout { get; set; } = 30000;
        public Action<HttpRequestMessage, Dictionary<string, string>, Config> OnBeforeSend;
        public List<Func<string, string>> ResponseTransformers = new List<Func<string, string>>();
        public object Query { get; set; }
        public object Data { get; set; }
        public Dictionary<string, IEnumerable<string>> Headers { get; set; } = new Dictionary<string, IEnumerable<string>>();

    }
}