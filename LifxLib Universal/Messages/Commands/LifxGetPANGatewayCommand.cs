using System;

namespace LifxLib.Messages
{
    public class LifxGetPanGatewayCommand : LifxCommand
    {
        private const CommandPacketType PACKET_TYPE = CommandPacketType.GetPanGateway;
        
        public LifxGetPanGatewayCommand()
            : base(PACKET_TYPE)
        {
            base.IsBroadcastCommand = true;
            base.IsDiscoveryCommand = true;
        }
    }
}
