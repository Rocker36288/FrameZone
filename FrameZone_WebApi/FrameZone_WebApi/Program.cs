using FrameZone_WebApi.Configuration;
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


// ========== ��Ʈw�s�u�]�w ==========

builder.Services.AddDbContext<AAContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("AA");
    options.UseSqlServer(connectionString);

    // �}�o����ܸԲӿ��~
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
});


//========== ���U�]�w ==========

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings")
);

builder.Services.Configure<VerificationSettings>(
    builder.Configuration.GetSection("VerificationSettings")
);


// ========== CORS �]�w ==========

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

// ========== JWT �]�w ==========

/// <summery>
/// ���U JWT Bearer Token ����
/// </summery>
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey ���]�w");

builder.Services.AddAuthentication(options =>
{
    // �w�]�ϥ� JWT Bearer ����
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,                          // ���ҵo���
        ValidateAudience = true,                        // ���ұ�����
        ValidateLifetime = true,                        // ���ҹL���ɶ�
        ValidateIssuerSigningKey = true,                // ����ñ�����_
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey)
        ),
        ClockSkew = TimeSpan.Zero                       // �����\�ɶ�����
    };

    // Token ���Ҩƥ�
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            // Token ���ҥ��Ѯ�
            Console.WriteLine($"JWT ���ҥ���: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            return Task.CompletedTask;
        }
    };  
});

// ========== ���U�̿�`�J�A�� (DI�`�J) ==========

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<PostRepository>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<PostService>();
builder.Services.AddSingleton<JwtHelper>();
builder.Services.AddHttpContextAccessor();


builder.Services.AddControllers()
    .AddJsonOptions(options =>
     {
        options.JsonSerializerOptions.PropertyNamingPolicy = 
            System.Text.Json.JsonNamingPolicy.CamelCase;            // �ϥξm�p�R�W�k
         options.JsonSerializerOptions.WriteIndented = true;        // �榡�� JSON
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
Console.WriteLine("FrameZone WebAPI ���b�Ұ�...");
Console.WriteLine($"����: {app.Environment.EnvironmentName}");
Console.WriteLine($"API ���I: https://localhost:7213/api/");
Console.WriteLine("====================================");

app.Run();
