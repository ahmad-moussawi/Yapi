using Newtonsoft.Json;

namespace Yapi
{
    public class Response<T>
    {
        private readonly string content;
        public readonly int StatusCode;

        public bool IsSuccess { get => StatusCode >= 200 && StatusCode < 300; }

        public Response(string content, int statusCode)
        {
            this.content = content;
            this.StatusCode = statusCode;
        }

        public T Json()
        {
            return JsonConvert.DeserializeObject<T>(content);
        }

        public string Raw()
        {
            return content;
        }
    }

    public class Response : Response<dynamic>
    {
        public Response(string content, int statusCode) : base(content, statusCode)
        {

        }

    }
}