using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocketGameProtobuf;
using System.Reflection;
using SocketGameServer.Servers;

namespace SocketGameServer.Controller
{
    class ControllerManager
    {
        protected Dictionary<RequestCode, BaseController> controllDic = new Dictionary<RequestCode, BaseController>();
        private Server server;

        public ControllerManager(Server server)
        {
            this.server = server;

            UserController userController = new UserController();
            controllDic.Add(userController.GetRequestCode, userController);

            RoomController roomController = new RoomController();
            controllDic.Add(roomController.GetRequestCode, roomController);

            GameController gameController = new GameController();
            controllDic.Add(gameController.GetRequestCode, gameController);
        }

        /// <summary>
        /// 處理請求
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="client"></param>
        /// <param name="isUPD"></param>
        public void HandleRequest(MainPack pack, Client client, bool isUPD = false)
        {
            if(controllDic.TryGetValue(pack.RequestCode, out BaseController controller))
            {
                string metname = pack.ActionCode.ToString();
                MethodInfo method = controller.GetType().GetMethod(metname);
                if(method == null)
                {
                    Console.WriteLine("沒有找到對應的處理方法:" + metname);
                    return;
                }

                object[] objs;
                if (isUPD)//UDP
                {
                    objs = new object[] { client, pack };
                    method.Invoke(controller, objs);
                }
                else//TCP
                {
                    objs = new object[] { server, client, pack };
                    object ret = method.Invoke(controller, objs);
                    if (ret != null)
                    {
                        client.Send(ret as MainPack);
                        Console.WriteLine("發送數據:");
                    }
                }                
            }
            else
            {
                Console.WriteLine("沒有找到對應的 controller 處理");
            }
        }
    }
}
