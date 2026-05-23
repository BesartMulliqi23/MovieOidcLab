using Microsoft.AspNetCore.Identity;

namespace AuthServer.Api.Identity;

public sealed class ApplicationUser : IdentityUser<int>
{
    public string? DisplayName { get; set; }
}