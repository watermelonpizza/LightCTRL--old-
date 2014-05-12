using System;

namespace LifxLib.Messages
{
    public class LifxPowerStateMessage : LifxMessage
    {
        private const MessagePacketType PACKET_TYPE = MessagePacketType.PowerState;

        public LifxPowerStateMessage(LifxDataPacket packet)
            : base(packet, PACKET_TYPE)
        {

        }

        public LifxPowerState PowerState
        {
            get 
            {
                if (BitConverter.ToUInt16(ReceivedData.Payload, 0) == 0)
                    return LifxPowerState.Off;
                if (BitConverter.ToUInt16(ReceivedData.Payload, 0) == UInt16.MaxValue)
                    return LifxPowerState.On;
                else
                    return LifxPowerState.Unknown;
            }
        }

        public Boolean GetPowerStateAsBool()
        {
            if (PowerState == LifxPowerState.On)
                return true;
            else
                return false;
        }
    }
}
