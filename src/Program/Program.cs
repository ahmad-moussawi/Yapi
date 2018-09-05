using System;
using System.Threading.Tasks;

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

            var response = await http.Send("get", "todos");

            var todos = response.IsSuccess ? response.Json() : null;

            Console.WriteLine(todos[0]);
        }
    }
}
