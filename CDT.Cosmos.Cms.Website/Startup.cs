using System;
using System.Security.Authentication;
using CDT.Cosmos.Cms.Common.Data;
using CDT.Cosmos.Cms.Common.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;

namespace CDT.Cosmos.Cms.Website
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //
            //  Start C/CMS Publisher Website required items
            //
            services.Configure<SiteCustomizationsConfig>(Configuration.GetSection("SiteCustomizations"));
            var redisSection = Configuration.GetSection("RedisContextConfig");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            //
            // End C/CMS required section
            //

            // For ResettaStone
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS",
                "CA-Response-Portal-bfc617b86937.json");

            services.ConfigureApplicationCookie(o =>
            {
                o.ExpireTimeSpan = TimeSpan.FromDays(5);
                o.SlidingExpiration = true;
            });

            services.AddControllersWithViews();

            //
            //  Start C/CMS Publisher Website required items
            // Implement REDIS and distributed caching only if configured.
            //
            if (redisSection == null) return;

            //https://docs.microsoft.com/en-us/aspnet/core/performance/caching/middleware?view=aspnetcore-3.1
            var redisContext = redisSection.Get<RedisContextConfig>();

            services.Configure<RedisContextConfig>(redisSection);
            var redisOptions = new ConfigurationOptions
            {
                Password = redisContext.Password,
                Ssl = true,
                SslProtocols = SslProtocols.Tls12,
                AbortOnConnectFail = redisContext.AbortConnect
            };
            redisOptions.EndPoints.Add(redisContext.Host, 6380);
            redisOptions.ConnectTimeout = 2000;
            redisOptions.ConnectRetry = 3;
            services.AddStackExchangeRedisCache(options => { options.ConfigurationOptions = redisOptions; });
            services.AddResponseCaching(options => { options.UseCaseSensitivePaths = false; });

            services.AddMvc()
                .AddNewtonsoftJson(options =>
                    options.SerializerSettings.ContractResolver =
                        new DefaultContractResolver());
            //
            // End C/CMS required section
            //
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime,
            IDistributedCache cache)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseStatusCodePagesWithReExecute("/Error/{0}");

                // The default HSTS value is 30 days. You may want to change this for production scenarios,
                // see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            if (cache != null)
                lifetime.ApplicationStarted.Register(() =>
                {
                    var options = new DistributedCacheEntryOptions();
                    //var redisCache = (Microsoft.Extensions.Caching.StackExchangeRedis.RedisCache)cache;
                    cache.Set("cachedTimeUTC", System.Text.Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("u")), options);
                });

            //https://docs.microsoft.com/en-us/aspnet/core/performance/caching/middleware?view=aspnetcore-3.1
            app.UseResponseCaching();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");

                // This route must go last.  A page name can't conflict with any of the above.
                // This route allows page titles to become URLs.
                endpoints.MapControllerRoute(
                    "DynamicPage",
                    "/{id?}/{lang?}",
                    new
                    {
                        controller = "Home",
                        action = "Index"
                    });
            });
        }
    }
}