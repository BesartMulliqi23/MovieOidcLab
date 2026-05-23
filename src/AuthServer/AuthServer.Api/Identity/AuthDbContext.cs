using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Api.Identity;

public sealed class AuthDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) {}
}