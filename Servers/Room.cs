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
        //紀錄已發送遊戲結果的玩家
        private List<Client> getResultClientList = new List<Client>();

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
                player.HP = c.GetUserInfo.HP;
                pack.Add(player);
            }

            return pack;
        }

        /// <summary>
        /// 設定房間玩家訊息
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="hp"></param>
        void SetPlayerInfo(string userName, int hp)
        {
            for (int i = 0; i < clientList.Count; i++)
            {
                if(clientList[i].GetUserInfo.UserName == userName)
                {
                    clientList[i].GetUserInfo.HP = hp;
                    break;
                }
            }
        }

        /// <summary>
        /// 廣播
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

            //房間只剩1人關閉房間
            if (clientList.Count == 1)
            {
                client.GetRoom = null;
                server.RemoveRoom(this);
                return;
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
            pack.Str = "遊戲準備開始...";
            Broadcast(null, pack);
            Thread.Sleep(1000);

            for (int i = 2; i > 0; i--)
            {
                pack.Str = i.ToString();
                Broadcast(null, pack);
                Thread.Sleep(1000);
            }

            getResultClientList.Clear();
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
            if(clientList.Count == 1)
            {
                server.RemoveRoom(this);
            }
            else
            {
                //其他玩家退出
                clientList.Remove(client);
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

        /// <summary>
        /// 玩家攻擊
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        public void PlayerAttack(Client client, MainPack pack)
        {
            int attackPower = 10;//攻擊力

            MainPack mainPack = new MainPack();
            mainPack.ActionCode = ActionCode.PlayerAttack;
            mainPack.Str = "PlayerAttack";

            AttackPack attackPack = pack.PlayerPack[0].AttackPack;
            List<PlayerPack> playerPacks = GetPlayerInfo().ToList();
            foreach (var player in attackPack.AttackNames)
            {
                for (int i = 0; i < playerPacks.Count; i++)
                {
                    if (player == playerPacks[i].PlayerName && player != client.GetUserInfo.UserName)
                    {
                        PlayerPack playerPack = new PlayerPack();
                        playerPack.PlayerName = playerPacks[i].PlayerName;

                        if (playerPacks[i].HP - attackPower >= 0)
                        {
                            int hp = playerPacks[i].HP - attackPower;
                            SetPlayerInfo(player, hp);
                            playerPack.HP = hp;
                        }

                        mainPack.PlayerPack.Add(playerPack);
                        break;
                    }
                }
            }

            pack.Str = "PlayerAttack";
            Broadcast(null, mainPack);

            UpdateCharacterList();
        }

        /// <summary>
        /// 更新玩家訊息列表
        /// </summary>
        void UpdateCharacterList()
        {
            List<Client> dieClientList = new List<Client>();

            MainPack pack = new MainPack();
            pack.ActionCode = ActionCode.UpdateCharacterList;
            foreach (var player in clientList)
            {
                PlayerPack playerPack = new PlayerPack();
                playerPack.PlayerName = player.GetUserInfo.UserName;
                playerPack.HP = player.GetUserInfo.HP;
                pack.PlayerPack.Add(playerPack);

                //紀錄死亡玩家
                if (player.GetUserInfo.HP <= 0) dieClientList.Add(player);
            }

            pack.Str = "UpdateCharacterList";
            Broadcast(null, pack);

            JudgeGameResult(dieClientList);
        }

        /// <summary>
        /// 判斷遊戲結果
        /// </summary>
        /// <param name="dieClientList"></param>
        void JudgeGameResult(List<Client> dieClientList)
        {
            if (dieClientList.Count > 0)
            {
                MainPack pack = new MainPack();
                pack.ActionCode = ActionCode.GameResult;
                pack.Str = "GameResult";

                foreach (var player in dieClientList)
                {
                    if (!getResultClientList.Contains(player))
                    {
                        PlayerPack playerPack = new PlayerPack();
                        playerPack.PlayerName = player.GetUserInfo.UserName;
                        pack.PlayerPack.Add(playerPack);
                        pack.ReturnCode = ReturnCode.Fail;
                        
                        getResultClientList.Add(player);
                        player.Send(pack);
                    }
                }

                int surviveCount = clientList.Where(x => x.GetUserInfo.HP > 0).Count();
                if (surviveCount == 1)
                {
                    pack = new MainPack();
                    pack.ActionCode = ActionCode.GameResult;
                    pack.Str = "GameResult";
                    Client player = clientList.Where(x => x.GetUserInfo.HP > 0)
                                              .FirstOrDefault();

                    PlayerPack playerPack = new PlayerPack();
                    playerPack.PlayerName = player.GetUserInfo.UserName;
                    pack.PlayerPack.Add(playerPack);
                    pack.ReturnCode = ReturnCode.Succeed;
                    player.Send(pack);
                }
            }
        }
    }
}
