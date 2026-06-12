import { getValidAccessToken, refreshTokens } from "./auth";

const moviesApi = "https://localhost:7131";

export type Movie = {
    id: number;
    title: string;
    description?: string;
    releaseYear?: number;
    watchedAt?: string;
    rating?: number;
    comment?: string;
};

export type MovieInput = {
    title: string;
    description?: string;
    releaseYear?: number;
    watchedAt?: string;
    rating?: number;
    comment?: string;
};

export async function getMovies() {
    return authorizedFetch<Movie[]>("/api/movies");
}

export async function createMovie(movie: MovieInput) {
    return authorizedFetch<Movie>("/api/movies", {
        method: "POST",
        body: JSON.stringify(movie)
    });
}

export async function updateMovie(id: number, movie: MovieInput) {
    await authorizedFetchNContent(`/api/movies/${id}`, {
        method: "PUT",
        body: JSON.stringify(movie)
    });
}

export async function deleteMovie(id: number) {
    await authorizedFetchNContent(`/api/movies/${id}`, {
        method: "DELETE"
    });
}

async function authorizedFetch<T>(path: string, init: RequestInit = {}) {
    const response = await sendAuthorized(path, init);

    if (!response.ok) {
        throw new Error(`API request failed: ${response.status}`);
    }

    return (await response.json()) as T;
}

async function authorizedFetchNContent(path: string, init: RequestInit = {}) {
    const response = await sendAuthorized(path, init);

    if (!response.ok) {
        throw new Error(`API request failed: ${response.status}`);
    }
}

async function sendAuthorized(path: string, init: RequestInit = {}) {
    let response = await fetch(`${moviesApi}${path}`, {
        ...init,
        headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${await getValidAccessToken()}`,
            ...init.headers
        }
    });

    if (response.status === 401) {
        await refreshTokens();

        response = await fetch(`${moviesApi}${path}`, {
            ...init,
            headers: {
                "Content-Type": "application/json",
                Authorization: `Bearer ${await getValidAccessToken()}`,
                ...init.headers
            }
        });
    }
    
    return response;
}
