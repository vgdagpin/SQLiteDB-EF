using System;
using System.Collections.Generic;
using VGD.SQLiteDB.Attributes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLiteDB_Testing
{
    public class Student
    {
        [PrimaryKey]
        public string StudentID { get; set; }

        [Required]
        public string Name { get; set; }
        public string Address { get; set; }
        public string Number { get; set; }
    }
}
