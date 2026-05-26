using AuthServer.Api.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Api.OAuth.Services;

public sealed class AuthorizeRequestValidator
{
    private readonly AuthDbContext _dbContext;
    public AuthorizeRequestValidator(AuthDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AuthorizeValidationResult> ValidateAsync(AuthorizeRequest request)
    {
        if (request.ResponseType != "code")
        {
            return AuthorizeValidationResult.Failure(
                "unsupported_response_type", "Only response_type=code is supported."
            );
        }

        if (string.IsNullOrWhiteSpace(request.ClientId))
        {
            return AuthorizeValidationResult.Failure(
                "invalid_request",
                "client_id is required."
            );
        }

        var client = await _dbContext.OAuthClients
            .Include(c => c.RedirectUris)
            .Include(c => c.AllowedScopes)
            .SingleOrDefaultAsync(c => c.ClientId == request.ClientId);

        if (client == null)
        {
            return AuthorizeValidationResult.Failure(
                "invalid_client",
                "The client_id is not registered."
            );
        }

        if (!client.IsActive)
        {
            return AuthorizeValidationResult.Failure(
                "invalid_client",
                "The client is inactive."
            );
        }

        if (string.IsNullOrWhiteSpace(request.RedirectUri))
        {
            return AuthorizeValidationResult.Failure(
                "invalid_request",
                "redirect_uri is required."
            );
        }

        var redirectUriAllowed = client.RedirectUris.Any(uri => uri.RedirectUri == request.RedirectUri);

        if (!redirectUriAllowed)
        {
            return AuthorizeValidationResult.Failure(
                "invalid_request",
                "redirect_uri is not registered for this client."
            );
        }

        var requestedScopes = ParseScopes(request.Scope);

        if (requestedScopes.Count == 0)
        {
            return AuthorizeValidationResult.Failure(
                "invalid_scope",
                "At least one scope is required."
            );
        }

        var allowedScopes = client.AllowedScopes
            .Select(s => s.Scope)
            .ToHashSet(StringComparer.Ordinal);

        var invalidScopes = requestedScopes
            .Where(s => !allowedScopes.Contains(s))
            .ToArray();

        if (invalidScopes.Length > 0)
        {
            return AuthorizeValidationResult.Failure(
                "invalid_scope",
                $"The following scopes are not allowed: {string.Join(", ", invalidScopes)}"
            );
        }

        if (requestedScopes.Contains("openid", StringComparer.Ordinal) && 
            string.IsNullOrWhiteSpace(request.Nonce))
        {
            return AuthorizeValidationResult.Failure(
                "invalid_request",
                "nonce is required when requesting the openid scope."
            );
        }

        if (client.RequirePkce)
        {
            if (string.IsNullOrWhiteSpace(request.CodeChallenge))
            {
                return AuthorizeValidationResult.Failure(
                    "invalid_request",
                    "code_challenge is required for this client."
                );
            }

            if (request.CodeChallengeMethod != "S256")
            {
                return AuthorizeValidationResult.Failure(
                    "invalid_request",
                    "Only code_challenge_method=S256 is supported."
                );
            }
        }

        return AuthorizeValidationResult.Success(client, requestedScopes);
    }

    private static IReadOnlyCollection<string> ParseScopes(string? scope)
    {
        if (string.IsNullOrWhiteSpace(scope))
        {
            return Array.Empty<string>(); 
        }

        return scope.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }
}