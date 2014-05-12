using System;

namespace LifxLib.Messages
{
    public class LifxGetPowerStateCommand : LifxCommand
    {
        private const CommandPacketType PACKET_TYPE = CommandPacketType.GetPowerState;

        public LifxGetPowerStateCommand()
            : base(PACKET_TYPE)
        {

        }
    }
}
