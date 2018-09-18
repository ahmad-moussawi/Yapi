using System;
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

        public Response<T> Then(Action<T> onSuccess)
        {
            if (onSuccess != null && IsSuccess)
            {
                onSuccess(Json());
            }

            return this;
        }

        public Response<T> Catch(Action<T> onFail)
        {
            if (onFail != null && !IsSuccess)
            {
                onFail(Json());
            }

            return this;

        }

        public Response<T> Finally(Action<T> always)
        {
            if (always != null)
            {
                always(Json());
            }

            return this;
        }
    }

    public class Response : Response<dynamic>
    {
        public Response(string content, int statusCode) : base(content, statusCode)
        {

        }

    }
}