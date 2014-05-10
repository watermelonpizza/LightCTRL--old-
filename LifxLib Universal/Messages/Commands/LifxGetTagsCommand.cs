﻿using System;

namespace LifxLib.Messages
{
    public class LifxGetTagsCommand : LifxCommand
    {
        private const UInt16 PACKET_TYPE = 0x1A;

        public LifxGetTagsCommand()
            : base(PACKET_TYPE, new LifxTagsMessage())
        {
            
        }
    }
}
