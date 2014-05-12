using System;

namespace LifxLib.Messages
{
    public class LifxGetTagsCommand : LifxCommand
    {
        private const CommandPacketType PACKET_TYPE = CommandPacketType.GetTags;

        public LifxGetTagsCommand()
            : base(PACKET_TYPE)
        {
            
        }
    }
}
