using System;
using System.Collections;
using System.Collections.Generic;
using VGD.SQLiteDB.Attributes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SQLite;

namespace VGD.SQLiteDB
{
    public class DBSet<T> : IDBSet<T>
    {
        private DataTable originalDataTable = new DataTable();
        private List<T> mainCache = new List<T>();

        private List<T> addedSingleData = new List<T>();
        private List<T> addedMultipleData = new List<T>();
        private List<T> deletedData = new List<T>();
        private Dictionary<object, T> editedData = new Dictionary<object, T>();

        internal SQLiteDB sqliteDBObj { get; set; }

        internal void preloadData()
        {
            string _query = "SELECT ";
            PropertyInfo[] _props = typeof(T).GetProperties();
            mainCache = new List<T>();

            foreach (PropertyInfo _prop in _props)
            {
                if (!_prop.CanWrite) continue;
                if (attrIsExcluded(_prop)) continue;

                _query += _prop.Name + ",";
            }

            _query = _query.Trim(',');

            _query += string.Format(" FROM {0}", typeof(T).Name);

            originalDataTable = sqliteDBObj.select(_query);

            for (int i = 0; i < originalDataTable.Rows.Count; i++)
            {
                T _tInstance = (T)Activator.CreateInstance<T>();

                foreach (PropertyInfo _prop in _props)
                {
                    if (!_prop.CanWrite) continue;
                    if (attrIsExcluded(_prop)) continue;

                    object _obj = originalDataTable.Rows[i][_prop.Name];                    

                    setValueTo(_tInstance, _prop, _obj);
                }

                mainCache.Add(_tInstance);
            }
        }

        internal void createIfNotExist()
        {
            PropertyInfo[] _properties = typeof(T).GetProperties();
            bool _autoIncFound = false;
            bool _primaryKeyFound = false;

            string _query = "CREATE TABLE " + typeof(T).Name + " (";

            foreach (PropertyInfo _property in _properties)
            {
                if (attrIsPrimaryKey(_property))
                    _primaryKeyFound = true;

                if (!_property.CanWrite)
                    continue;

                if (attrIsAutoIncrement(_property))
                {
                    _primaryKeyFound = true;

                    if (_property.PropertyType.Name != "Int32")
                        throw new ArgumentException("Autoincrement can be applied only to Int32 and Int64");

                    if (_autoIncFound)
                        throw new ArgumentException("Autoincrement defined more than once.");
                    _autoIncFound = true;
                }

                _query += propertyInfoToString(_property);
            }

            _query = _query.Trim().Trim(',').Trim() + ")";

            if (!_primaryKeyFound) throw new Exception("No primary key defined.");

            try
            {
                sqliteDBObj.execute(_query);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("already exists"))
                    return;
            }
        }

        private void setValueTo(T instance, PropertyInfo propertyInfo, object value)
        {
            string _type = propertyInfo.PropertyType.BaseType.Name;

            string _data = value.ToString();

            if (_type == "Enum")
            {
                propertyInfo.SetValue(instance, Enum.Parse(propertyInfo.PropertyType, value.ToString()));
                return;
            }

            switch (propertyInfo.PropertyType.Name)
            {
                case "Int32":
                    propertyInfo.SetValue(instance, Convert.ToInt32(value));
                    break;
                case "Boolean":
                    propertyInfo.SetValue(instance, Convert.ToBoolean(value));
                    break;
                case "Nullable`1":
                    if (value == null || value.ToString() == "")
                        propertyInfo.SetValue(instance, null);
                    else
                        propertyInfo.SetValue(instance, value);
                    break;
                case "DateTime":
                    propertyInfo.SetValue(instance, DateTime.Parse(value.ToString()));
                    break;
                case "TimeSpan":
                    propertyInfo.SetValue(instance, TimeSpan.Parse(value.ToString()));
                    break;
                case "Decimal":
                    propertyInfo.SetValue(instance, Decimal.Parse(value.ToString()));
                    break;
                case "Double":
                    propertyInfo.SetValue(instance, Double.Parse(value.ToString()));
                    break;
                case "Uri":
                    if (value.ToString() == "")
                    {
                        propertyInfo.SetValue(instance, null);
                    }
                    else
                    {
                        propertyInfo.SetValue(instance, new Uri(value.ToString()));
                    }
                    break;
                default:
                    if (value is System.DBNull)
                    {
                        propertyInfo.SetValue(instance, null);
                    }
                    else
                    {
                        propertyInfo.SetValue(instance, value);
                    }
                    break;
            }
        }

        public void Add(T TEntity)
        {
            addedSingleData.Add(TEntity);

            T _e = TEntity;

            foreach (PropertyInfo _prop in typeof(T).GetProperties())
            {
                if (attrIsExcluded(_prop))
                    _prop.SetValue(_e, null);
            }

            mainCache.Add(_e);            
        }

        public void AddRange(IEnumerable<T> TEntityList)
        {
            addedMultipleData.AddRange(TEntityList);

            foreach (T _t in TEntityList)
            {               
                foreach (PropertyInfo _prop in typeof(T).GetProperties())
                {
                    if (attrIsExcluded(_prop))
                        _prop.SetValue(_t, null);
                }

                mainCache.Add(_t);
            }            
        }

        public void Delete(T TEntity)
        {
            mainCache.Remove(TEntity);

            deletedData.Add(TEntity);
        }

        public void Delete(Func<T, bool> predicate)
        {
            mainCache.Where(predicate).ToList().ForEach(t =>
            {
                deletedData.Add(t);
            });

            deletedData.ForEach(t =>
            {
                mainCache.Remove(t);
            });
        }

        public void SaveChanges()
        {
            try
            {
                if (!allDataAreUniform(addedMultipleData))
                {
                    addedMultipleData.Clear();
                    throw new Exception("Columns are not equal.");
                }

                if (addedMultipleData.Count() > 0)
                    insertMultipleDataWithLimit(500);

                addedSingleData.ForEach(t => sqliteDBObj.execute(insertQueryParser(t)));

                deletedData.ForEach(t => sqliteDBObj.execute(deleteQueryParser(t)));

                editedData.ToList().ForEach(p => 
                    {
                        string _editQuery = editQueryParser(p.Key, p.Value);
                        sqliteDBObj.execute(_editQuery);
                    });

                addedMultipleData.Clear();
                addedSingleData.Clear();
                deletedData.Clear();
                editedData.Clear();

                //preloadData();
            }
            catch (SQLiteException ex)
            {
                if (ex.Message.Contains("may not be NULL"))
                {
                    throw new MissingRequiredColumnException(ex.Message);
                }

                throw;
            }
        }

        private void insertMultipleDataWithLimit(int limit)
        {
            List<T> _tempList = addedMultipleData;

            while (_tempList.Count > 0)
            {
                // gets limitted data at a time
                int _takeLimitLeft = (_tempList.Count > limit) ? limit : _tempList.Count;
                var _toProcess = _tempList.Take(_takeLimitLeft);


                string _insertQuery = generateInsertMultipleDataAtOnce(_toProcess);

                if (!string.IsNullOrEmpty(_insertQuery))
                    sqliteDBObj.execute(_insertQuery);

                _tempList.RemoveRange(0, _takeLimitLeft);
            }
        }

        private bool allDataAreUniform(IEnumerable<T> TEntities)
        {
            bool[] _props = new bool[typeof(T).GetProperties().Count()];
            bool _firstData = true;

            foreach (T _t in TEntities)
            {
                PropertyInfo[] _pi = typeof(T).GetProperties();

                bool[] _temp = new bool[typeof(T).GetProperties().Count()];
                for (int i = 0; i < _pi.Count(); i++)
                    _temp[i] = _pi[i].GetValue(_t) != null;

                if (_firstData)
                    _props = _temp;
                else
                    if (!Enumerable.SequenceEqual(_props, _temp))
                        return false;

                _firstData = false;
            }

            return true;
        }

        private string generateInsertMultipleDataAtOnce(IEnumerable<T> TEntities)
        {
            PropertyInfo[] _properties = typeof(T).GetProperties();
            List<T> _entities = TEntities.ToList();
            string _retValQuery = string.Empty;

            string _headQuery = string.Format("INSERT INTO \"{0}\" (", typeof(T).Name); ;


            foreach (PropertyInfo _property in _properties)
            {
                //if (_property.GetValue(_entities.First()) == null) continue;
                if (!_property.CanWrite) continue;

                if (attrIsAutoIncrement(_property))
                    //    if (Convert.ToInt32(_property.GetValue(_entities.First())) == 0)
                    continue;

                _headQuery += "\"" + _property.Name + "\", ";
            }

            _headQuery = _headQuery.Trim().Trim(',') + ") VALUES ";

            _retValQuery = _headQuery;

            for (int i = 0; i < _entities.Count; i++)
            {
                _retValQuery += "(";
                foreach (PropertyInfo _property in _properties)
                {
                    //if (_property.GetValue(_entities[i]) == null) continue;
                    if (!_property.CanWrite) continue;

                    if (attrIsAutoIncrement(_property))
                        //    if (Convert.ToInt32(_property.GetValue(_entities[i])) == 0)
                        continue;

                    _retValQuery += propertyToData(_entities[i], _property) + ",";
                }

                _retValQuery = _retValQuery.Trim().Trim(',') + "),";
            }

            _retValQuery = _retValQuery.Trim(',');

            return _retValQuery;
        }        

        public T Update(T oldEntity, T newEntity)
        {
            PropertyInfo[] _properties = typeof(T).GetProperties();
            bool _hasPrimaryKey = false; // flag to check if has primary key
            T _newInstanceNewEntity = (T)Activator.CreateInstance<T>(); // new empty instance
            bool _editDataIndexed = false; // flag if data has been indexed to editedData List

            foreach (PropertyInfo _property in _properties)
            {
                // if no value for the new property, use the original value
                // this will change only the property of what has been defined to update
                if (_property.GetValue(newEntity) == null)
                    _property.SetValue(_newInstanceNewEntity, _property.GetValue(oldEntity));
                else
                    _property.SetValue(_newInstanceNewEntity, _property.GetValue(newEntity));

                // if not yet indexed to editedData List, index it..
                if (!_editDataIndexed)
                {
                    if (attrIsPrimaryKey(_property)) // only index the first primary key.
                    {
                        _hasPrimaryKey = true;
                        object _key = _property.GetValue(oldEntity);

                        // this is to replace the existing indexed update
                        if (editedData.ContainsKey(_key))
                            editedData[_key] = _newInstanceNewEntity;
                        else
                            editedData.Add(_key, _newInstanceNewEntity);

                        _editDataIndexed = true;
                    }
                }
            }

            // replace the old data with new data on cache.
            mainCache[mainCache.IndexOf(oldEntity)] = _newInstanceNewEntity;

            if (!_hasPrimaryKey)
                throw new Exception("Primary key not defined.");

            return newEntity;
        }

        private string insertQueryParser(T TEntity)
        {
            PropertyInfo[] _properties = typeof(T).GetProperties();
            string _entityName = typeof(T).Name;

            string _query = string.Empty;

            _query = "INSERT INTO " + _entityName + "(";

            foreach (PropertyInfo _property in _properties)
            {
                if (!_property.CanWrite) continue;
                if (_property.GetValue(TEntity) == null) continue;

                if (attrIsAutoIncrement(_property))
                    if (Convert.ToInt32(_property.GetValue(TEntity)) == 0)
                        continue;

                _query += _property.Name + ", ";
            }

            _query = _query.Trim().Trim(',');
            _query += ") VALUES (";

            foreach (PropertyInfo _property in _properties)
            {
                if (!_property.CanWrite) continue;
                if (_property.GetValue(TEntity) == null) continue;

                if (attrIsAutoIncrement(_property))
                    if (Convert.ToInt32(_property.GetValue(TEntity)) == 0)
                        continue;

                _query += propertyToData(TEntity, _property) + ",";
            }

            _query = _query.Trim().Trim(',');
            _query += ")";

            return _query;
        }

        private string deleteQueryParser(T TEntity)
        {
            PropertyInfo[] _properties = typeof(T).GetProperties();

            string _query = "DELETE FROM " + typeof(T).Name + " WHERE ";

            foreach (PropertyInfo _property in _properties)
                if (attrIsPrimaryKey(_property))
                    _query += _property.Name + " = " + propertyToData(TEntity, _property);

            return _query;
        }

        private string editQueryParser(object key, T newEntity)
        {
            PropertyInfo[] _properties = typeof(T).GetProperties();
            PropertyInfo _pk = null;

            string _query = "UPDATE " + typeof(T).Name + " SET ";

            foreach (PropertyInfo _property in _properties)
            {
                if (_property.GetValue(newEntity) == null) continue;

                if (attrIsPrimaryKey(_property) && _pk == null)
                    _pk = _property;

                _query += _property.Name + " = " + propertyToData(newEntity, _property) + ",";
            }

            _query = _query.Trim().Trim(',');

            _query += "WHERE " + _pk.Name + " = " + propertyToData(newEntity, _pk);

            return _query;
        }

        private string propertyToData(T TEntity, PropertyInfo propertyInfo)
        {
            switch (propertyInfo.PropertyType.Name)
            {
                case "DateTime":
                    return string.Format("\"{0:yyyy-MM-dd}\"", DateTime.Parse(propertyInfo.GetValue(TEntity).ToString()));
                case "Boolean":
                    bool _val = Convert.ToBoolean(propertyInfo.GetValue(TEntity));
                    return string.Format("\"{0}\"", (_val) ? 1 : 0);
                default:
                    object _data = propertyInfo.GetValue(TEntity);
                    if (_data == null)
                        return string.Format("\"{0}\"", "");
                    return string.Format("\"{0}\"", _data.ToString().Replace("\"", "'"));
            }
        }        

        

        private string propertyInfoToString(PropertyInfo propertyInfo)
        {
            StringBuilder _sb = new StringBuilder();
            DefaultValue _defaultValue = new DefaultValue("");

            bool _isNullable = propertyTypeIsNullable(propertyInfo);
            bool _autoInc = attrIsAutoIncrement(propertyInfo);
            bool _isPrimaryKey = attrIsPrimaryKey(propertyInfo) || _autoInc;
            bool _hasDefaultValue = attrHasDefaultValue(ref _defaultValue, propertyInfo);
            bool _isRequired = attrIsRequired(propertyInfo);

            if (_isRequired || _isPrimaryKey)
                _isNullable = false;

            string _name = propertyInfo.Name;

            _sb.Append(string.Format("{0} {1} {2} {3} {4} {5}, ",
                    propertyInfo.Name,
                    dbDataType(propertyInfo),
                    (_isPrimaryKey) ? "PRIMARY KEY" : "",
                    (_autoInc) ? "AUTOINCREMENT" : "",
                    (_isNullable) ? "NULL" : "NOT NULL",
                    (_hasDefaultValue) ? _defaultValue.Value : ""));

            string _retVal = _sb.ToString();

            return _retVal;
        }

        private string dbDataType(PropertyInfo propertyInfo)
        {
            MaxValue _maxValue = new MaxValue(255);

            bool _hasMaxValue = attrHasMaxValue(ref _maxValue, propertyInfo);

            switch (propertyInfo.PropertyType.Name)
            {
                case "Int32":
                case "Boolean":
                    return "INTEGER";
                case "DateTime":
                    return "DATE";
                default:
                    return string.Format("VARCHAR({0})", _maxValue.Max);
            }
        }

        private bool propertyTypeIsNullable(PropertyInfo propertyInfo)
        {
            bool _canBeNull = !propertyInfo.PropertyType.IsValueType ||
                (Nullable.GetUnderlyingType(propertyInfo.PropertyType) != null);

            return _canBeNull;
        }

        private bool attrIsPrimaryKey(PropertyInfo propertyInfo)
        {
            return Attribute.IsDefined(propertyInfo, typeof(PrimaryKey)) || Attribute.IsDefined(propertyInfo, typeof(AutoIncrement));
        }

        private bool attrIsExcluded(PropertyInfo propertyInfo)
        {
            return Attribute.IsDefined(propertyInfo, typeof(Exclude));
        }

        private bool attrIsAutoIncrement(PropertyInfo propertyInfo)
        {
            return Attribute.IsDefined(propertyInfo, typeof(AutoIncrement));
        }

        private bool attrIsRequired(PropertyInfo propertyInfo)
        {
            return Attribute.IsDefined(propertyInfo, typeof(Required));
        }

        private bool attrHasMaxValue(ref MaxValue maxValue, PropertyInfo propertyInfo)
        {
            if (Attribute.IsDefined(propertyInfo, typeof(MaxValue)))
            {
                maxValue = Attribute.GetCustomAttribute(propertyInfo, typeof(MaxValue)) as MaxValue;
                return true;
            }

            return false;
        }

        private bool attrHasDefaultValue(ref DefaultValue defaultValue, PropertyInfo propertyInfo)
        {
            if (Attribute.IsDefined(propertyInfo, typeof(DefaultValue)))
            {
                defaultValue = Attribute.GetCustomAttribute(propertyInfo, typeof(DefaultValue)) as DefaultValue;
                return true;
            }

            return false;
        }

        public int Count()
        {
            return mainCache.Count;
        }

        public void Dispose()
        {
            mainCache = null;
            originalDataTable = null;
            editedData = null;
            deletedData = null;
            addedMultipleData = null;
            addedSingleData = null;
        }

        public object GetData(Func<T, bool> predicate, string propertyName)
        {
            T _t = mainCache.Where(predicate).SingleOrDefault();

            string _query = "SELECT " + propertyName + " FROM " + typeof(T).Name + " WHERE ";

            var _p = typeof(T).GetProperty(propertyName);

            if (_p == null) throw new Exception("Property " + propertyName + " not found.");
            
            foreach (PropertyInfo _prop in typeof(T).GetProperties())
            {
                if (attrIsPrimaryKey(_prop))
                {
                    _query += _prop.Name + " = '" + _prop.GetValue(_t) + "'";
                    break;
                }
            }



            DataTable _result = sqliteDBObj.select(_query);
            object _data = _result.Rows[0][propertyName];

            _result.Dispose();

            return _data;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return mainCache.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return mainCache.GetEnumerator();
        }
    }
}
