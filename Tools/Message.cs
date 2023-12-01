using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocketGameProtobuf;
using Google.Protobuf;

namespace SocketGameServer.Tools
{
    class Message
    {
        private byte[] buffer = new byte[1024];
        public byte[] GetBuffer { get { return buffer; }}

        //當前存放Index
        private int startIndex;
        public int GetStartIndex { get { return startIndex; } }

        //剩餘空間
        public int GetRemSize { get { return buffer.Length - startIndex; } }

        /// <summary>
        /// 解析Buffer
        /// </summary>
        /// <param name="len">消息長度</param>
        /// <param name="handleRequest">回傳方法</param>
        public void ReadBuffer(int len, Action<MainPack> handleRequest)
        {
            //當前存放長度
            startIndex += len;

            //數據不完整 4 = (包頭(數據長度int固定4個byte))
            if (startIndex <= 4) return;

            //包體長度
            int count = BitConverter.ToInt32(buffer, 0);

            while (true)
            {
                //消息完整
                if (startIndex >= count + 4)
                {
                    MainPack pack = (MainPack)MainPack.Descriptor.Parser.ParseFrom(buffer, 4, count);
                    //解析消息回調方法
                    handleRequest(pack);

                    Array.Copy(buffer, count + 4, buffer, 0, startIndex - count - 4);
                    startIndex -= count + 4;
                }
                else break;
            }
        }

        /// <summary>
        /// 數據封裝TCP
        /// </summary>
        /// <param name="pack"></param>
        /// <returns></returns>
        public static byte[] PackData(MainPack pack)
        {
            //包體
            byte[] data = pack.ToByteArray();
            //包頭
            byte[] head = BitConverter.GetBytes(data.Length);

            return head.Concat(data).ToArray();
        }

        /// <summary>
        /// 數據封裝UDP
        /// </summary>
        /// <param name="pack"></param>
        /// <returns></returns>
        public static byte[] PackDataUDP(MainPack pack)
        {
            return pack.ToByteArray();
        }
    }
}
