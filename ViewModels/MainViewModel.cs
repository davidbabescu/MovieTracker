using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MovieTracker.Models;
using MovieTracker.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace MovieTracker.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IMovieApiService _movieApiService;
        private readonly IStorageService _storageService;

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _statusMessage = "Welcome! Search for a movie to get started.";

        [ObservableProperty]
        private double _pendingRating = 7.0;

        [ObservableProperty]
        private UserMovie? _selectedUserMovie;

        public ObservableCollection<Movie> SearchResults { get; } = new();
        public ObservableCollection<UserMovie> WatchlistMovies { get; } = new();
        public ObservableCollection<UserMovie> WatchedMovies { get; } = new();

        public int WatchedCount => WatchedMovies.Count;
        public int WatchlistCount => WatchlistMovies.Count;

        public double AverageRating =>
            WatchedMovies.Any(m => m.Rating > 0)
                ? WatchedMovies.Where(m => m.Rating > 0).Average(m => m.Rating)
                : 0.0;

        public string AverageRatingDisplay =>
            AverageRating > 0 ? $"{AverageRating:0.0}" : "—";

        public bool HasSearchResults => SearchResults.Count > 0;
        public bool HasNoSearchResults => SearchResults.Count == 0;
        public bool HasNoWatchlist => WatchlistMovies.Count == 0;
        public bool HasNoWatched => WatchedMovies.Count == 0;

        public string SearchTabHeader =>
            HasSearchResults ? $"🔍  Results ({SearchResults.Count})" : "🔍  Search Results";

        public MainViewModel(IMovieApiService movieApiService, IStorageService storageService)
        {
            _movieApiService = movieApiService;
            _storageService = storageService;

            SearchResults.CollectionChanged += OnSearchResultsChanged;
            WatchlistMovies.CollectionChanged += OnWatchlistChanged;
            WatchedMovies.CollectionChanged += OnWatchedChanged;

            _ = LoadSavedMoviesAsync();
        }

        private void OnSearchResultsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasSearchResults));
            OnPropertyChanged(nameof(HasNoSearchResults));
            OnPropertyChanged(nameof(SearchTabHeader));
        }

        private void OnWatchlistChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(WatchlistCount));
            OnPropertyChanged(nameof(HasNoWatchlist));
        }

        private void OnWatchedChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(WatchedCount));
            OnPropertyChanged(nameof(HasNoWatched));
            OnPropertyChanged(nameof(AverageRating));
            OnPropertyChanged(nameof(AverageRatingDisplay));
        }

        private void RefreshStats()
        {
            OnPropertyChanged(nameof(WatchedCount));
            OnPropertyChanged(nameof(WatchlistCount));
            OnPropertyChanged(nameof(AverageRating));
            OnPropertyChanged(nameof(AverageRatingDisplay));
            OnPropertyChanged(nameof(HasNoWatchlist));
            OnPropertyChanged(nameof(HasNoWatched));
        }

        private async Task LoadSavedMoviesAsync()
        {
            var saved = await _storageService.LoadMoviesAsync();
            foreach (var movie in saved)
            {
                if (movie.IsWatched)
                    WatchedMovies.Add(movie);
                else
                    WatchlistMovies.Add(movie);
            }
            RefreshStats();
        }

        private async Task PersistAllMoviesAsync()
        {
            var all = WatchlistMovies.Concat(WatchedMovies).ToList();
            await _storageService.SaveMoviesAsync(all);
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                StatusMessage = "Please enter a movie title to search.";
                return;
            }

            IsLoading = true;
            StatusMessage = $"Searching for \"{SearchQuery}\"...";
            SearchResults.Clear();

            var results = await _movieApiService.SearchMoviesAsync(SearchQuery);

            foreach (var movie in results)
                SearchResults.Add(movie);

            StatusMessage = results.Count > 0
                ? $"Found {results.Count} result{(results.Count != 1 ? "s" : "")} for \"{SearchQuery}\"."
                : $"No results found for \"{SearchQuery}\".";

            IsLoading = false;
        }

        [RelayCommand]
        private async Task AddToWatchlistAsync(Movie movie)
        {
            bool alreadyExists = WatchlistMovies.Any(m => m.Movie.Id == movie.Id)
                                 || WatchedMovies.Any(m => m.Movie.Id == movie.Id);

            if (alreadyExists)
            {
                StatusMessage = $"\"{movie.Title}\" is already in your list.";
                return;
            }

            var entry = new UserMovie { Movie = movie };
            WatchlistMovies.Add(entry);
            StatusMessage = $"\"{movie.Title}\" added to Watchlist.";
            await PersistAllMoviesAsync();
        }

        [RelayCommand]
        private async Task MarkAsWatchedAsync(UserMovie userMovie)
        {
            if (userMovie.IsWatched)
            {
                StatusMessage = $"\"{userMovie.Movie.Title}\" is already marked as watched.";
                return;
            }

            WatchlistMovies.Remove(userMovie);
            userMovie.IsWatched = true;
            WatchedMovies.Add(userMovie);

            if (SelectedUserMovie?.Movie.Id == userMovie.Movie.Id)
                RefreshSelectedMovie();

            StatusMessage = $"\"{userMovie.Movie.Title}\" moved to Watched.";
            await PersistAllMoviesAsync();
        }

        [RelayCommand]
        private async Task MarkSearchResultAsWatchedAsync(Movie movie)
        {
            var existingWatchlist = WatchlistMovies.FirstOrDefault(m => m.Movie.Id == movie.Id);
            if (existingWatchlist != null)
            {
                await MarkAsWatchedAsync(existingWatchlist);
                return;
            }

            if (WatchedMovies.Any(m => m.Movie.Id == movie.Id))
            {
                StatusMessage = $"\"{movie.Title}\" is already in Watched.";
                return;
            }

            var entry = new UserMovie { Movie = movie, IsWatched = true };
            WatchedMovies.Add(entry);
            StatusMessage = $"\"{movie.Title}\" added to Watched.";
            await PersistAllMoviesAsync();
        }

        [RelayCommand]
        private async Task RateMovieAsync()
        {
            if (SelectedUserMovie == null)
            {
                StatusMessage = "Select a movie from your list to rate it.";
                return;
            }

            if (PendingRating < 1.0 || PendingRating > 10.0)
            {
                StatusMessage = "Rating must be between 1 and 10.";
                return;
            }

            SelectedUserMovie.Rating = PendingRating;
            RefreshStats();
            StatusMessage = $"\"{SelectedUserMovie.Movie.Title}\" rated {PendingRating:0.#}/10.";
            await PersistAllMoviesAsync();
        }

        [RelayCommand]
        private async Task RemoveMovieAsync(UserMovie userMovie)
        {
            WatchlistMovies.Remove(userMovie);
            WatchedMovies.Remove(userMovie);

            if (SelectedUserMovie?.Movie.Id == userMovie.Movie.Id)
                SelectedUserMovie = null;

            RefreshStats();
            StatusMessage = $"\"{userMovie.Movie.Title}\" removed from your list.";
            await PersistAllMoviesAsync();
        }

        [RelayCommand]
        private void SelectUserMovie(UserMovie? userMovie)
        {
            SelectedUserMovie = userMovie;

            if (userMovie != null)
                PendingRating = userMovie.Rating > 0 ? userMovie.Rating : 7.0;
        }

        [RelayCommand]
        private void ClearSelection()
        {
            SelectedUserMovie = null;
        }

        private void RefreshSelectedMovie()
        {
            OnPropertyChanged(nameof(SelectedUserMovie));
        }
    }
}
