using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VGD.SQLiteDB.Attributes
{
    public class AutoIncrement : System.Attribute { }

    public class Exclude : System.Attribute { }

    public class MaxValue : System.Attribute
    {
        private int maxValue = 255;

        public int Max
        {
            get
            {
                return maxValue;
            }
        }

        public MaxValue(int max)
        {
            maxValue = max;
        }
    }

    public class Required : System.Attribute { }

    public class DefaultValue : System.Attribute
    {
        object defaultValue = null;

        public object Value
        {
            get
            {
                return string.Format("DEFAULT \"{0}\"", defaultValue);
            }
        }

        public DefaultValue(object defaultValue)
        {
            if (defaultValue == null)
                throw new ArgumentNullException("Default value is null");

            this.defaultValue = defaultValue;
        }
    }

    public class PrimaryKey : System.Attribute { }
}
