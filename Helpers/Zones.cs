using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnotherRoadUpdateTool.Helpers
{
    class Zones : ZoneTool
    {
        protected override void OnEnable()
        {
            ARUT.WriteLog("Loading Zonetool");
            m_mode = Mode.Select;
            base.OnEnable();
        }
    }
}
