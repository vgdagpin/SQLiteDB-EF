using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VGD.SQLiteDB.Attributes;

namespace SQLiteDB_Testing
{
    public class Person
    {
        [AutoIncrement]
        public int ID { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName
        {
            get
            {                
                return string.Format("{0}, {1}", LastName, FirstName);
            }
        }

        public DateTime Birthday { get; set; }

        [Exclude]
        public string About { get; set; }
        public TimeSpan LifeRemaining { get; set; }
        public Gender Gender { get; set; }
        public decimal Income { get; set; }
        public double Height { get; set; }
        public Uri Website { get; set; }
        public bool IsWorking { get; set; }
    }

    public enum Gender
    {
        Male,
        Female
    }
}
