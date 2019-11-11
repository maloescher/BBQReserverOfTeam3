using System;
using System.Data.SQLite;

namespace BBQReserverBot.Controllers
{
     public class DatabaseController
     {
         private static string _database = @"URI=file:bbq.db";
         private static SQLiteConnection _connection;
         private static SQLiteCommand _command;
         
         public static SQLiteCommand CreateDatabase()
         {
             _connection = new SQLiteConnection(_database);
             _connection.Open();

             _command = new SQLiteCommand(_connection);

             _command.CommandText = @"CREATE TABLE IF NOT EXISTS users(id INTEGER PRIMARY KEY,
                     name TEXT)";
             _command.ExecuteNonQuery();
             
             _command.CommandText = @"CREATE TABLE IF NOT EXISTS records(id INTEGER PRIMARY KEY AUTOINCREMENT,
                    fromTime TIMESTAMP, toTime TIMESTAMP, userID int, FOREIGN KEY (userID) REFERENCES users(id))";
             _command.ExecuteNonQuery();

             return _command;
         }

         public static void ExecuteCommand(String command)
         {
             if (_connection == null)
             {
                 CreateDatabase();
             }
             
             _command.CommandText = command;
             _command.ExecuteNonQuery();
         }
     }
 }