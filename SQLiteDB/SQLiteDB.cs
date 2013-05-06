using System;
using System.Collections.Generic;
using VGD.SQLiteDB.Attributes;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;

namespace VGD.SQLiteDB
{
    public class SQLiteDB : ISQLiteDB
    {
        private string defaultPass = "";
        private string password;

        public string DataFile { get; internal set; }

        public string Password
        {
            get
            {
                if (string.IsNullOrEmpty(password))
                    return defaultPass;
                else
                    return password;
            }
            set { password = value; }
        }

        public string ConnectionString
        {
            internal get
            {
                if (!string.IsNullOrEmpty(DataFile) && !string.IsNullOrEmpty(Password))
                    return string.Format("Data Source={0};Password={1}", DataFile, Password);
                else if (!string.IsNullOrEmpty(DataFile) && string.IsNullOrEmpty(Password))
                    return string.Format("Data Source={0}", DataFile);
                else
                    return string.Empty;
            }
            set { throw new NotImplementedException(); }
        }

        internal SQLiteDB() { }

        public static ISQLiteDB InitDataFile(string path, string password = null)
        {
            SQLiteDB _retVal = new SQLiteDB();

            _retVal.DataFile = path;

            if (!string.IsNullOrEmpty(password))
                _retVal.Password = password;

            _retVal.CanConnect();

            return _retVal;
        }

        public bool CanConnect()
        {
            bool _isConnected = false;

                if (string.IsNullOrEmpty(ConnectionString))
                    throw new SQLiteException("Connection string is empty. Database path is required.");

                using (SQLiteConnection con = new SQLiteConnection(ConnectionString))
                {
                    con.Open();
                    con.ChangePassword(Password);
                    con.Close();

                    _isConnected = true;
                }            

            return _isConnected;
        }

        internal bool execute(string query)
        {
            bool _isExecuted = false;

            using (SQLiteConnection con = new SQLiteConnection(ConnectionString))
            {
                con.Open();
                SQLiteCommand cmd = new SQLiteCommand(query, con);
                _isExecuted = Convert.ToBoolean(cmd.ExecuteScalar());
                //con.ChangePassword(Password);
            }

            return _isExecuted;
        }

        internal DataTable select(string query)
        {
            DataTable dt = new DataTable();

            using (SQLiteConnection con = new SQLiteConnection(ConnectionString))
            {
                con.Open();
                con.ChangePassword("");
                SQLiteCommand cmd = new SQLiteCommand(query, con);

                using (SQLiteDataAdapter da = new SQLiteDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
                con.ChangePassword(Password);
            }

            return dt;
        }

        public IEnumerable<ITableInfo> GetTables()
        {
            DataTable _dt = new DataTable();
            List<TableInfo> _retVal = new List<TableInfo>();
            string _query = "SELECT * FROM sqlite_master WHERE type = \"table\"";

            using (SQLiteConnection con = new SQLiteConnection(ConnectionString))
            {
                con.Open();
                con.ChangePassword("");
                SQLiteCommand cmd = new SQLiteCommand(_query, con);

                using (SQLiteDataAdapter da = new SQLiteDataAdapter(cmd)) { da.Fill(_dt); }

                con.ChangePassword(Password);
            }

            var _data = _dt.Columns;

            for (int i = 0; i < _dt.Rows.Count; i++)
            {
                TableInfo _tblInfo = new TableInfo();
                string _tblName = _dt.Rows[i].Field<string>("name");

                if (!_tblName.StartsWith("sqlite_"))
                {
                    _tblInfo.Name = _dt.Rows[i].Field<string>("name");

                    _retVal.Add(_tblInfo);
                }
            }

            return _retVal;
        }

        public IDBSet<T> LoadContext<T>()
        {
            DBSet<T> _retVal = new DBSet<T>()
            {
                sqliteDBObj = this
            };

            _retVal.createIfNotExist();
            _retVal.preloadData();

            return _retVal;
        }        
    }
}
