﻿using System;

namespace LifxLib.Messages
{
    public class LifxGetPANGatewayCommand : LifxCommand
    {
        private const UInt16 PACKET_TYPE = 0x02;
        
        public LifxGetPANGatewayCommand()
            : base(PACKET_TYPE, new LifxPANGatewayStateMessage())
        {
            base.IsBroadcastCommand = true;
            base.IsDiscoveryCommand = true;
        }
    }
}
