using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using MovieTracker.Models;

namespace MovieTracker.Services
{
    public class StorageService : IStorageService
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public StorageService()
        {
            _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "movies_data.json");
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<List<UserMovie>> LoadMoviesAsync()
        {
            if (!File.Exists(_filePath))
                return new List<UserMovie>();

            try
            {
                var json = await File.ReadAllTextAsync(_filePath);

                if (string.IsNullOrWhiteSpace(json))
                    return new List<UserMovie>();

                return JsonSerializer.Deserialize<List<UserMovie>>(json, _jsonOptions)
                       ?? new List<UserMovie>();
            }
            catch (JsonException)
            {
                return new List<UserMovie>();
            }
            catch (IOException)
            {
                return new List<UserMovie>();
            }
        }

        public async Task SaveMoviesAsync(List<UserMovie> movies)
        {
            var json = JsonSerializer.Serialize(movies, _jsonOptions);
            await File.WriteAllTextAsync(_filePath, json);
        }
    }
}
