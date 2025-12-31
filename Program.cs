using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Nass.Data;
using Nass.Helpers;
using Nass.Hubs;
using Nass.Services.Email;
using Nass.Services.SMS;
using System.Text;

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
     //developer mode only 
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll",
//        policy => policy.AllowAnyOrigin()
//                        .AllowAnyHeader()
//                        .AllowAnyMethod());
//});

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
builder.Services.AddSwaggerGen();

// JWT AUTHENTICATION
var jwtSettings = builder.Configuration.GetSection("Jwt");

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

// Swagger always enabled
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Nassad API V1");
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
  // developer mode
//app.UseCors("AllowLocalhost"); // MUST be before Authentication/Authorization
// 🔐 Authentication must come before Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers and routes
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=index}/{id?}"
);

// Map SignalR hub
app.MapHub<NotificationHub>("/notificationHub");

app.Run();
