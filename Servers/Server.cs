using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using SocketGameServer.Controller;
using SocketGameProtobuf;

namespace SocketGameServer.Servers
{
    class Server
    {
        private Socket socket;

        //存放所有連接的客戶端
        private List<Client> clientList = new List<Client>();
        public List<Client> GetClientList { get { return clientList; } }

        private ControllerManager controllerManager;

        //存放所有房間
        private List<Room> roomList = new List<Room>();
        public int GetRoomCount { get { return roomList.Count; } }

        public Server(int port)
        {
            controllerManager = new ControllerManager(this);

            //Socket初始化
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //綁定
            socket.Bind(new IPEndPoint(IPAddress.Any, port));
            //監聽
            socket.Listen(0);
            //開始接收
            StartAccect();
        }

        /// <summary>
        /// 開始接收
        /// </summary>
        void StartAccect()
        {
            socket.BeginAccept(AccectCallBack, null);
        }
        /// <summary>
        /// 接收CallBack
        /// </summary>
        void AccectCallBack(IAsyncResult iar)
        {
            Socket client = socket.EndAccept(iar);
            clientList.Add(new Client(client, this));
            StartAccect();
        }

        /// <summary>
        /// 處理請求
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="client"></param>
        public void HandleRequest(MainPack pack, Client client)
        {
            controllerManager.HandleRequest(pack, client);
        }

        /// <summary>
        /// 移除客戶端
        /// </summary>
        /// <param name="client"></param>
        public void RemoveClient(Client client)
        {
            clientList.Remove(client);
        }

        /// <summary>
        /// 創建房間
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        public MainPack CreateRoom(Client client, MainPack pack)
        {
            try
            {
                Room room = new Room(this, client, pack.RoomPack[0]);
                roomList.Add(room);

                foreach (PlayerPack p in room.GetRoomPlayerInfo())
                {
                    pack.PlayerPack.Add(p);
                }

                pack.ReturnCode = ReturnCode.Succeed;
                return pack;
            }
            catch (Exception)
            {
                pack.ReturnCode = ReturnCode.Fail;
                return pack;
            } 
        }

        /// <summary>
        /// 查詢房間
        /// </summary>
        /// <returns></returns>
        public MainPack SearchRoom()
        {
            MainPack pack = new MainPack();
            pack.ActionCode = ActionCode.SearchRoom;
            try
            {
                if(roomList.Count == 0)
                {
                    pack.ReturnCode = ReturnCode.NotRoom;
                    return pack;
                }

                foreach (Room room in roomList)
                {                    
                    pack.RoomPack.Add(room.GetRoomInfo);
                }

                pack.ReturnCode = ReturnCode.Succeed;
            }
            catch (Exception)
            {
                pack.ReturnCode = ReturnCode.Fail;
            }            

            return pack;
        }

        /// <summary>
        /// 加入房間
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        public MainPack JoinRoom(Client client, MainPack pack)
        {
            foreach (Room r in roomList)
            {
                if(r.GetRoomInfo.RoomName.Equals(pack.Str))
                {
                    if(r.GetRoomInfo.State == 0)
                    {
                        //可以加入房間
                        r.Join(client);
                        pack.RoomPack.Add(r.GetRoomInfo);
                        foreach(PlayerPack p in r.GetRoomPlayerInfo())
                        {
                            pack.PlayerPack.Add(p);
                        }
                        pack.ReturnCode = ReturnCode.Succeed;
                        return pack;
                    }
                    else
                    {
                        //無法加入房間
                        pack.ReturnCode = ReturnCode.Fail;
                        return pack;
                    }
                }
            }

            //沒有此房間
            pack.ReturnCode = ReturnCode.NotRoom;
            return pack;
        }

        /// <summary>
        /// 離開房間
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        public MainPack ExitRoom(Client client, MainPack pack)
        {
            if (client.GetRoom == null)
            {
                pack.ReturnCode = ReturnCode.Fail;
                return null;
            }

            client.GetRoom.Exit(this, client);
            pack.ReturnCode = ReturnCode.Succeed;
            return pack;
        }

        /// <summary>
        /// 移除房間
        /// </summary>
        /// <param name="room"></param>
        public void RemoveRoom(Room room)
        {
            roomList.Remove(room);
        }

        /// <summary>
        /// 聊天
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        public void Chat(Client client, MainPack pack)
        {
            client.GetRoom.Broadcast(client, pack);
        }
    }
}
