using System;
using System.Text;
using System.Text.Json;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace FauG.Gateway.Core.Middleware;

public class RoutingTransformProvider : ITransformProvider
{
    public void Apply(TransformBuilderContext context)
    {
        if(context.Route.RouteId == "ai-route")
        {
            context.AddRequestTransform(async transformContext =>
            {
                var httpContext = transformContext.HttpContext;

                // intercept if there is a body
                if(httpContext.Request.Method == HttpMethods.Post && httpContext.Request.ContentType.StartsWith("application/json"))
                {
                    httpContext.Request.Body.Position = 0;
                    using var reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8, leaveOpen:true);
                    var body = await reader.ReadToEndAsync();

                    if (!string.IsNullOrEmpty(body))
                    {
                        var json = JsonDocument.Parse(body);

                        if(json.RootElement.TryGetProperty("model", out var modelProperty))
                        {
                            string modelName = modelProperty.GetString() ?? "";

                            // routing
                            // proxying the request based off of the model name
                            // will replace with the properties from the database and the actual key provider
                            if(modelName.Contains("llama", StringComparison.InvariantCultureIgnoreCase) || modelName.Contains("mixtral", StringComparison.InvariantCultureIgnoreCase))
                            {
                                transformContext.ProxyRequest.RequestUri = new Uri("https://api.groq.com/openai/v1/chat/completions");
                                transformContext.ProxyRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "I_DON_KNOW_BUT_WILL_IMPLEMENT_USING_DATABASE");
                            }
                            if(modelName.Contains("gpt", StringComparison.InvariantCultureIgnoreCase))
                            {
                                transformContext.ProxyRequest.RequestUri = new Uri("https://api.openai.com/v1/chat/completions");
                                transformContext.ProxyRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer:", "AGAIN_WILL_)LOAD_THE_VALUE_FROM_CACHE_OR_DATABASE");
                            }
                        }
                    }
                }
            });
        }
    }

    public void ValidateCluster(TransformClusterValidationContext context)
    {
        // not needed
    }

    public void ValidateRoute(TransformRouteValidationContext context)
    {
        // not needed
    }
}
