using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LineBOT.Models.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

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
                    policy.WithOrigins("http://localhost:9528", "http://localhost:54264", "http://192.168.88.65:80")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });
            #endregion

            #region 常用項目
            //錯誤代碼設定
            ErrorMessageOptions.Options = Configuration.GetSection("ErrorCodeSettings").Get<ErrorMessageOptions>();
            #endregion

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
