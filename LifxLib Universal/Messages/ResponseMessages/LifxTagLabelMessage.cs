using System;
using System.Text;

namespace LifxLib.Messages
{
    public class LifxTagLabelMessage : LifxMessage
    {
        private const MessagePacketType PACKET_TYPE = MessagePacketType.TagLabel;

        public LifxTagLabelMessage(LifxDataPacket packet)
            : base(packet, PACKET_TYPE)
        {

        }

        public UInt64 Tag
        {
            get
            {
                return BitConverter.ToUInt64(base.ReceivedData.Payload, 0);
            }
        }

        public String TagLabel
        {
            get 
            {
                return Encoding.UTF8.GetString(base.ReceivedData.Payload, 8, 32);
            }
        }
    }
}
