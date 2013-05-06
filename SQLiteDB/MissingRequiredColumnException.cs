using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VGD.SQLiteDB
{
    public class MissingRequiredColumnException : ApplicationException
    {
        public MissingRequiredColumnException() : base() { }

        public MissingRequiredColumnException(string message) : base(message) { }
    }
}
