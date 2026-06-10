using System;

namespace MovieTracker.Models
{
    public class UserMovie
    {
        public Movie Movie { get; set; } = new();

        public double Rating { get; set; }

        public bool IsWatched { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.Now;

        public string WatchedStatus => IsWatched ? "Watched" : "Unwatched";

        public string RatingDisplay => Rating > 0 ? $"{Rating:0.#}/10" : "Not rated";
    }
}
