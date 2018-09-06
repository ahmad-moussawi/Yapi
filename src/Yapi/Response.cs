using Newtonsoft.Json;

namespace Yapi
{
    public class Response<T>
    {
        private readonly string content;
        private readonly int statusCode;

        public bool IsSuccess { get => statusCode >= 200 && statusCode < 300; }

        public Response(string content, int statusCode)
        {
            this.content = content;
            this.statusCode = statusCode;
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