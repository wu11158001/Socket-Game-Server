using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocketGameProtobuf;
using SocketGameServer.Servers;

namespace SocketGameServer.Controller
{
    class GameController : BaseController
    {
        public GameController()
        {
            requestCode = RequestCode.Game;
        }

        /// <summary>
        /// 退出遊戲
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        public MainPack ExitGame(Server server, Client client, MainPack pack)
        {
            client.GetRoom.ExitGame(client);
            return null;
        }

        /// <summary>
        /// 更新角色位置
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        public MainPack UpdatePos(Server server, Client client, MainPack pack)
        {
            client.GetRoom.Broadcast(client, pack);
            return null;
        }

        /// <summary>
        /// 發射子彈
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        public MainPack UpdateAni(Server server, Client client, MainPack pack)
        {
            client.GetRoom.Broadcast(client, pack);
            return null;
        }
    }
}
