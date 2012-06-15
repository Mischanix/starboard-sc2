using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SQLite;
using System.Text;

using Starboard.Model;

namespace Starboard.Automation
{
    class Database : IDisposable
    {
        SQLiteConnection dbConnection;

        public Database(string fileName)
        {
            dbConnection = new SQLiteConnection(String.Format(
                "Data Source=.\\{0};UseUTF16Encoding=True;", fileName
                ));
            dbConnection.Open();

            var dbInit = "create table if not exists player ( id TEXT primary key, name TEXT, race INTEGER )";
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = dbInit;
            cmd.ExecuteNonQuery();
        }

        public void Dispose()
        {
            dbConnection.Close();
        }

        public string DisplayName(string id)
        {
            var cmd = dbConnection.CreateCommand();

            cmd.Parameters.Add("@id", DbType.String);
            cmd.Parameters["@id"].Value = id;

            cmd.CommandText = "select name from player where id=@id;";
            var result = cmd.ExecuteScalar();
            if (result == DBNull.Value)
            {
                return null;
            }
            return (string)result;
        }

        public Race? DisplayRace(string id)
        {
            var cmd = dbConnection.CreateCommand();

            cmd.Parameters.Add("@id", DbType.String);
            cmd.Parameters["@id"].Value = id;

            cmd.CommandText = "select race from player where id=@id;";
            var result = cmd.ExecuteReader();
            if (result.HasRows && result.NextResult())
            {
                int race = result.GetInt32(0);
                if (Enum.IsDefined(typeof(Race), race))
                    return (Race)race;
            }
            return null;
        }

        public void UpdateDisplayName(string id, string name)
        {
            var cmd = dbConnection.CreateCommand();

            cmd.Parameters.Add("@id", DbType.String);
            cmd.Parameters["@id"].Value = id;
            cmd.Parameters.Add("@name", DbType.String);
            cmd.Parameters["@name"].Value = name;

            cmd.CommandText = "insert or replace into player values (@id, @name, (select race from player where id=@id));";
            cmd.ExecuteNonQuery();
        }

        public void UpdateDisplayRace(string id, Model.Race race)
        {
            var cmd = dbConnection.CreateCommand();

            cmd.Parameters.Add("@id", DbType.String);
            cmd.Parameters["@id"].Value = id;
            cmd.Parameters.Add("@race", DbType.Int32);
            cmd.Parameters["@race"].Value = (int)race;

            cmd.CommandText = "insert or replace into player values (@id, (select name from player where id=@id), @race);";
            cmd.ExecuteNonQuery();
        }
    }
}
