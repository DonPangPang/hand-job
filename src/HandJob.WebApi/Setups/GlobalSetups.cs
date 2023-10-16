using System.Text;
using HandJob.Common;
using HandJob.Domain.Entities;
using HandJob.Domain.Responses;
using HandJob.Domain.ViewModels;
using HandJob.WebApi.Auth;
using HandJob.WebApi.Options;
using HandJob.WebApi.Vars;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using AutoMapper;
using HandJob.WebApi.Filters;
using HandJob.WebApi.Services;
using Microsoft.OpenApi.Models;
using HandJob.WebApi.Data;
using Microsoft.EntityFrameworkCore;

namespace HandJob.WebApi.Setups;

public static class GlobalSetups
{
    public static void AddHandJobServices(this IServiceCollection services)
    {
        services.AddConfigure();
        services.AddAutoMapperSetup();
        services.AddAuthSetup();
        services.AddCorsSetup();
        services.AddSwaggerSetup();
        services.AddLazyResolution();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<Session>();

        services.AddDbContext<HandJobDbContext>(opts =>
        {
            opts.UseSqlite("Data Source=handJob.db");
        });
    }

    public static void AddJson(this IMvcBuilder builder)
    {
        builder.AddNewtonsoftJson(setup =>
                {
                    setup.SerializerSettings.ContractResolver
                        = new CamelCasePropertyNamesContractResolver();
                    setup.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                })
                /*添加XML*/.AddXmlDataContractSerializerFormatters()
                .ConfigureApiBehaviorOptions(setup =>
                {
                    setup.InvalidModelStateResponseFactory = context =>
                    {
                        var problemDetails = new ValidationProblemDetails(context.ModelState)
                        {
                            Type = "http://www.baidu.com",
                            Title = "有错误",
                            Status = StatusCodes.Status422UnprocessableEntity,
                            Detail = "请看详细信息",
                            Instance = context.HttpContext.Request.Path
                        };

                        problemDetails.Extensions.Add("traceId", context.HttpContext.TraceIdentifier);

                        return new UnprocessableEntityObjectResult(problemDetails)
                        {
                            ContentTypes = { "application/problem+json" }
                        };
                    };
                });
    }

    private static void AddLazyResolution(this IServiceCollection services)
    {
        services.AddTransient(
            typeof(Lazy<>),
            typeof(LazilyResolved<>));
    }

    private class LazilyResolved<T> : Lazy<T> where T : notnull
    {
        public LazilyResolved(IServiceProvider serviceProvider)
            : base(serviceProvider.GetRequiredService<T>)
        {
        }
    }

    private static void AddCorsSetup(this IServiceCollection services)
    {
        services.AddCors(opt =>
        {
            opt.AddDefaultPolicy(policyBuilder =>
            {
                policyBuilder.AllowCredentials()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .SetIsOriginAllowed(_ => true);
            });
        });
    }

    private static void AddAuthSetup(this IServiceCollection services)
    {
        IConfiguration? configuration = services.BuildServiceProvider().GetService<IConfiguration>();

        var para = configuration?.GetSection("TokenParameter").Get<PermissionRequirement>()!;

        services.AddAuthorization(opts =>
        {
            opts.AddPolicy(GlobalVars.Permission, policy =>
            {
                policy.Requirements.Add(new PermissionRequirement());
            });
        });

        services.AddAuthentication(opts =>
        {
            opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opts.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer("Bearer", options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidAudience = para.Audience,
                ValidIssuer = para.Issuer,
                IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(para.Secret))
            };

            options.SaveToken = true;

            options.Events = new JwtBearerEvents
            {
                //此处为权限验证失败后触发的事件
                OnChallenge = context =>
                {
                    //此处代码为终止.Net Core默认的返回类型和数据结果，这个很重要哦，必须
                    context.HandleResponse();
                    //自定义自己想要返回的数据结果，我这里要返回的是Json对象，通过引用Newtonsoft.Json库进行转换
                    //自定义返回的数据类型
                    context.Response.ContentType = "application/json";
                    //自定义返回状态码，默认为401 我这里改成 200
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    //输出Json数据结果
                    var result = new ResponseBox()
                    {
                        Code = 401,
                        Message = "Token校验失败"
                    }.ToJson();
                    context.Response.WriteAsync(result);
                    return Task.FromResult(0);
                }
            };
        }
        );
    }

    private static void AddAutoMapperSetup(this IServiceCollection services)
    {
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
    }

    private static void AddConfigure(this IServiceCollection services)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build(); //编译成对象

        services.AddOptions().Configure<AppSettings>(config.GetSection("Appsettings"));
    }
    private static IServiceCollection AddSwaggerSetup(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Api Document", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                Description = "在下框中输入请求头中需要添加Jwt授权Token：Bearer Token",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });

            c.DocumentFilter<SwaggerEnumFilter>();

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }
            });

            //var xmlFile = $"app-doc.xml";
            //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            //var sharedXml = Path.Combine(AppContext.BaseDirectory, "shared-doc.xml");

            ////... and tell Swagger to use those XML comments.
            //c.IncludeXmlComments(xmlPath, true);
            //c.IncludeXmlComments(sharedXml, true);
        });

        return services;
    }
}
