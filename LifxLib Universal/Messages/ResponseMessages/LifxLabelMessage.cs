using System;
using System.Text;

namespace LifxLib.Messages
{
    public class LifxLabelMessage : LifxMessage
    {
        private const MessagePacketType PACKET_TYPE = MessagePacketType.Label;

        public LifxLabelMessage(LifxDataPacket packet)
            : base(packet, PACKET_TYPE)
        {

        }

        public String BulbLabel
        {
            get 
            {
                return Encoding.UTF8.GetString(base.ReceivedData.Payload, 0, base.ReceivedData.Payload.Length);
            }
        }
    }
}
