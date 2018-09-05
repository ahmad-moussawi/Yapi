using Newtonsoft.Json;

namespace Yapi
{
    public class Response
    {
        private readonly string content;
        private readonly int statusCode;

        public bool IsSuccess { get => statusCode >= 200 && statusCode < 300; }

        public Response(string content, int statusCode)
        {
            this.content = content;
            this.statusCode = statusCode;
        }

        public T Json<T>()
        {
            return JsonConvert.DeserializeObject<T>(content);
        }

        public dynamic Json() => Json<dynamic>();

        public string Raw()
        {
            return content;
        }

    }
}