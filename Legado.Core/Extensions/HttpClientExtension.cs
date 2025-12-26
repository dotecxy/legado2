using Microsoft.Extensions.DependencyInjection;
using Polly;
using System;
using System.Collections.Generic;
using System.Text;

namespace Legado.Core.Extensions
{
    public static class HttpClientExtension
    {
        public static void AddHttpPollyClient(this IServiceCollection services)
        {
            services.AddHttpClient("PollyWaitAndRetry")
            .AddTransientHttpErrorPolicy(policyBuilder =>
            policyBuilder.WaitAndRetryAsync(
             retryCount: 3,
            retryNumber => TimeSpan.FromMilliseconds(600)));
        }
    }
}
