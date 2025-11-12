using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMDB2025InserterNew
{
    public static class ConsoleUI
    {
        public static void ShowMainMenu(SqlConnection conn)
        {

            bool exit = false;

            while (!exit)
            {
                Console.WriteLine("\n═══════════════════════════════════════");
                Console.WriteLine("MAIN MENU");
                Console.WriteLine("═══════════════════════════════════════");
                Console.WriteLine("1. Search Movies");
                Console.WriteLine("2. Search Persons");
                Console.WriteLine("3. Add Movie");
                Console.WriteLine("4. Add Person");
                Console.WriteLine("5. Update Movie");
                Console.WriteLine("6. Delete Movie");
                Console.WriteLine("7. Exit");
                Console.WriteLine("═══════════════════════════════════════");
                Console.Write("Select an option (1-7): ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        SearchMovies(conn);
                        break;
                    // TODO
                    case "2":
                        SearchPersons(conn);
                        break;
                    case "3":
                        AddMovie(conn);
                        break;
                    case "4":
                        AddPerson(conn);
                        break;
                    case "5":
                        UpdateMovie(conn);
                        break;
                    case "6":
                        DeleteMovie(conn);
                        break;
                    case "7":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option");
                        break;
                }
            }
        }

        static void SearchMovies(SqlConnection conn)
        {
            Console.WriteLine("\n--- SEARCH MOVIES ---");
            Console.Write("Enter search term: ");
            string searchTerm = Console.ReadLine();


            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_SearchMovies", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SearchTerm", searchTerm);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        Console.WriteLine("\nSearch Results:");
                        Console.WriteLine("─────────────────────────────────────────────────────────────────────");
                        Console.WriteLine($"{"ID",-8} {"Title",-40} {"Year",-6} {"Type",-15} {"Runtime",-8}");
                        Console.WriteLine("─────────────────────────────────────────────────────────────────────");

                        int count = 0;
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string title = reader.GetString(1);
                            string year = reader.IsDBNull(3) ? "N/A" : reader.GetInt32(3).ToString();
                            string type = reader.GetString(5);
                            string runtime = reader.IsDBNull(6) ? "N/A" : reader.GetInt32(6).ToString();

                            Console.WriteLine($"{id,-8} {/*TruncateString(*/title, /*40),*/-40} {year,-6} {type,-15} {runtime,-8}");
                            count++;

                            if (count >= 10)
                            {
                                Console.WriteLine("Showing first 10 results");
                                break;
                            }
                        }

                        if (count == 0)
                        {
                            Console.WriteLine("No movies found.");
                        }
                        else
                        {
                            Console.WriteLine($"\nTotal result show: {count}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n Error: {ex.Message}");
            }
        }

        static void SearchPersons(SqlConnection conn)
        {
            Console.WriteLine("\n--- SEARCH PERSONS ---");
            Console.Write("Enter search term: ");
            string searchTerm = Console.ReadLine();

            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_SearchPersons", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SearchTerm", searchTerm);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        Console.WriteLine("\nSearch Results:");
                        Console.WriteLine("────────────────────────────────────────────────────────");
                        Console.WriteLine($"{"ID",-10} {"Name",-35} {"Birth",-8} {"Death",-8}");
                        Console.WriteLine("────────────────────────────────────────────────────────");

                        int count = 0;
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string name = reader.GetString(1);
                            string birthYear = reader.IsDBNull(2) ? "N/A" : reader.GetInt32(2).ToString();
                            string deathYear = reader.IsDBNull(3) ? "N/A" : reader.GetInt32(3).ToString();

                            Console.WriteLine($"{id,-10} {/*TruncateString(*/name/*, 35)*/,-35} {birthYear,-8} {deathYear,-8}");
                            count++;

                            if (count >= 10)
                            {
                                Console.WriteLine("Showing first 10 results");
                                break;
                            }
                        }

                        if (count == 0)
                        {
                            Console.WriteLine("No results found");
                        }
                        else
                        {
                            Console.WriteLine($"Total result show: {count}");
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error{ex.Message}");
            }
        }

        static void AddMovie(SqlConnection conn)
        {
            Console.WriteLine("\n--- ADD NEW MOVIE ---");

            Console.Write("Primary title: ");
            string primaryTitle = Console.ReadLine();

            Console.Write("Original title (Enter to skip): ");
            string originalTitle = Console.ReadLine();

            Console.Write("Is adult? (y/n): ");
            bool isAdult = Console.ReadLine()?.ToLower() == "y"; ;

            Console.Write("Start year (Enter to skip): ");
            string startYearInput = Console.ReadLine();
            int? startYear = string.IsNullOrWhiteSpace(startYearInput) ? null : int.Parse(startYearInput);

            Console.Write("End year (Enter to skip): ");
            string endYearInput = Console.ReadLine();
            int? endYear = string.IsNullOrWhiteSpace(endYearInput) ? null : int.Parse(endYearInput);

            Console.Write("Runtime (minutes): ");
            string runtimeInput = Console.ReadLine();
            int? runtime = string.IsNullOrWhiteSpace(runtimeInput) ? null : int.Parse(runtimeInput);

            Console.Write("Type (movie, short etc.): ");
            string type = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(type))
            {
                type = "movie";
            }

            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_AddMovie", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PrimaryTitle", primaryTitle);
                    cmd.Parameters.AddWithValue("@OriginalTitle", string.IsNullOrWhiteSpace(originalTitle));
                    cmd.Parameters.AddWithValue("@IsAdult", isAdult);
                    cmd.Parameters.AddWithValue("@StartYear", startYear.HasValue ? startYear.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@EndYear", endYear.HasValue ? endYear.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@RuntimeMinutes", runtime.HasValue ? runtime.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@TypeName", type);

                    object result = cmd.ExecuteScalar();
                    Console.WriteLine($"Movie added successfully - New ID: {result}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding movie: {ex.Message}");
            }
        }

        static void AddPerson(SqlConnection conn)
        {
            Console.WriteLine("\n--- ADD NEW PERSON ---");

            Console.Write("Name: ");
            string name = Console.ReadLine();

            Console.Write("Birth year (Enter to skip): ");
            string birthYearInput = Console.ReadLine();
            int? birthYear = string.IsNullOrWhiteSpace(birthYearInput) ? null : int.Parse(birthYearInput);

            Console.Write("Death year (Enter to skip): ");
            string deathYearInput = Console.ReadLine();
            int? deathYear = string.IsNullOrWhiteSpace(deathYearInput) ? null : int.Parse(deathYearInput);

            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_AddPerson", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@PrimaryName", name);
                    cmd.Parameters.AddWithValue("@BirthYear", birthYear.HasValue ? birthYear.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@DeathYear", deathYear.HasValue ? deathYear.Value : DBNull.Value);

                    object result = cmd.ExecuteScalar();
                    Console.WriteLine($"Person added successfully - New ID: {result}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding person: {ex.Message}");
            }
        }

        static void UpdateMovie(SqlConnection conn)
        {
            Console.WriteLine("--- UPDATE MOVIE ---");

            Console.Write("Enter Movie ID to update: ");
            if (!int.TryParse(Console.ReadLine(), out int titleId))
            {
                Console.WriteLine("Invalid ID");
                return;
            }

            Console.WriteLine("Searching for movie...");

            string currentTitle = "";
            string currentOriginalTitle = "";
            int? currentStartYear = null;
            int? currentEndYear = null;
            int? currentRuntime = null;
            string currentType = "";
            bool currentIsAdult = false;
            bool movieFound = false;


            using (SqlCommand searchCmd = new SqlCommand("sp_GetMovieById", conn))
            {
                searchCmd.CommandType = CommandType.StoredProcedure;
                searchCmd.Parameters.AddWithValue("@TitleId", titleId);

                using (SqlDataReader reader = searchCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        movieFound = true;
                        currentTitle = reader.GetString(1);
                        currentOriginalTitle = reader.IsDBNull(2) ? "" : reader.GetString(2);
                        currentStartYear = reader.IsDBNull(3) ? null : (int?)reader.GetInt32(3);
                        currentEndYear = reader.IsDBNull(4) ? null : (int?)reader.GetInt32(4);
                        currentType = reader.GetString(5);
                        currentRuntime = reader.IsDBNull(6) ? null : (int?)reader.GetInt32(6);
                        currentIsAdult = reader.GetBoolean(7);

                        Console.WriteLine($"\nFound: {currentTitle} ({currentStartYear?.ToString() ?? "N/A"})");
                        Console.WriteLine($"Current Type: {currentType}");
                        Console.WriteLine($"Current Runtime: {currentRuntime?.ToString() ?? "N/A"} minutes");
                        Console.WriteLine($"Is Adult: {(currentIsAdult ? "Yes" : "No")}");
                    }


                }
            }

            if (!movieFound)
            {
                Console.WriteLine("Movie not found");
                return;
            }

            Console.WriteLine("\nEnter new values (press Enter to keep current value): ");

            Console.Write("Primary Title: ");
            string primaryTitle = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(primaryTitle))
            {
                primaryTitle = currentTitle;

            }

            Console.Write("Original Title: ");
            string originalTitle = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(primaryTitle))
            {
                originalTitle = currentOriginalTitle;

            }

            Console.Write("Is Adult? (y/n): ");
            string adultInput = Console.ReadLine();
            bool isAdult = string.IsNullOrWhiteSpace(adultInput) ? currentIsAdult : adultInput.ToLower() == "y";

            Console.Write("Start year: ");
            string startYearInput = Console.ReadLine();
            int? startYear = string.IsNullOrWhiteSpace(startYearInput) ? currentStartYear : int.Parse(startYearInput);

            Console.Write("End year: ");
            string endYearInput = Console.ReadLine();
            int? endYear = string.IsNullOrWhiteSpace(endYearInput) ? currentEndYear : int.Parse(endYearInput);

            Console.Write("Runtime (minutes): ");
            string runtimeInput = Console.ReadLine();
            int? runtime = string.IsNullOrWhiteSpace(runtimeInput) ? currentRuntime : int.Parse(runtimeInput);

            Console.Write("Type: ");
            string type = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(type))
            {
                type = currentType;
            }

            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_UpdateMovie", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TitleId", titleId);
                    cmd.Parameters.AddWithValue("@PrimaryTitle", primaryTitle);
                    cmd.Parameters.AddWithValue("@OriginalTitle", string.IsNullOrWhiteSpace(originalTitle) ? DBNull.Value : originalTitle);
                    cmd.Parameters.AddWithValue("@IsAdult", isAdult);
                    cmd.Parameters.AddWithValue("@StartYear", startYear.HasValue ? startYear.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@EndYear", endYear.HasValue ? endYear.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@RuntimeMinutes", runtime.HasValue ? runtime.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@TypeName", type);

                    cmd.ExecuteNonQuery();
                    Console.WriteLine("Movie updated successfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating movie: {ex.Message}");
            }
        }

        static void DeleteMovie(SqlConnection conn)
        {
            Console.WriteLine("\n--- DELETE MOVIE ---");

            Console.Write("Enter Movie ID to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int titleId))
            {
                Console.WriteLine("Invalid ID");
                return;
            }

            Console.Write($"Are you sure you want to delete movie with ID: {titleId} (y/n): ");
            if (Console.ReadLine()?.ToLower() != "y")
            {
                Console.WriteLine("Deletion of movie cancelled.");
                return;
            }

            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_DeleteMovie", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TitleId", titleId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bool success = reader.GetInt32(0) == 1;
                            string message = reader.GetString(1);
                            Console.WriteLine($"{message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Deletion of movie failed error: {ex.Message}");
            }
        }
    }
}