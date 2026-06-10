import React, { useEffect, useState, useRef } from "react";
import { createMovie, getMovies, type Movie } from "./api";
import { clearTokens, getAccessToken, getCurrentUser, handleCallback, login } from "./auth";
import "./App.css";

export default function App() {
    const [movies, setMovies] = useState<Movie[]>([]);
    const [title, setTitle] = useState("Interstellar");
    const [status, setStatus] = useState("");

    const callbackHandled = useRef(false);

    const user = getCurrentUser();

    useEffect(() => {
        if (window.location.pathname === "/callback") {
            if (callbackHandled.current) return;

            callbackHandled.current = true;

            handleCallback()
                .then(() => {
                    window.history.replaceState({}, "", "/");
                    setStatus("Signed in.");
                    return loadMovies();
                })
                .catch(error => setStatus(error.message));

            return;
        }

        if (getAccessToken()) {
            loadMovies();
        }
    }, []);

    async function loadMovies() {
        const result = await getMovies();
        setMovies(result);
    }

    async function submit(event: React.SubmitEvent<HTMLFormElement>) {
        event.preventDefault();

        await createMovie({
            title,
            description: "Created from the React SPA",
            releaseYear: 2014,
            watchedAt: new Date().toISOString().slice(0, 10),
            rating: 5,
            comment: "OAuth-powered movie entry."
        });

        setTitle("");
        await loadMovies();
    }

    if (!getAccessToken()) {
        return (
            <main className="shell">
                <h1>Movies</h1>

                <button onClick={login}>Sign in</button>

                {status && <p>{status}</p>}
            </main>
        );
    }

    return (
        <main className="shell">
            <header>
                <h1>Movies</h1>

                {user && (
                    <p className="user-line">
                        Signed in as {user.name ?? user.email ?? user.sub}
                    </p>
                )}

                <button onClick={() => {
                    clearTokens();
                    window.location.reload();
                }}>
                    Sign out locally
                </button>
            </header>

            <form onSubmit={submit}>
                <input 
                    value={title} 
                    onChange={e => setTitle(e.target.value)} 
                />
                <button type="submit">Add movie</button>
            </form>

            {status && <p>{status}</p>}

            <ul>
                {movies.map(movie => (
                    <li key={movie.id}>
                        <strong>{movie.title}</strong>
                        {movie.releaseYear && <span> ({movie.releaseYear})</span>}
                    </li>
                ))}
            </ul>
        </main>
    );
}