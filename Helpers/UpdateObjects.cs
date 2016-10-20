using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using static ToolBase;

namespace AnotherRoadUpdateTool.Helpers
{
    class UpdateObjects
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
        private UILabel lInformation;
        private string fromSelected;
        private string toSelected;

        public UpdateObjects(UILabel lInformation, Vector3 m_startPosition, Vector3 m_mousePosition, List<UICheckBox> deletes, List<UICheckBox> types, string fromSelected, string toSelected)
        {
            this.lInformation = lInformation;
            this.m_startPosition = m_startPosition;
            this.m_mousePosition = m_mousePosition;
            this.deletes = deletes;
            this.types = types;
            this.fromSelected = fromSelected;
            this.toSelected = toSelected;
        }

        public void ApplyUpdates()
        {
            //test our absolutes selected area for tiny selections and ignore them
            string xy = Math.Abs(m_startPosition.x - m_mousePosition.x) + " : " + Math.Abs(m_startPosition.y - m_mousePosition.y);
            if (Math.Abs(m_startPosition.x - m_mousePosition.x) < 20)
            {
                if (Math.Abs(m_startPosition.y - m_mousePosition.y) < 20)
                {
                    //ARUT.WriteLog("Math.Abs(m_startPosition.x - m_mousePosition.x) + '' : '' + Math.Abs(m_startPosition.y - m_mousePosition.y) :" + xy);
                    //ARUT.WriteLog("(Math.Abs(m_startPosition.y - m_mousePosition.y) < 20 :" + (Math.Abs(m_startPosition.y - m_mousePosition.y) < 20));
                    return;
                }
            }

            //WriteLog("fromSelect & toSelect :" + fromSelected + " & " + toSelected);
            if (fromSelected == string.Empty)
                return;
            if (toSelected == string.Empty)
                return;
            try
            {
                int totalCost = 0;
                ToolBase.ToolErrors errors;
                ConvertSegments(toSelected, fromSelected, false, out totalCost, out errors);
            }
            catch (Exception ex)
            {
                ARUT.WriteError("Error in ApplyUpdates ", ex);
            }
        }

        private int ConvertSegments(string convertTo, string convertFrom, bool test, out int totalCost, out ToolErrors errors)
        {
            int num = 0;
            totalCost = 0;
            int tempCost = 0;
            int issues = 0;
            errors = 0;

            StringWriter sw = new StringWriter();
            //sw.WriteLine(String.Format("Entering ConvertObjects at {0}.", DateTime.Now.TimeOfDay));

            NetInfo info = PrefabCollection<NetInfo>.FindLoaded(convertTo);

            if (info == null)
            {
                //sw.WriteLine("Could not find the object: " + convertTo + ", aborting.");
                return num;
            }

            NetSegment[] buffer = Singleton<NetManager>.instance.m_segments.m_buffer;

            //sw.WriteLine("Filling Singleton<NetManager>.instance.m_segments.m_buffer. Found: " + buffer.Length);

            for (int i = 0; i < buffer.Length - 1; i++)
            {
                NetSegment segment = buffer[i];

                //Validate in selected area
                if (segment.Info == null)
                {
                    //sw.WriteLine(String.Format("Segment {0} is Null.", segment.Info.name));
                }
                else if (!segment.Info.name.Contains(convertFrom))
                {
                    //sw.WriteLine(String.Format("Segment {0} is not a converting item.", segment.Info.name));
                }
                else if (ValidateSelectedArea(segment) == false)
                {
                    //sw.WriteLine(String.Format("Segment {0} is not in the selected area.", segment.Info.name));
                }
                else
                {
                    bool skip = false;
                    //Are we not equal
                    bool Curved = AngleBetween(segment.m_startDirection, segment.m_endDirection, 1);

                    string seg = segment.Info.name;

                    NetTool.ControlPoint point;
                    NetTool.ControlPoint point2;
                    NetTool.ControlPoint point3;

                    bool Bridge = seg.Contains("Bridge ") == true;
                    bool Slope = seg.Contains("Slope ") == true;
                    bool Tunnel = seg.Contains("Tunnel ") == true;
                    bool Ground = types[(int)tp.Ground].isChecked;
                    //sw.WriteLine(String.Format("Ground: {0}; Bridge: {1}; Slope: {2}; Tunnel: {3}; Curved: {4}", Ground, Bridge, Slope, Tunnel, Curved));

                    // we need to handle Ground, Bridge, Elevated, Slope, Tunnel, railroads, Pipe, Power Lines, 
                    if (Ground == false && (Bridge || Slope || Tunnel || Curved)) { skip = true; }
                    else if (types[(int)tp.Tunnel].isChecked == false && seg.Contains("Tunnel")) { skip = true; }
                    else if (types[(int)tp.Bridge].isChecked == false && seg.Contains("Elevated")) { skip = true; }
                    else if (types[(int)tp.Slope].isChecked == false && seg.Contains("Slope")) { skip = true; }
                    else if (types[(int)tp.Curve].isChecked == false && Curved) { skip = true; }

                    //sw.WriteLine(segment.Info.name + " converting to " + convertTo + ".");
                    //sw.WriteLine("About to call GetSegmentControlPoints.\n");

                    GetSegmentControlPoints(i, out point, out point2, out point3);
                    bool visualize = false;
                    bool autoFix = true;
                    bool needMoney = true;
                    bool invert = false;
                    ushort num3 = 0;
                    ushort num4 = 0;
                    int num5 = 0;
                    int num6 = 0;

                    //test for bad index
                    if ((point.m_position == new Vector3()) && (point2.m_position == new Vector3()) && (point3.m_position == new Vector3())) { }
                    else if (skip == true) { }
                    else
                    {
                        try
                        {
                            //sw.WriteLine("About to call NetTool.Create test mode.\n");
                            //Validate in area and other errors (no money!)
                            errors = NetTool.CreateNode(info, point, point2, point3, NetTool.m_nodePositionsSimulation, 0x3e8, true, visualize, autoFix, needMoney, invert, false, 0, out num3, out num4, out num5, out num6);
                            //sw.WriteLine("Test Cost: " + num5);
                            tempCost = num5;
                        }
                        catch (Exception ex)
                        {
                            ARUT.WriteError("Error testing convert of: " + segment.Info.name + " to " + convertTo + ".", ex);
                        }
                        if (errors == 0)
                        {
                            try
                            {
                                errors = NetTool.CreateNode(info, point, point2, point3, NetTool.m_nodePositionsMain, 0x3e8, false, visualize, autoFix, needMoney, invert, false, 0, out num3, out num4, out num5, out num6);
                                num++;
                                totalCost += tempCost;
                            }
                            catch (Exception ex)
                            {
                                string lenght = "Left Segment Lenght: " + Math.Abs((float)(segment.m_startLeftSegment - segment.m_endLeftSegment)).ToString();
                                lenght += " Right Segment Lenght: " + Math.Abs((float)(segment.m_startRightSegment - segment.m_endRightSegment)).ToString();
                                string message = "Error converting: " + segment.Info.name + " to " + convertTo + "; errors: " + errors + "; " + lenght;
                                //sw.WriteLine(lenght);
                                ARUT.WriteError(message, ex);
                                //sw.WriteLine(message);
                                issues += 1;
                                try
                                {
                                    //lets retry this once
                                    errors = NetTool.CreateNode(info, point, point2, point3, NetTool.m_nodePositionsMain, 0x3e8, false, visualize, autoFix, needMoney, invert, false, 0, out num3, out num4, out num5, out num6);
                                    num++;
                                    totalCost += tempCost;

                                }
                                catch
                                { //do nothing }
                                }
                            }
                        }
                        else
                        {
                            //sw.WriteLine("Error test convert: " + segment.Info.name + " to " + convertTo + ". Message22: " + errors);
                            issues += 1;
                        }
                    }
                }
            }
            lInformation.text = "Items converted: " + num + " Total Cost: " + totalCost + " Recorded issues: " + issues;
            //WriteLog("" + sw);
            return num;
        }

        private bool AngleBetween(Vector3 deg1, Vector3 deg2, int compare)
        {
            bool result = false;

            Vector2 v1 = new Vector2(deg1.x, deg1.z);
            Vector2 v2 = new Vector2(deg2.x, deg2.z);

            // WriteLog("v1 & v2 are: " + v1 + " & " + v2);

            float a1 = Vector2.Angle(new Vector2(), v1);
            float a2 = Vector2.Angle(new Vector2(), v2);

            //WriteLog("a1 & a2 are: " + a1 + " & " + a2);

            float angle = Vector2.Angle(v1, v2);

            //WriteLog("The angle between v1 & v2 is: " + angle);

            ////the angles are based from a stright line so the 45 dergrees must be accounted for
            //angle -= 45;
            if (angle >= 270) { angle -= 270; }
            if (angle >= 180) { angle -= 180; }
            if (angle >= 90) { angle -= 90; }

            //WriteLog("The angle is: " + angle);
            result = (angle > compare);

            return result;
        }

        protected bool ValidateSelectedArea(NetSegment segment)
        {
            var minX = this.m_startPosition.x < this.m_mousePosition.x ? this.m_startPosition.x : this.m_mousePosition.x;
            var minZ = this.m_startPosition.z < this.m_mousePosition.z ? this.m_startPosition.z : this.m_mousePosition.z;
            var maxX = this.m_startPosition.x > this.m_mousePosition.x ? this.m_startPosition.x : this.m_mousePosition.x;
            var maxZ = this.m_startPosition.z > this.m_mousePosition.z ? this.m_startPosition.z : this.m_mousePosition.z;

            Vector3 position = segment.m_middlePosition;
            float positionDiff = Mathf.Max(Mathf.Max(minX - 16f - position.x, minZ - 16f - position.z), Mathf.Max(position.x - maxX - 16f, position.z - maxZ - 16f));

            if (positionDiff < 0f)
            {
                return true;
            }
            return false;
        }

        private bool ValidateSelectedArea(Building bd)
        {
            var minX = this.m_startPosition.x < this.m_mousePosition.x ? this.m_startPosition.x : this.m_mousePosition.x;
            var minZ = this.m_startPosition.z < this.m_mousePosition.z ? this.m_startPosition.z : this.m_mousePosition.z;
            var maxX = this.m_startPosition.x > this.m_mousePosition.x ? this.m_startPosition.x : this.m_mousePosition.x;
            var maxZ = this.m_startPosition.z > this.m_mousePosition.z ? this.m_startPosition.z : this.m_mousePosition.z;

            Vector3 position = bd.m_position;

            float positionDiff = Mathf.Max(Mathf.Max(minX - 16f - position.x, minZ - 16f - position.z), Mathf.Max(position.x - maxX - 16f, position.z - maxZ - 16f));

            if (positionDiff < 0f)
            {
                return true;
            }
            return false;
        }

        private void GetSegmentControlPoints(int segmentIndex, out NetTool.ControlPoint startPoint, out NetTool.ControlPoint middlePoint, out NetTool.ControlPoint endPoint)
        {
            //WriteLog("Entering GetSegmentControlPoints");
            NetManager net = Singleton<NetManager>.instance;
            startPoint = new NetTool.ControlPoint();
            middlePoint = new NetTool.ControlPoint();
            endPoint = new NetTool.ControlPoint();

            if (segmentIndex >= net.m_segments.m_buffer.Length)
            {
                ARUT.WriteLog("GetSegmentControlPoints:: segmentIndex >= net.m_segments.m_buffer.Length: segmentIndex: " + segmentIndex + " net.m_segments.m_buffer.Length: " + net.m_segments.m_buffer.Length);
                return;
            }

            NetInfo prefab = net.m_segments.m_buffer[segmentIndex].Info;

            startPoint.m_node = net.m_segments.m_buffer[segmentIndex].m_startNode;
            startPoint.m_segment = 0;
            startPoint.m_position = net.m_nodes.m_buffer[startPoint.m_node].m_position;
            startPoint.m_direction = net.m_segments.m_buffer[segmentIndex].m_startDirection;
            startPoint.m_elevation = net.m_nodes.m_buffer[startPoint.m_node].m_elevation;
            startPoint.m_outside = (net.m_nodes.m_buffer[startPoint.m_node].m_flags & NetNode.Flags.Outside) != NetNode.Flags.None;

            endPoint.m_node = net.m_segments.m_buffer[segmentIndex].m_endNode;
            endPoint.m_segment = 0;
            endPoint.m_position = net.m_nodes.m_buffer[endPoint.m_node].m_position;
            endPoint.m_direction = -net.m_segments.m_buffer[segmentIndex].m_endDirection;
            endPoint.m_elevation = net.m_nodes.m_buffer[endPoint.m_node].m_elevation;
            endPoint.m_outside = (net.m_nodes.m_buffer[endPoint.m_node].m_flags & NetNode.Flags.Outside) != NetNode.Flags.None;

            middlePoint.m_node = 0;
            middlePoint.m_segment = (ushort)segmentIndex;
            middlePoint.m_position = startPoint.m_position + startPoint.m_direction * (prefab.GetMinNodeDistance() + 1f);
            middlePoint.m_direction = startPoint.m_direction;
            middlePoint.m_elevation = Mathf.Lerp(startPoint.m_elevation, endPoint.m_elevation, 0.5f);
            middlePoint.m_outside = false;
            //WriteLog("Leaving GetSegmentControlPoints");
        }
    }
}
