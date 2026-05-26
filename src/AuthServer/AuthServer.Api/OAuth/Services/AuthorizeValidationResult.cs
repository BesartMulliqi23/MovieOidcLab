// This gives us a clean result object instead of throwing random errors from the controller

using AuthServer.Api.OAuth.Entities;

namespace AuthServer.Api.OAuth.Services;

public sealed class AuthorizeValidationResult
{
    private AuthorizeValidationResult(
        bool succeeded,
        OAuthClient? client,
        IReadOnlyCollection<string> requestedScopes,
        string? error,
        string? errorDescription
    )
    {
        Succeeded = succeeded;
        Client = client;
        RequestedScopes = requestedScopes;
        Error = error;
        ErrorDescription = errorDescription;
    }

    public bool Succeeded { get; }
    public OAuthClient? Client { get; }
    public IReadOnlyCollection<string> RequestedScopes { get; }
    public string? Error { get; }
    public string? ErrorDescription { get; }

    public static AuthorizeValidationResult Success(OAuthClient client, IReadOnlyCollection<string> requestedScopes)
    {
        return new AuthorizeValidationResult(true, client, requestedScopes, null, null);
    }

    public static AuthorizeValidationResult Failure(string error, string errorDescription)
    {
        return new AuthorizeValidationResult(false, null, Array.Empty<string>(), error, errorDescription);
    }
}