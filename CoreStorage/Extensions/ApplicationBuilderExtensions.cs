using Autofac;
using Microsoft.AspNetCore.Builder;

namespace CoreStorage.Extensions
{
  public static class ApplicationBuilderExtensions
  {
    public static IApplicationBuilder UseWebSocketsManager(this IApplicationBuilder app, IContainer container)
    {
      var manager = container.Resolve<IWebSocketsManager>();
      app.Use(async (http, next) =>
      {
        if (http.WebSockets.IsWebSocketRequest)
        {
          var webSocket = await http.WebSockets.AcceptWebSocketAsync();
          manager.Add(webSocket);
        }
        else await next();
      });
      return app;
    }
  }
}