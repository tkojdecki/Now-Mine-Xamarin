﻿using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace NowMine.Network
{
    public class ServerConnection
    {
        public delegate void ServerConnectedEventHandler(object s, EventArgs e);
        public event ServerConnectedEventHandler ServerConnected;
        private string serverAddress = "";

        private UDPConnector _udpConnector;
        public UDPConnector udpConnector
        {
            get
            {
                if (_udpConnector == null)
                {
                    _udpConnector = new UDPConnector();
                }
                return _udpConnector;
            }
        }

        private TCPConnector _tcpConnector;
        public TCPConnector tcpConnector
        {
            get
            {
                if (_tcpConnector == null)
                {
                    _tcpConnector = new TCPConnector();
                }
                return _tcpConnector;
            }
        }


        protected virtual void OnServerConnected()
        {
            ServerConnected?.Invoke(this, EventArgs.Empty);
        }

        public bool findServer()
        {
            tcpConnector.MessegeReceived += OnServerFound;
            Device.StartTimer(TimeSpan.FromSeconds(3), () =>
           {
               Task.Factory.StartNew(async () =>
               {
                   await udpConnector.sendBroadcastUdp("NowMine!");
                   await tcpConnector.receiveTCP();
                   if (string.IsNullOrEmpty(serverAddress))
                   {
                       return true;
                   }
                       return false;
               });
               return false;
            });
            return false;
        }

        public async Task<IList<YoutubeInfo>> getQueue()
        {
            //tcpConnector.MessegeReceived += OnQueueReceived;
            byte[] bQueue = await tcpConnector.getBSON("GetQueue", serverAddress);
            using (MemoryStream ms = new MemoryStream(bQueue))
            using (BsonReader reader = new BsonReader(ms))
            {
                reader.ReadRootValueAsArray = true;
                JsonSerializer serializer = new JsonSerializer();
                IList<YoutubeInfo> ytInfos = serializer.Deserialize<IList<YoutubeInfo>>(reader);
                Debug.WriteLine("YTINFO COUNT: {0}", ytInfos.Count);
                return ytInfos;
            }
            //await tcpConnector.receiveTCP();
        }

        //private void OnQueueReceived(object source, MessegeEventArgs args)
        //{
        //    string messege = args.messege;

        //    //message -> from json  to music piece
        //    //QueuePage.show queue
        //}

        private void OnServerFound(object source, MessegeEventArgs args)
        {
            string messege = args.messege;
            //tutaj sprawdzanie czy to ip itd
            serverAddress = messege;
            OnServerConnected();
        }

        public bool isWifi()
        {
            Debug.WriteLine("NET: Getting Wifi status");
            var networkConnection = DependencyService.Get<INetworkConnection>();
            networkConnection.CheckNetworkConnection();
            bool status = networkConnection.IsConnected;
            Debug.WriteLine("NET: Wifi status is {0}", status);
            return status;
        }
    }
}
