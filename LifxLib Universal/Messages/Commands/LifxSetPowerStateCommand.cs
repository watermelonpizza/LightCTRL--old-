using System;

namespace LifxLib.Messages
{
    public class LifxSetPowerStateCommand : LifxCommand
    {
        private LifxPowerState mStateToSet = LifxPowerState.Unknown;
        private const CommandPacketType PACKET_TYPE = CommandPacketType.SetPowerState;

        public LifxSetPowerStateCommand(LifxPowerState stateToSet)
            : base(PACKET_TYPE)
        {
            mStateToSet = stateToSet;
        }

        #region ILifxMessage Members

        public override byte[] GetRawMessage()
        {
            if (mStateToSet == LifxPowerState.Unknown)
            {
                throw new ArgumentException("Invalid Powerstate: " + mStateToSet.ToString());
            }
            byte[] bytes = BitConverter.GetBytes((UInt16)mStateToSet);
            return bytes;
        }

        #endregion
    }
}
