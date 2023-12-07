using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocketGameProtobuf;
using SocketGameServer.Servers;

namespace SocketGameServer.Controller
{
    class UserController : BaseController
    {
        public UserController()
        {
            requestCode = RequestCode.User;
        }

        /// <summary>
        /// 註冊
        /// </summary>
        /// <returns></returns>
        public MainPack Logon(Server servers, Client client, MainPack pack)
        {
            if (client.GetUserData.Logon(pack, client.GetMySqlConnection))
            {
                pack.ReturnCode = ReturnCode.Succeed;
            }
            else
            {
                pack.ReturnCode = ReturnCode.Fail;
            }

            return pack;
        }

        /// <summary>
        /// 登入
        /// </summary>
        /// <returns></returns>
        public MainPack Login(Server server, Client client, MainPack pack)
        {
            if(server.GetClientList.Any(list => list.GetUserInfo.UserName == pack.LoginPack.UserName))
            {
                pack.ReturnCode = ReturnCode.DuplicateLogin;
                Console.WriteLine(pack.LoginPack.UserName + " => 重複登入");
                return pack;
            }

            if (client.GetUserData.Login(pack, client.GetMySqlConnection))
            {
                pack.ReturnCode = ReturnCode.Succeed;
                client.GetUserInfo.UserName = pack.LoginPack.UserName;
                client.GetUserInfo.TotalKill = client.GetUserData.SearchKillCount(pack, client.GetMySqlConnection);

                Console.WriteLine(pack.LoginPack.UserName + " => 已登入");
            }
            else
            {
                pack.ReturnCode = ReturnCode.Fail;
            }

            return pack;
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        public MainPack Logout(Server server, Client client, MainPack pack)
        {
            Console.WriteLine(client.GetUserInfo.UserName + " => 用戶登出");
            server.RemoveClient(client);
            return null;
        }
    }
}
