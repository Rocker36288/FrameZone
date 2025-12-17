using FrameZone_WebApi.Helpers;
using FrameZone_WebApi.Models;
using FrameZone_WebApi.Repositories;
using FrameZone_WebApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


// ========== 資料庫連線設定 ==========

builder.Services.AddDbContext<AAContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("AA");
    options.UseSqlServer(connectionString);

    // 開發時顯示詳細錯誤
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
});


// ========== CORS 設定 ==========

var policyName = "Angular";
builder.Services.AddCors(options =>
    {
        options.AddPolicy(policyName, policy =>
        {
            policy.WithOrigins("http://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();

        });
    });

// ========== JWT 設定 ==========

/// <summery>
/// 註冊 JWT Bearer Token 驗證
/// </summery>
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey 未設定");

builder.Services.AddAuthentication(options =>
{
    // 預設使用 JWT Bearer 驗證
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,                          // 驗證發行者
        ValidateAudience = true,                        // 驗證接收者
        ValidateLifetime = true,                        // 驗證過期時間
        ValidateIssuerSigningKey = true,                // 驗證簽章金鑰
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey)
        ),
        ClockSkew = TimeSpan.Zero                       // 不允許時間偏移
    };

    // Token 驗證事件
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            // Token 驗證失敗時
            Console.WriteLine($"JWT 驗證失敗: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            // Token 驗證成功時
            Console.WriteLine("JWT 驗證成功");
            return Task.CompletedTask;
        }
    };  
});

// ========== 註冊依賴注入服務 (DI注入) ==========

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddSingleton<JwtHelper>();
builder.Services.AddHttpContextAccessor();


builder.Services.AddControllers()
    .AddJsonOptions(options =>
     {
         options.JsonSerializerOptions.PropertyNamingPolicy = null;     // 保持原始屬性名稱
         options.JsonSerializerOptions.WriteIndented = true;            // 格式化 JSON
     });

builder.Services.AddEndpointsApiExplorer();

//// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//}

app.UseCors(policyName);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Console.WriteLine("====================================");
Console.WriteLine("FrameZone WebAPI 正在啟動...");
Console.WriteLine($"環境: {app.Environment.EnvironmentName}");
Console.WriteLine($"API 端點: https://localhost:7213/api/");
Console.WriteLine("====================================");

app.Run();
