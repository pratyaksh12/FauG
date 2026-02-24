using System;
using System.Text;
using System.Text.Json;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;
using DotNetEnv;
using FauG.Gateway.Core.Services;

namespace FauG.Gateway.Core.Middleware;

public class RoutingTransformProvider : ITransformProvider
{
    public void Apply(TransformBuilderContext context)
    {
        if(context.Route.RouteId == "ai-routes")
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
                    httpContext.Request.Body.Position = 0;

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
                                var openAiKey = Environment.GetEnvironmentVariable("OPEN_AI_KEY");
                                transformContext.ProxyRequest.RequestUri = new Uri("https://api.openai.com/v1/chat/completions");
                                transformContext.ProxyRequest.Headers.Remove("Authorization");
                                transformContext.ProxyRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", openAiKey);
                            }
                        }
                    }
                }
            });

            context.AddResponseTransform(async responseContext =>
            {
                if(responseContext.ProxyResponse != null && responseContext.ProxyResponse.IsSuccessStatusCode)
                {
                    if(responseContext.HttpContext.Items.TryGetValue("Auth", out var authObject) && authObject is AuthContext authContext)
                    {
                        var bytes = await responseContext.ProxyResponse.Content.ReadAsByteArrayAsync();
                        var responseBody = Encoding.UTF8.GetString(bytes);
                        
                        
                        var newContent = new ByteArrayContent(bytes);
                        foreach(var header in responseContext.ProxyResponse.Content.Headers)
                        {
                            newContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        }
                        responseContext.ProxyResponse.Content = newContent;

                        try
                        {
                            var json = JsonDocument.Parse(responseBody);
                            if(json.RootElement.TryGetProperty("usage", out var usage) && usage.TryGetProperty("total_tokens", out var tokens) && json.RootElement.TryGetProperty("model", out var model))
                            {
                                int totalTokens = tokens.GetInt32();
                                var logger = responseContext.HttpContext.RequestServices.GetRequiredService<RequestLogService>();
                                _ = logger.LogUsageAsync(authContext, model.ToString(), totalTokens, StatusCodes.Status200OK);
                                
                            }
                        }
                        catch
                        {
                            // ignoring the error for now.
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
