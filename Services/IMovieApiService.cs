using System.Collections.Generic;
using System.Threading.Tasks;
using MovieTracker.Models;

namespace MovieTracker.Services
{
    public interface IMovieApiService
    {
        Task<List<Movie>> SearchMoviesAsync(string query);
    }
}
