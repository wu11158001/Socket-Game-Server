using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SocketGameProtobuf;
using SocketGameServer.Controller;
using SocketGameServer.Tools;

namespace SocketGameServer.Servers
{
    class UDPServer
    {
        Socket udpServer;
        IPEndPoint bindEP;//本地監聽ip
        EndPoint remoteEP;//遠端ip

        Server server;
        ControllerManager controllerManager;

        Byte[] buffer = new Byte[1024];//消息緩存
        Thread receiveThread;//接收線程

        public UDPServer(int port, Server server, ControllerManager controllerManager)
        {
            this.server = server;
            this.controllerManager = controllerManager;

            udpServer = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            bindEP = new IPEndPoint(IPAddress.Any, port);
            remoteEP = (EndPoint)bindEP;
            udpServer.Bind(bindEP);

            receiveThread = new Thread(ReceiveMsg);
            receiveThread.Start();

            Console.WriteLine("UDP 服務已啟動");
        }

        ~UDPServer()
        {
            if (receiveThread != null)
            {
                receiveThread.Abort();
                receiveThread = null;
            }
        }

        /// <summary>
        /// 接收訊息
        /// </summary>
        public void ReceiveMsg()
        {
            while(true)
            {
                int len = udpServer.ReceiveFrom(buffer, ref remoteEP);
                MainPack pack = (MainPack)MainPack.Descriptor.Parser.ParseFrom(buffer, 0, len);
                HandleRequest(pack, remoteEP);
            }
        }

        /// <summary>
        /// 處理請求
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="ipEndPoint"></param>
        public void HandleRequest(MainPack pack, EndPoint ipEndPoint)
        {
            Client client = server.ClientFromUserName(pack.User);
            if (client.IEP == null)
            {
                client.IEP = ipEndPoint;
            }            

            controllerManager.HandleRequest(pack, client, true);
        }

        /// <summary>
        /// 發送消息
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="point"></param>
        public void SendUDP(MainPack pack, EndPoint point)
        {
            byte[] buff = Message.PackDataUDP(pack);
            udpServer.SendTo(buff, buff.Length, SocketFlags.None, point);
        }
    }
}
