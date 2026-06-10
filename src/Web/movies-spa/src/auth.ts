const authServer = "https://localhost:7290";
const clientId = "movies-spa";
const redirectUri = "http://localhost:5173/callback";

export type TokenResponse = {
    access_token: string;
    token_type: "Bearer";
    expires_in: number;
    scope: string;
    id_token?: string;
    refresh_token?: string;
};

export type AuthUser = {
    sub: string;
    email?: string;
    name?: string;
};

export async function login() {
    const state = crypto.randomUUID();
    const nonce = crypto.randomUUID();
    const codeVerifier = base64UrlEncode(crypto.getRandomValues(new Uint8Array(32)));
    const codeChallenge = await createCodeChallenge(codeVerifier);

    sessionStorage.setItem("oauth_state", state);
    sessionStorage.setItem("oauth_nonce", nonce);
    sessionStorage.setItem("pkce_code_verifier", codeVerifier);

    const params = new URLSearchParams({
        response_type: "code",
        client_id: clientId,
        redirect_uri: redirectUri,
        scope: "openid profile movies.read movies.write offline_access",
        state,
        nonce,
        code_challenge: codeChallenge,
        code_challenge_method: "S256"
    });

    window.location.href = `${authServer}/connect/authorize?${params}`;
}

export async function handleCallback() {
    const params = new URLSearchParams(window.location.search);
    const code = params.get("code");
    const state = params.get("state");
    const expectedState = sessionStorage.getItem("oauth_state");
    const codeVerifier = sessionStorage.getItem("pkce_code_verifier");

    if (!code || !state || state !== expectedState || !codeVerifier) {
        throw new Error("Invalid OAuth callback");
    }

    const body = new URLSearchParams({
        grant_type: "authorization_code",
        client_id: clientId,
        code,
        redirect_uri: redirectUri,
        code_verifier: codeVerifier
    });

    const response = await fetch(`${authServer}/connect/token`, {
        method: "POST",
        headers: { "Content-Type": "application/x-www-form-urlencoded" },
        body
    });

    if (!response.ok) {
        throw new Error("Token exchange failed.");
    }

    const tokens = (await response.json()) as TokenResponse;

    if (tokens.id_token) {
        const payload = decodeJwtPayload(tokens.id_token);
        const expectedNonce = sessionStorage.getItem("oauth_nonce");

        if (payload.nonce !== expectedNonce) {
            clearTokens();
            throw new Error("Invalid ID token nonce.");
        }
    }

    saveTokens(tokens);  

    sessionStorage.removeItem("oauth_state");
    sessionStorage.removeItem("oauth_nonce");
    sessionStorage.removeItem("pkce_code_verifier");
}

export async function refreshTokens() {
    const refreshToken = sessionStorage.getItem("refresh_token");

    if (!refreshToken) {
        throw new Error("No refresh token available.");
    }

    const body = new URLSearchParams({
        grant_type: "refresh_token",
        client_id: clientId,
        refresh_token: refreshToken
    });

    const response = await fetch(`${authServer}/connect/token`, {
        method: "POST",
        headers: { "Content-Type": "application/x-www-form-urlencoded" },
        body
    });

    if (!response.ok) {
        clearTokens();
        throw new Error("Refresh failed.");
    }

    saveTokens((await response.json()) as TokenResponse);
}

export function getAccessToken() {
    return sessionStorage.getItem("access_token")
}

export function clearTokens() {
    sessionStorage.removeItem("access_token");
    sessionStorage.removeItem("id_token");
    sessionStorage.removeItem("refresh_token");
    sessionStorage.removeItem("expires_at");
}

function saveTokens(tokens: TokenResponse) {
    sessionStorage.setItem("access_token", tokens.access_token);
    sessionStorage.setItem("expires_at", String(Date.now() + tokens.expires_in * 1000));

    if (tokens.id_token) sessionStorage.setItem("id_token", tokens.id_token);
    if (tokens.refresh_token) sessionStorage.setItem("refresh_token", tokens.refresh_token);
}

async function createCodeChallenge(codeVerifier: string) {
    const bytes = new TextEncoder().encode(codeVerifier);
    const hash = await crypto.subtle.digest("SHA-256", bytes);
    return base64UrlEncode(new Uint8Array(hash));
}

function base64UrlEncode(bytes: Uint8Array) {
    return btoa(String.fromCharCode(...bytes))
        .replace(/\+/g, "-")
        .replace(/\//g, "_")
        .replace(/=+$/, "");
}

export function getCurrentUser() : AuthUser | null {
    const idToken = sessionStorage.getItem("id_token");

    if (!idToken) return null;

    const payload = decodeJwtPayload(idToken);

    return {
        sub: String(payload.sub),
        email: payload.email ? String(payload.email) : undefined,
        name: payload.name? String(payload.name) : undefined
    };
}

function decodeJwtPayload(token: string) : Record<string, unknown> {
    const parts = token.split(".");

    if (parts.length !== 3) throw new Error("Invalid JWT format.");

    const base64Url = parts[1];
    const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
    const json = atob(base64.padEnd(base64.length + ((4 - base64.length % 4 ) % 4), "="));

    return JSON.parse(json) as Record<string, unknown>;
}

export function isAccessTokenExpiringSoon() {
    const expiresAt = Number(sessionStorage.getItem("expires_at"));

    if (!expiresAt) return true;

    const oneMinuteFromNow = Date.now() + 60_000;

    return expiresAt <= oneMinuteFromNow;
}

export async function getValidAccessToken() {
    if (isAccessTokenExpiringSoon()) await refreshTokens();

    return getAccessToken();
}