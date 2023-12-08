using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using SocketGameProtobuf;

namespace SocketGameServer.DAO
{
    class UserData
    {
        private readonly string listName = "sys", tableName = "userdata", userName = "username", password = "password", killCount = "killcount";

        /// <summary>
        /// 註冊
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="mySqlConnection"></param>
        /// <returns></returns>
        public bool Logon(MainPack pack, MySqlConnection mySqlConnection)
        {
            //用戶名
            string userName = pack.LoginPack.UserName;
            //密碼
            string password = pack.LoginPack.Password;
                        
            try
            {
                //插入數據
                string sql = $"INSERT INTO {this.listName}.{this.tableName} ({this.userName}, {this.password}, {this.killCount}) VALUES (@userName, @password, @killCount)";

                MySqlCommand comd = new MySqlCommand(sql, mySqlConnection);

                comd.Parameters.AddWithValue("@userName", userName);
                comd.Parameters.AddWithValue("@password", password);
                comd.Parameters.AddWithValue("@killCount", 0);

                comd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        /// <summary>
        /// 登入
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="mySqlConnection"></param>
        /// <returns></returns>
        public bool Login(MainPack pack, MySqlConnection mySqlConnection)
        {
            //用戶名
            string userName = pack.LoginPack.UserName;
            //密碼
            string password = pack.LoginPack.Password;

            string sql = $"SELECT * FROM {this.tableName} WHERE {this.userName} = @userName AND {this.password} = @password";
            MySqlCommand cmd = new MySqlCommand(sql, mySqlConnection);

            cmd.Parameters.AddWithValue("@userName", userName);
            cmd.Parameters.AddWithValue("@password", password);

            MySqlDataReader read = cmd.ExecuteReader();

            bool result = read.HasRows;
            read.Close();

            return result;
        }

        /// <summary>
        /// 搜索擊殺數
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="mySqlConnection"></param>
        /// <returns></returns>
        public int SearchKillCount(MainPack pack, MySqlConnection mySqlConnection)
        {
            //用戶名
            string userName = pack.LoginPack.UserName;

            string sql = $"SELECT {this.killCount} FROM {this.tableName} WHERE {this.userName} = @userName";
            MySqlCommand cmd = new MySqlCommand(sql, mySqlConnection);

            cmd.Parameters.AddWithValue("@userName", userName);

            // 執行查詢並獲取查詢結果
            object result = cmd.ExecuteScalar();

            // 检查结果是否为null
            if (result != null) return Convert.ToInt32(result);
            else Console.WriteLine($"{userName} => 搜索擊殺數錯誤");

            return 0;
        }

        /// <summary>
        /// 更新擊殺數
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="mySqlConnection"></param>
        /// <returns></returns>
        public bool UpdateKillCount(MainPack pack, MySqlConnection mySqlConnection)
        {
            //用戶名
            string userName = pack.PlayerPack[0].PlayerName;
            int newKillsValue = pack.PlayerPack[0].TotalKill + 1;

            string sql = $"UPDATE {tableName} SET {this.killCount} = @newKillsValue WHERE {this.userName} = @userNameToUpdate";
            MySqlCommand cmd = new MySqlCommand(sql, mySqlConnection);

            // 使用参数化查询以防止 SQL 注入
            cmd.Parameters.AddWithValue("@newKillsValue", newKillsValue);
            cmd.Parameters.AddWithValue("@userNameToUpdate", userName);

            int rowsAffected = cmd.ExecuteNonQuery();
            return rowsAffected > 0;
        }
    }
}
