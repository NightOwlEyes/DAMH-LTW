using Doanmh.Model;
using Doanmh.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
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
                context.Response.Headers.Add("Token-Expired", "true");
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
});

// Repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// CORS config
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyAllowOrigins", policy =>
    {
        policy.WithOrigins("https://localhost:7218")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

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
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/user/swagger.json", "User API");
    c.RoutePrefix = "swagger-user";
});

// Swagger UI cho PRODUCT API tại /swagger-product
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/product/swagger.json", "Product API");
    c.RoutePrefix = "swagger-product";
});

// Truy cập login.html tại /login
app.MapGet("/login", () => Results.Redirect("/login.html"));

// Middleware
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
