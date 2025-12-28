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
//signalR 
builder.Services.AddSignalR();

//customer service
builder.Services.AddScoped<ICustomerTenetService, CustomerTenetService>();

// MVC + API
builder.Services.AddControllersWithViews();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
    );
// JWT Token
builder.Services.AddSingleton<JwtService>();
//TwilioSmsService
builder.Services.AddSingleton<TwilioSmsService>();
// EF Core
builder.Services.AddDbContext<NassadContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SQLConnection"))
);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// =======================
// JWT AUTHENTICATION
// =======================

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

//EMAIL SERVICE and SMS SERVICE
builder.Services.AddScoped<IEmailService<EmailService>, EmailService>();

builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<NotificationService>();
// =======================
// BUILD APP
// =======================

var app = builder.Build();

//temp for debug developer 
app.UseDeveloperExceptionPage();
// =======================
// MIDDLEWARE
// =======================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 🔐 VERY IMPORTANT ORDER
app.UseAuthentication(); // <-- MUST be before Authorization
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=index}/{id?}"
);


app.MapHub<NotificationHub>("/notificationHub");

app.Run();
