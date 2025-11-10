using System.Diagnostics;
using Microsoft.Data.SqlClient;

namespace IMDB2025InserterNew
{
    internal class Program
    {
        static void Main(string[] args)
        {
            

            string connectionString = "Server=localhost;Database=IMDB;" +
                "integrated security=True;TrustServerCertificate=True;";

            Stopwatch sw = new Stopwatch();
            sw.Start();

            SqlConnection sqlConn = new SqlConnection(connectionString);
            sqlConn.Open();
            SqlTransaction sqlTrans = sqlConn.BeginTransaction();

            //SqlCommand cmd = new SqlCommand("SET IDENTITY_INSERT Titles ON;", sqlConn, sqlTrans);
            //cmd.ExecuteNonQuery();

            //PreparedSql preparedSql = new PreparedSql(sqlConn, sqlTrans);
            BulkSql bulkSql = new BulkSql();

            Dictionary<string, int> TitleTypes = new Dictionary<string, int>();

            string titleBasics = "c:\\Datamatiker_4sem\\SQL Mandatory Assignment\\title.basics.tsv";
            //string nameBasics = "c:\\Datamatiker_4sem\\SQL Mandatory Assignment\\name.basics.tsv";
            //string titleCrew = "c:\\Datamatiker_4sem\\SQL Mandatory Assignment\\title.crew.tsv";

            IEnumerable<string> imdbData = File.ReadAllLines(titleBasics).Skip(1).Take(10000);
            foreach (string titleString in imdbData)
            {
                string[] values = titleString.Split('\t');
                if (values.Length == 9)
                {
                    if (!TitleTypes.ContainsKey(values[1]))
                    {
                        AddTitleType(values[1], sqlConn, sqlTrans, TitleTypes);
                    }

                    try
                    {
                        Title title = new Title
                        {
                            Id = int.Parse(values[0].Substring(2)),
                            TitleType = TitleTypes[values[1]],
                            PrimaryTitle = values[2],
                            OriginalTitle = values[3] == "\\N" ? null : values[3],
                            IsAdult = values[4] == "1",
                            StartYear = values[5] == "\\N" ? null : int.Parse(values[5]),
                            EndYear = values[6] == "\\N" ? null : int.Parse(values[6]),
                            RuntimeMinutes = values[7] == "\\N" ? null : int.Parse(values[7]),
                            Genres = values[8] == "\\N" ? new List<string>() : values[8].Split(',').ToList()
                        };

                        //SqlCommand sqlComm = new SqlCommand(title.ToSQL(), sqlConn, sqlTrans);
                        //sqlComm.ExecuteNonQuery();

                        //preparedSql.InsertTitle(title);

                        bulkSql.InsertTitle(title);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error parsing line: " + titleString);
                        Console.WriteLine(ex.Message);
                    }
                }
                else
                {
                    Console.WriteLine("Not 9 values: " + titleString);
                }
            }
            Console.WriteLine("Millisekunder: " + sw.ElapsedMilliseconds);

            sw.Restart();

            SqlCommand cmd = new SqlCommand("SET IDENTITY_INSERT Titles ON;", sqlConn, sqlTrans);
            cmd.ExecuteNonQuery();
            bulkSql.InsertIntoDB(sqlConn, sqlTrans);

            cmd = new SqlCommand("SET IDENTITY_INSERT Titles OFF;", sqlConn, sqlTrans);
            cmd.ExecuteNonQuery();
            sqlTrans.Rollback();
            sqlConn.Close();

            sw.Stop();
            Console.WriteLine("Millisekunder: " + sw.ElapsedMilliseconds);
            Console.WriteLine("Alle records: " + 1200 * sw.ElapsedMilliseconds);
            Console.WriteLine("Alle records i timer: " + (1200.0 * sw.ElapsedMilliseconds) / 1000.0 / 60.0 / 60.0);

            void AddTitleType(string titleType, SqlConnection sqlConn, SqlTransaction sqlTrans, Dictionary<string, int> TitleTypes)
            {
                if (!TitleTypes.ContainsKey(titleType))
                {
                    SqlCommand sqlComm = new SqlCommand(
                        "INSERT INTO TitleTypes (Type) VALUES ('" + titleType + "'); " +
                        "SELECT SCOPE_IDENTITY();", sqlConn, sqlTrans);
                    int newId = Convert.ToInt32(sqlComm.ExecuteScalar());
                    TitleTypes[titleType] = newId;
                }
            }
        }
    }
}
