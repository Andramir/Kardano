using System;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Data.Sqlite;
using System.Windows;

namespace Kordano
{
    internal class Database
    {
        SqliteConnection con;
        public Database()
        {
            con = new SqliteConnection($"Data Source = .\\kardano.db");
            con.Open();
        }
        public string AddUser(string login, string pwd)
        {
            string hash = String.Concat(SHA256.Create()
                .ComputeHash(Encoding.UTF8.GetBytes(pwd))
                .Select(item => item.ToString("x2")));
            SqliteCommand cmd = new()
            {
                Connection = con,
                CommandText = $"INSERT INTO Users (login, pass) VALUES ('{login}', '{hash}')"
            };
            try
            {
                int exec = cmd.ExecuteNonQuery();
                MessageBox.Show("Пользователь зарегистрирован");
                return "Пользователь зарегистрирован";

            }
            catch (SqliteException e)
            {
                MessageBox.Show("Пользователь уже существует");
                if (e.Message.StartsWith("SQLite Error 19"))
                {
                    return "Пользователь уже существует";
                }
                else { return e.Message; }
                
            }
        }

        public string CheckUser(string login, string pwd)
        {
            SqliteCommand cmd = new()
            {
                Connection = con,
                CommandText = $"SELECT pass FROM Users WHERE login = '{login}'",
            };
            SqliteDataReader exec = cmd.ExecuteReader();
            if (!exec.HasRows)
            {
                return "Пользователь не найден";
            }
            else
            {
                string hash = String.Concat(SHA256.Create()
                .ComputeHash(Encoding.UTF8.GetBytes(pwd))
                .Select(item => item.ToString("x2")));
                exec.Read();
                string res = exec.GetString(0);

                if (res == hash)
                {
                    return "auth";
                }
                else { return "Неправильный пароль"; }
            }

        }
    }
}
