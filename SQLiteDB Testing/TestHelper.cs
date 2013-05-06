using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SQLiteDB_Testing
{
    public class TestHelper
    {
        public static Student GetStudent_A
        {
            get
            {
                Student _student = new Student
                    {
                        StudentID = "2012-1234",
                        Name = "Vincent Dagpin",
                        Address = "Dipolog City",
                        Number = "09167849875"
                    };

                return _student;
            }
        }

        public static Student GetStudent_A_Edited
        {
            get
            {
                Student _student = new Student
                {
                    StudentID = "2012-1234",
                    Name = "Vincent Dagpin",
                    Address = "Dipolog City",
                    Number = "09161111111"
                };

                return _student;
            }
        }

        public static Student GetStudent_B
        {
            get
            {
                Student _student = new Student
                    {
                        StudentID = "2013-1234",
                        Name = "Marcelius Dagpin",
                        Address = "Dipolog City",
                        Number = "0912345678"
                    };

                return _student;
            }
        }

        public static List<Student> AThousandsOfStudents
        {
            get
            {
                List<Student> _students = new List<Student>();

                for (int i = 1; i < 15000; i++)
                {
                    Student _newStud = new Student()
                    {
                        StudentID = i.ToString(),
                        Name = "Vincent",
                        Number = "09161122334"
                    };

                    _students.Add(_newStud);
                }

                return _students;
            }
        }

        public static List<Person> AHundredPeople
        {
            get
            {
                List<Person> _people = new List<Person>();

                for (int i = 1; i < 100; i++)
                {
                    Person _person = new Person()
                    {
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

                    _people.Add(_person);
                }

                return _people;
            }
        }

        public static Person GetPerson_A
        {
            get
            {
                Person _person = new Person 
                { 
                    FirstName = "Vincent", 
                    LastName = "Dagpin", 
                    ID = 16, 
                    Birthday = new DateTime(1989, 9, 27) ,
                    IsWorking = true,
                    About = new string('x', 10000)
                };

                return _person;
            }
        }

        public static Person GetPerson_B
        {
            get
            {
                Person _person = new Person 
                {
                    ID = 17,
                    FirstName = "Marcelius", 
                    LastName = "Dagpin", 
                };

                return _person;
            }
        }

        public static bool AreEqual(object first, object second)
        {
            return AreEqual<object>(first, second);
        }

        public static bool AreEqual<T>(T first, T second)
        {
            PropertyInfo[] _properties = typeof(T).GetProperties();
            bool _isEqual = true;

            foreach (PropertyInfo _property in _properties)
            {
                object _firstData = _property.GetValue(first);
                object _secondData = _property.GetValue(second);

                if (_firstData == null && _secondData == null) continue;
                if (attrIsExcluded(_property)) continue;

                if (!_firstData.Equals(_secondData))
                {
                    _isEqual = false;
                    break;
                }
            }

            return _isEqual;
        }

      


        private static bool attrIsExcluded(PropertyInfo propertyInfo)
        {
            return Attribute.IsDefined(propertyInfo, typeof(VGD.SQLiteDB.Attributes.Exclude));
        }

        public static string GetNewTestPath()
        {
            string _testPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "TestPath");

            if (!Directory.Exists(_testPath))
                Directory.CreateDirectory(_testPath);

            string _path = Path.Combine(
                _testPath,
                Guid.NewGuid().ToString("N").Substring(0, 5) + ".db");

            return _path;
        }

        public static string GetNewTestPath(string path)
        {
            string _testPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "TestPath");

            if (!Directory.Exists(_testPath))
                Directory.CreateDirectory(_testPath);

            string _path = Path.Combine(
                _testPath,
                path);

            return _path;
        }

        public static List<string> SearchFiles(string path, params string[] filters)
        {
            List<string> _retVal = new List<string>();

            System.IO.DirectoryInfo info = new System.IO.DirectoryInfo(path);
            foreach (System.IO.DirectoryInfo info2 in info.GetDirectories())
                _retVal.AddRange(SearchFiles(info2.FullName, filters));

            foreach (System.IO.FileInfo info3 in info.GetFiles())
            {
                string _extension = info3.Extension;

                if (filters.Contains(_extension))
                {
                    _retVal.Add(info3.FullName);
                }
            }

            return _retVal;
        }
    }
}
