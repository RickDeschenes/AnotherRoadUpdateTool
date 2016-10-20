using ICities;
using System;
using System.Reflection;

namespace AnotherRoadUpdateTool.Helpers
{
    public class MaxAreas : AreasExtensionBase
    {
        public MaxAreas()
        {
        }

        public override void OnCreated(IAreas areas)
        {
            base.OnCreated(areas);
            if (ARUT.AdjustAreas == true)
            {
                ARUT.WriteLog("OnCreate MaxAreas: Spaces: " + ARUT.MaxAreas);
                areas.maxAreaCount = ARUT.MaxAreas;
            }
        }

        public int SetMaxAreas(int areas)
        {
            IAreas iareas = base.areaManager;
            if (ARUT.AdjustAreas == true)
            {
                iareas.maxAreaCount = areas;
            }
            else
                areas = iareas.maxAreaCount;
            return areas;
        }

    }
}
