using System;
using System.Collections.Generic;
using Windows.Networking;
using LifxLib.Messages;

namespace LifxLib
{
    public class LifxPanController
    {
        public string MACAddress { get; set; }
        public string IPAddress { get; set; }
        public List<LifxBulb> Bulbs { get; set; }

        /// <summary>
        /// Uninitialized bulb, for detection for instance
        /// </summary>
        public LifxPanController()
        {
            MACAddress = "";
            IPAddress = "";
            Bulbs = new List<LifxBulb>();
        }

        public static LifxPanController UninitializedPanController
        {
            get { return new LifxPanController() { IPAddress = LifxHelper.GetLocalBroadcastIPAddress(), MACAddress = "" }; }
        }
    }
}
