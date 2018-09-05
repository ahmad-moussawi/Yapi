using System.Collections.Generic;

namespace Yapi
{
    public class Config
    {
        public string UserAgent { get; set; } = "Yapi CSharp Client";
        public Dictionary<string, IEnumerable<string>> HeadersCommon { get; set; } = new Dictionary<string, IEnumerable<string>>();
        public Dictionary<string, IEnumerable<string>> HeadersGet { get; set; } = new Dictionary<string, IEnumerable<string>>();
        public Dictionary<string, IEnumerable<string>> HeadersPost { get; set; } = new Dictionary<string, IEnumerable<string>>();
        public Dictionary<string, IEnumerable<string>> HeadersPut { get; set; } = new Dictionary<string, IEnumerable<string>>();
        public Dictionary<string, IEnumerable<string>> HeadersDelete { get; set; } = new Dictionary<string, IEnumerable<string>>();

        public Dictionary<string, IEnumerable<string>> HeadersFor(string method)
        {
            method = method.ToLower();

            if (method == "get") return HeadersGet;
            if (method == "post") return HeadersPost;
            if (method == "put") return HeadersPut;
            if (method == "delete") return HeadersDelete;

            // empty
            return new Dictionary<string, IEnumerable<string>>();
        }
    }
}