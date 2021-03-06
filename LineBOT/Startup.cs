using System;
using System.Collections.Generic;
using System.Linq;
using LineBOT.Models.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LineBOT
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
            #region 啟用跨域請求 Cross-Origin Requests (CORS)
            //參考項目 https://blog.johnwu.cc/article/ironman-day26-asp-net-core-cross-origin-requests.html
            //官方文件 https://docs.microsoft.com/zh-tw/aspnet/core/security/cors?view=aspnetcore-3.1
            services.AddCors(options =>
            {
                // CorsPolicy 是自訂的 Policy 名稱
                options.AddPolicy("CorsPolicy", policy =>
                {
                    //指定網域才可進入讀取資料
                    policy
                    //.WithOrigins("http://localhost:9528", "http://localhost:54264", "http://192.168.88.65:80")
                          .AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                          //.AllowCredentials();
                });
            });
            #endregion

            services.Configure<LineBotSetting>(Configuration.GetSection("LineSettings"));
            services.Configure<WebSocketConnectOptions>(Configuration.GetSection("WebSocketConnectOptions"));

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            #region 啟用跨域請求 Cross-Origin Requests (CORS)
            //CORS設定 - 全域套用
            app.UseCors("CorsPolicy");
            /*
             * CORS區域套用
             * 請在Controller 或 Action 掛上
             * [EnableCors("CorsPolicy")]
             */
            #endregion

            //判定使用環境
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseExceptionHandler("/error"); //使Exception集中至ErrorControllers管理
            }
            else
            {
                app.UseExceptionHandler("/error-local-development"); //使Exception集中至ErrorControllers管理
            }

            app.UseRouting();

            //使用驗證
            app.UseAuthentication();
            //放在UseAuthentication之後
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
