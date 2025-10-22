using Doanmh.Model;
using Doanmh.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers().AddJsonOptions(options =>
{
	options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
	options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
builder.Services.AddEndpointsApiExplorer();

// JWT Key (dùng để ký token)
var jwtKey = "this is a secret key for jwt token generation";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero // Bắt lỗi hết hạn token chính xác
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception is SecurityTokenExpiredException)
            {
                context.Response.Headers.Append("Token-Expired", "true");
            }

            Console.WriteLine($"Authentication failed for {context.Request.Path}: {context.Exception.Message}");
            Console.WriteLine($"Token: {context.Request.Headers["Authorization"]}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine($"Token validated successfully for {context.Request.Path}");
            return Task.CompletedTask;
        }
    };
});

// Swagger config
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("user", new OpenApiInfo { Title = "User API", Version = "v1" });
    c.SwaggerDoc("product", new OpenApiInfo { Title = "Product API", Version = "v1" });
    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        if (!apiDesc.TryGetMethodInfo(out var methodInfo))
            return false;

        var groupName = apiDesc.GroupName ?? "user";
        return string.Equals(groupName, docName, StringComparison.OrdinalIgnoreCase);
    });

    // Thêm JWT Authentication vào Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// CORS config
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseCors("AllowAll");

// Rewrite các route ngắn → file HTML
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower();

    var rewriteMap = new Dictionary<string, string>
    {
        ["/dndk/login"] = "/dndk/login.html"
    };

    if (rewriteMap.TryGetValue(path ?? "", out var newPath))
    {
        context.Request.Path = newPath;
    }

    await next();
});

// Static files (wwwroot folder)
app.UseStaticFiles();

// Swagger middleware
app.UseSwagger();

// Swagger UI cho USER API tại /swagger-user
// ✅ Đúng: chỉ gọi 1 lần thôi
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/user/swagger.json", "User API");
    options.SwaggerEndpoint("/swagger/product/swagger.json", "Product API");
    options.RoutePrefix = "swagger"; // truy cập tại /swagger
});




// Truy cập login.html tại /login
app.MapGet("/login", () => Results.Redirect("/login.html"));

// Middleware
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
