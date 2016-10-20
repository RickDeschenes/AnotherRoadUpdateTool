using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace AnotherRoadUpdateTool.Helpers
{
    internal class DeleteObjects
    {
        private enum tp
        {
            Ground = 0,
            Bridge = 1,
            Slope = 2,
            Tunnel = 3,
            Curve = 4
        }

        private enum p
        {
            Roads = 0,
            Railroads = 1,
            Highways = 2,
            PowerLines = 3,
            WaterPipes = 4,
            HeatPipes = 5,
            Airplanes = 6,
            Shipping = 7,
            Pedestrian = 8,
            Bicycle = 9,
            Tram = 10,
            Metro = 11,
            Buildings = 12,
            Trees = 13,
            Props = 14
        }
        
        internal Vector3 m_startPosition;
        internal Vector3 m_mousePosition;
        internal List<UICheckBox> deletes = new List<UICheckBox>();
        internal List<UICheckBox> types = new List<UICheckBox>();

        private List<ushort> segmentsToDelete;

        public DeleteObjects(Vector3 _startPosition,
                        Vector3 _mousePosition,
                        List<UICheckBox> _deletes,
                        List<UICheckBox> _types)
        {
            m_startPosition = _startPosition;
            m_mousePosition = _mousePosition;
            deletes = _deletes;
            types = _types;
        }

        internal void ApplyDeletes()
        {
            string errors = "Deletes";
            try
            {
                if (deletes[(int)p.Roads].isChecked || deletes[(int)p.Railroads].isChecked || deletes[(int)p.Highways].isChecked || deletes[(int)p.PowerLines].isChecked || deletes[(int)p.WaterPipes].isChecked || deletes[(int)p.HeatPipes].isChecked || deletes[(int)p.Airplanes].isChecked || deletes[(int)p.Shipping].isChecked)
                    BulldozeLanes();
                errors = "Buildings";
                if (deletes[(int)p.Buildings].isChecked)
                    BulldozeBuildings();
                errors = "Props";
                if (deletes[(int)p.Props].isChecked)
                    BulldozeProps();
                errors = "Trees";
                if (deletes[(int)p.Trees].isChecked)
                    BulldozeTrees();
            }
            catch (Exception ex)
            {
                ARUT.WriteError("Error in ApplyDeletes at: " + errors, ex);
            }
        }

        private void BulldozeLanes()
        {
            segmentsToDelete = new List<ushort>();

            var minX = this.m_startPosition.x < this.m_mousePosition.x ? this.m_startPosition.x : this.m_mousePosition.x;
            var minZ = this.m_startPosition.z < this.m_mousePosition.z ? this.m_startPosition.z : this.m_mousePosition.z;
            var maxX = this.m_startPosition.x > this.m_mousePosition.x ? this.m_startPosition.x : this.m_mousePosition.x;
            var maxZ = this.m_startPosition.z > this.m_mousePosition.z ? this.m_startPosition.z : this.m_mousePosition.z;

            int gridMinX = Mathf.Max((int)((minX - 16f) / 64f + 135f), 0);
            int gridMinZ = Mathf.Max((int)((minZ - 16f) / 64f + 135f), 0);
            int gridMaxX = Mathf.Min((int)((maxX + 16f) / 64f + 135f), 269);
            int gridMaxZ = Mathf.Min((int)((maxZ + 16f) / 64f + 135f), 269);

            //string xy = "Values gridMinX, gridMinZ, gridMaxX, gridMaxY: " + gridMinX + ", " + gridMinZ + ", " + gridMaxX + ", " + gridMaxZ;
            //WriteLog("About to loop: " + xy);

            for (int i = gridMinZ; i <= gridMaxZ; i++)
            {
                for (int j = gridMinX; j <= gridMaxX; j++)
                {
                    try
                    {
                        //WriteLog("In the for loops ");
                        ushort num5 = NetManager.instance.m_segmentGrid[i * 270 + j];
                        int num6 = 0;

                        while (num5 != 0u)
                        {
                            //WriteLog("In the while ");
                            var segment = NetManager.instance.m_segments.m_buffer[(int)((UIntPtr)num5)];
                            bool curved = (segment.m_cornerAngleEnd != segment.m_cornerAngleStart);

                            Vector3 position = segment.m_middlePosition;
                            float positionDiff = Mathf.Max(Mathf.Max(minX - 16f - position.x, minZ - 16f - position.z), Mathf.Max(position.x - maxX - 16f, position.z - maxZ - 16f));

                            if (positionDiff < 0f)
                            {
                                string seg = segment.Info.name;
                                //Add Validated segments
                                if (ValidateSegment(seg, curved) == false)
                                    segmentsToDelete.Add(num5);
                            }
                            num5 = NetManager.instance.m_segments.m_buffer[(int)((UIntPtr)num5)].m_nextGridSegment;

                            if (++num6 >= 262144)
                            {
                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ARUT.WriteError("Error in DeleteLanes segmentsToDelete (segment loop) ", ex);
                    }
                }   //for (int j = gridMinX; j <= gridMaxX; j++)
            }   //for (int i = gridMinZ; i <= gridMaxZ; i++)
            foreach (var segment in segmentsToDelete)
            {
                try
                {
                    SimulationManager.instance.AddAction(this.ReleaseSegment(segment));
                }
                catch (Exception ex)
                {
                    ARUT.WriteError("Error in DeleteLanes segmentsToDelete (Delete loop).", ex); ;
                }
            }
            NetManager.instance.m_nodesUpdated = true;

            ARUT.segcount = segmentsToDelete.Count;
        }

        private bool ValidateSegment(string seg, bool curved)
        {
            bool skip = false;
            // we need to handle Ground, Bridge, Elevated, Slope, Tunnel, railroads, Pipe, Power Lines, 
            if (types[(int)tp.Ground].isChecked == false && (seg.Contains("Bridge ") == false || seg.Contains("Slope ") == false || seg.Contains("Tunnel ") == false)) { skip = true; }
            else if (types[(int)tp.Tunnel].isChecked == false && seg.Contains("Tunnel")) { skip = true; }
            else if (types[(int)tp.Bridge].isChecked == false && seg.Contains("Elevated")) { skip = true; }
            else if (types[(int)tp.Slope].isChecked == false && seg.Contains("Slope")) { skip = true; }
            else if ((types[(int)tp.Curve].isChecked == false && curved)) { skip = true; }
            else if (deletes[(int)p.Shipping].isChecked == false && seg.Contains("Pedestrian ")) { skip = true; }
            else if (deletes[(int)p.Shipping].isChecked == false && seg.Contains("Bicycle ")) { skip = true; }
            else if (deletes[(int)p.Shipping].isChecked == false && seg.Contains("Tram ")) { skip = true; }
            else if (deletes[(int)p.Shipping].isChecked == false && seg.Contains("Metro ")) { skip = true; }
            else if (deletes[(int)p.Roads].isChecked == false && seg.Contains("Road")) { skip = true; }
            else if (deletes[(int)p.Railroads].isChecked == false && seg.Contains("Train")) { skip = true; }
            else if (deletes[(int)p.Highways].isChecked == false && seg.Contains("Highway")) { skip = true; }
            else if (deletes[(int)p.PowerLines].isChecked == false && seg.Contains("Power")) { skip = true; }
            else if (deletes[(int)p.WaterPipes].isChecked == false && seg.Contains("Water Pipe")) { skip = true; }
            else if (deletes[(int)p.HeatPipes].isChecked == false && seg.Contains("Heating Pipe")) { skip = true; }
            else if (deletes[(int)p.Airplanes].isChecked == false && seg.Contains("Airplane")) { skip = true; }
            else if (deletes[(int)p.Shipping].isChecked == false && seg.Contains("Ship")) { skip = true; }
            return skip;
        }

        private void BulldozeBuildings()
        {
            List<ushort> buildingsToDelete = new List<ushort>();

            var minX = this.m_startPosition.x < this.m_mousePosition.x ? this.m_startPosition.x : this.m_mousePosition.x;
            var minZ = this.m_startPosition.z < this.m_mousePosition.z ? this.m_startPosition.z : this.m_mousePosition.z;
            var maxX = this.m_startPosition.x > this.m_mousePosition.x ? this.m_startPosition.x : this.m_mousePosition.x;
            var maxZ = this.m_startPosition.z > this.m_mousePosition.z ? this.m_startPosition.z : this.m_mousePosition.z;

            int gridMinX = Mathf.Max((int)((minX - 16f) / 64f + 135f), 0);
            int gridMinZ = Mathf.Max((int)((minZ - 16f) / 64f + 135f), 0);
            int gridMaxX = Mathf.Min((int)((maxX + 16f) / 64f + 135f), 269);
            int gridMaxZ = Mathf.Min((int)((maxZ + 16f) / 64f + 135f), 269);

            for (int i = gridMinZ; i <= gridMaxZ; i++)
            {
                for (int j = gridMinX; j <= gridMaxX; j++)
                {
                    ushort num5 = BuildingManager.instance.m_buildingGrid[i * 270 + j];
                    int num6 = 0;
                    while (num5 != 0u)
                    {
                        var building = BuildingManager.instance.m_buildings.m_buffer[(int)((UIntPtr)num5)];

                        Vector3 position = building.m_position;
                        float positionDiff = Mathf.Max(Mathf.Max(minX - 16f - position.x, minZ - 16f - position.z), Mathf.Max(position.x - maxX - 16f, position.z - maxZ - 16f));
                        if (positionDiff < 0f && building.m_parentBuilding <= 0)
                        {
                            buildingsToDelete.Add(num5);
                        }
                        num5 = BuildingManager.instance.m_buildings.m_buffer[(int)((UIntPtr)num5)].m_nextGridBuilding;
                        if (++num6 >= 262144)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }

            foreach (ushort building in buildingsToDelete)
            {
                SimulationManager.instance.AddAction(this.ReleaseBuilding(building));
            }
        }

        private void BulldozeTrees()
        {
            List<uint> treesToDelete = new List<uint>();
            var minX = this.m_startPosition.x < this.m_mousePosition.x ? this.m_startPosition.x : this.m_mousePosition.x;
            var minZ = this.m_startPosition.z < this.m_mousePosition.z ? this.m_startPosition.z : this.m_mousePosition.z;
            var maxX = this.m_startPosition.x > this.m_mousePosition.x ? this.m_startPosition.x : this.m_mousePosition.x;
            var maxZ = this.m_startPosition.z > this.m_mousePosition.z ? this.m_startPosition.z : this.m_mousePosition.z;

            int num = Mathf.Max((int)((minX - 8f) / 32f + 270f), 0);
            int num2 = Mathf.Max((int)((minZ - 8f) / 32f + 270f), 0);
            int num3 = Mathf.Min((int)((maxX + 8f) / 32f + 270f), 539);
            int num4 = Mathf.Min((int)((maxZ + 8f) / 32f + 270f), 539);
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    uint num5 = TreeManager.instance.m_treeGrid[i * 540 + j];
                    int num6 = 0;
                    while (num5 != 0u)
                    {
                        var tree = TreeManager.instance.m_trees.m_buffer[(int)((UIntPtr)num5)];
                        Vector3 position = tree.Position;
                        float num7 = Mathf.Max(Mathf.Max(minX - 8f - position.x, minZ - 8f - position.z), Mathf.Max(position.x - maxX - 8f, position.z - maxZ - 8f));
                        if (num7 < 0f)
                        {

                            treesToDelete.Add(num5);
                        }
                        num5 = TreeManager.instance.m_trees.m_buffer[(int)((UIntPtr)num5)].m_nextGridTree;
                        if (++num6 >= 262144)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            foreach (uint tree in treesToDelete)
            {
                TreeManager.instance.ReleaseTree(tree);
            }
            TreeManager.instance.m_treesUpdated = true;
        }

        private void BulldozeProps()
        {
            List<ushort> propsToDelete = new List<ushort>();
            var minX = this.m_startPosition.x < this.m_mousePosition.x ? this.m_startPosition.x : this.m_mousePosition.x;
            var minZ = this.m_startPosition.z < this.m_mousePosition.z ? this.m_startPosition.z : this.m_mousePosition.z;
            var maxX = this.m_startPosition.x > this.m_mousePosition.x ? this.m_startPosition.x : this.m_mousePosition.x;
            var maxZ = this.m_startPosition.z > this.m_mousePosition.z ? this.m_startPosition.z : this.m_mousePosition.z;

            int num = Mathf.Max((int)((minX - 16f) / 64f + 135f), 0);
            int num2 = Mathf.Max((int)((minZ - 16f) / 64f + 135f), 0);
            int num3 = Mathf.Min((int)((maxX + 16f) / 64f + 135f), 269);
            int num4 = Mathf.Min((int)((maxZ + 16f) / 64f + 135f), 269);
            for (int i = num2; i <= num4; i++)
            {
                for (int j = num; j <= num3; j++)
                {
                    ushort num5 = PropManager.instance.m_propGrid[i * 270 + j];
                    int num6 = 0;
                    while (num5 != 0u)
                    {
                        var prop = PropManager.instance.m_props.m_buffer[(int)((UIntPtr)num5)];
                        Vector3 position = prop.Position;
                        float num7 = Mathf.Max(Mathf.Max(minX - 16f - position.x, minZ - 16f - position.z), Mathf.Max(position.x - maxX - 16f, position.z - maxZ - 16f));

                        if (num7 < 0f)
                        {
                            propsToDelete.Add(num5);
                        }
                        num5 = PropManager.instance.m_props.m_buffer[(int)((UIntPtr)num5)].m_nextGridProp;
                        if (++num6 >= 262144)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            foreach (ushort prop in propsToDelete)
            {
                PropManager.instance.ReleaseProp(prop);
            }
            PropManager.instance.m_propsUpdated = true;
        }

        private IEnumerator ReleaseSegment(ushort segment)
        {
            string segname = NetManager.instance.m_segments.m_buffer[segment].Info.name;
            try
            {
                NetManager.instance.ReleaseSegment(segment, false);
            }
            catch (Exception ex)
            {
                ARUT.WriteError("ReleaseSegment, segment: " + segname, ex);
            }
            yield return null;
        }

        private IEnumerator ReleaseBuilding(ushort building)
        {
            BuildingManager.instance.ReleaseBuilding(building);
            yield return null;
        }
    }
    
}
