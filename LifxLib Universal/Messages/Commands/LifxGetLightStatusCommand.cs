using System;

namespace LifxLib.Messages
{
    public class LifxGetLightStateCommand : LifxCommand
    {
        private const CommandPacketType PACKET_TYPE = CommandPacketType.GetLightState;

        public LifxGetLightStateCommand()
            : base(PACKET_TYPE)
        {

        }
    }
}
