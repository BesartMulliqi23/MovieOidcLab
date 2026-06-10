using AuthServer.Api.Identity;
using AuthServer.Api.OAuth.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<AuthDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("AuthDb")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
{
    options.User.RequireUniqueEmail = true;

    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;

    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
})
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders(); 

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "__Host.AuthServer";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";

    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);

    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendApps", policy =>
    {
        policy
            .WithOrigins("http://localhost:5174", "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddScoped<AuthorizeRequestValidator>();
builder.Services.AddScoped<AuthorizationCodeService>();
builder.Services.AddScoped<SigningKeyService>();
builder.Services.AddScoped<AccessTokenService>();
builder.Services.AddScoped<TokenExchangeService>();
builder.Services.AddScoped<IdTokenService>();
builder.Services.AddScoped<RefreshTokenService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("FrontendApps");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
