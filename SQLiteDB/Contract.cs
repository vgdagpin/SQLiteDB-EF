using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VGD.SQLiteDB
{
    /// <summary>
    /// Interface IDBSet
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDBSet<T> : IEnumerable<T>, IDisposable
    {
        /// <summary>
        /// Adds the specified T entity.
        /// </summary>
        /// <param name="TEntity">The T entity.</param>
        void Add(T TEntity);
        /// <summary>
        /// Adds the range.
        /// </summary>
        /// <param name="TEntityList">The T entity list.</param>
        void AddRange(IEnumerable<T> TEntityList);
        /// <summary>
        /// Deletes the specified T entity.
        /// </summary>
        /// <param name="TEntity">The T entity.</param>
        void Delete(T TEntity);
        /// <summary>
        /// Deletes the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        void Delete(Func<T, bool> predicate);
        /// <summary>
        /// Updates the specified old entity.
        /// </summary>
        /// <param name="oldEntity">The old entity.</param>
        /// <param name="newEntity">The new entity.</param>
        /// <returns>`0.</returns>
        T Update(T oldEntity, T newEntity);
        ///// <summary>
        ///// Counts this instance.
        ///// </summary>
        ///// <returns>System.Int32.</returns>
        //int Count();
        /// <summary>
        /// Saves the changes.
        /// </summary>
        void SaveChanges();
        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>System.Object.</returns>
        object GetData(Func<T, bool> predicate, string propertyName);
    }

    /// <summary>
    /// Interface ITableInfo
    /// </summary>
    public interface ITableInfo
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }
        /// <summary>
        /// Gets the columns.
        /// </summary>
        /// <value>The columns.</value>
        IEnumerable<IColumnInfo> Columns { get; }
    }

    /// <summary>
    /// Interface IColumnInfo
    /// </summary>
    public interface IColumnInfo
    {
        /// <summary>
        /// Gets a value indicating whether [allow DB null].
        /// </summary>
        /// <value><c>true</c> if [allow DB null]; otherwise, <c>false</c>.</value>
        bool AllowDBNull { get; }
        /// <summary>
        /// Gets a value indicating whether [auto increment].
        /// </summary>
        /// <value><c>true</c> if [auto increment]; otherwise, <c>false</c>.</value>
        bool AutoIncrement { get; }
        /// <summary>
        /// Gets the auto increment seed.
        /// </summary>
        /// <value>The auto increment seed.</value>
        int AutoIncrementSeed { get; }
        /// <summary>
        /// Gets the auto increment step.
        /// </summary>
        /// <value>The auto increment step.</value>
        int AutoIncrementStep { get; }
        /// <summary>
        /// Gets the caption.
        /// </summary>
        /// <value>The caption.</value>
        string Caption { get; }
        /// <summary>
        /// Gets the name of the column.
        /// </summary>
        /// <value>The name of the column.</value>
        string ColumnName { get; }
        /// <summary>
        /// Gets the type of the data.
        /// </summary>
        /// <value>The type of the data.</value>
        Type DataType { get; }
        /// <summary>
        /// Gets the default value.
        /// </summary>
        /// <value>The default value.</value>
        object DefaultValue { get; }
        /// <summary>
        /// Gets the length of the max.
        /// </summary>
        /// <value>The length of the max.</value>
        int MaxLength { get; }
        /// <summary>
        /// Gets a value indicating whether this <see cref="IColumnInfo"/> is unique.
        /// </summary>
        /// <value><c>true</c> if unique; otherwise, <c>false</c>.</value>
        bool Unique { get; }
    }

    /// <summary>
    /// Interface ISQLiteDB
    /// </summary>
    public interface ISQLiteDB
    {
        /// <summary>
        /// Gets the data file.
        /// </summary>
        /// <value>The data file.</value>
        string DataFile { get; }
        /// <summary>
        /// Sets the password.
        /// </summary>
        /// <value>The password.</value>
        string Password { set; }
        /// <summary>
        /// Sets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        string ConnectionString { set; }

        /// <summary>
        /// Determines whether this instance can connect.
        /// </summary>
        /// <returns><c>true</c> if this instance can connect; otherwise, <c>false</c>.</returns>
        bool CanConnect();
        /// <summary>
        /// Loads the context.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>IDBSet{``0}.</returns>
        IDBSet<T> LoadContext<T>();
        /// <summary>
        /// Gets the tables.
        /// </summary>
        /// <returns>IEnumerable{ITableInfo}.</returns>
        IEnumerable<ITableInfo> GetTables();
    }

   
}
