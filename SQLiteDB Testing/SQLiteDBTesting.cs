using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VGD.SQLiteDB;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace SQLiteDB_Testing
{
    [TestClass]
    public class SQLiteDBTesting
    {
        [TestMethod]
        public void CanInitializeDataFile()
        {
            string _testDBPath = TestHelper.GetNewTestPath();

            ISQLiteDB _db = SQLiteDB.InitDataFile(_testDBPath);

            Assert.IsTrue(File.Exists(_testDBPath));
        }

        [TestMethod]
        public void CanCreateTable()
        {
            ISQLiteDB _db = SQLiteDB.InitDataFile(TestHelper.GetNewTestPath());

            using (IDBSet<Person> _personTable = _db.LoadContext<Person>()) { }

            IEnumerable<ITableInfo> _tableNames = _db.GetTables();

            Assert.IsTrue(_tableNames.First().Name == "Person");
        }

        [TestMethod]
        public void CanInsertDataToTable()
        {
            ISQLiteDB _db = SQLiteDB.InitDataFile(TestHelper.GetNewTestPath());

            using (IDBSet<Person> _personTable = _db.LoadContext<Person>())
            {
                _personTable.Add(TestHelper.GetPerson_A);
                _personTable.Add(TestHelper.GetPerson_B);

                _personTable.SaveChanges();

                Assert.AreEqual(2, _personTable.Count());
            }

            using (IDBSet<Person> _personTable = _db.LoadContext<Person>())
            {
                Person _first = _personTable.First();
                Person _last = _personTable.Last();

                Assert.IsTrue(TestHelper.AreEqual<Person>(_first, TestHelper.GetPerson_A));
                Assert.IsTrue(TestHelper.AreEqual<Person>(_last, TestHelper.GetPerson_B));

            }
        }

        [TestMethod]
        public void CanDeleteData()
        {
            ISQLiteDB _db = SQLiteDB.InitDataFile(TestHelper.GetNewTestPath());

            using (IDBSet<Student> _students = _db.LoadContext<Student>())
            {
                _students.Add(TestHelper.GetStudent_A);

                _students.Add(TestHelper.GetStudent_B);

                _students.SaveChanges();
            }

            using (IDBSet<Student> _students = _db.LoadContext<Student>())
            {
                _students.Delete(s => s.StudentID == TestHelper.GetStudent_B.StudentID);

                _students.SaveChanges();
            }

            using (IDBSet<Student> _students = _db.LoadContext<Student>())
            {
                Assert.AreEqual(1, _students.Count());
            }            
        }

        [TestMethod]
        public void CanUpdateData()
        {
            ISQLiteDB _db = SQLiteDB.InitDataFile(TestHelper.GetNewTestPath());

            using (IDBSet<Student> _students = _db.LoadContext<Student>())
            {
                _students.Add(TestHelper.GetStudent_A);

                _students.Add(TestHelper.GetStudent_B);

                _students.SaveChanges();
            }

            using (IDBSet<Student> _students = _db.LoadContext<Student>())
            {
                _students.Update(_students.First(), TestHelper.GetStudent_A_Edited);
                _students.SaveChanges();
            }

            using (IDBSet<Student> _students = _db.LoadContext<Student>())
            {
                Assert.IsTrue(
                    TestHelper.AreEqual<Student>(
                        _students.First(), 
                        TestHelper.GetStudent_A_Edited));
            }
        }

        [TestMethod]
        public void CanSelectData()
        {
            ISQLiteDB _db = SQLiteDB.InitDataFile(TestHelper.GetNewTestPath());

            using (IDBSet<Student> _students = _db.LoadContext<Student>())
            {
                _students.Add(TestHelper.GetStudent_A);

                _students.Add(TestHelper.GetStudent_B);

                _students.SaveChanges();
            }

            using (IDBSet<Student> _students = _db.LoadContext<Student>())
            {
                Assert.IsTrue(
                    TestHelper.AreEqual<Student>(
                        _students.First(), 
                        TestHelper.GetStudent_A));
            }
        }

        [TestMethod]
        public void CanGetTableNames()
        {
            ISQLiteDB _db = SQLiteDB.InitDataFile(TestHelper.GetNewTestPath());

            using (IDBSet<Person> _personTable = _db.LoadContext<Person>()) { }
            using (IDBSet<Student> _studentTable = _db.LoadContext<Student>()) { }

            IEnumerable<ITableInfo> _tableNames = _db.GetTables();

            Assert.IsTrue(_tableNames.First().Name == "Person");
            Assert.AreEqual(2, _tableNames.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(MissingRequiredColumnException))]
        public void CannotInsertDataWithMissingRequiredColumn()
        {
            ISQLiteDB _db = SQLiteDB.InitDataFile(TestHelper.GetNewTestPath());

            Student _student = new Student() { Number = "12345" };

            using (IDBSet<Student> _studentsTable = _db.LoadContext<Student>())
            {
                _studentsTable.Add(_student);

                _studentsTable.SaveChanges();
            }
        }

        [TestMethod]
        public void CanInsert15ThousandRowsAtOnce()
        {
            ISQLiteDB _db = SQLiteDB.InitDataFile(TestHelper.GetNewTestPath());
            
            using (IDBSet<Student> _studentsTable = _db.LoadContext<Student>())
            {
                _studentsTable.AddRange(TestHelper.AThousandsOfStudents);

                _studentsTable.SaveChanges();
            }

            using (IDBSet<Student> _studentsTable = _db.LoadContext<Student>())
            {
                Assert.AreEqual(TestHelper.AThousandsOfStudents.Count(), _studentsTable.Count());
            }
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void CannotInsertMultipleRowsWithDifferentColumnCountAtOnce()
        {
            ISQLiteDB _db = SQLiteDB.InitDataFile(TestHelper.GetNewTestPath());

            List<Student> _students = new List<Student>()
            {
                new Student(){ StudentID = "1", Name = "aw", Address= "x" },
                new Student(){ StudentID = "2", Name = "ew" },
                new Student(){ StudentID = "3", Name = "ow" }
            };

            using (IDBSet<Student> _studentsTable = _db.LoadContext<Student>())
            {
                _studentsTable.AddRange(_students);

                _studentsTable.SaveChanges();
            }

            using (IDBSet<Student> _studentsTable = _db.LoadContext<Student>())
            {
                Assert.AreEqual(_students.Count(), _studentsTable.Count());
            }
        }

        [TestMethod]
        public void CanInsertWithDifferentDataType()
        {
            ISQLiteDB _db = SQLiteDB.InitDataFile(TestHelper.GetNewTestPath());

            Person _user = new Person()
            {
                ID = 7,
                FirstName = "Vincent",
                LastName = "Dagpin",
                Birthday = new DateTime(1989, 9, 27),
                Height = 5.8,
                Gender = Gender.Male,
                Income = (decimal)20000.00,
                LifeRemaining = new TimeSpan(1, 2, 3),
                Website = new Uri("http://vrynxzent.info"),
                About = new string('x', 10000)
            };

            using (IDBSet<Person> _users = _db.LoadContext<Person>())
            {
                _users.Add(_user);

                _users.SaveChanges();
            }

            using (IDBSet<Person> _users = _db.LoadContext<Person>())
            {
                Assert.IsTrue(
                    TestHelper.AreEqual<Person>(
                        _user, 
                        _users.First()));
            }
        }

        [TestMethod]
        public void CanManageAutoIncrement()
        {
            ISQLiteDB _db = SQLiteDB.InitDataFile(TestHelper.GetNewTestPath());

            using (IDBSet<Person> _users = _db.LoadContext<Person>())
            {
                _users.AddRange(TestHelper.AHundredPeople);

                _users.SaveChanges();
            }

            using (IDBSet<Person> _users = _db.LoadContext<Person>())
            {
                Person _last = _users.Last();

                Assert.AreEqual(TestHelper.AHundredPeople.Count, _last.ID);
            }
        }

        [TestMethod]
        public void CanGetExcludedPropertyData()
        {
            ISQLiteDB _db = SQLiteDB.InitDataFile(TestHelper.GetNewTestPath());

            using (IDBSet<Person> _users = _db.LoadContext<Person>())
            {
                _users.AddRange(TestHelper.AHundredPeople);

                _users.SaveChanges();
            }

            using (IDBSet<Person> _users = _db.LoadContext<Person>())
            {
                string _data = (string)_users.GetData(p => p.ID == 1, "About");

                Assert.AreEqual(TestHelper.AHundredPeople[0].About, _data);
            }
        }
    }

    
}
