using System.Diagnostics;
using Microsoft.Data.SqlClient;

namespace IMDB2025InserterNew
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Server=localhost;Database=IMDBNEW;" +
            "integrated security=True;TrustServerCertificate=True;";

            string basePath = @"C:\Datamatiker_4sem\SQL Mandatory Assignment\";

            Console.WriteLine("Starting IMDB Data Import...");
            Stopwatch totalTime = Stopwatch.StartNew();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                BulkTitles.ImportTitles(conn, basePath + "title.basics.tsv");
                BulkPersons.ImportPersons(conn, basePath + "name.basics.tsv");
                BulkCrews.ImportTitleCrew(conn, basePath + "title.crew.tsv");

                Console.WriteLine($"\nTotal import time: {totalTime.Elapsed}");
            }
        }
    }
}
