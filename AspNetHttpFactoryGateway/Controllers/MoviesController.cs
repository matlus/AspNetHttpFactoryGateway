using System.Collections.Generic;
using System.Threading.Tasks;
using AspNetHttpFactoryGateway.Managers;
using AspNetHttpFactoryGateway.Managers.Models;
using Microsoft.AspNetCore.Mvc;

namespace AspNetHttpFactoryGateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly MovieManager _movieManager = new MovieManager();

        [HttpGet("getallmoviesasync")]
        public async Task<IEnumerable<IEnumerable<Movie>>> GetAllMoviesAsync()
        {
            return await _movieManager.GetAllMoviesAsync().ConfigureAwait(false);

        }

        [HttpGet("getallmovies")]
        public IEnumerable<IEnumerable<Movie>> GetAllMovies()
        {
            return _movieManager.GetAllMovies()
            .ContinueWith(moviesTask =>
            {
                return moviesTask.Result;
            }).Result;
        }

        [HttpGet("getallmovies2")]
        public async Task<IEnumerable<IEnumerable<Movie>>> GetAllMovies2()
        {
            return await _movieManager.GetAllMovies();
        }
    }
}
