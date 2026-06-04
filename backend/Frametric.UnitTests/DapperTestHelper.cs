// Frametric — Cinematic Analytics Platform
// Copyright (C) 2026 Jesús J. Otero Martínez <jesusoteromartinez@outlook.com>
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Frametric.UnitTests;

public class TestDbConnection : DbConnection
{
    private readonly DbCommand _command;
    public TestDbConnection(DbCommand command) { _command = command; }
    protected override DbCommand CreateDbCommand() => _command;
    public override string ConnectionString { get; set; } = "";
    public override string Database => "";
    public override string DataSource => "";
    public override string ServerVersion => "";
    public override ConnectionState State => ConnectionState.Open;
    public override void Open() { }
    public override void Close() { }
    public override void ChangeDatabase(string databaseName) { }
    protected override DbTransaction BeginDbTransaction(IsolationLevel il) => throw new NotImplementedException();
}

public class TestDbCommand : DbCommand
{
    private readonly DbDataReader _reader;
    private readonly object _scalarResult;

    public TestDbCommand(DbDataReader reader, object scalarResult)
    {
        _reader = reader;
        _scalarResult = scalarResult;
    }

    public override string CommandText { get; set; } = "";
    public override int CommandTimeout { get; set; }
    public override CommandType CommandType { get; set; }
    protected override DbConnection? DbConnection { get; set; }
    protected override DbParameterCollection DbParameterCollection => new TestDbParameterCollection();
    protected override DbTransaction? DbTransaction { get; set; }
    public override bool DesignTimeVisible { get; set; }
    public override UpdateRowSource UpdatedRowSource { get; set; }

    public override void Cancel() { }
    protected override DbParameter CreateDbParameter() => new TestDbParameter();
    
    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => _reader;
    
    protected override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        => Task.FromResult(_reader);

    public override int ExecuteNonQuery() => 0;
    
    public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken) => Task.FromResult(0);

    public override object? ExecuteScalar() => _scalarResult;
    
    public override Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken) => Task.FromResult<object?>(_scalarResult);

    public override void Prepare() { }
}

public class TestDbParameter : DbParameter
{
    public override DbType DbType { get; set; }
    public override ParameterDirection Direction { get; set; }
    public override bool IsNullable { get; set; }
    public override string ParameterName { get; set; } = "";
    public override string SourceColumn { get; set; } = "";
    public override bool SourceColumnNullMapping { get; set; }
    public override object? Value { get; set; }
    public override int Size { get; set; }
    public override void ResetDbType() { }
}

public class TestDbParameterCollection : DbParameterCollection
{
    private readonly List<object> _list = new();
    public override int Count => _list.Count;
    public override object SyncRoot => _list;

    public override int Add(object value)
    {
        _list.Add(value);
        return _list.Count - 1;
    }

    public override void AddRange(Array values)
    {
        foreach (var v in values) _list.Add(v);
    }

    public override void CopyTo(Array array, int index)
    {
        ((ICollection)_list).CopyTo(array, index);
    }

    public override void Clear() => _list.Clear();
    public override bool Contains(object value) => _list.Contains(value);
    public override int IndexOf(object value) => _list.IndexOf(value);
    public override void Insert(int index, object value) => _list.Insert(index, value);
    public override void Remove(object value) => _list.Remove(value);
    public override void RemoveAt(int index) => _list.RemoveAt(index);
    public override bool Contains(string value) => false;
    public override int IndexOf(string value) => -1;
    public override void RemoveAt(string parameterName) { }

    protected override DbParameter GetParameter(int index) => (DbParameter)_list[index];
    protected override DbParameter GetParameter(string parameterName) => throw new NotImplementedException();
    protected override void SetParameter(int index, DbParameter value) => _list[index] = value;
    protected override void SetParameter(string parameterName, DbParameter value) => throw new NotImplementedException();

    public override IEnumerator GetEnumerator() => _list.GetEnumerator();
}

public class MultiResultSetDbDataReader : DbDataReader
{
    private readonly List<IReadOnlyList<object>> _resultSets;
    private readonly List<System.Reflection.PropertyInfo[]> _resultProps;
    private int _currentResultSet = 0;
    private int _currentIndex = -1;

    public MultiResultSetDbDataReader(IEnumerable<IEnumerable<object>> resultSets)
    {
        _resultSets = new List<IReadOnlyList<object>>();
        _resultProps = new List<System.Reflection.PropertyInfo[]>();
        foreach (var s in resultSets)
        {
            var list = new List<object>();
            foreach (var item in s)
            {
                list.Add(item);
            }
            _resultSets.Add(list);
            if (list.Count == 0)
            {
                _resultProps.Add(Array.Empty<System.Reflection.PropertyInfo>());
            }
            else
            {
                _resultProps.Add(list[0].GetType().GetProperties());
            }
        }
    }

    public override bool Read()
    {
        if (_currentResultSet >= _resultSets.Count) return false;
        _currentIndex++;
        return _currentIndex < _resultSets[_currentResultSet].Count;
    }

    public override Task<bool> ReadAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(Read());
    }

    public override bool NextResult()
    {
        _currentResultSet++;
        _currentIndex = -1;
        return _currentResultSet < _resultSets.Count;
    }

    public override Task<bool> NextResultAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(NextResult());
    }

    private System.Reflection.PropertyInfo[] CurrentProps => _resultProps[_currentResultSet];
    private object CurrentRow => _resultSets[_currentResultSet][_currentIndex];

    public override int FieldCount => CurrentProps.Length;

    public override string GetName(int ordinal) => CurrentProps[ordinal].Name;

    public override int GetOrdinal(string name)
    {
        for (int i = 0; i < CurrentProps.Length; i++)
        {
            if (string.Equals(CurrentProps[i].Name, name, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return -1;
    }

    public override object GetValue(int ordinal)
    {
        return CurrentProps[ordinal].GetValue(CurrentRow) ?? DBNull.Value;
    }

    public override int GetValues(object[] values)
    {
        int count = Math.Min(FieldCount, values.Length);
        for (int i = 0; i < count; i++)
        {
            values[i] = GetValue(i);
        }
        return count;
    }

    public override bool IsDBNull(int ordinal) => GetValue(ordinal) == DBNull.Value;

    public override object this[int ordinal] => GetValue(ordinal);
    public override object this[string name] => GetValue(GetOrdinal(name));

    public override int Depth => 0;
    public override bool IsClosed => false;
    public override int RecordsAffected => 0;
    public override bool HasRows => _currentResultSet < _resultSets.Count && _resultSets[_currentResultSet].Count > 0;

    public override Type GetFieldType(int ordinal)
    {
        var t = CurrentProps[ordinal].PropertyType;
        return Nullable.GetUnderlyingType(t) ?? t;
    }
    public override string GetDataTypeName(int ordinal) => GetFieldType(ordinal).Name;
    public override IEnumerator GetEnumerator() => throw new NotImplementedException();

    public override bool GetBoolean(int ordinal) => (bool)GetValue(ordinal);
    public override byte GetByte(int ordinal) => (byte)GetValue(ordinal);
    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length) => throw new NotImplementedException();
    public override char GetChar(int ordinal) => (char)GetValue(ordinal);
    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length) => throw new NotImplementedException();
    public override Guid GetGuid(int ordinal) => (Guid)GetValue(ordinal);
    public override short GetInt16(int ordinal) => (short)GetValue(ordinal);
    public override int GetInt32(int ordinal) => (int)GetValue(ordinal);
    public override long GetInt64(int ordinal) => (long)GetValue(ordinal);
    public override DateTime GetDateTime(int ordinal) => (DateTime)GetValue(ordinal);
    public override string GetString(int ordinal) => (string)GetValue(ordinal);
    public override decimal GetDecimal(int ordinal) => (decimal)GetValue(ordinal);
    public override double GetDouble(int ordinal) => (double)GetValue(ordinal);
    public override float GetFloat(int ordinal) => (float)GetValue(ordinal);
}

