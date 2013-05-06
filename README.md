SQLiteDB-EF
===========

An easy implementation of some Entity Framework functionalities to Latest SQLite

**Sample Code**

     [TestMethod]
        public void CanInsertDataToTable()
        {
            ISQLiteDB _db = SQLiteDB.InitDataFile("C:/Users/Enteng/Desktop/sample.db", "dbPasswordHere");

            using (IDBSet<Person> _personTable = _db.LoadContext<Person>())
            {
                _personTable.Add(new Person() { Name = "Vincent Dagpin", PersonID = 16, Address = "Dipolog City", Birthdate = new DateTime(1989, 9, 27) });
                _personTable.Add(new Person() { Name = "Marcelius Dagpin" });

                _personTable.SaveChanges();
            }

            using (IDBSet<Person> _personTable = _db.LoadContext<Person>())
            {
                Person _last = _personTable.First();
                Person _first = _personTable.Last();

                Assert.AreEqual(_last.Name, "Vincent Dagpin");
                Assert.AreEqual(_first.Name, "Marcelius Dagpin");
            }
        }
        
        
Here is the **Person Entity**

using System;
using System.Collections.Generic;
using VGD.SQLiteDB.Attributes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

      namespace SQLiteDB_Testing
      {
          public class Person
          {
              [AutoIncrement, PrimaryKey]
              public int PersonID { get; set; }
      
              [MaxValue(199)]
              public string Name { get; set; }
              
              public string Address { get; set; }
              
              public DateTime Birthdate { get; set; }
          }
      }
