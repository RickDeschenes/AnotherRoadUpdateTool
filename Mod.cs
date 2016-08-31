using ICities;
using UnityEngine;
using static UnityEngine.Object;

/// <summary>
/// Updated from SkylinesRoadUpdate added global preferences
/// added Additional selections
/// </summary>
namespace AnotherRoadUpdateTool
{
    public class Mod : IUserMod
    {
        public string Description
        {
            get { return "Another Road Update tool"; }
        }

        public string Name
        {
            get { return "Another Road Update Tool"; }
        }
    }

    public class LoadingExtension : LoadingExtensionBase
    {
        public RoadUpdateTool updateTool;

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            updateTool = FindObjectOfType<RoadUpdateTool>();
            if(updateTool == null)
            {
                GameObject gameController = GameObject.FindWithTag("GameController");
                updateTool = gameController.AddComponent<RoadUpdateTool>();
            }
            updateTool.InitGui(mode);
        }
    }

}
