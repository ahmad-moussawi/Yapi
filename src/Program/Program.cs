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

            var http = new Yapi.Client("https://jsonplaceholder.typicode.com", config);

            var response = await http.Send("get", "todos");

            var todos = response.IsSuccess ? response.Json() : null;

            Console.WriteLine(todos[0]);
        }
    }
}
