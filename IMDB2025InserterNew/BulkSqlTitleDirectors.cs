using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMDB2025InserterNew
{
    public class BulkSqlTitleDirectors
    {
        public DataTable TitleDirectorsTable { get; set; }

        public BulkSqlTitleDirectors()
        {
            TitleDirectorsTable = new DataTable();
            TitleDirectorsTable.Columns.Add("TitleId", typeof(int));
            TitleDirectorsTable.Columns.Add("PersonId", typeof(int));
        }

        public void InsertDirector(int titleId, int personId)
        {
            DataRow row = TitleDirectorsTable.NewRow();
            row["TitleId"] = titleId;
            row["PersonId"] = personId;
            TitleDirectorsTable.Rows.Add(row);
        }

    }
}
