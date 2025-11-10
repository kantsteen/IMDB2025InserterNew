using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMDB2025InserterNew
{
    public class Title
    {
        public int TitleId { get; set; }
        public int TitleType { get; set; }
        public string PrimaryTitle { get; set; }
        public string? OriginalTitle { get; set; }
        public bool IsAdult { get; set; }
        public int? StartYear { get; set; }
        public int? EndYear { get; set; }
        public int? RuntimeMinutes { get; set; }
        public List<string>? Genres { get; set; }

        //public Title()
        //{
        //    Genres = new List<string>();
        //}

        //public string ToSQL()
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.Append($"INSERT INTO Titles (Id, TypeId, PrimaryTitle, " +
        //        $"OriginalTitle, IsAdult, StartYear, EndYear, RuntimeMinutes) " +
        //        $"VALUES (");
        //    sb.Append($"{Id}, ");
        //    sb.Append($"{TitleType}, ");
        //    sb.Append($"'{PrimaryTitle.Replace("'", "''")}', ");
        //    sb.Append(OriginalTitle != null ? $"'{OriginalTitle.Replace("'", "''")}', " : "NULL, ");
        //    sb.Append($"{(IsAdult ? 1 : 0)}, ");
        //    sb.Append(StartYear.HasValue ? $"{StartYear.Value}, " : "NULL, ");
        //    sb.Append(EndYear.HasValue ? $"{EndYear.Value}, " : "NULL, ");
        //    sb.Append(RuntimeMinutes.HasValue ? $"{RuntimeMinutes.Value}" : "NULL");
        //    sb.Append(");");
        //    //foreach (string genre in Genres)
        //    //{
        //    //    sb.AppendLine();
        //    //    sb.Append($"INSERT INTO TitleGenres (TitleId, Genre) VALUES ({Id}, '{genre.Replace("'", "''")}');");
        //    //}
        //    return sb.ToString();
        //}
    }
}
