using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AnotherRoadUpdateTool.Helpers
{
    class ToggleServices
    {
        private Vector3 m_mousePosition;
        private Vector3 m_startPosition;
        private PanelMain plMain;

        public ToggleServices()
        { }

        public ToggleServices(PanelMain plMain, Vector3 m_startPosition, Vector3 m_mousePosition)
        {
            this.plMain = plMain;
            this.m_startPosition = m_startPosition;
            this.m_mousePosition = m_mousePosition;
        }

        internal void ApplyServices()
        {
            ARUT.WriteLog("Entering ApplyServices.");
            Building[] buffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

            bool toggle = plMain.cbToggle.isChecked;
            byte rate = 0;

            for (int i = 0; i < buffer.Length; i++)
            {
                Building bd = buffer[i];

                if (plMain.Services(PanelMain.sr.Beautification) == false && bd.Info.m_class.m_service == ItemClass.Service.Beautification) { }
                else if (plMain.Services(PanelMain.sr.Education) == false && bd.Info.m_class.m_service == ItemClass.Service.Education) { }
                else if (plMain.Services(PanelMain.sr.Electricity) == false && bd.Info.m_class.m_service == ItemClass.Service.Electricity) { }
                else if (plMain.Services(PanelMain.sr.FireDepartment) == false && bd.Info.m_class.m_service == ItemClass.Service.FireDepartment) { }
                else if (plMain.Services(PanelMain.sr.Garbage) == false && bd.Info.m_class.m_service == ItemClass.Service.Garbage) { }
                else if (plMain.Services(PanelMain.sr.HealthCare) == false && bd.Info.m_class.m_service == ItemClass.Service.HealthCare) { }
                else if (plMain.Services(PanelMain.sr.Monument) == false && bd.Info.m_class.m_service == ItemClass.Service.Monument) { }
                else if (plMain.Services(PanelMain.sr.PoliceDepartment) == false && bd.Info.m_class.m_service == ItemClass.Service.PoliceDepartment) { }
                else if (plMain.Services(PanelMain.sr.PublicTransport) == false && bd.Info.m_class.m_service == ItemClass.Service.PublicTransport) { }
                else if (plMain.Services(PanelMain.sr.Water) == false && bd.Info.m_class.m_service == ItemClass.Service.Water) { }
                else if (ValidateSelectedArea(bd, m_startPosition, m_mousePosition) == false)
                {
                    //WriteLog(String.Format("Segment {0} is not in the selected area.", bd.Info.name));
                }
                else
                {
                    rate = 0;
                    if (toggle == true) { rate = 100; }

                    if (bd.m_flags.IsFlagSet(Building.Flags.Active) != toggle)
                    {
                        bd.m_flags ^= Building.Flags.Active;
                        bd.m_productionRate = rate;
                    }
                }
                buffer[i] = bd;
            }
            ARUT.WriteLog("Leaving ApplyServices");
        }
        private bool ValidateSelectedArea(Building bd, Vector3 start, Vector3 end)
        {
            var minX = start.x < end.x ? start.x : end.x;
            var minZ = start.z < end.z ? start.z : end.z;
            var maxX = start.x > end.x ? start.x : end.x;
            var maxZ = start.z > end.z ? start.z : end.z;

            Vector3 position = bd.m_position;

            float positionDiff = Mathf.Max(Mathf.Max(minX - 16f - position.x, minZ - 16f - position.z), Mathf.Max(position.x - maxX - 16f, position.z - maxZ - 16f));

            if (positionDiff < 0f)
            {
                return true;
            }
            return false;
        }

    }
}
