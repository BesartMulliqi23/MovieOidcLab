using AuthServer.Api.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Api.OAuth.Services;

public sealed class TokenExchangeService
{
    private readonly AuthDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AccessTokenService _accessTokenService;
    private readonly IdTokenService _idTokenService;

    public TokenExchangeService(
        AuthDbContext dbContext, 
        UserManager<ApplicationUser> userManager, 
        AccessTokenService accessTokenService,
        IdTokenService idTokenService
    )
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _accessTokenService = accessTokenService;
        _idTokenService = idTokenService;
    }

    public async Task<TokenExchangeResult> ExchangeAuthorizationCodeAsync(TokenRequest request)
    {
        if (request.GrantType != "authorization_code")
        {
            return TokenExchangeResult.Failure("unsupported_grant_type", "Only authorization_code is supported.");
        }

        if (string.IsNullOrWhiteSpace(request.ClientId) || 
            string.IsNullOrWhiteSpace(request.Code) ||
            string.IsNullOrWhiteSpace(request.RedirectUri) ||
            string.IsNullOrWhiteSpace(request.CodeVerifier))
        {
            return TokenExchangeResult.Failure("invalid_request", "client_id, code, redirect_uri, and code_verifier are required.");
        }

        var client = await _dbContext.OAuthClients
            .SingleOrDefaultAsync(c => c.ClientId == request.ClientId);

        if (client == null || !client.IsActive)
        {
            return TokenExchangeResult.Failure("invalid_client", "The client is invalid.");
        }

        var codeHash = AuthorizationCodeService.HashCode(request.Code);

        var authorizationCode = await _dbContext.AuthorizationCodes
            .SingleOrDefaultAsync(c => c.CodeHash == codeHash);

        if (authorizationCode is null)
        {
            return TokenExchangeResult.Failure("invalid_grant", "The authorization code is invalid.");
        }

        if (authorizationCode.OAuthClientId != client.Id)
        {
            return TokenExchangeResult.Failure("invalid_grant", "The authorization code was not issued to this client.");
        }

        if (authorizationCode.RedirectUri != request.RedirectUri)
        {
            return TokenExchangeResult.Failure("invalid_grant", "redirect_uri does not match the original authorization request.");
        }

        if (authorizationCode.ExpiresAt < DateTimeOffset.UtcNow)
        {
            return TokenExchangeResult.Failure("invalid_grant", "The authorization code has expired.");
        }

        if (authorizationCode.ConsumedAt is not null)
        {
            return TokenExchangeResult.Failure("invalid_grant", "The authorization code has already been used.");
        }

        if (!PkceService.ValidateS256(request.CodeVerifier, authorizationCode.CodeChallenge))
        {
            return TokenExchangeResult.Failure("invalid_grant", "PKCE validation failed.");
        }

        var user = await _userManager.FindByIdAsync(authorizationCode.UserId.ToString());

        if (user is null)
        {
            return TokenExchangeResult.Failure("invalid_grant", "The user no longer exists.");
        }

        authorizationCode.ConsumedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        var tokenResponse = _accessTokenService.CreateAccessToken(user, client.ClientId, authorizationCode.Scope);

        var requestedScopes = authorizationCode.Scope
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (requestedScopes.Contains("openid", StringComparer.Ordinal))
        {
            var idToken = _idTokenService.CreateIdToken(user, client.ClientId, authorizationCode.Nonce);

            tokenResponse = tokenResponse with { IdToken = idToken };
        }

        return TokenExchangeResult.Success(tokenResponse);
    }
}

public sealed class TokenExchangeResult
{
    private TokenExchangeResult(bool succeeded, TokenResponse? tokenResponse, string? error, string? errorDescription)
    {
        Succeeded = succeeded;
        TokenResponse = tokenResponse;
        Error = error;
        ErrorDescription = errorDescription;    
    }

    public bool Succeeded { get; }
    public TokenResponse? TokenResponse { get; }
    public string? Error { get; }
    public string? ErrorDescription { get; }

    public static TokenExchangeResult Success(TokenResponse tokenResponse)
    {
        return new TokenExchangeResult(true, tokenResponse, null, null);
    }

    public static TokenExchangeResult Failure(string error, string  errorDescription)
    {
        return new TokenExchangeResult(false, null, error, errorDescription);
    }
}