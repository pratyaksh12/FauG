using System;
using System.Text;
using System.Text.RegularExpressions;
using FauG.Gateway.Core.Services;

namespace FauG.Gateway.Core.Middleware;

public class SecurityMiddleware(RequestDelegate next, JailbreakService scanner)
{
    private readonly string EmailRegex = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b";

    public async Task InvokeAsync(HttpContext context)
    {
        if(context.Request.Method == HttpMethods.Post && context.Request.Path.StartsWithSegments("/v1/chat/completions"))
        {
            context.Request.EnableBuffering();

            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen:true);
            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            if (scanner.IsJailBreak(body))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("prompt injection detected. Jailbreak attempt discovered by qualfire's Sentinel model.");
                return;
            }

            if(Regex.IsMatch(body, EmailRegex, RegexOptions.IgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("emails are not suppose to be present in the prompts");

                return;
            }
        }

        await next(context);
    }
}
