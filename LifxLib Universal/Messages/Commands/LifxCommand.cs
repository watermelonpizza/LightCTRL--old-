using System;

namespace LifxLib.Messages
{
    /// <summary>
    /// Message sent to/from bulb, this is abstract class, inherited by the message implmentations
    /// </summary>
    public abstract class LifxCommand
    {
        private CommandPacketType mPacketType = CommandPacketType.Unknown;
        private Int32 mTimeoutMs = 0;
        private DateTime mTimestamp = DateTime.Now;
        private Boolean mIsBroadcastCommand = false;
        private UInt16 mRetryCount = 3;
        private Boolean mIsDiscoveryCommand = false;

        public LifxCommand(CommandPacketType packetType)
        {
            mPacketType = packetType;
        }

        public virtual byte[] GetRawMessage()
        { 
            return new byte[0];
        }

        public CommandPacketType PacketType
        {
            get {  return mPacketType; }
        }

        public Int32 Timeout
        {
            get{ return mTimeoutMs;}
            set{ mTimeoutMs = value;}
        }

        public DateTime TimeStamp
        {
            get { return mTimestamp; }
            set { mTimestamp = value; }
        }

        public Boolean IsBroadcastCommand
        {
            get { return mIsBroadcastCommand; }
            set { mIsBroadcastCommand = value; }
        }

        public UInt16 RetryCount
        {
            get { return mRetryCount; }
            set { mRetryCount = value; }
        }

        public Boolean IsDiscoveryCommand
        {
            get { return mIsDiscoveryCommand; }
            set { mIsDiscoveryCommand = value; }
        }

    }
}
