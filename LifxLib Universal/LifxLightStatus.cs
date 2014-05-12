using System;

namespace LifxLib
{
    public class LifxLightStatus
    {
        public LifxColor Colour { get; set; }
        public LifxPowerState PowerState { get; set; }
        public UInt16 DimState { get; set; }
        public String Label { get; set; }
        public UInt64 Tags { get; set; }

        public LifxLightStatus()
        {
            Colour = new LifxColor();
            PowerState = LifxPowerState.Unknown;
            DimState = 0;
            Label = "";
            Tags = 0;
        }
    }
}




