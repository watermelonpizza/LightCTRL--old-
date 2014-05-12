using System;

namespace LifxLib.Messages
{
    public class LifxGetTagLabelCommand : LifxCommand
    {
        private const CommandPacketType PACKET_TYPE = CommandPacketType.GetTagLabels;
        private UInt64 mTags = 0;

        public LifxGetTagLabelCommand(UInt64 tags)
            : base(PACKET_TYPE)
        {
            mTags = tags;
        }

        #region ILifxMessage Members

        public override byte[] GetRawMessage()
        {
            return BitConverter.GetBytes(mTags);
        }

        #endregion

    }
}
