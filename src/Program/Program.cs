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
            var config = new Config();

            config.OnBeforeSend = (request, c) => {
                foreach (var header in request.Headers)
                {
                    Console.WriteLine(header.Key + ": " + string.Join(", ", header.Value));
                }
            };

            // config.HeadersCommon.Add("Authorization", new [] {"INVALIDAUTH"});

            var http = new Yapi.Client("https://jsonplaceholder.typicode.com", config);

            var response = await http.Send("get", "todos");

            var todos = response.IsSuccess ? response.Json() : null;

            http.DefaultConfig.HeadersCommon["Authorization"] = new [] {"cobSession=08062013_2:1c8b1eb4e029165c8c729b4a4994aa430a586b0190e855984cb54558d13d37ad6cb893545c4555a9cec49aa42d00db2b012226878d8dbdbf16715e87ff76f2f3"};

            var r2 = await http.Send("get", "todos");

            Console.WriteLine(todos[0]);
        }
    }
}
