import React, { useState } from "react";
import "./App.css";

const authServerBaseUrl = "https://localhost:7290";

function getReturnUrl() {
    const params = new URLSearchParams(window.location.search);
    return params.get("returnUrl") ?? `${authServerBaseUrl}/api/account/me`;
}

export default function App() {
    const path = window.location.pathname;

    if (path === "/register") {
        return <RegisterPage />;
    }

    return <LoginPage />;
}

function LoginPage() {
    const [email, setEmail] = useState("alice@example.com");
    const [password, setPassword] = useState("Password123");
    const [error, setError] = useState<string | null>(null);

    async function submit(event: React.SubmitEvent<HTMLFormElement>) {
        event.preventDefault();
        setError(null);

        const response = await fetch(`${authServerBaseUrl}/api/account/login`, {
            method: "POST",
            credentials: "include",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ email, password, rememberMe: true})
        });

        if (!response.ok) {
            setError("Invalid email or password");
            return;
        }

        window.location.href = getReturnUrl();
    }

    return (
        <main className="auth-shell">
            <form className="auth-panel" onSubmit={submit}>
                <h1>Sign In</h1>

                <label>
                    Email
                    <input 
                        type="email" 
                        value={email} 
                        onChange={e => setEmail(e.target.value)} 
                    />
                </label>

                <label>
                    Password
                    <input 
                        type="password" 
                        value={password} 
                        onChange={e => setPassword(e.target.value)} 
                    />
                </label>

                {error && <p className="error">{error}</p>}

                <button type="submit">Sign In</button>

                <a href={`/register${window.location.search}`}>Create account</a>
            </form>
        </main>
    );
}

function RegisterPage() {
    const [displayName, setDisplayName] = useState("Alice");
    const [email, setEmail] = useState("alice@example.com");
    const [password, setPassword] = useState("Password123");
    const [error, setError] = useState<string | null>(null);

    async function submit(event: React.SubmitEvent<HTMLFormElement>) {
        event.preventDefault();
        setError(null);

        const response = await fetch(`${authServerBaseUrl}/api/account/register`, {
            method: "POST",
            credentials: "include",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ displayName, email, password })
        });

        if (!response.ok) {
            setError("Registration failed");
            return;
        }

        window.location.href = getReturnUrl();
    }

    return (
        <main className="auth-shell">
            <form className="auth-panel" onSubmit={submit}>
                <h1>Create Account</h1>

                <label>
                    Display Name
                    <input 
                        type="text"
                        value={displayName}
                        onChange={e => setDisplayName(e.target.value)}
                    />
                </label>

                <label>
                    Email
                    <input 
                        type="email"
                        value={email}
                        onChange={e => setEmail(e.target.value)}
                    />
                </label>

                <label>
                    Password
                    <input 
                        type="password"
                        value={password}
                        onChange={e => setPassword(e.target.value)}
                    />
                </label>

                {error && <p className="error">{error}</p>}

                <button type="submit">Create account</button>

                <a href={`/login${window.location.search}`}>Already have an account?</a>
            </form>
        </main>
    );
}