﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NowMine.Network;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Diagnostics;

namespace NowMine
{
    [XamlCompilation (XamlCompilationOptions.Compile)]
    public partial class YoutubeSearchPage : ContentPage
    {
        YoutubeProvider youtubeProvider;
        ServerConnection serverConnection;

        public delegate void SuccessfulQueuedEventHandler(object s, PiecePosArgs e);
        public event SuccessfulQueuedEventHandler SuccessfulQueued;

        public YoutubeSearchPage(ServerConnection serverConnection)
        {
            InitializeComponent();
            youtubeProvider = new YoutubeProvider();
            this.serverConnection = serverConnection;
            //searchButton.Clicked += searchButton_Click;
            this.Title = "Kolejkuj";
            this.BackgroundColor = Color.Purple;
        }

        public void entSearch_Completed(object s, EventArgs e)
        {
            var entSearch = (Entry)s;
            string text = entSearch.Text;
            var infos = youtubeProvider.LoadVideosKey(text);
            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += AddToQueue_Tapped;
            searchBoard.Children.Clear();
            foreach (YoutubeInfo yi in infos)
            {
                var musicPiece = new MusicPiece(yi);
                musicPiece.GestureRecognizers.Add(tapGestureRecognizer);
                searchBoard.Children.Add(musicPiece);
            }
        }

        private async void AddToQueue_Tapped(object sender, EventArgs e)
        {
            var musicPiece = (MusicPiece)sender;
            int qPos = await serverConnection.SendToQueue(musicPiece.Info);
            Debug.WriteLine("Sended to server {0}; Position on queue {1}", musicPiece.Title, qPos);
            if (qPos != -2)
                OnSuccessfulQueued(musicPiece, qPos);
            else
                Debug.WriteLine("Something gone wronge with queue");
        }

        protected virtual void OnSuccessfulQueued(MusicPiece musicPiece, int qPos)
        {
            SuccessfulQueued?.Invoke(this, new PiecePosArgs(musicPiece, qPos));
        }
    }

    public class PiecePosArgs : EventArgs
    {
        public MusicPiece MusicPiece { get; set; }
        public int QPos { get; set; }
        public PiecePosArgs(MusicPiece musicPiece, int qPos)
        {
            this.MusicPiece = musicPiece;
            MusicPiece.GestureRecognizers.Clear();
            this.QPos = qPos;
        }
    }
}
