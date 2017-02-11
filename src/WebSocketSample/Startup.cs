namespace WebSocket.JogoDaVelha
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //    loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            //  loggerFactory.AddDebug();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvc();
            app.Map("/ws/tab", SocketHandler.MapTab);
            app.Map("/ws/chat", SocketHandler.MapChat);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
             // Add framework services.
            services.AddMvc();

        }
    }

     
}