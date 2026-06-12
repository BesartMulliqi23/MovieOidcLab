import React, { useEffect, useState, useRef } from "react";
import { createMovie, deleteMovie, getMovies, updateMovie, type Movie, type MovieInput } from "./api";
import { clearTokens, getAccessToken, getCurrentUser, handleCallback, login } from "./auth";
import "./App.css";

const emptyForm: MovieInput = {
    title: "",
    description: "",
    releaseYear: undefined,
    watchedAt: new Date().toISOString().slice(0, 10),
    rating: 5,
    comment: ""
};

export default function App() {
    const [movies, setMovies] = useState<Movie[]>([]);
    const [form, setForm] = useState<MovieInput>({
        ...emptyForm,
        title: "Interstellar"
    });
    const [editingMovieId, setEditingMovieId] = useState<number | null>(null);
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
        setStatus("");

        if (!form.title.trim()) {
            setStatus("Title is required");
            return;
        }

        const movieInput = normalizeMovieInput(form);

        if (editingMovieId !== null) {
            await updateMovie(editingMovieId, movieInput);
            setStatus("Movie updated.");
        }
        else {
            await createMovie(movieInput);
            setStatus("Movie added.");
        }

        setEditingMovieId(null);
        setForm(emptyForm);
        await loadMovies();
    }

    function editMovie(movie: Movie) {
        setEditingMovieId(movie.id);
        setForm({
            title: movie.title,
            description: movie.description ?? "",
            releaseYear: movie.releaseYear,
            watchedAt: movie.watchedAt ?? new Date().toISOString().slice(0, 10),
            rating: movie.rating,
            comment: movie.comment ?? ""
        });
    }

    async function removeMovie(id: number) {
        await deleteMovie(id);
        setStatus("Movie deleted.");
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
            <header className="topbar">
                <div>
                    <h1>Movies</h1>

                    {user && (
                        <p className="user-line">
                            Signed in as {user.name ?? user.email ?? user.sub}
                        </p>
                    )}
                </div>

                <button onClick={() => {
                    clearTokens();
                    window.location.reload();
                }}>
                    Sign out locally
                </button>
            </header>

            <form className="movie-form" onSubmit={submit}>
                <input 
                    placeholder="Title"
                    value={form.title} 
                    onChange={e => setForm({ ...form, title: e.target.value })} 
                />

                <input 
                    placeholder="Description"
                    value={form.description ?? ""} 
                    onChange={e => setForm({ ...form, description: e.target.value })} 
                />

                <div className="form-row">
                    <input 
                        type="number" 
                        placeholder="Release Year" 
                        value={form.releaseYear ?? ""}
                        onChange={e => setForm({ ...form, releaseYear: e.target.value ? Number(e.target.value) : undefined })} 
                    />

                    <input 
                        type="date" 
                        value={form.watchedAt ?? ""}
                        onChange={e => setForm({ ...form, watchedAt: e.target.value })}
                    />

                    <input 
                        type="number" 
                        min="1"
                        max="5"
                        placeholder="Rating"
                        value={form.rating ?? ""}
                        onChange={e => setForm({ ...form, rating: e.target.value ? Number(e.target.value) : undefined })}
                    />
                </div>

                <textarea 
                    placeholder="Comment"
                    value={form.comment ?? ""}
                    onChange={e => setForm({ ...form, comment: e.target.value })}
                />

                <div className="actions">
                    <button type="submit">
                        {editingMovieId ? "Save changes" : "Add movie"}
                    </button>

                    {editingMovieId && (
                        <button
                            type="button"
                            className="secondary"
                            onClick={() => {
                                setEditingMovieId(null);
                                setForm(emptyForm);
                            }}
                        >
                            Cancel
                        </button>
                    )}
                </div>
            </form>

            {status && <p>{status}</p>}

            <section className="movie-list">
                {movies.map(movie => (
                    <article className="movie-item" key={movie.id}>
                        <div>
                            <h2>{movie.title}</h2>
                            <p>
                                {movie.releaseYear ?? "Unknown year"}
                                {movie.watchedAt && <span> · Watched {movie.watchedAt}</span>}
                                {movie.rating && <span> · {movie.rating}/5</span>}
                            </p>
                            {movie.description && <p>{movie.description}</p>}
                            {movie.comment && <p className="comment">{movie.comment}</p>}
                        </div>

                        <div className="item-actions">
                            <button type="button" className="secondary" onClick={() => editMovie(movie)}>
                                Edit
                            </button>
                            <button type="button" className="danger" onClick={() => removeMovie(movie.id)}>
                                Delete
                            </button>
                        </div>
                    </article>
                ))}
            </section>
        </main>
    );
}

function normalizeMovieInput(form: MovieInput) : MovieInput {
    return {
        title: form.title.trim(),
        description: form.description?.trim() || undefined,
        releaseYear: form.releaseYear,
        watchedAt: form.watchedAt || undefined,
        rating: form.rating,
        comment: form.comment?.trim() || undefined
    };
}