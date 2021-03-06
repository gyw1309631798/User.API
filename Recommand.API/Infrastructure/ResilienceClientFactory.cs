﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Polly;
using Resilience;

namespace Recommand.API.Infrastructure
{
    public class ResilienceClientFactory
    {

        private ILogger<ResilienceClientFactory> _logger;
        private readonly ILogger<ResilienceHttpClient> _loggerHttpClient;
        private IHttpContextAccessor _httpContextAccessor;
        //重试次数
        private int _retryCount;
        //熔断之前允许的异常次数
        private int _exceptionCountAllowedBeforeBreaking;

        public ResilienceClientFactory(ILogger<ResilienceClientFactory> logger
            , IHttpContextAccessor httpContextAccessor
            , int retryCount
            , int exceptionCountAllowedBeforeBreaking
            , ILogger<ResilienceHttpClient> loggerHttpClient)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _retryCount = retryCount;
            _exceptionCountAllowedBeforeBreaking = exceptionCountAllowedBeforeBreaking;
            _loggerHttpClient = loggerHttpClient;
        }



        public ResilienceHttpClient GetResilienceHttpClient() =>
            new ResilienceHttpClient("recommand_api", CreatePolicy, _loggerHttpClient, _httpContextAccessor);



        private Policy[] CreatePolicy(string origin)
        {
            return new Policy[]
            {
        Policy.Handle<HttpRequestException>()
          .WaitAndRetryAsync(
            // number of retries
            _retryCount,
            // exponential backofff
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            // on retry
            (exception, timeSpan, retryCount, context) =>
            {
              var msg = $"第 {retryCount} 次重试 " +
                        $"of {context.PolicyKey} " +
                        $"at {context.ExecutionKey}, " +
                        $"due to: {exception}.";
              _logger.LogWarning(msg);
              _logger.LogDebug(msg);
            }),
        Policy.Handle<HttpRequestException>()
                .CircuitBreakerAsync( 
                   // number of exceptions before breaking circuit
                   _exceptionCountAllowedBeforeBreaking,
                   // time circuit opened before retry
                   TimeSpan.FromMinutes(1),
                   (exception, duration) =>
                   {
                        // on circuit opened
                        _logger.LogWarning("熔断器打开");
                   },
                   () =>
                   {
                        // on circuit closed
                        _logger.LogWarning("熔断器关闭");
                   })

            };


        }



    }
}
