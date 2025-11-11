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
    public static class BULKPERSONS
    {

        public static void ImportPersons(SqlConnection conn, string filename)
        {
            Console.WriteLine("\n=== Importing Persons ===");
            Stopwatch sw = Stopwatch.StartNew();

            Dictionary<string, int> professions = new Dictionary<string, int>();
            DataTable personsTable = CreatePersonsDataTable();
            DataTable personProfessionsTable = CreatePersonProfessionsDataTable();
            DataTable knownForTable = CreateKnownForDataTable();

            int lineCount = 0;
            int errorCount = 0;

            foreach (string line in File.ReadLines(filename).Skip(1))
            {
                lineCount++;
                if (lineCount % 50000 == 0)
                    Console.WriteLine($"Processed {lineCount:N0} persons...");

                string[] values = line.Split('\t');
                if (values.Length != 6)
                {
                    errorCount++;
                    continue;
                }

                try
                {
                    int personId = int.Parse(values[0].Substring(2)); // Remove "nm"

                    // Add person row (truncate long names)
                    DataRow personRow = personsTable.NewRow();
                    personRow["PersonId"] = personId;
                    personRow["PrimaryName"] = values[1];
                    personRow["BirthYear"] = values[2] == "\\N" ? DBNull.Value : int.Parse(values[2]);
                    personRow["DeathYear"] = values[3] == "\\N" ? DBNull.Value : int.Parse(values[3]);
                    personsTable.Rows.Add(personRow);

                    // Parse professions - insert them immediately
                    if (values[4] != "\\N")
                    {
                        string[] profList = values[4].Split(',');
                        foreach (string prof in profList)
                        {
                            // Get or create profession immediately
                            if (!professions.ContainsKey(prof))
                            {
                                int profId = InsertLookupValue(conn, "Professions", "Profession", prof);
                                professions[prof] = profId;
                            }

                            DataRow junctionRow = personProfessionsTable.NewRow();
                            junctionRow["PersonId"] = personId;
                            junctionRow["ProfessionId"] = professions[prof];
                            personProfessionsTable.Rows.Add(junctionRow);
                        }
                    }

                    // Parse known-for titles
                    if (values[5] != "\\N")
                    {
                        string[] titleList = values[5].Split(',');
                        foreach (string titleStr in titleList)
                        {
                            try
                            {
                                int titleId = int.Parse(titleStr.Substring(2));
                                DataRow kfRow = knownForTable.NewRow();
                                kfRow["PersonId"] = personId;
                                kfRow["TitleId"] = titleId;
                                knownForTable.Rows.Add(kfRow);
                            }
                            catch { } // Skip invalid title IDs
                        }
                    }

                    if (personsTable.Rows.Count >= 10000)
                    {
                        BulkInsertPersons(conn, personsTable);
                        personsTable.Clear();
                    }

                    // Also batch the junction tables to prevent memory buildup
                    if (personProfessionsTable.Rows.Count >= 50000)
                    {
                        BulkInsertPersonProfessions(conn, personProfessionsTable);
                        personProfessionsTable.Clear();
                    }

                    if (knownForTable.Rows.Count >= 50000)
                    {
                        InsertKnownForTitles(conn, knownForTable);
                        knownForTable.Clear();
                    }
                }
                catch (Exception ex)
                {
                    errorCount++;
                    if (errorCount <= 10)
                        Console.WriteLine($"Error on line {lineCount}: {ex.Message}");
                }
            }

            if (personsTable.Rows.Count > 0)
                BulkInsertPersons(conn, personsTable);

            // Insert remaining junction table data
            if (personProfessionsTable.Rows.Count > 0)
                BulkInsertPersonProfessions(conn, personProfessionsTable);

            if (knownForTable.Rows.Count > 0)
                InsertKnownForTitles(conn, knownForTable);

            sw.Stop();
            Console.WriteLine($"Imported {lineCount:N0} persons in {sw.Elapsed}");
            Console.WriteLine($"Errors: {errorCount}");
        }

        static DataTable CreatePersonsDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("PersonId", typeof(int));
            dt.Columns.Add("PrimaryName", typeof(string));
            dt.Columns.Add("BirthYear", typeof(int));
            dt.Columns.Add("DeathYear", typeof(int));
            return dt;
        }

        static DataTable CreatePersonProfessionsDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("PersonId", typeof(int));
            dt.Columns.Add("ProfessionId", typeof(int)); // Changed from ProfessionName
            return dt;
        }

        static DataTable CreateKnownForDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("PersonId", typeof(int));
            dt.Columns.Add("TitleId", typeof(int));
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

        static void BulkInsertPersons(SqlConnection conn, DataTable dt)
        {
            using (SqlBulkCopy bulk = new SqlBulkCopy(conn))
            {
                bulk.DestinationTableName = "Persons";
                bulk.BatchSize = 5000;
                bulk.BulkCopyTimeout = 600;
                bulk.WriteToServer(dt);
            }
        }

        static void BulkInsertPersonProfessions(SqlConnection conn, DataTable dt)
        {
            if (dt.Rows.Count == 0) return;

            using (SqlBulkCopy bulk = new SqlBulkCopy(conn))
            {
                bulk.DestinationTableName = "PersonProfessions";
                bulk.BatchSize = 10000;
                bulk.WriteToServer(dt);
            }
        }

        static void InsertKnownForTitles(SqlConnection conn, DataTable dt)
        {
            if (dt.Rows.Count == 0) return;

            using (SqlBulkCopy bulk = new SqlBulkCopy(conn))
            {
                bulk.DestinationTableName = "PersonKnownForTitles";
                bulk.BatchSize = 10000;
                bulk.WriteToServer(dt);
            }
        }
    }
}
