using System;

namespace LifxLib.Messages
{
    public class LifxPanGatewayStateMessage : LifxMessage
    {
        public enum LifxPanServiceType
        {
            UDP = (byte)0x01,
            TCP = (byte)0x02
        }

        public const MessagePacketType PACKET_TYPE = MessagePacketType.PanGateway;

        public LifxPanGatewayStateMessage(LifxDataPacket packet)
            : base(packet, PACKET_TYPE)
        {

        }


        public LifxPanServiceType ServiceType
        {
            get
            {
                return (LifxPanServiceType)base.ReceivedData.Payload[0];
            }
        }


        public UInt32 Port
        {
            get
            {
                return BitConverter.ToUInt32(ReceivedData.Payload, 1);
            }
            set
            {
                byte[] bytes = BitConverter.GetBytes(value);

                Array.Copy(bytes, 0, ReceivedData.Payload, 1, bytes.Length);
            }
        }
    }
}
