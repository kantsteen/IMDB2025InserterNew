using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMDB2025InserterNew
{
    public class BulkSqlName
    {
        public DataTable NameDataTable { get; set; }

        public BulkSqlName()
        {
            NameDataTable = new DataTable();
            NameDataTable.Columns.Add("PersonId", typeof(int));
            NameDataTable.Columns.Add("PrimaryName", typeof(string));
            NameDataTable.Columns.Add("BirthYear", typeof(int));
            NameDataTable.Columns.Add("DeathYear", typeof(int));
            NameDataTable.Columns.Add("PrimaryProfession", typeof(List<string>));
            NameDataTable.Columns.Add("KnownForTitles", typeof(List<string>));
        }

        public void InsertName(Name name)
        {
            DataRow row = NameDataTable.NewRow();
            row["PersonId"] = name.PersonId;
            row["PrimaryName"] = name.PrimaryName;
            row["BirthYear"] = name.BirthYear;
            row["DeathYear"] = name.DeathYear;
            row["PrimaryProfession"] = name.PrimaryProfession;
            row["KnownForTitles"] = name.KnownForTitles;
            NameDataTable.Rows.Add(row);
        }

        public void InsertIntoDB(SqlConnection sqlConn, SqlTransaction sqlTrans)
        {
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(sqlConn, SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.KeepNulls, sqlTrans))
            {
                bulkCopy.ColumnMappings.Add("PersonId", "PersonId");
                bulkCopy.ColumnMappings.Add("PrimaryName", "PrimaryName");
                bulkCopy.ColumnMappings.Add("BirthYear", "BirthYear");
                bulkCopy.ColumnMappings.Add("DeathYear", "DeathYear");
                bulkCopy.ColumnMappings.Add("PrimaryProfession", "PrimaryProfession");
                bulkCopy.ColumnMappings.Add("KnownForTitles", "KnownForTitles");
                bulkCopy.WriteToServer(NameDataTable);
            }
        }
    }
}
