using System.Collections.Generic;
using System.Threading.Tasks;
using MovieTracker.Models;

namespace MovieTracker.Services
{
    public interface IStorageService
    {
        Task<List<UserMovie>> LoadMoviesAsync();
        Task SaveMoviesAsync(List<UserMovie> movies);
    }
}
