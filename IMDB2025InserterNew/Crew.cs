using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMDB2025InserterNew
{
    public class Crew
    {

        // tconst	directors	writers

        public int TitleId { get; set; }
        public List<string>? Directors { get; set; }
        public List<string>? Writers { get; set; }
    }
}
