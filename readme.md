![Yapi Logo](./logo.png)


# Yapi Http Client
A lightweight Http Client for C#, built on top of the `HttpClient` class.

This library was built to remove the strict restrictions in the HttpClient shipped with dotnet.

# Installation
```
dotnet add package Yapi
```

# Usage
```cs
 var http = new Yapi.Client("https://jsonplaceholder.typicode.com");

// Add default headers for all requests
http.HeadersCommon.Add("Content-Type", new[] { "application/json" });

// Add default headers for GET requests only
http.HeadersGet.Add("Content-Type", new[] { "application/bson" });

// Log request/response in the console
http.Debug = true;

// invoke the request
var response = await http.Get<Todo>("todos/1");

response.Then(todo =>
{
    Console.WriteLine($"getting todo #: {todo.Id}");
})
.Catch(err =>
{
    Console.WriteLine("Request failed, more details : " + response.Raw());
})
.Finally(r =>
{
    // log repsonse
});
```