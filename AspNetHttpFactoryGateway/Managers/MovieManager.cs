using AspNetHttpFactoryGateway.Managers.Models;
using AspNetHttpFactoryGateway.Managers.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspNetHttpFactoryGateway.Managers
{
    internal sealed class MovieManager
    {
        private readonly MovieServiceGateway _movieServiceGateway;

        public MovieManager()
        {
            _movieServiceGateway = MakeMovieServiceGateway();
        }

        public Task<IEnumerable<Movie>[]> GetAllMovies()
        {
            return _movieServiceGateway.GetAllMovies();
        }

        public async Task<IEnumerable<IEnumerable<Movie>>> GetAllMoviesAsync()
        {
            return await _movieServiceGateway.GetAllMoviesAsync();
        }

        private MovieServiceGateway MakeMovieServiceGateway()
        {
            return new MovieServiceGateway();
        }
    }
}
