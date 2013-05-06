using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VGD.SQLiteDB
{
    public class ColumnInfo : IColumnInfo
    {

        public bool AllowDBNull
        {
            get { throw new NotImplementedException(); }
        }

        public bool AutoIncrement
        {
            get { throw new NotImplementedException(); }
        }

        public int AutoIncrementSeed
        {
            get { throw new NotImplementedException(); }
        }

        public int AutoIncrementStep
        {
            get { throw new NotImplementedException(); }
        }

        public string Caption
        {
            get { throw new NotImplementedException(); }
        }

        public string ColumnName
        {
            get { throw new NotImplementedException(); }
        }

        public Type DataType
        {
            get { throw new NotImplementedException(); }
        }

        public object DefaultValue
        {
            get { throw new NotImplementedException(); }
        }

        public int MaxLength
        {
            get { throw new NotImplementedException(); }
        }

        public bool Unique
        {
            get { throw new NotImplementedException(); }
        }
    }
}
