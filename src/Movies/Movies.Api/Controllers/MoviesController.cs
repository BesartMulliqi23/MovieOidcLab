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

    [Authorize(Policy = "movies.read")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Movie>> GetMovieById(int id)
    {
        var userId = GetUserId();

        var movie = await _dbContext.Movies
            .SingleOrDefaultAsync(movie => movie.Id == id && movie.OwnerUserId == userId);

        return movie is null ? NotFound() : Ok(movie);
    }

    [Authorize(Policy = "movies.write")]
    [HttpPost]
    public async Task<ActionResult<Movie>> CreateMovie(CreateMovieRequest request)
    {
        var validationProblem = ValidateMovieInput(request.Title, request.Rating);

        if (validationProblem is not null)
        {
            return validationProblem;
        }

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

    [Authorize(Policy = "movies.write")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateMovie(int id, UpdateMovieRequest request)
    {
        var validationProblem = ValidateMovieInput(request.Title, request.Rating);

        if (validationProblem is not null)
        {
            return validationProblem;
        }

        var userId = GetUserId();

        var movie = await _dbContext.Movies
            .SingleOrDefaultAsync(movie => movie.Id == id && movie.OwnerUserId == userId);

        if (movie is null)
        {
            return NotFound();
        }

        movie.Title = request.Title;
        movie.Comment = request.Comment;
        movie.Description = request.Description;
        movie.ReleaseYear = request.ReleaseYear;
        movie.WatchedAt = request.WatchedAt;
        movie.Rating = request.Rating;
        movie.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    [Authorize(Policy = "movies.write")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteMovie(int id)
    {
        var userId = GetUserId();

        var movie = await _dbContext.Movies
            .SingleOrDefaultAsync(movie => movie.Id == id && movie.OwnerUserId == userId);

        if (movie is null)
        {
            return NotFound();
        }

        _dbContext.Movies.Remove(movie);
        await _dbContext.SaveChangesAsync();

        return NoContent();
    }

    private string GetUserId()
    {
        return User.FindFirstValue("sub") ??
            throw new InvalidOperationException("Token does not contain 'sub' claim.");
    }

    private ActionResult? ValidateMovieInput(string title, int? rating)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            ModelState.AddModelError(nameof(title), "Title is required"); 
        }

        if (rating < 1 || rating > 5)
        {
            ModelState.AddModelError(nameof(rating), "Rating must be between 1 and 5");    
        }

        return ModelState.IsValid ? null : ValidationProblem(ModelState);
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

public sealed record UpdateMovieRequest(
    string Title,
    string? Description,
    int? ReleaseYear,
    DateOnly? WatchedAt,
    int? Rating,
    string? Comment
);