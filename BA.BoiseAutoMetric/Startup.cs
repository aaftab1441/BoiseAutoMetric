using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BA.BoiseAutoMetric.Adapters;
using BA.BoiseAutoMetric.Configuration;
using BA.Common.Services;
using Microsoft.Extensions.Logging;
using reCAPTCHA.AspNetCore;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;
using Microsoft.Extensions.Hosting;
using System.IO;
using Akismet;
using System;

namespace BA.BoiseAutoMetric
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddSingleton(
        new AkismetClient(
        (Configuration.GetSection("AkismetApiKey")).ToString(),
        new Uri(Configuration["WebAppUrl"]),
        "spam-killer")
    );

            services.AddMvc(options =>
            { options.EnableEndpointRouting = false; })
                    .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddSingleton<ISmtpService, SmtpService>();
            services.AddRecaptcha(Configuration.GetSection("RecaptchaSettings"));
            //services.AddTransient<IRecaptchaService, RecaptchaService>();
            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Errer");
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseResponseCompression();
            app.UseCookiePolicy();
            app.UseMvc();
            app.UseStatusCodePagesWithRedirects("/errors/{0}");

        }
    }
}
