using System;
using System.Net;
using System.Threading.Tasks;
using Authentication.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Authentication.Api.Handlers
{
    public class ServiceExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ServiceExceptionHandler> _logger;

        public ServiceExceptionHandler(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = loggerFactory?.CreateLogger<ServiceExceptionHandler>() ??
                      throw new ArgumentNullException(nameof(loggerFactory));
        }

        // TODO: look into possibly extracting out error logging method

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ServiceExceptionBase ex)
            {
                if (context.Response.HasStarted)
                {
                    _logger.LogWarning("The response has already started, the http status code middleware will not be executed.");
                    throw;
                }

                _logger.LogError(ex, ex.Message);

                context.Response.Clear();
                context.Response.StatusCode = ex.ResponseCode;

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new ErrorResponse
                {
                    ErrorCode = ex.ErrorCode,
                    ErrorMessage = ex.Message,
                    StatusCode = ex.ResponseCode
                }, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }));
            }
            catch (NotImplementedException ex)
            {
                _logger.LogError(ex, ex.Message);

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync("Method has not been implemented. Please contact support.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                context.Response.Clear();
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonConvert.SerializeObject(new ErrorResponse
                {
                    ErrorMessage = ex.Message,
                    StatusCode = (int)HttpStatusCode.InternalServerError
                }, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }));
            }
        }
    }
}