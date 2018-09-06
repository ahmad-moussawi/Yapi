using System.Collections.Generic;

namespace Yapi
{
    public class Config
    {
        public string UserAgent { get; set; } = "Yapi CSharp Client";
        public int Timeout { get; set; } = 30000;

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
    }
}