using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocketGameProtobuf;
using Google.Protobuf.Collections;
using System.Threading;

namespace SocketGameServer.Servers
{
    class Room
    {
        private Server server;

        //房間內所有客戶端
        private List<Client> clientList = new List<Client>();

        //房間訊息
        private RoomPack roomInfo;
        public RoomPack GetRoomInfo 
        { 
            get 
            {
                roomInfo.CurrCount = clientList.Count;
                return roomInfo; 
            } 
        }

        public Room(Server server, Client client, RoomPack pack)
        {
            this.server = server;
            roomInfo = pack;
            clientList.Add(client);
            client.GetRoom = this;
        }

        /// <summary>
        /// 獲取房間玩家訊息
        /// </summary>
        /// <returns></returns>
        public RepeatedField<PlayerPack> GetPlayerInfo()
        {
            RepeatedField<PlayerPack> pack = new RepeatedField<PlayerPack>();
            foreach (Client c in clientList)
            {
                PlayerPack player = new PlayerPack();
                player.PlayerName = c.GetUserInfo.UserName;
                pack.Add(player);
            }

            return pack;
        }

        /// <summary>
        /// 廣播TCP
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        public void Broadcast(Client client, MainPack pack)
        {
            foreach (Client c in clientList)
            {
                //排除的cliemt
                if (c.Equals(client)) continue;

                c.Send(pack);
            }
        }

        /// <summary>
        /// 廣播UDP
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        public void BroadcastUDP(Client client, MainPack pack)
        {
            foreach (Client c in clientList)
            {
                //排除的cliemt
                if (c.Equals(client)) continue;

                c.SendUDP(pack);
            }
        }

        /// <summary>
        /// 添加客戶端
        /// </summary>
        /// <param name="client"></param>
        public void Join(Client client)
        {
            clientList.Add(client);

            //房間滿人
            if(roomInfo.MaxCount >= clientList.Count)
            {
                roomInfo.State = 1;
            }

            client.GetRoom = this;

            MainPack pack = new MainPack();
            pack.ActionCode = ActionCode.PlayerList;

            //賦值
            foreach (PlayerPack player in GetPlayerInfo())
            {
                pack.PlayerPack.Add(player);
            }

            Broadcast(client, pack);
        }

        /// <summary>
        /// 離開房間
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        public void Exit(Server server ,Client client)
        {
            MainPack pack = new MainPack();
            if (roomInfo.State == 2)//遊戲已開始
            {
                ExitGame(client);            
            }
            else
            {
                //房主離開房間
                if (client == clientList[0])
                {
                    client.GetRoom = null;
                    pack.ActionCode = ActionCode.Exit;
                    Broadcast(client, pack);
                    server.RemoveRoom(this);
                    return;
                }
            }            

            clientList.Remove(client);
            roomInfo.State = 0;
            client.GetRoom = null;                        
            pack.ActionCode = ActionCode.PlayerList;

            //賦值
            foreach (PlayerPack player in GetPlayerInfo())
            {
                pack.PlayerPack.Add(player);
            }

            Broadcast(client, pack);
        }

        /// <summary>
        /// 開始遊戲
        /// </summary>
        public ReturnCode StartGame(Client client)
        {
            if (client != clientList[0]) return ReturnCode.Fail;

            Thread startTime = new Thread(CountDownTime);
            startTime.Start();

            return ReturnCode.Succeed;
        }

        /// <summary>
        /// 倒計時
        /// </summary>
        void CountDownTime()
        {
            MainPack pack = new MainPack();
            pack.ActionCode = ActionCode.Chat;
            pack.Str = "準備開始遊戲";
            Broadcast(null, pack);
            Thread.Sleep(1000);

            for (int i = 2; i > 0; i--)
            {
                pack.Str = i.ToString();
                Broadcast(null, pack);
                Thread.Sleep(1000);
            }

            pack.ActionCode = ActionCode.ServerStartGame;

            foreach (var c in clientList)
            {
                PlayerPack player = new PlayerPack();
                c.GetUserInfo.HP = 100;
                player.PlayerName = c.GetUserInfo.UserName;
                player.HP = c.GetUserInfo.HP;
                pack.PlayerPack.Add(player);
            }

            Broadcast(null, pack);
        }

        /// <summary>
        /// 退出遊戲
        /// </summary>
        /// <param name="client"></param>
        public void ExitGame(Client client)
        {
            MainPack pack = new MainPack();
            if(client == clientList[0])
            {
                //房主退出
                pack.ActionCode = ActionCode.ExitGame;
                pack.Str = "ExitGame";
                Broadcast(client, pack);
                server.RemoveRoom(this);
                client.GetRoom = null;
            }
            else
            {
                //其他玩家退出
                clientList.Remove(client);
                client.GetRoom = null;
                pack.ActionCode = ActionCode.UpdateCharacterList;
                foreach (var player in clientList)
                {
                    PlayerPack playerPack = new PlayerPack();
                    playerPack.PlayerName = player.GetUserInfo.UserName;
                    playerPack.HP = player.GetUserInfo.HP;
                    pack.PlayerPack.Add(playerPack);
                }

                pack.Str = client.GetUserInfo.UserName;
                Broadcast(client, pack);
            }
        }
    }
}
