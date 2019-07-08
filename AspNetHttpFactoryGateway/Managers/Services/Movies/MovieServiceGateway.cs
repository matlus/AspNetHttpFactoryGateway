using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using AspNetHttpFactoryGateway.Managers.Models;
using Newtonsoft.Json;

namespace AspNetHttpFactoryGateway.Managers.Services
{
    internal sealed class MovieServiceGateway
    {
        /// <summary>
        /// HttpClient is threadsafe and thus a single instance can
        /// make multiple concurrent calls across multiple threads.
        ///  So having a static member (that is re-used), in a non-static class is ok
        /// </summary>
        private static readonly HttpClient s_httpClient = new HttpClient(); 
        private static readonly string[] sources =
        {
            "https://matlusstorage.blob.core.windows.net/membervideos/action.json",
            "https://matlusstorage.blob.core.windows.net/membervideos/drama.json",
            "https://matlusstorage.blob.core.windows.net/membervideos/thriller.json",
            "https://matlusstorage.blob.core.windows.net/membervideos/scifi.json",
        };

        /// <summary>
        /// Since the only place this method would have an "await" would be in the last line
        /// "Task.WhenAll" and it returns. We should not mark this method with the async modifier.
        /// At the call site there is no difference, however, the code generated for this method
        /// will be quite a bit less (without the await). i.e. no state machine
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<Movie>[]> GetAllMoviesByGenreConcurrently()
        {
            var allMoviesTasks = new List<Task<IEnumerable<Movie>>>();

            foreach (var url in sources)
            {
                allMoviesTasks.Add(DownloadMoviesAsync(url));
            }

            return Task.WhenAll(allMoviesTasks);
        }

        /// <summary>
        /// Notice that this method returns a <see cref="Task"/> but is Not async
        /// Callers may choose the materialize the result by awaiting the returned
        /// <see cref="Task"/> and using the ContinueWith callback, or not materializing
        /// the returned task and simply passing it to their caller if they don't need the
        /// result of the task themselves.
        /// Since the only place this method would have an "await" would be in the last line
        /// "Task.WhenAll" and it returns. We should not mark this method with the async modifier.
        /// At the call site there is no difference, however, the code generated for this method
        /// will be quite a bit less (without the await). i.e. no state machine
        /// </summary>
        /// <returns></returns>
        public Task<IEnumerable<Movie>[]> GetAllMovies()
        {
            var allMoviesTasks = new List<Task<IEnumerable<Movie>>>();
            allMoviesTasks.Add(DownloadMoviesAsync("https://matlusstorage.blob.core.windows.net/membervideos/AllMovies.json"));
            return Task.WhenAll(allMoviesTasks);
        }

        /// <summary>
        /// Notice that this method is async but it calls the <see cref="GetAllMovies"/>
        /// method that returns as <see cref="Task"/> but is not async
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<IEnumerable<Movie>>> GetAllMoviesAsync()
        {
            return await GetAllMovies();
        }

        /// <summary>
        /// When the returned content is large, rather than using <see cref="HttpContent"/>.ReadAsStringAsync
        /// (Which will alloated memory to hold the entire content),
        /// use <see cref="HttpContent"/>.ReadAsStreamAsync to read from the stream directly, thus
        /// preventing additional memory allocations and thus reducing pressure on the GC
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Movie>> DownloadMoviesAsync(string url)
        {
            var httpResponseMessage = await s_httpClient.GetAsync(url).ConfigureAwait(false);
            httpResponseMessage.EnsureSuccessStatusCode();
            using (var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync())
            using (var streamReader = new StreamReader(responseStream))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var jsonSerializer = new JsonSerializer();
                return jsonSerializer.Deserialize<IEnumerable<Movie>>(jsonReader);
            }
        }
    }
}
