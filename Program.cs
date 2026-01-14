using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Nass.Data;
using Nass.Helpers;
using Nass.Hubs;
using System.Text;
using Microsoft.OpenApi.Models;
using Nass.SMS;
using Nass.Email;

var builder = WebApplication.CreateBuilder(args);

// =======================
// SERVICES
// =======================

// SignalR
builder.Services.AddSignalR();

// Customer service
builder.Services.AddScoped<ICustomerTenetService, CustomerTenetService>();

// MVC + API
builder.Services.AddControllersWithViews();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
    );
//allow API to access from nassad.ca
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNassad",
        policy =>
        {
            policy
                .WithOrigins(
                    "https://nassad.ca",
                    "https://www.nassad.ca"
                )
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// JWT Token
builder.Services.AddSingleton<JwtService>();

// Twilio SMS Service
builder.Services.AddSingleton<TwilioSmsService>();

// EF Core
builder.Services.AddDbContext<NassadContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("monsteraspConnection"),
         //builder.Configuration.GetConnectionString("SQLConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure() // transient retry
    )
);

// Swagger (enabled in all environments)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Nassad API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter JWT token"
        });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


// JWT AUTHENTICATION
var jwtSettings = builder.Configuration.GetSection("JwtSwagger");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Key"])
        )
    };
});

// EMAIL SERVICE and SMS SERVICE
builder.Services.AddScoped<IEmailService<EmailService>, EmailService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<NotificationService>();


// =======================
// BUILD APP
// =======================
var app = builder.Build();

// =======================
// APPLY EF CORE MIGRATIONS AUTOMATICALLY
// =======================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NassadContext>();
    db.Database.Migrate(); // automatically applies pending migrations
}

// =======================
// MIDDLEWARE
// =======================

// Developer exception page
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// using just in development Enviroment 
if (app.Environment.IsDevelopment())
    {
    app.UseDeveloperExceptionPage();

    // Swagger ONLY in Development
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nassad API V1");
    });
    }
else
    {
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    }
// use http
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
// Allow to access just from nassad.ca to access API
app.UseCors("AllowNassad");
//  Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers and routes
app.MapControllers();
//  Protect Swagger endpoints
app.MapSwagger().RequireAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=index}/{id?}"
);

// Map SignalR hub
app.MapHub<NotificationHub>("/notificationHub");

app.Run();
