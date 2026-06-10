using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MovieTracker.Models;

namespace MovieTracker.Services
{
    public class MovieApiService : IMovieApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        private const string ApiKey = "3cf95474c6eb3a921161f71adc5edff9";
        private const string BaseUrl = "https://api.themoviedb.org/3";

        public MovieApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<List<Movie>> SearchMoviesAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<Movie>();

            var encodedQuery = Uri.EscapeDataString(query);
            var requestUrl = $"{BaseUrl}/search/movie?api_key={ApiKey}&query={encodedQuery}&language=en-US&page=1";

            try
            {
                var response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var searchResult = JsonSerializer.Deserialize<TmdbSearchResponse>(content, _jsonOptions);

                return searchResult?.Results ?? new List<Movie>();
            }
            catch (HttpRequestException)
            {
                return new List<Movie>();
            }
            catch (JsonException)
            {
                return new List<Movie>();
            }
        }
    }

    internal class TmdbSearchResponse
    {
        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("results")]
        public List<Movie> Results { get; set; } = new();

        [JsonPropertyName("total_results")]
        public int TotalResults { get; set; }

        [JsonPropertyName("total_pages")]
        public int TotalPages { get; set; }
    }
}
