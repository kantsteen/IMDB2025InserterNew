using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMDB2025InserterNew
{
    public class BulkSqlCrew
    {
        public DataTable CrewDataTable { get; set; }

        public BulkSqlCrew()
        {
            CrewDataTable = new DataTable();
            CrewDataTable.Columns.Add("TitleId", typeof(int));
            CrewDataTable.Columns.Add("Directors", typeof(string));
            CrewDataTable.Columns.Add("Writers", typeof(string));
        }

        public void InsertCrew(Crew crew)
        {
            DataRow row = CrewDataTable.NewRow();
            row["TitleId"] = crew.TitleId;
            row["Directors"] = crew.Directors;
            row["Writers"] = crew.Writers;
            CrewDataTable.Rows.Add(row);

        }

        public void InsertIntoDB(SqlConnection sqlConn, SqlTransaction sqlTrans)
        {
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(sqlConn, SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.KeepNulls, sqlTrans))
            {
                bulkCopy.ColumnMappings.Add("TitleId", "TitleId");
                bulkCopy.ColumnMappings.Add("Directors", "Directors");
                bulkCopy.ColumnMappings.Add("Writers", "Writers");
                bulkCopy.WriteToServer(CrewDataTable);
            }
        }
    }
}
