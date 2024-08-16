using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Webhooks.GraphQL {
    static class WebServer {
        public record WebhookRequest(DateTimeOffset Date, string Signature, string Body);

        /// <summary>
        /// Starts a Kestrel web server that listens until cancellation and 
        /// handles all project POSTs with the `handleProjectsMessage` function.
        /// </summary>
        public static async Task<WebApplication> Fly(
            Func<WebhookRequest, Task<IResult>> handleProjectsMessage,
            string[] urls,
            CancellationToken tok
        ) {
            var builder = WebApplication.CreateBuilder();
            builder.Host.UseSerilog(Log.Logger);

            //COREY Customized so I can test more easilly
            builder.WebHost.UseUrls("http://*:5005");

            var app = builder.Build();


            //verifying that nothing is hitting the server at all
            app.Use( next => async context => {

                Log.Information( "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" );
                Log.Information( "Request entered Webhook server" );
                Log.Information( "PATH: " + context.Request.Path );
                Log.Information( "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!" );

                await next( context );
            } );

            app.MapGet("/ping", () => Results.Json(new {
                data = "pong",
                now = DateTime.UtcNow,
            }));
            app.MapPost("/projects",
                async (
                    HttpRequest request
                    //,
                    //[FromHeader(Name = "X-XL-Webhook-Signature")] string sig,
                    //[FromHeader(Name = "Date")] DateTimeOffset date
                ) => {
                    Log.Information("Acessed Hebhook!!!!!!!!!!!!!!!!");
                    Log.Information("Headers: {H}", request.Headers);


                    //Corey's Debugging customization:
                    return Results.Ok();


                    //using var r = new StreamReader(request.Body);
                    //var body = await r.ReadToEndAsync();
                    //var req = new WebhookRequest(date, sig, body);
                    //return await handleProjectsMessage(req);
                });

            await app.StartAsync(tok);
            return app;
        }
    }
}
