using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocketGameProtobuf;
using SocketGameServer.Servers;

namespace SocketGameServer.Controller
{
    class RoomController : BaseController
    {
        public RoomController()
        {
            requestCode = RequestCode.Room;
        }

        /// <summary>
        /// 創建房間
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public MainPack CreateRoom(Server server, Client client, MainPack pack)
        {
            return server.CreateRoom(client, pack);
        }

        /// <summary>
        /// 查找房間
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        public MainPack SearchRoom(Server server, Client client, MainPack pack)
        {
            return server.SearchRoom();
        }

        /// <summary>
        /// 加入房間
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        public MainPack JoinRoom(Server server, Client client, MainPack pack)
        {
            return server.JoinRoom(client, pack);
        }

        /// <summary>
        /// 離開(房間/遊戲)
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        public MainPack Exit(Server server, Client client, MainPack pack)
        {
            return server.ExitRoom(client, pack);
        }

        /// <summary>
        /// 條天
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        public MainPack Chat(Server server, Client client, MainPack pack)
        {
            pack.Str = client.GetUserInfo.UserName + ":" + pack.Str;
            server.Chat(client, pack);

            return null;
        }

        /// <summary>
        /// 開始遊戲
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        public MainPack StartGame(Server server, Client client, MainPack pack)
        {
            pack.ReturnCode = client.GetRoom.StartGame(client);
            return pack;
        }
    }
}
