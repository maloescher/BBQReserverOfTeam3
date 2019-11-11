using System;
using System.Data.SQLite;

namespace BBQReserverBot.Controllers
{
     public class DatabaseController
     {
         public static SQLiteCommand CreateDatabase()
         {
             string cs = @"URI=file:bbq.db";

             var con = new SQLiteConnection(cs);
             con.Open();

             var cmd = new SQLiteCommand(con);

             cmd.CommandText = @"CREATE TABLE IF NOT EXISTS users(id INTEGER PRIMARY KEY,
                     name TEXT)";
             cmd.ExecuteNonQuery();
             
             cmd.CommandText = @"CREATE TABLE IF NOT EXISTS records(id INTEGER PRIMARY KEY AUTOINCREMENT,
                    fromTime TIMESTAMP, toTime TIMESTAMP, userID int, FOREIGN KEY (userID) REFERENCES users(id))";
             cmd.ExecuteNonQuery();

             return cmd;
         }
     }
 }