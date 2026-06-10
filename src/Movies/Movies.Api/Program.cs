using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Movies.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<MoviesDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("MoviesDb"));
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Authentication:Authority"];
        options.Audience = builder.Configuration["Authentication:Audience"];
        options.RequireHttpsMetadata = true;
        options.MapInboundClaims = false;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("movies.read", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context => 
            context.User.FindFirst("scope")?.Value
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Contains("movies.read") == true);
    });

    options.AddPolicy("movies.write", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context => 
            context.User.FindFirst("scope")?.Value
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Contains("movies.write") == true);
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("MoviesSpa", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("MoviesSpa");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
