using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VGD.SQLiteDB
{
    public class TableInfo : ITableInfo
    {

        public string Name
        {
             get; internal set;
        }

        public IEnumerable<IColumnInfo> Columns
        {
            get { throw new NotImplementedException(); }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
