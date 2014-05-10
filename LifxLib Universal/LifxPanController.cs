using System;
using System.Collections.Generic;
using Windows.Networking;
using LifxLib.Messages;

namespace LifxLib
{
    public class LifxPanController
    {
        private string mMacAddress = "";        
        private HostName mIpAddress;
        List<LifxBulb> mBulbs = new List<LifxBulb>();

       
        public LifxPanController(string macAddress, HostName ipAddress)
        {
            mMacAddress = macAddress;
            mIpAddress = ipAddress;
        }

        /// <summary>
        /// Uninitialized bulb, for detection for instance
        /// </summary>
        public LifxPanController()
        { 
            
        }


        public static LifxPanController UninitializedPanController
        {
            get { return new LifxPanController(); }
        }

        
        /// <summary>
        /// Will return empty so that pan controller messages are sent to all bulbs
        /// </summary>
        public string MacAddress
        {
            get { return mMacAddress; }
            set { mMacAddress = value; }
        }

        public HostName HostName
        {
            get { return mIpAddress; }
            set { mIpAddress = value; }
        }
        
        public List<LifxBulb> Bulbs
        {
            get { return mBulbs; }
            set { mBulbs = value; }
        }
    }
}
