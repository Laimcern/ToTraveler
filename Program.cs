using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ToTraveler;
using ToTraveler.Auth;
using ToTraveler.Auth.Model;
using DotNetEnv;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.OpenApi.Models;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

//https://stackoverflow.com/questions/37371264/invalidoperationexception-unable-to-resolve-service-for-type-microsoft-aspnetc
builder.Services.TryAddSingleton<HttpContextAccessor, HttpContextAccessor>();

builder.Services.AddTransient<JwtTokenService>();
builder.Services.AddTransient<SessionService>();
builder.Services.AddScoped<AuthSeeder>();

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

var JWT_Secret = Environment.GetEnvironmentVariable("JWT_SECRET");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters.ValidAudience = builder.Configuration["Jwt:ValidAudience"];
    options.TokenValidationParameters.ValidIssuer = builder.Configuration["Jwt:ValidIssuer"];
    options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWT_Secret));
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });
    c.ExampleFilters();    // Registers example filters
});

builder.Services.AddSwaggerExamplesFromAssemblyOf<AuthController>(); // Register example providers


//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("DevCors", policy =>
//    {
//        policy.WithOrigins(
//            "http://localhost:5173",
//            "http://192.168.239.143:5173"
//            ) 
//            .AllowAnyMethod()
//            .AllowAnyHeader()
//            .AllowCredentials();
//    });
//});

var app = builder.Build();



//request catcher
app.Use(async (HttpContext, next) =>
{
    await next();
});

using var scope = app.Services.CreateScope();

var dbContext = scope.ServiceProvider.GetService<AppDbContext>();

var dbSeeder = scope.ServiceProvider.GetRequiredService<AuthSeeder>();

await dbSeeder.SeedAsync();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseCors("DevCors");

app.UseStaticFiles();

app.UseDefaultFiles();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("index.html");

app.Run();
