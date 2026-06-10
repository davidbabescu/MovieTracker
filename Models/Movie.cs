using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MovieTracker.Models
{
    public class Movie
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("release_date")]
        public string ReleaseDate { get; set; } = string.Empty;

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }

        [JsonPropertyName("overview")]
        public string Overview { get; set; } = string.Empty;

        [JsonPropertyName("vote_average")]
        public double VoteAverage { get; set; }

        [JsonPropertyName("genre_ids")]
        public List<int> GenreIds { get; set; } = new();

        [JsonIgnore]
        public string PosterUrl => string.IsNullOrEmpty(PosterPath)
            ? "https://placehold.co/200x300?text=No+Image"
            : $"https://image.tmdb.org/t/p/w500{PosterPath}";

        [JsonIgnore]
        public string Year => ReleaseDate?.Length >= 4 ? ReleaseDate[..4] : "N/A";

        [JsonIgnore]
        public string DisplayTitle => $"{Title} ({Year})";
    }
}
