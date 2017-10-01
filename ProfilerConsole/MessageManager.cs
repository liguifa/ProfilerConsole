using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace ProfilerConsole
{
    public class MessageManager
    {
        private static MessageManager mMessageManager;
        private Socket mAppServer;
        private List<Socket> mClients = new List<Socket>();
        private static object mSyncRoot = new object();

        private MessageManager()
        {

        }

        public static MessageManager GetInstance()
        {
            if(mMessageManager == null)
            {
                mMessageManager = new MessageManager();
            }
            return mMessageManager;
        }

        public void Stert()
        {
            lock(mSyncRoot)
            {
                if (mAppServer == null)
                {
                    mAppServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    mAppServer.Bind(new IPEndPoint(IPAddress.Any, 0927));
                    mAppServer.Listen(0936);
                    mAppServer.BeginAccept(Accept, mAppServer);
                    ProfilerEventSet.AddEvent += ProfilerEventSet_AddEvent;
                }
            }
        }

        private void ProfilerEventSet_AddEvent(ProfilerEvent obj)
        {
            Parallel.ForEach(mClients, client =>
            {
                var buffer = GetBuffer(obj);
                Send(client, buffer);
            });
        }

        private void Accept(IAsyncResult ar)
        {
            //这就是客户端的Socket实例，我们后续可以将其保存起来
            Socket client = mAppServer.EndAccept(ar);
            mClients.Add(client);
            //给客户端发送一个欢迎消息
            //client.Send(Encoding.Unicode.GetBytes("connection finsh."));
            //Receive(client, Encoding.Default.GetBytes("connection finsh."));
            //var request = HttpContext.Current.Request;
            byte[] buffer = new byte[1024];
            client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, a => ReceiveMessage(a, buffer), client);
        }

        private void Send(Socket client, byte[] buffer)
        {

            //Task.Run(() =>
            //{
            //    client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback((ar) =>
            //    {
            //        string msg = Encoding.Default.GetString(buffer, 0, buffer.Length);
            //        //Handshake(client, msg);
            //        client.Send()
            //    }), mAppServer);
            //});
            client.Send(buffer);
        }

        public void ReceiveMessage(IAsyncResult ar,byte[] buffer)
        {
            try
            {
                var socket = ar.AsyncState as Socket;
                var length = socket.EndReceive(ar);
                var message = this.AnalyzeMessage(buffer, buffer.Length, socket, true);
                if (message.Contains("Sec-WebSocket-Key"))
                {
                    this.Handshake(socket, message);
                }
                else
                {
                    message = this.AnalyzeMessage(buffer, buffer.Length, socket);
                    //HRequest ttpWebRequest request = StringToObject<Request>(message);
                    //if (request.IsConnect)
                    //{
                    //    this.mClients.Add(socket);
                    //}
                    //else
                    //{
                    //    this.Send(request.CourseId, request.Message, request.UserType, request.Username);
                    //}
                }
                //接收下一个消息(因为这是一个递归的调用，所以这样就可以一直接收消息了）
                buffer = new byte[1024];
                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, a => ReceiveMessage(a, buffer), socket);
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
            }
        }

        public static T StringToObject<T>(string str)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                byte[] bytes = Convert.FromBase64String(str);
                stream.Write(bytes, 0, bytes.Length);
                stream.Position = 0;
                IFormatter formatter = new BinaryFormatter();
                return (T)formatter.Deserialize(stream);
            }
        }

        //public byte[] EnclosureMessage(string msg, string from, string to, string courseId)
        //{
        //    Response response = new Response()
        //    {
        //        CourseId = courseId,
        //        From = from,
        //        Message = msg,
        //        Server = Dns.GetHostName(),
        //        To = to
        //    };
        //    string responseJson = JsonConvert.SerializeObject(response);
        //    byte[] content = null;
        //    byte[] temp = Encoding.UTF8.GetBytes(responseJson);
        //    if (temp.Length < 126)
        //    {
        //        content = new byte[temp.Length + 2];
        //        content[0] = 0x81;
        //        content[1] = (byte)temp.Length;
        //        Array.Copy(temp, 0, content, 2, temp.Length);
        //    }
        //    else if (temp.Length < 0xFFFF)
        //    {
        //        content = new byte[temp.Length + 4];
        //        content[0] = 0x81;
        //        content[1] = 126;
        //        content[2] = (byte)(temp.Length & 0xFF);
        //        content[3] = (byte)(temp.Length >> 8 & 0xFF);
        //        Array.Copy(temp, 0, content, 4, temp.Length);
        //    }
        //    else
        //    {
        //        // 暂不处理超长内容  
        //    }
        //    return content;
        //}
        private string AnalyzeMessage(byte[] recBytes, int length, Socket client, bool isHandshake = false)
        {
            if (!isHandshake && this.mClients.Contains(client))
            {
                if (length < 2)
                {
                    return string.Empty;
                }
                bool fin = (recBytes[0] & 0x80) == 0x80; // 1bit，1表示最后一帧  
                if (!fin)
                {
                    return string.Empty;// 超过一帧暂不处理 
                }
                bool mask_flag = (recBytes[1] & 0x80) == 0x80; // 是否包含掩码  
                if (!mask_flag)
                {
                    return string.Empty;// 不包含掩码的暂不处理
                }
                int payload_len = recBytes[1] & 0x7F; // 数据长度  
                byte[] masks = new byte[4];
                byte[] payload_data;
                if (payload_len == 126)
                {
                    Array.Copy(recBytes, 4, masks, 0, 4);
                    payload_len = (UInt16)(recBytes[2] << 8 | recBytes[3]);
                    payload_data = new byte[payload_len];
                    Array.Copy(recBytes, 8, payload_data, 0, payload_len);
                }
                else if (payload_len == 127)
                {
                    Array.Copy(recBytes, 10, masks, 0, 4);
                    byte[] uInt64Bytes = new byte[8];
                    for (int i = 0; i < 8; i++)
                    {
                        uInt64Bytes[i] = recBytes[9 - i];
                    }
                    UInt64 len = BitConverter.ToUInt64(uInt64Bytes, 0);
                    payload_data = new byte[len];
                    for (UInt64 i = 0; i < len; i++)
                    {
                        payload_data[i] = recBytes[i + 14];
                    }
                }
                else
                {
                    Array.Copy(recBytes, 2, masks, 0, 4);
                    payload_data = new byte[payload_len];
                    Array.Copy(recBytes, 6, payload_data, 0, payload_len);
                }
                for (var i = 0; i < payload_len; i++)
                {
                    payload_data[i] = (byte)(payload_data[i] ^ masks[i % 4]);
                }
                return Encoding.Default.GetString(payload_data);
            }
            string req = Encoding.Default.GetString(recBytes);
            req = req.Substring(0, req.IndexOf('\0'));
            return req;
        }

        private byte[] GetBuffer(ProfilerEvent @event)
        {
            DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(ProfilerEvent));
            MemoryStream ms = new MemoryStream();
            js.WriteObject(ms, @event);
            return ms.GetBuffer();
        }

        private void Handshake(Socket client, string clientMsg)
        {
            if (client.Connected)
            {
                try
                {
                    string key = string.Empty;
                    Regex reg = new Regex(@"Sec\-WebSocket\-Key:(.*?)\r\n");
                    Match m = reg.Match(clientMsg);
                    if (m.Value != "")
                    {
                        key = Regex.Replace(m.Value, @"Sec\-WebSocket\-Key:(.*?)\r\n", "$1").Trim();
                    }
                    byte[] secKeyBytes = SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"));
                    string secKey = Convert.ToBase64String(secKeyBytes);
                    var responseBuilder = new StringBuilder();
                    responseBuilder.Append("HTTP/1.1 101 Switching Protocols" + "\r\n");
                    responseBuilder.Append("Upgrade:websocket" + "\r\n");
                    responseBuilder.Append("Connection:Upgrade" + "\r\n");
                    responseBuilder.Append("Sec-WebSocket-Accept:" + secKey + "\r\n\r\n");
                    client.Send(Encoding.Default.GetBytes(responseBuilder.ToString()));
                }
                catch (Exception e)
                {
                }
            }
        }

        ~ MessageManager()
        {
            //mAppServer.Disconnect(true);
            //mAppServer.Close();
            //mAppServer.Dispose();
            //mMessageManager = null;
        }
    }
}
