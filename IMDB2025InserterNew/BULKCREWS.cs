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
    public static class BULKCREWS
    {
        public static void ImportTitleCrew(SqlConnection conn, string filename)
        {
            Console.WriteLine("\n=== Importing Title Crew ===");
            Stopwatch sw = Stopwatch.StartNew();

            DataTable writersTable = CreateTitleWritersDataTable();
            DataTable directorsTable = CreateTitleDirectorsDataTable();

            int lineCount = 0;
            int errorCount = 0;

            foreach (string line in File.ReadLines(filename).Skip(1).Take(100000))
            {
                lineCount++;
                if (lineCount % 100000 == 0)
                    Console.WriteLine($"Processed {lineCount:N0} crew records...");

                string[] values = line.Split('\t');
                if (values.Length != 3)
                {
                    errorCount++;
                    continue;
                }

                try
                {
                    int titleId = int.Parse(values[0].Substring(2));

                    // Directors
                    if (values[1] != "\\N")
                    {
                        string[] directorList = values[1].Split(',');
                        foreach (string dirStr in directorList)
                        {
                            try
                            {
                                int directorId = int.Parse(dirStr.Substring(2));
                                DataRow row = directorsTable.NewRow();
                                row["TitleId"] = titleId;
                                row["PersonId"] = directorId;
                                directorsTable.Rows.Add(row);
                            }
                            catch { }
                        }
                    }

                    // Writers
                    if (values[2] != "\\N")
                    {
                        string[] writerList = values[2].Split(',');
                        foreach (string wrStr in writerList)
                        {
                            try
                            {
                                int writerId = int.Parse(wrStr.Substring(2));
                                DataRow row = writersTable.NewRow();
                                row["TitleId"] = titleId;
                                row["PersonId"] = writerId;
                                writersTable.Rows.Add(row);
                            }
                            catch { }
                        }
                    }

                    if (directorsTable.Rows.Count >= 50000)
                    {
                        BulkInsertTitleDirectors(conn, directorsTable);
                        directorsTable.Clear();
                    }

                    if (writersTable.Rows.Count >= 50000)
                    {
                        BulkInsertTitleWriters(conn, writersTable);
                        writersTable.Clear();
                    }
                }
                catch (Exception ex)
                {
                    errorCount++;
                }
            }

            if (directorsTable.Rows.Count > 0)
                BulkInsertTitleDirectors(conn, directorsTable);
            if (writersTable.Rows.Count > 0)
                BulkInsertTitleWriters(conn, writersTable);

            sw.Stop();
            Console.WriteLine($"Imported {lineCount:N0} crew records in {sw.Elapsed}");
            Console.WriteLine($"Errors: {errorCount}");
        }

        static DataTable CreateTitleWritersDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("TitleId", typeof(int));
            dt.Columns.Add("PersonId", typeof(int));
            return dt;
        }

        static DataTable CreateTitleDirectorsDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("TitleId", typeof(int));
            dt.Columns.Add("PersonId", typeof(int));
            return dt;
        }

        static void BulkInsertTitleDirectors(SqlConnection conn, DataTable dt)
        {
            using (SqlBulkCopy bulk = new SqlBulkCopy(conn))
            {
                bulk.DestinationTableName = "TitleDirectors";
                bulk.BatchSize = 10000;
                bulk.WriteToServer(dt);
            }
        }

        static void BulkInsertTitleWriters(SqlConnection conn, DataTable dt)
        {
            using (SqlBulkCopy bulk = new SqlBulkCopy(conn))
            {
                bulk.DestinationTableName = "TitleWriters";
                bulk.BatchSize = 10000;
                bulk.WriteToServer(dt);
            }
        }
    }
}
