using IMDB.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMDB_Fabric.Client
{
    public class MovieEvent : IMovieEvent
    {
        public void LikeMovie(Movie movie)
        {
            Console.WriteLine($"Movie: {movie.Name} ({movie.Year}), \r\n\t{movie.ImageUrl}");
        }

        public void LikeStar(Star star)
        {
            Console.WriteLine($"Actor: {star.Name} ({star.Birthdate:yyyy-MM-dd}), \r\n\t{star.ImageUrl}");
        }
    }
}
