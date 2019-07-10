# Asp.Net Core To Await or Not To Await
This projects shows how to use inherently async APIs without using the async-await modifiers. As well, this project demonstrates the best practice with regards to "To Await, or not to "Await", thus reducing the potential overhead of async-await
When working with async-await most people think one needs to (or must) await any method they’re calling that is “awaitable”, i.e. a method is is marked with the async modifier and returns a Task or Task<T>. This will work, but is not the correctly way to think about async-await.
Every method that is marked with the async modifier has additional overhead. Each time you await some method there is some additional overhead. So only await an async method if you need the result of the method within the method itself. If you’re simply passing the result on to the caller, then return the task of the method you’re calling rather than awaiting it. There is no change to the signature of the methods in question.

>It should be noted that in ASP.NET Core, there is no synchronization context. That means using `GetAwaiter().GetResult()` or `.Result` >or `.Wait()` will not cause deadlocks. Of course, without a synchronization context, it is imperative that your code is stateless and >threadsafe.

For example the method below does not use the returned value of the `WhenAll` method (which is an awaitable method). So there is no need to await it in this method. The Caller is free to await it or pass it on to its Caller if need be
```C#
        /// <summary>
        /// Notice that this method returns a <see cref="Task"/> but is Not async
        /// Callers may choose to materialize the result by awaiting the returned
        /// <see cref="Task"/> or using the ContinueWith callback, or not materializing
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
```
Notice that the method signature is the same either way. That is if the above method were marked with the async modifier and awaited the `WhenAll()` method the signature of the method would not change.

In the sample code, the caller of this method also in turn simply returns the result without awaiting it like so:
```C#
        public Task<IEnumerable<Movie>[]> GetAllMovies()
        {
            return _movieServiceGateway.GetAllMovies();
        }
```
Finally the Web API Controller that is making use of the result has two options.
1. Use the ContinueWith() (callback) method on Task
2. Mark the method with the `async` modifier and use await within the method like so:
```C#
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
```

| Implementation | async Modifier | Endpoint | Requests Per Second |
|----------------|----------------|----------|---------------------|
| await all the way |	Yes | GetAllMoviesAsync | 1536.46 |
| ContinueWith in Controller | No | GetAllMovies | 1542.57 |
| await only in Controller | Yes | GetAllMovies2 | 1588.13 |
| .GetAwaiter().GetResult() in Controller | No | GetAllMoviesSync | 1375.76 |

