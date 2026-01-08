using Azure.Identity;
using FrameZone.API.Repositories;
using FrameZone.API.Repositories.Interfaces;
using FrameZone.API.Services;
using FrameZone.API.Services.Interfaces;
using FrameZone_WebApi.Configuration;
using FrameZone_WebApi.Helpers;
using FrameZone_WebApi.Middlewares;
using FrameZone_WebApi.Middlewares;
using FrameZone_WebApi.Models;
using FrameZone_WebApi.Repositories;
using FrameZone_WebApi.Repositories.Interfaces;
using FrameZone_WebApi.Repositories.Member;
using FrameZone_WebApi.Services;
using FrameZone_WebApi.Services.Interfaces;
using FrameZone_WebApi.Services.Member;
using FrameZone_WebApi.Socials.Hubs;
using FrameZone_WebApi.Shopping.Configuration;
using FrameZone_WebApi.Shopping.Repositories;
using FrameZone_WebApi.Shopping.Services;
using FrameZone_WebApi.Socials.Repositories;
using FrameZone_WebApi.Socials.Services;
using FrameZone_WebApi.Videos.Helpers;
using FrameZone_WebApi.Videos.Repositories;
using FrameZone_WebApi.Videos.Respositories;
using FrameZone_WebApi.Videos.Services;
using FrameZone_WebApi.PhotographerBooking.Repositories;
using FrameZone_WebApi.PhotographerBooking.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SixLabors.ImageSharp;
using System.Text;
using System.Text;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;
using static FrameZone_WebApi.Videos.Helpers.AaContextFactoryHelper;
using FrameZone_WebApi.Hubs;

var builder = WebApplication.CreateBuilder(args);

//// ========== Azure Key Vault 設定 ==========
//var keyVaultUri = builder.Configuration["KeyVault:VaultUri"];
//if (!string.IsNullOrEmpty(keyVaultUri))
//{
//    try
//    {
//        Console.WriteLine($"正在連接到 Key Vault: {keyVaultUri}");

//        builder.Configuration.AddAzureKeyVault(
//            new Uri(keyVaultUri),
//            new DefaultAzureCredential());

//        Console.WriteLine("✓ Key Vault 連接成功!");
//    }
//    catch (Exception ex)
//    {
//        Console.WriteLine($"✗ Key Vault 連接失敗: {ex.Message}");
//        Console.WriteLine("將使用本地配置繼續運行...");
//    }
//}
//else
//{
//    Console.WriteLine("未設定 Key Vault URI，使用本地配置");
//}


////////////--------------------------在應用程式啟動前下載 FFmpeg---------------
var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
var ffmpegPath = Path.Combine(wwwrootPath, "FFmpeg");

// 確保目錄存在
Directory.CreateDirectory(ffmpegPath);

// 檢查是否已存在 FFmpeg
var ffmpegExe = Path.Combine(ffmpegPath, "ffmpeg.exe");
if (!File.Exists(ffmpegExe))
{
    Console.WriteLine("FFmpeg 不存在,正在下載...");
    try
    {
        await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, ffmpegPath);
        Console.WriteLine("! FFmpeg 下載完成!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"X FFmpeg 下載失敗: {ex.Message}");
        Console.WriteLine("請手動下載 FFmpeg 並放置到 wwwroot/FFmpeg/ 目錄");
    }
}
else
{
    Console.WriteLine("! FFmpeg 已存在");
}

// 設定 FFmpeg 路徑
FFmpeg.SetExecutablesPath(ffmpegPath);
////////////--------------------------

// Add services to the container.


// ========== 資料庫連線設定 ==========

builder.Services.AddDbContext<AAContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("AA");
    options.UseSqlServer(connectionString);

    // 開發時顯示詳細錯誤
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
});


//========== 註冊設定 ==========

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings")
);

builder.Services.Configure<VerificationSettings>(
    builder.Configuration.GetSection("VerificationSettings")
);

//========== 金流設定 ==========
builder.Services.Configure<ECPaySettings>(
    builder.Configuration.GetSection("ECPaySettings")
);



// ========== CORS 設定 ==========

var policyName = "Angular";
builder.Services.AddCors(options =>
{
    
    options.AddPolicy(policyName, policy =>
    {
        policy.WithOrigins("https://localhost:4200", "http://localhost:4200")
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
            return Task.CompletedTask;
        },

        // SignalR 的 Token 驗證（從 Query String 取得 Token）
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            // 如果是 SignalR Hub 請求且有 Token，則使用它
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

// 註冊 SignalR 服務
builder.Services.AddSignalR(options =>
{
    // SignalR 配置選項
    options.EnableDetailedErrors = builder.Environment.IsDevelopment(); // 開發環境顯示詳細錯誤
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60); // 客戶端超時時間
    options.KeepAliveInterval = TimeSpan.FromSeconds(30); // 保持連線間隔
});

// 配置記憶體快取
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 1024 * 1024 * 200;
    options.CompactionPercentage = 0.25;
});

// 註冊 Azure Blob Storage 設定
builder.Services.Configure<AzureBlobStorageSettings>(
    builder.Configuration.GetSection(AzureBlobStorageSettings.SectionName));

// 設定 Google 認證
builder.Services.Configure<GoogleAuthSettings>(
    builder.Configuration.GetSection("GoogleAuth")
);

// 註冊 HttpClient
builder.Services.AddHttpClient();


// ========== 註冊依賴注入服務 (DI注入) ==========

builder.Services.AddScoped<IPhotoRepository, PhotoRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<IMemberProfileRepository, MemberProfileRepository>();
builder.Services.AddScoped<IUserLogRepository, UserLogRepository>();
builder.Services.AddScoped<IMemberSecurityRepository, MemberSecurityRepository>();
builder.Services.AddScoped<IMemberNotificationRepository, MemberNotificationRepository>();
builder.Services.AddScoped<IMemberPrivacyRepository, MemberPrivacyRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IExifService, ExifService>();
builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddScoped<ITagCategorizationService, TagCategorizationService>();
builder.Services.AddScoped<IBackgroundGeocodingService, BackgroundGeocodingService>();
builder.Services.AddSingleton<IBackgroundAIAnalysisService, BackgroundAIAnalysisService>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
builder.Services.AddScoped<IMemberProfileService, MemberProfileService>();
builder.Services.AddScoped<IUserLogService, UserLogService>();
builder.Services.AddScoped<IMemberSecurityService, MemberSecurityService>();
builder.Services.AddScoped<IMemberNotificationService, MemberNotificationService>();
builder.Services.AddScoped<IMemberPrivacyService, MemberPrivacyService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddScoped<IAzureComputerVisionService, AzureComputerVisionService>();
builder.Services.AddHttpClient<IGooglePlacesService, GooglePlacesService>(client =>
{
    // 設定 Google Places API 的基本配置
    client.Timeout = TimeSpan.FromSeconds(20);  // 20 秒超時
    client.DefaultRequestHeaders.Add("User-Agent", "FrameZone-PhotoApp/1.0");
});
builder.Services.AddScoped<IClaudeApiService, ClaudeApiService>();

builder.Services.AddScoped<IPhotographerRepository, PhotographerRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IAvailableSlotRepository, AvailableSlotRepository>();
builder.Services.AddScoped<IPhotographerService, FrameZone_WebApi.PhotographerBooking.Services.PhotographerService>();
builder.Services.AddScoped<IBookingService, BookingService>();


builder.Services.AddHttpClient<IGeocodingService, GeocodingService>();
builder.Services.AddScoped<IGeocodingService, GeocodingService>();

builder.Services.AddSingleton<JwtHelper>();
builder.Services.AddHttpContextAccessor();

// ========== 社群服務 (DI注入) ==========
builder.Services.AddScoped<PostRepository>();
builder.Services.AddScoped<PostLikeRepository>();
builder.Services.AddScoped<PostShareRepository>();
builder.Services.AddScoped<PostViewRepository>();
builder.Services.AddScoped<CommentRepository>();
builder.Services.AddScoped<ChatRoomRepository>();
builder.Services.AddScoped<MessageRepository>();
builder.Services.AddScoped<FollowRepository>();

builder.Services.AddScoped<PostService>();
builder.Services.AddScoped<PostLikeService>();
builder.Services.AddScoped<PostShareService>();
builder.Services.AddScoped<PostViewService>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<ChatRoomService>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<FollowService>();
builder.Services.AddSingleton<SocialChatConnectionManager>();

// ========== 影片服務 (DI注入) ==========
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<AaContextFactoryHelper>();
builder.Services.AddHttpClient(); // 註冊 HttpClient 工廠
builder.Services.AddHostedService<ChannelEnsureHostedService>();
builder.Services.AddScoped<VideoCreatorRepository>(); // 註冊 Repository
builder.Services.AddScoped<VideoUploadRepository>();// 註冊 Repository
builder.Services.AddScoped<VideoRepository>();// 註冊 Repository
builder.Services.AddScoped<VideoTranscodeServices>();
builder.Services.AddScoped<IVideoUploadService, VideoUploadService>();
builder.Services.AddScoped<VideoServices>();
builder.Services.AddScoped<VideoCreatorService>();
builder.Services.AddScoped<VideoPlayerService>();

//========== 購物中心 (DI注入) ==========
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddScoped<FavoriteService>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IFaqService, FaqService>();
builder.Services.AddScoped<IShoppingMemberService, ShoppingMemberService>();
builder.Services.AddScoped<IShoppingStoreService, ShoppingStoreService>();
builder.Services.AddScoped<ECPayService>();  //綠界注入



// 調整上傳限制 (1GB)
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 1024L * 1024L * 1024L; // 1GB
});
//=======================================


// ========== 日誌設定 ==========
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ========== 檔案上傳大小限制設定 ==========
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600;
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 104857600;
});


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;            // 使用駝峰命名法
        options.JsonSerializerOptions.WriteIndented = true;        // 格式化 JSON
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSignalR();

// ========== Swagger 配置 ==========
builder.Services.AddSwaggerGen(options =>
{
    // API 基本資訊
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FrameZone WebAPI",
        Version = "v1",
        Description = "FrameZone 照片管理平台 API 文件",
        Contact = new OpenApiContact
        {
            Name = "FrameZone Team",
            Email = "support@framezone.com"
        }
    });

    // 加入 JWT Bearer 驗證支援
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "請輸入 JWT Token，格式：Bearer {token}"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });

    // 處理可能的 XML 註釋檔案
    try
    {
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            Console.WriteLine($"✓ 已載入 XML 註釋檔案: {xmlFile}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠ 無法載入 XML 註釋檔案: {ex.Message}");
        // XML 註釋檔案不存在也沒關係，繼續執行
    }

    // 自訂 Schema ID 生成規則，避免重複
    options.CustomSchemaIds(type => type.FullName);

    // 支援檔案上傳
    options.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = JsonSchemaType.String,
        Format = "binary"
    });
});


//// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

var app = builder.Build();

// 開發環境啟用 Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "FrameZone WebAPI v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "FrameZone API 文件";
        options.DisplayRequestDuration();
    });
}

// 確保 Blob 容器存在
using (var scope = app.Services.CreateScope())
{
    var blobService = scope.ServiceProvider.GetRequiredService<IBlobStorageService>();
    await blobService.EnsureContainersExistAsync();
}


app.UseCors(policyName);

// ==========  啟用 wwwroot 靜態檔案(影片要用的) ==========
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".m3u8"] = "application/vnd.apple.mpegurl";
provider.Mappings[".ts"] = "video/mp2t";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});
//=======================================
app.UseHttpsRedirection();

app.UseQueryStringToken();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<SocialChatHub>("/hubs/chat");
app.MapControllers();

app.MapHub<NotificationHub>("/hubs/notification");

Console.WriteLine("====================================");
Console.WriteLine("FrameZone WebAPI 正在啟動...");
Console.WriteLine($"環境: {app.Environment.EnvironmentName}");
Console.WriteLine($"API 端點: https://localhost:7213/api/");
Console.WriteLine($"Swagger 文件: https://localhost:7213/swagger");
Console.WriteLine("====================================");

app.Run();