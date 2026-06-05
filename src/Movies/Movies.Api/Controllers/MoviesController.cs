using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Movies.Api.Data;
using Movies.Api.Entities;

namespace Movies.Api.Controllers;

[ApiController]
[Route("api/movies")]
public sealed class MoviesController : ControllerBase
{
    private readonly MoviesDbContext _dbContext;

    public MoviesController(MoviesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [Authorize(Policy = "movies.read")]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<Movie>>> GetMovies()
    {
        var userId = GetUserId();

        var movies = await _dbContext.Movies
            .Where(movie => movie.OwnerUserId == userId)
            .OrderByDescending(movie => movie.CreatedAt)
            .ToListAsync();

        return Ok(movies);
    }

    [Authorize(Policy = "movies.write")]
    [HttpPost]
    public async Task<ActionResult<Movie>> CreateMovie(CreateMovieRequest request)
    {
        var movie = new Movie
        {
            OwnerUserId = GetUserId(),
            Title = request.Title,
            Description = request.Description,
            ReleaseYear = request.ReleaseYear,
            WatchedAt = request.WatchedAt,
            Rating = request.Rating,
            Comment = request.Comment,
        };

        _dbContext.Movies.Add(movie);
        await _dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMovieById), new { id = movie.Id }, movie);
    }

    [Authorize(Policy = "movies.read")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Movie>> GetMovieById(int id)
    {
        var userId = GetUserId();

        var movie = await _dbContext.Movies
            .SingleOrDefaultAsync(movie => movie.Id == id && movie.OwnerUserId == userId);

        return movie is null ? NotFound() : Ok(movie);
    }

    private string GetUserId()
    {
        return User.FindFirstValue("sub") ??
            throw new InvalidOperationException("Token does not contain 'sub' claim.");
    }
}

public sealed record CreateMovieRequest(
    string Title,
    string? Description,
    int? ReleaseYear,
    DateOnly? WatchedAt,
    int? Rating,
    string? Comment
);