﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace pr3
{
    internal class Program
    {
        public static List<Leaders> Leaders = new List<Leaders>();

        public static List<ViewModelUserSettings> remoteIPAddress = new List<ViewModelUserSettings>();

        public static List<ViewModelGames> viewModelGames = new List<ViewModelGames>();

        private static int localPort = 5001;

        public static int MaxSpeed = 15;
        static void Main(string[] args)
        {

        }
    }
}

