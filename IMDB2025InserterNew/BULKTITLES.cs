using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMDB2025InserterNew
{
    public static class BulkTitles
    {
        public static void ImportTitles(SqlConnection conn, string filename)
        {
            Console.WriteLine("\n=== Importing Titles ===");
            Stopwatch sw = Stopwatch.StartNew();

            Dictionary<string, int> titleTypes = new Dictionary<string, int>();
            Dictionary<string, int> genres = new Dictionary<string, int>();

            DataTable titlesTable = CreateTitlesDataTable();
            DataTable genresTable = CreateGenresDataTable();
            DataTable titleGenresTable = CreateTitleGenresDataTable();

            int lineCount = 0;
            int errorCount = 0;

            foreach (string line in File.ReadLines(filename).Skip(1))
            {
                lineCount++;
                if (lineCount % 100000 == 0)
                    Console.WriteLine($"Processed {lineCount:N0} titles...");

                string[] values = line.Split('\t');
                if (values.Length != 9)
                {
                    errorCount++;
                    continue;
                }

                try
                {
                    int titleId = int.Parse(values[0].Substring(2));

                    // Get or create title type
                    string typeStr = values[1];
                    if (!titleTypes.ContainsKey(typeStr))
                    {
                        int typeId = InsertLookupValue(conn, "TitleTypes", "Type", typeStr);
                        titleTypes[typeStr] = typeId;
                    }

                    DataRow titleRow = titlesTable.NewRow();
                    titleRow["TitleId"] = titleId;
                    titleRow["TypeId"] = titleTypes[typeStr];
                    titleRow["PrimaryTitle"] = values[2];
                    titleRow["OriginalTitle"] = values[3] == "\\N" ? DBNull.Value : values[3];
                    titleRow["IsAdult"] = values[4] == "1";
                    titleRow["StartYear"] = values[5] == "\\N" ? DBNull.Value : int.Parse(values[5]);
                    titleRow["EndYear"] = values[6] == "\\N" ? DBNull.Value : int.Parse(values[6]);
                    titleRow["RuntimeMinutes"] = values[7] == "\\N" ? DBNull.Value : int.Parse(values[7]);
                    titlesTable.Rows.Add(titleRow);

                    // Parse genres
                    if (values[8] != "\\N")
                    {
                        string[] genreList = values[8].Split(',');
                        foreach (string genre in genreList)
                        {
                            // Get or create genre
                            if (!genres.ContainsKey(genre))
                            {
                                DataRow genreRow = genresTable.NewRow();
                                genreRow["Genre"] = genre;
                                genresTable.Rows.Add(genreRow);

                                genres[genre] = 0;
                            }

                            // Add to junction table 
                            DataRow junctionRow = titleGenresTable.NewRow();
                            junctionRow["TitleId"] = titleId;
                            junctionRow["GenreName"] = genre;
                            titleGenresTable.Rows.Add(junctionRow);
                        }
                    }

                    // Bulk insert every 50,000 rows to manage memory
                    if (titlesTable.Rows.Count >= 50000)
                    {
                        BulkInsertTitles(conn, titlesTable);
                        titlesTable.Clear();
                    }
                }
                catch (Exception ex)
                {
                    errorCount++;
                    if (errorCount <= 10)
                        Console.WriteLine($"Error on line {lineCount}: {ex.Message}");
                }
            }

            // Insert remaining rows
            if (titlesTable.Rows.Count > 0)
                BulkInsertTitles(conn, titlesTable);

            // Bulk insert genres
            if (genresTable.Rows.Count > 0)
                BulkInsertGenres(conn, genresTable);

            // Get genre IDs and insert title-genre relationships
            LoadGenreIds(conn, genres);
            InsertTitleGenres(conn, titleGenresTable, genres);

            sw.Stop();
            Console.WriteLine($"Imported {lineCount:N0} titles in {sw.Elapsed}");
            Console.WriteLine($"Errors: {errorCount}");
        }

        static DataTable CreateTitlesDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("TitleId", typeof(int));
            dt.Columns.Add("TypeId", typeof(int));
            dt.Columns.Add("PrimaryTitle", typeof(string));
            dt.Columns.Add("OriginalTitle", typeof(string));
            dt.Columns.Add("IsAdult", typeof(bool));
            dt.Columns.Add("StartYear", typeof(int));
            dt.Columns.Add("EndYear", typeof(int));
            dt.Columns.Add("RuntimeMinutes", typeof(int));
            return dt;
        }

        static DataTable CreateGenresDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("GenreId", typeof(int));
            dt.Columns.Add("Genre", typeof(string));
            return dt;
        }

        static DataTable CreateTitleGenresDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("TitleId", typeof(int));
            dt.Columns.Add("GenreName", typeof(string));
            return dt;
        }

        static int InsertLookupValue(SqlConnection conn, string tableName, string columnName, string value)
        {
            using (SqlCommand cmd = new SqlCommand(
                $"INSERT INTO {tableName} ({columnName}) VALUES (@Value); SELECT SCOPE_IDENTITY();", conn))
            {
                cmd.Parameters.AddWithValue("@Value", value);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        static void BulkInsertTitles(SqlConnection conn, DataTable dt)
        {
            using (SqlBulkCopy bulk = new SqlBulkCopy(conn))
            {
                bulk.DestinationTableName = "Titles";
                bulk.BatchSize = 10000;
                bulk.BulkCopyTimeout = 600;
                bulk.WriteToServer(dt);
            }
        }

        static void BulkInsertGenres(SqlConnection conn, DataTable dt)
        {
            using (SqlBulkCopy bulk = new SqlBulkCopy(conn))
            {
                bulk.DestinationTableName = "Genres";
                bulk.WriteToServer(dt);
            }
        }

        static void LoadGenreIds(SqlConnection conn, Dictionary<string, int> genres)
        {
            using (SqlCommand cmd = new SqlCommand("SELECT GenreId, Genre FROM Genres", conn))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string genre = reader.GetString(1);
                    if (genres.ContainsKey(genre))
                        genres[genre] = reader.GetInt32(0);
                }
            }
        }

        static void InsertTitleGenres(SqlConnection conn, DataTable tempTable, Dictionary<string, int> genreIds)
        {
            DataTable finalTable = new DataTable();
            finalTable.Columns.Add("TitleId", typeof(int));
            finalTable.Columns.Add("GenreId", typeof(int));

            foreach (DataRow row in tempTable.Rows)
            {
                string genreName = row["GenreName"].ToString();
                if (genreIds.ContainsKey(genreName) && genreIds[genreName] > 0)
                {
                    DataRow newRow = finalTable.NewRow();
                    newRow["TitleId"] = row["TitleId"];
                    newRow["GenreId"] = genreIds[genreName];
                    finalTable.Rows.Add(newRow);
                }
            }

            using (SqlBulkCopy bulk = new SqlBulkCopy(conn))
            {
                bulk.DestinationTableName = "TitleGenres";
                bulk.BatchSize = 10000;
                bulk.WriteToServer(finalTable);
            }
        }
    }
}
