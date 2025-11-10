using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IMDB2025InserterNew
{
    public class Name
    {
        //nconst primaryName birthYear deathYear   primaryProfession knownForTitles

        public int PersonId { get; set; }
        public string PrimaryName { get; set; }
        public int? BirthYear { get; set; }
        public int? DeathYear { get; set; }
        public List<string>? PrimaryProfession { get; set; }
        public List<string>? KnownForTitles { get; set; }
    }
}
