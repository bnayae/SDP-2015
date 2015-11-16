using IMDB.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMDB;

namespace IMDB_Fabric.Client
{
    public class ImdbEvents : IImdbEvents, IImdbTopRatedEvents, IImdbFaultEvents
    {
        public void Changed(ImdbType type, ProfileRate[] items)
        {
            if (type == ImdbType.Star)
                Console.ForegroundColor = ConsoleColor.White;
            else if(type == ImdbType.Movie)
                Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine($"--------- Top {type} ------------");
            foreach (var item in items)
            {
                Console.WriteLine($"{item.Name}: {item.Count}");
            }
            Console.WriteLine($"-----------------------------------");
            Console.WriteLine();
            Console.ResetColor();
        }

        public void LikeMovie(Movie movie)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Movie: {movie.Name} ({movie.Year}) by {movie.Sender.Name} \r\n\t{movie.Sender.ImageUrl}");
            Console.WriteLine();
            Console.ResetColor();
        }

        public void LikeStar(Star star)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Actor: {star.Name} ({star.Birthdate:yyyy-MM-dd}) by {star.Sender.Name} \r\n\t{star.Sender.ImageUrl}");
            Console.WriteLine();
            Console.ResetColor();
        }

        public void ParserError(string url)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"Parse Error: {url}");
            Console.WriteLine();
            Console.ResetColor();
        }
    }
}
