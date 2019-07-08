namespace AspNetHttpFactoryGateway.Managers.Models
{
    public class Movie
    {
        public string Title { get; }
        public int Year { get; }
        public string Genre { get; }
        public string ImageUrl { get; }

        public Movie(string title, int year, string genre, string imageUrl)
        {
            Title = title;
            Year = year;
            Genre = genre;
            ImageUrl = imageUrl;
        }
    }
}
