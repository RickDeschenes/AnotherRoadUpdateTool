using ICities;
using System;
using System.Reflection;

namespace AnotherRoadUpdateTool
{
    public class MaxAreas : AreasExtensionBase
    {
        public MaxAreas()
        {
        }

        public override void OnCreated(IAreas areas)
        {
            base.OnCreated(areas);
            if (RoadUpdateTool.AdjustAreas == true)
            {
                RoadUpdateTool.WriteLog("OnCreate MaxAreas: Spaces: " + RoadUpdateTool.MaxAreas);
                areas.maxAreaCount = RoadUpdateTool.MaxAreas;
            }
        }

        public int SetMaxAreas(int areas)
        {
            IAreas iareas = base.areaManager;
            if (RoadUpdateTool.AdjustAreas == true)
            {
                iareas.maxAreaCount = areas;
            }
            else
                areas = iareas.maxAreaCount;
            return areas;
        }

    }
}
