using ICities;
using System.Reflection;

namespace AnotherRoadUpdateTool.Helpers
{ 
    public class UnlockRoads : MilestonesExtensionBase
    {
        public UnlockRoads()
        {
            if (ARUT.AllRoads == true)
                unlockRoads();
        }

        public override void OnRefreshMilestones()
        {
            base.milestonesManager.UnlockMilestone("Basic Road Created");
            this.unlockRoads();
        }

        private void unlockRoads()
        {
            for (int i = 0; i < PrefabCollection<NetInfo>.LoadedCount(); i++)
            {
                NetInfo loaded = PrefabCollection<NetInfo>.GetLoaded((uint)i);
                if (loaded != null && loaded.m_class != null && loaded.m_class.name != null && this.isRoad(loaded.m_class))
                {
                    loaded.m_UnlockMilestone = null;
                }
            }
            for (int j = 0; j < PrefabCollection<BuildingInfo>.LoadedCount(); j++)
            {
                BuildingInfo buildingInfo = PrefabCollection<BuildingInfo>.GetLoaded((uint)j);
                if (buildingInfo != null && buildingInfo.m_class != null && buildingInfo.m_class.name != null && this.isRoad(buildingInfo.m_class))
                {
                    buildingInfo.m_UnlockMilestone = null;
                    IntersectionAI mBuildingAI = buildingInfo.m_buildingAI as IntersectionAI;
                    if (mBuildingAI != null)
                    {
                        UnlockRoads.setPrivateVariable<MilestoneInfo>(mBuildingAI, "m_cachedUnlockMilestone", null);
                    }
                }
            }
        }

        private bool isRoad(ItemClass itemClass)
        {
            string str = itemClass.name;
            return (str.Contains("Road") ? true : str.Contains("Highway"));
        }

        private static void setPrivateVariable<T>(object obj, string fieldName, T value)
        {
            obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(obj, value);
        }
    }
}
