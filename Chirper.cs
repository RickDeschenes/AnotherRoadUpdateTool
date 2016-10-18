using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnotherRoadUpdateTool
{
    public class Chirper : ChirperExtensionBase
    {
        private static IChirper thisChirper;

        private static bool toggleState = true;

        public bool ToggleState { get { return toggleState; } }

        public override void OnCreated(IChirper chirper)
        {
            if (thisChirper == null)
                thisChirper = chirper;
        }

        public bool Toggle()
        {
            if (toggleState == true)
            {
                //Toggle Chirper Off
                thisChirper.ShowBuiltinChirper(false);
                toggleState = false;
                return false;
            }

            //thisChirper.DestroyBuiltinChirper();
            else
            {
                //Toggle Chirper On
                thisChirper.ShowBuiltinChirper(true);
                toggleState = true;
                return true;
            }
        }

        public bool Toggle(bool onOff)
        {
            if (onOff == true)
            {
                //Toggle Chirper Off
                thisChirper.ShowBuiltinChirper(true);
                toggleState = true;
                return true;
            }

            //thisChirper.DestroyBuiltinChirper();
            else
            {
                //Toggle Chirper On
                thisChirper.ShowBuiltinChirper(false);
                toggleState = false;
                return false;
            }
        }
        
        public override void OnUpdate()
        {
        }

    }
}
