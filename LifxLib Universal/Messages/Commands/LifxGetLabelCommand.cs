using System;

namespace LifxLib.Messages
{
    public class LifxGetLabelCommand : LifxCommand
    {
        private const UInt16 PACKET_TYPE = 0x17;

        public LifxGetLabelCommand()
            : base(PACKET_TYPE, new LifxLabelMessage())
        {
            
        }
    }
}
