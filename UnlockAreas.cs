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
            RoadUpdateTool.WriteLog("OnCreate MaxAreas: Spaces: " + Properties.Settings.Default.MaxAreas);
            base.OnCreated(areas);
            areas.maxAreaCount = Properties.Settings.Default.MaxAreas;
        }

        public int SetMaxAreas(int areas)
        {
            IAreas iareas = base.areaManager;
            iareas.maxAreaCount = areas;
            return areas;
        }

    }
}
