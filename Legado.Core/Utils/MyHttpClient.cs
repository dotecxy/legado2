using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Legado.Core.Utils
{
    public class MyHttpClient : HttpClient
    {

        AsyncRetryPolicy<HttpResponseMessage> retryPolicy;

        public MyHttpClient()
        {
            CreatePolicy();
        }

        public MyHttpClient(HttpMessageHandler handler) : base(handler)
        {
            CreatePolicy();
        }

        public MyHttpClient(HttpMessageHandler handler, bool disposeHandler) : base(handler, disposeHandler)
        {
            CreatePolicy();
        }

        void CreatePolicy()
        {
            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => r.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
                .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            this.retryPolicy = retryPolicy;
        }

        public new async Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            var response = await retryPolicy.ExecuteAsync(() => base.GetAsync(requestUri));
            return response;
        }

        public new async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        {
            var response = await retryPolicy.ExecuteAsync(() => base.PostAsync(requestUri, content));
            return response;
        }

    }
}
