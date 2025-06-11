using Doanmh.Model;
using Doanmh.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

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

// Static files (wwwroot folder)
app.UseStaticFiles();

// Swagger middleware (bắt buộc để hiển thị Swagger)
app.UseSwagger();

// Swagger UI cho USER API tại /swagger-user
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/user/swagger.json", "User API");
    c.RoutePrefix = "swagger-user"; // => https://localhost:7218/swagger-user
});

// Swagger UI cho PRODUCT API tại /swagger-product
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/product/swagger.json", "Product API");
    c.RoutePrefix = "swagger-product"; // => https://localhost:7218/swagger-product
});

// Truy cập login.html tại /login
app.MapGet("/login", () => Results.Redirect("/login.html"));

// Middleware
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
