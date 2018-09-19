using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
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

            http.Debug = true;

            var response = await http.Send<dynamic>("get", "todos/1");

            response.Then(r =>
            {
                Console.WriteLine("Response success");
                Console.WriteLine(JsonConvert.SerializeObject(r));
            })
            .Catch(err =>
            {
                Console.WriteLine("Response failed");
                Console.WriteLine(JsonConvert.SerializeObject(err));
            })
            .Finally(r =>
            {
                // log repsonse
            });


        }
    }
}
