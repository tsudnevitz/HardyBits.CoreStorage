using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using CoreStorage.Extensions;
using CoreStorage.Models;
using CoreStorage.Services;
using CoreStorage.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CoreStorage
{
  public class Startup
  {
    private IContainer ApplicationContainer { get; set; }
    private IConfigurationRoot Configuration { get; }

    public Startup(IHostingEnvironment env)
    {
      var builder = new ConfigurationBuilder()
        .SetBasePath(env.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
        .AddEnvironmentVariables();

      Configuration = builder.Build();
    }

    public IServiceProvider ConfigureServices(IServiceCollection services)
    {
      services.AddMvc();

      var builder = new ContainerBuilder();
      builder.RegisterType<FileChunkValidator>().As<IValidator<UploadInfo>>().SingleInstance();
      builder.RegisterType<UploadingFileCatalog>().As<IUploadingFileCatalog>().SingleInstance();
      builder.Populate(services);

      ApplicationContainer = builder.Build();
      return new AutofacServiceProvider(ApplicationContainer);
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      loggerFactory.AddConsole(Configuration.GetSection("Logging"));
      loggerFactory.AddDebug();
      
      app.UseWebSockets();
      app.UseWebSocketsManager(ApplicationContainer);
      app.Use(async (http, next) =>
      {
        if (http.WebSockets.IsWebSocketRequest)
        {
          var webSocket = await http.WebSockets.AcceptWebSocketAsync();
          while (webSocket.State == WebSocketState.Open)
          {
            var token = CancellationToken.None;
            var buffer = new ArraySegment<Byte>(new Byte[4096]);
            var received = await webSocket.ReceiveAsync(buffer, token);

            switch (received.MessageType)
            {
              case WebSocketMessageType.Text:
                var request = Encoding.UTF8.GetString(buffer.Array,
                  buffer.Offset,
                  buffer.Count);
                var type = WebSocketMessageType.Text;
                var data = Encoding.UTF8.GetBytes("Echo from server :" + request);
                buffer = new ArraySegment<Byte>(data);
                await webSocket.SendAsync(buffer, type, true, token);
                break;
            }
          }
        }
        else
        {
          await next();
        }
      });
      app.UseMvc();
    }
  }

  public interface IWebSocketsManager
  {
    void Add(WebSocket webSocket);
  }
}
