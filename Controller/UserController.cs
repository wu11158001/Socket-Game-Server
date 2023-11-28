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
            if (client.GetUserData.Login(pack, client.GetMySqlConnection))
            {
                pack.ReturnCode = ReturnCode.Succeed;
                client.UserName = pack.LoginPack.UserName;
            }
            else
            {
                pack.ReturnCode = ReturnCode.Fail;
            }

            return pack;
        }
    }
}
