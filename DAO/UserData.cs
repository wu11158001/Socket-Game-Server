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
                string sql = "INSERT INTO `sys`.`userdata` (`username`, `password`) VALUES ('" + userName + "', '" + password + "')";
                MySqlCommand comd = new MySqlCommand(sql, mySqlConnection);
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

            string sql = "SELECT * FROM userdata WHERE username='" + userName + "' AND password='" + password + "'";
            MySqlCommand cmd = new MySqlCommand(sql, mySqlConnection);
            MySqlDataReader read = cmd.ExecuteReader();

            bool result = read.HasRows;
            read.Close();

            return result;
        }
    }
}
