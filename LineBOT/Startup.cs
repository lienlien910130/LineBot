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
            #region �ҥθ��ШD Cross-Origin Requests (CORS)
            //�ѦҶ��� https://blog.johnwu.cc/article/ironman-day26-asp-net-core-cross-origin-requests.html
            //�x���� https://docs.microsoft.com/zh-tw/aspnet/core/security/cors?view=aspnetcore-3.1
            services.AddCors(options =>
            {
                // CorsPolicy �O�ۭq�� Policy �W��
                options.AddPolicy("CorsPolicy", policy =>
                {
                    //���w����~�i�i�JŪ�����
                    policy.WithOrigins("http://localhost:9528", "http://localhost:54264", "http://192.168.88.65:80")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });
            #endregion

            #region �`�ζ���
            //���~�N�X�]�w
            ErrorMessageOptions.Options = Configuration.GetSection("ErrorCodeSettings").Get<ErrorMessageOptions>();
            #endregion

            services.AddControllers();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            #region �ҥθ��ШD Cross-Origin Requests (CORS)
            //CORS�]�w - ����M��
            app.UseCors("CorsPolicy");
            /*
             * CORS�ϰ�M��
             * �ЦbController �� Action ���W
             * [EnableCors("CorsPolicy")]
             */
            #endregion

            //�P�w�ϥ�����
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseExceptionHandler("/error"); //��Exception������ErrorControllers�޲z
            }
            else
            {
                app.UseExceptionHandler("/error-local-development"); //��Exception������ErrorControllers�޲z
            }

            app.UseRouting();

            //�ϥ�����
            app.UseAuthentication();
            //��bUseAuthentication����
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
