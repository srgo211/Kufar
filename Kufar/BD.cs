using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kufar
{
    public class BD
    {
        private string hostname;
        private string username;
        private string password;
        private string database;
        private string charset;
        private string result;
        private MySqlConnection connectionDB;

        /// <summary>
        /// Подключение к БД
        /// </summary>
        /// <param name="db_hostname">Ввеите Хост и Порт(по умолчанию порт 3306)</param>
        /// <param name="db_username">Логин от БД</param>
        /// <param name="db_password">Пароль от БД</param>
        /// <param name="db_database">имя к подключаемой БД</param>
        /// <param name="db_charset">кодировка (по умолчанию utf8)</param>
        public BD(string db_hostname = "localhost", string db_username = "root", string db_password = "vertrigo", string db_database = "avito", string db_charset = "utf8")
        {
            hostname = db_hostname;
            username = db_username;
            password = db_password;
            database = db_database;
            charset = db_charset;
            result = String.Empty;
            string db_port = "3306";

            var m = db_hostname.Split(':');
            if (m.Length == 2)
            {
                db_hostname = m[0];
                db_port = m[1];
            }

            var connectionString = "server=" + db_hostname + ";user=" + db_username + ";database=" + db_database + ";port=" + db_port + ";password=" + db_password + ";pooling=False;";
            connectionDB = new MySqlConnection(connectionString);

            open();
        }

        public BD(string connectionString)
        {

            connectionDB = new MySqlConnection(connectionString);

            open();
        }


        public void open()
        {
            connectionDB.Open();
        }


        public void close()
        {
            connectionDB.Close();
        }


        /// <summary>
        /// Добавление|Обновление в БД (метод не возращает ничего)
        /// </summary>
        /// <param name="query">строка запроса</param>
        public void InsertUpdateBD(string query)
        {
            MySqlCommand command = new MySqlCommand(query, connectionDB);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Получение значений из БД
        /// </summary>
        /// <param name="query">строка запроса</param>
		/// <param name="splitSumbol">символ разделитель (по умл.|)</param>
        /// <returns></returns>
        public string SelectBD(string query, char splitSumbol = '|', char endOfline = '\n')
        {
            MySqlCommand command = new MySqlCommand(query, connectionDB);
            MySqlDataReader Reader = command.ExecuteReader();
            string line = string.Empty;
            //читаем ответ
            while (Reader.Read())
            {
                //перебираем полученные поля
                for (int i = 0; i < Reader.FieldCount; i++)
                {
                    //составляем строку для добавления в таблицу, по количеству полей
                    line = line + Reader[i].ToString() + splitSumbol;
                }
                //line = line.TrimEnd(splitSumbol) + Environment.NewLine;    //обрезаем последний разделитель столбцов
                line = line.Substring(0, line.Length - 1) + endOfline;
            }

            line = line.TrimEnd(endOfline);
            Reader.Close();
            return line;


        }

        /// <summary>
        ///Блокировка таблицы в БД (для многопотока)
        /// </summary>
        /// <param name="tables">название таблицы</param>
        public void LockTables(string tables)
        {
            string query = String.Format("LOCK TABLES {0} WRITE", tables);
            MySqlCommand command = new MySqlCommand(query, connectionDB);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Разблокировка БД
        /// </summary>
        public void UnLockTables()
        {
            string query = "UNLOCK TABLES";
            MySqlCommand command = new MySqlCommand("UNLOCK TABLES", connectionDB);
            command.ExecuteNonQuery();
        }




    }//public class DB
}
