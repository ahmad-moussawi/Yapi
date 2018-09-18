using System;
using System.Threading.Tasks;
using Yapi;

namespace Program
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        async static Task MainAsync(string[] args)
        {
            var http = new Yapi.Client("https://jsonplaceholder.typicode.com");

            http.HeadersCommon.Add("Content-Type", new[] { "application/json" });
            http.HeadersCommon.Add("Ballout", new[] { "Ballout Value" });
            http.HeadersGet.Add("Content-Type", new[] { "application/bson" });

            http.Debug = true;

            var response = await http.Send<dynamic>("get", "todos/1");

        }
    }
}
