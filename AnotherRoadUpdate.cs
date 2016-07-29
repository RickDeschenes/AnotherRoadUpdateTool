using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Threading;
using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using ICities;
using UnityEngine;
using System.Reflection;
using System.Diagnostics;

/// <summary>
/// Updated from SkylinesRoadUpdate added global preferences
/// added Additional selections
/// Added new to remove warning for reused m_
/// </summary>
namespace AnotherRoadUpdate
{
    public class RoadUpdateTool : DefaultTool
    {
        #region Declarations

        #region enums

        private enum Ups
        {
            Basic = 0,
            Highway = 1,
            Large = 2,
            Medium = 3,
            Oneway = 4
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
            Ground = 8,
            Bridge = 9,
            Slope = 10,
            Tunnel = 11,
            Buildings = 12,
            Trees = 13,
            Props = 14
        }

        private enum ops
        {
            Updates = 0,
            Deletes = 1,
            Services = 2
        }

        private enum dl
        {
            Toggle = 0,
            HealthCare = 1,
            PoliceDepartment = 2,
            FireDepartment = 3,
            PublicTransport = 4,
            Education = 5,
            Electricity = 6,
            Water = 7,
            Garbage = 8,
            Beautification = 9,
            Monument = 10
        }

        #endregion

        #region Variables

        private object m_dataLock = new object();

        private bool m_active;
        private bool m_selectable;
        private bool m_fromTypes;
        private bool m_toTypes;
        private bool m_toRoads;
        private bool m_fromRoads;
        private bool m_updates;
        private bool m_settings;
        
        private string fromSelected = string.Empty;
        private string toSelected = string.Empty;

        private Vector3 m_startPosition;
        private Vector3 m_startDirection;
        private Vector3 m_mouseDirection;
        private Vector3 m_cameraDirection;
        private new Vector3 m_mousePosition;
        private new bool m_mouseRayValid;
        private new Ray m_mouseRay;
        private new float m_mouseRayLength;

        private List<ushort> segmentsToDelete;

        private float m_maxArea = 400f;

        private UIButton mainButton;
        private UIPanel plMain;
        private UIPanel plOptions;
        private UIPanel plRoads;
        private UIPanel plDelete;
        private UIPanel plServices;
        private UIPanel plBasic;
        private UIPanel plHighway;
        private UIPanel plLarge;
        private UIPanel plMedium;
        private UIPanel plOneway;
        private UIPanel plToBasic;
        private UIPanel plToHighway;
        private UIPanel plToLarge;
        private UIPanel plToMedium;
        private UIPanel plToOneway;

        private UILabel lOptions;
        private UILabel lLines;
        private UILabel lProperties;
        private UILabel lSelectable;
        private UIButton btHelp;

        private UICheckBox cbCurved;

        List<UICheckBox> options = new List<UICheckBox>();
        List<UICheckBox> deletes = new List<UICheckBox>();
        List<UICheckBox> services = new List<UICheckBox>();

        List<UICheckBox> toTypes = new List<UICheckBox>();
        List<UICheckBox> fromTypes = new List<UICheckBox>();
        
        List<UICheckBox> toBasic = new List<UICheckBox>();
        List<UICheckBox> fromBasic = new List<UICheckBox>();

        List<UICheckBox> toLarge = new List<UICheckBox>();
        List<UICheckBox> fromLarge = new List<UICheckBox>();

        List<UICheckBox> toHighway = new List<UICheckBox>();
        List<UICheckBox> fromHighway = new List<UICheckBox>();

        List<UICheckBox> toMedium = new List<UICheckBox>();
        List<UICheckBox> fromMedium = new List<UICheckBox>();

        List<UICheckBox> toOneway = new List<UICheckBox>();
        List<UICheckBox> fromOneway = new List<UICheckBox>();
        
        #endregion

        //These strings are importent in that they control the interface
        private string[] m_options = new string[] { "Label Discription","Updates", "Deletes", "Services", "Label Selectable" };
        private string[] m_roads = new string[] { "Label ToFrom", "Basic", "Highway", "Large", "Medium", "Oneway" };
        private string[] m_basic = new string[] { "Gravel Road", "Basic Road", "Basic Road Bicycle", "Basic Road Decoration Grass", "Basic Road Decoration Trees", "Basic Road Elevated Bike", "Basic Road Elevated Tram", "Basic Road Elevated", "Basic Road Slope", "Basic Road Tram", "Basic Road Tunnel Bike", "Basic Road Tunnel Tram", "Basic Road Tunnel" };
        private string[] m_highway = new string[] { "Highway Barrier", "Highway Elevated", "Highway Tunnel", "Highway", "HighwayRamp Tunnel", "HighwayRamp", "HighwayRampElevated" };
        private string[] m_large = new string[] { "Large Oneway Decoration Grass", "Large Oneway Decoration Trees", "Large Oneway Elevated", "Large Oneway Road Tunnel", "Large Oneway", "Large Road Bicycle", "Large Road Bus", "Large Road Decoration Grass", "Large Road Decoration Trees", "Large Road Elevated Bike", "Large Road Elevated Bus", "Large Road Elevated", "Large Road Tunnel Bus", "Large Road Tunnel", "Large Road" };
        private string[] m_medium = new string[] { "Medium Road Bicycle", "Medium Road Bus", "Medium Road Decoration Grass", "Medium Road Decoration Trees", "Medium Road Elevated Bike", "Medium Road Elevated Bus", "Medium Road Elevated Tram", "Medium Road Elevated", "Medium Road Tram", "Medium Road Tunnel Bus", "Medium Road Tunnel Tram", "Medium Road Tunnel", "Medium Road" };
        private string[] m_oneway = new string[] { "Oneway Road Decoration Grass", "Oneway Road Decoration Trees", "Oneway Road Elevated Tram", "Oneway Road Elevated", "Oneway Road Tram", "Oneway Road Tunnel Tram", "Oneway Road Tunnel", "Oneway Road", "Oneway Tram Track" };
        private string[] m_deletes = new string[] { "Label Lines", "Roads", "Railroads", "Highways", "PowerLines", "Water Pipes", "Heat Pipes", "Airplanes", "Shipping", "Label Options", "Ground", "Bridge", "Slope", "Tunnel", "Label Properties", "Buildings", "Trees", "Props" };
        private string[] m_services = new string[] { "Toggle", "HealthCare", "PoliceDepartment", "FireDepartment", "PublicTransport", "Education", "Electricity", "Water", "Garbage", "Beautification", "Monument" };

        UserSettings us = new UserSettings();

        #endregion

        #region "Public Procedures"

        #region "Minor"

        protected override void Awake()
        {
            WriteLog("ARUT awake!", true);
            m_active = false;
            base.Awake();
        }

        protected override void OnEnable()
        {
            UIView.GetAView().FindUIComponent<UITabstrip>("MainToolstrip").selectedIndex = -1;
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            if (plMain != null)
                plMain.isVisible = false;
            base.OnDisable();
        }

        protected override void OnDestroy()
        {
            SetSettings();
            WriteLog("ARUT sleep!");
            base.OnDestroy();
        }

        protected override void OnToolGUI(Event e)
        {
            if (e.type == EventType.MouseDown && m_mouseRayValid && m_selectable)
            {
                if (e.button == 0)
                {
                    m_active = true;
                    this.m_startPosition = this.m_mousePosition;
                    this.m_startDirection = Vector3.forward;
                }
                if (e.button == 1)
                {
                    m_active = false;
                }
            }
            else if (e.type == EventType.MouseUp && m_active)
            {
                if (e.button == 0)
                {
                    if (options[(int)ops.Updates].isChecked)
                    {
                        WriteLog("Trying ApplyUpdates");
                        //handle Updates
                        ApplyUpdates();
                        WriteLog("Tried ApplyUpdates");
                    }
                    else if (options[(int)ops.Deletes].isChecked)
                    {
                        WriteLog("Trying ApplyDeletes");
                        //handle Updates
                        ApplyDeletes();
                        WriteLog("Tried ApplyDeletes");
                    }
                    else if (options[(int)ops.Services].isChecked)
                    {
                        WriteLog("Trying ApplyServices");
                        //Handle Services on/off
                        ApplyServices();
                        WriteLog("Tried ApplyServices");
                    }
                    m_active = false;
                }
            }
        }

        #endregion

        public void InitGui(LoadMode mode)
        {
            WriteLog("Entering InitGUI");

            mainButton = UIView.GetAView().FindUIComponent<UIButton>("MarqueeBulldozer");

            if (mainButton == null)
            {
                var RoadUpdateButton = UIView.GetAView().FindUIComponent<UIMultiStateButton>("BulldozerButton");

                mainButton = RoadUpdateButton.parent.AddUIComponent<UIButton>();
                mainButton.name = "RoadUpdateButton";
                mainButton.size = new Vector2(40, 40);
                mainButton.tooltip = "Another Road Update tool";
                mainButton.relativePosition = new Vector2
                (
                    RoadUpdateButton.relativePosition.x + RoadUpdateButton.width / 2.0f - (mainButton.width) - RoadUpdateButton.width,
                    RoadUpdateButton.relativePosition.y + RoadUpdateButton.height / 2.0f - mainButton.height / 2.0f
                );

                mainButton.normalFgSprite = RoadUpdateButton.normalFgSprite;
                mainButton.focusedFgSprite = RoadUpdateButton.focusedFgSprite;
                mainButton.hoveredFgSprite = RoadUpdateButton.hoveredFgSprite;

                mainButton.eventClick += Button_Clicked;

                plMain = UIView.GetAView().FindUIComponent("TSBar").AddUIComponent<UIPanel>();
                
                if (mode == LoadMode.LoadMap || mode == LoadMode.NewMap)
                    plMain.backgroundSprite = "GenericPanel";
                else
                    plMain.backgroundSprite = "SubcategoriesPanel";

                plMain.isVisible = false;
                plMain.name = "MarqueeBulldozerSettings";
                //Create the panels (Little like a tab view)
                int height = CreatePanels(plMain);

                //add events to update the settings
                plMain.size = new Vector2(575, height);

                btHelp = plMain.AddUIComponent<UIButton>();
                btHelp.size = new Vector2(25, 25);
                btHelp.relativePosition = new Vector3(575 - 25, height - 25);
                btHelp.text = "¿";
                btHelp.tooltip = "I will try to open a pdf file, if you do not have a viewer.... do not click.";
                btHelp.isVisible = true;
                btHelp.eventDoubleClick += btHelp_eventDoubleClick;

                plMain.relativePosition = new Vector2
                (
                    RoadUpdateButton.relativePosition.x + RoadUpdateButton.width / 2.0f - plMain.width,
                    RoadUpdateButton.relativePosition.y - (plMain.height - 10)
                );

                //We can load the users last session
                GetSettings(true);

                WriteLog("Leaving InitGUI", true);
                //WriteLog("Leaving InitGUI");
            }
        }
        
        protected override void OnToolLateUpdate()
        {
            Vector3 mousePosition = Input.mousePosition;
            Vector3 cameraDirection = Vector3.Cross(Camera.main.transform.right, Vector3.up);
            cameraDirection.Normalize();
            while (!Monitor.TryEnter(this.m_dataLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
            }
            try
            {
                this.m_mouseRay = Camera.main.ScreenPointToRay(mousePosition);
                this.m_mouseRayLength = Camera.main.farClipPlane;
                this.m_cameraDirection = cameraDirection;
                this.m_mouseRayValid = (!this.m_toolController.IsInsideUI && UnityEngine.Cursor.visible);
            }
            finally
            {
                Monitor.Exit(this.m_dataLock);
            }
        }

        #endregion

        #region "Private procedures"

        #region "GUI Layout"

        private int CreatePanels(UIPanel panel)
        {
            int plx = 1;
            int ply = 1;

            ply = GenerateOptions(panel, ply, plx);

            WriteLog("ply: " + ply + " plx: " + plx);

            GenerateplDelete(panel, ply, plx);
            GenerateplServices(panel, ply, plx);
            ply = GenerateplTypes(panel, ply, plx);

            GenerateRoadPanels(panel, ref plBasic, ref plToBasic, fromBasic, toBasic, m_basic, "Basic", ply, plx);
            GenerateRoadPanels(panel, ref plHighway, ref plToHighway, fromHighway, toHighway, m_highway, "Highway", ply, plx);
            GenerateRoadPanels(panel, ref plLarge, ref plToLarge, fromLarge, toLarge, m_large, "Large", ply, plx);
            GenerateRoadPanels(panel, ref plMedium, ref plToMedium, fromMedium, toMedium, m_medium, "Medium", ply, plx);
            GenerateRoadPanels(panel, ref plOneway, ref plToOneway, fromOneway, toOneway, m_oneway, "Oneway", ply, plx);
            
            //three rows in options, five in Road Types, 15 in Large roads
            return 25 * (3 + 6 + 16);
        }

        private int GenerateOptions(UIPanel panel, int ply, int plx)
        {
            //Show the road type option
            plOptions = panel.AddUIComponent<UIPanel>();
            plOptions.relativePosition = new Vector3(1, 1);
            plOptions.isVisible = true;
            plOptions.tooltip = "Select the type of updates to perform.";

            int cb = 0;
            foreach (string s in m_options)
            {
                if (s.StartsWith("Label "))
                {
                    if (s == "Label Selectable")
                    {
                        lSelectable = addUILabel(plOptions, ply, plx, "Selection is now available", false);
                    }
                    else
                        addUILabel(plOptions, ply, plx, "Another Road Update Tool.", true);
                    //This was the title, indent the rest
                }
                else
                {
                    string t = String.Format("Select to display the {0} options", s);
                    options.Add(addCheckbox(plOptions, ply + 25, plx, s, t, true));
                    //we will add ninty to each option or label
                    plx += 120;
                    options[cb].eventCheckChanged += Options_eventCheckChanged;
                    cb += 1;
                }
            }
            //return the top of the Road Types, Services, and Deletes panels
            return 50;
        }

        private int GenerateplTypes(UIPanel panel, int ply, int plx)
        {
            //Show the road type option
            plRoads = panel.AddUIComponent<UIPanel>();
            plRoads.relativePosition = new Vector3(1, 50);
            plRoads.isVisible = false;
            plRoads.tooltip = "Select the Road types to convert from and to";

            int x1 = 10;
            int x2 = 280;
            int y = 1;
            int cb = 0;
            //load the update types but hide them
            foreach (string s in m_roads)
            {
                if (s.StartsWith("Label "))
                {
                    try
                    {
                        addUILabel(plRoads, y, x1 - 5, "From ==> ", true);
                        addUILabel(plRoads, y, x2 - 5, "To ==> ", true);
                        y += 20;
                    }
                    catch (Exception ex)
                    {
                        WriteLog("error in GenerateplTypes: " + ex.Message + " stack:: " + ex.StackTrace);
                    }
                }
                else
                {
                    string t = String.Format("If checked {0} sections will be displayed.", s);
                    fromTypes.Add(addCheckbox(plRoads, y, x1, s, t, true));
                    toTypes.Add(addCheckbox(plRoads, y, x2, s, t, true));
                    fromTypes[cb].eventCheckChanged += FromTypes_eventCheckChanged;
                    toTypes[cb].eventCheckChanged += ToTypes_eventCheckChanged;
                    y += 25;
                    cb++;
                }
            }
            //add an option to ignor curved segments
            cbCurved = addCheckbox(plRoads, y + 5, 1, "Ignore Curves", "Allows you to delete all straight segments but ignore the curved ones.", true);
            cbCurved.eventCheckChanged += cbCurved_eventCheckChanged;

            plRoads.size = new Vector2(panel.width, 250);
            //return the top of the roads panals
            return ply + y;
        }

        private void GenerateplDelete(UIPanel panel, int ply, int plx)
        {
            //Show the road type option
            plDelete = panel.AddUIComponent<UIPanel>();
            plDelete.relativePosition = new Vector3(1, 50);
            plDelete.isVisible = false;
            plDelete.tooltip = "Select the type of items to delete.";

            int cb = 0;
            int x = 15;
            int y = 6;

            //load the bulldoze options but hide them (these are just options)
            addUILabel(plDelete, y - 5, 5, "Select your delete options.", true);
            y += 15;

            foreach (string s in m_deletes)
            {
                if (s == "Label Lines")
                {
                    lLines = addUILabel(plDelete, y, 10, "Lines", true);
                    y += 20;
                }
                else if (s == "Label Properties")
                {
                    lProperties = addUILabel(plDelete, y, 10, "Properties", true);
                    y += 20;
                }
                else if (s == "Label Options")
                {
                    lOptions = addUILabel(plDelete, y, 10, "Options", true);
                    y += 20;
                }
                else
                {
                    string t = "These items will be deleted.";
                    if (s == "Bridges" || s == "Ground" || s == "Tunnels") { t = "These types will be deleted.";  }
                    deletes.Add(addCheckbox(plDelete, y, x, s, t, true));
                    deletes[cb].eventCheckChanged += DeleteTypes_eventCheckChanged;
                    y += 25;
                    cb += 1;
                }
            }

            plDelete.size = new Vector2(panel.width, y + 25);
        }

        private void GenerateplServices(UIPanel panel, int ply, int plx)
        {
            //Show the road type option
            plServices = panel.AddUIComponent<UIPanel>();
            plServices.relativePosition = new Vector3(1, 50);
            plServices.isVisible = false;
            plServices.tooltip = "Select the type of services to toggle on and off.";

            int cb = 0;
            int y = 6;

            addUILabel(plServices, 1, 5, "Select your service options.", true);
            y += 20;

            foreach (string s in m_services)
            {
                string t = "These items will be toggled On or Off.";
                if (s == "Toggle")
                {
                    t = "Check to turn on services, unchecked to turn them off.";
                    services.Add(addCheckbox(plServices, y, 10, s, t, true));
                    services[cb].eventCheckChanged += ServiceTypes_eventCheckChanged;
                }
                else
                {
                    services.Add(addCheckbox(plServices, y, 15, s, t, true));
                    services[cb].eventCheckChanged += ServiceTypes_eventCheckChanged;
                }
                y += 25;
                cb += 1;
            }
            plServices.size = new Vector2(panel.width, y + 25);
        }

        private void GenerateRoadPanels(UIPanel panel, ref UIPanel pFrom, ref UIPanel pTo, List<UICheckBox> lFrom, List<UICheckBox> lTo, string[] road, string name, int ply, int plx)
        {
            WriteLog("Entering GenerateRoadPanels");
            //Show the road from option
            pFrom = panel.AddUIComponent<UIPanel>();
            pFrom.relativePosition = new Vector3(1, 230);
            pFrom.size = new Vector2(25 * 15, panel.width / 2);
            pFrom.isVisible = false;
            pFrom.tooltip = "Select the type of road to convert.";
            //Show the road to option
            pTo = panel.AddUIComponent<UIPanel>();
            pTo.relativePosition = new Vector3(280, 230);
            pTo.size = new Vector2(25 * 15, panel.width / 2);
            pTo.isVisible = false;
            pTo.tooltip = "Select the type of road to convert to.";

            int cb = 0;
            int y = 3;
            //load the update roads but hide them
            foreach (string s in road)
            {
                string t = String.Format("If checked {0} sections will be converted.", s);
                lFrom.Add(addCheckbox(pFrom, y, 11, s, t, true));
                t = String.Format("If checked sections will convert to {0}.", s);
                lTo.Add(addCheckbox(pTo, y, 11, s, t, true));
                y += 25;
                lFrom[cb].eventCheckChanged += FromRoad_eventCheckChanged;
                lTo[cb].eventCheckChanged += ToRoad_eventCheckChanged;
                cb++;
            }
            WriteLog("Leaving GenerateRoadPanels");
        }


        #region "Adding Controls"

        private UILabel addUILabel(UIPanel panel, int yPos, int xPos, string text, bool hidden)
        {
            UILabel lb = panel.AddUIComponent<UILabel>();
            lb.relativePosition = new Vector3(xPos, yPos);
            lb.height = 0;
            lb.width = 80;
            lb.text = text;
            lb.isVisible = hidden;
            return lb;
        }

        private UICheckBox addCheckbox(UIPanel panel, int yPos, int xPos, string text, string tooltip, bool hidden)
        {
            var cb = panel.AddUIComponent<UICheckBox>();
            cb.relativePosition = new Vector3(xPos, yPos);
            cb.height = 0;
            cb.width = 80;

            var label = panel.AddUIComponent<UILabel>();
            label.relativePosition = new Vector3(xPos + 25, yPos + 3);
            cb.label = label;
            cb.text = text;

            UISprite uncheckSprite = cb.AddUIComponent<UISprite>();
            uncheckSprite.height = 20;
            uncheckSprite.width = 20;
            uncheckSprite.relativePosition = new Vector3(0, 0);
            uncheckSprite.spriteName = "check-unchecked";

            UISprite checkSprite = cb.AddUIComponent<UISprite>();
            checkSprite.height = 20;
            checkSprite.width = 20;
            checkSprite.relativePosition = new Vector3(0, 0);
            checkSprite.spriteName = "check-checked";

            cb.checkedBoxObject = checkSprite;

            cb.tooltip = tooltip;
            label.tooltip = cb.tooltip;
            cb.isChecked = false;
            cb.label.isVisible = hidden;
            cb.isVisible = hidden;

            return cb;
        }

        #endregion

        #endregion

        #region "Settings"

        private void SetSettings()
        {
            if (m_settings == true) { return; }
            m_settings = true;

            WriteLog("Entering SetSetting deletes[1].isChecked: " + deletes[1].isChecked);

            us.ToOneway = toTypes[(int)Ups.Oneway].isChecked;
            us.ToMedium = toTypes[(int)Ups.Medium].isChecked;
            us.ToHighway = toTypes[(int)Ups.Highway].isChecked;
            us.ToLarge = toTypes[(int)Ups.Large].isChecked;
            us.ToBasic = toTypes[(int)Ups.Basic].isChecked;
            us.Oneway = fromTypes[(int)Ups.Oneway].isChecked;
            us.Medium = fromTypes[(int)Ups.Medium].isChecked;
            us.Highway = fromTypes[(int)Ups.Highway].isChecked;
            us.Large = fromTypes[(int)Ups.Large].isChecked;
            us.Basic = fromTypes[(int)Ups.Basic].isChecked;

            us.Roads = deletes[(int)p.Roads].isChecked;
            us.Railroads = deletes[(int)p.Railroads].isChecked;
            us.Highways = deletes[(int)p.Highways].isChecked;
            us.PowerLines = deletes[(int)p.PowerLines].isChecked;
            us.WaterPipes = deletes[(int)p.WaterPipes].isChecked;
            us.HeatPipes = deletes[(int)p.HeatPipes].isChecked;
            us.Airplanes = deletes[(int)p.Airplanes].isChecked;
            us.Shipping = deletes[(int)p.Shipping].isChecked;

            us.Ground = deletes[(int)p.Ground].isChecked;
            us.Bridge = deletes[(int)p.Bridge].isChecked;
            us.Slope = deletes[(int)p.Slope].isChecked;
            us.Tunnel = deletes[(int)p.Tunnel].isChecked;

            us.Buildings = deletes[(int)p.Buildings].isChecked;
            us.Trees = deletes[(int)p.Trees].isChecked;
            us.Props = deletes[(int)p.Props].isChecked;

            us.Update = options[(int)ops.Updates].isChecked;
            us.Delete = options[(int)ops.Deletes].isChecked;
            us.Services = options[(int)ops.Services].isChecked;
            us.Toggle = deletes[(int)dl.Toggle].isChecked;

            us.Save();

            WriteLog("Leaving SetSettings deletes[1].isChecked: " + deletes[1].isChecked);
            m_settings = false;
        }

        private void GetSettings(bool toBasic)
        {
            if (m_settings == true) { return; }
            m_settings = true;

            int loc = 0;
            WriteLog("Entering GetSettings deletes[1].isChecked: " + deletes[1].isChecked);
            try
            {
                toTypes[(int)Ups.Oneway].isChecked = us.ToOneway;
                toTypes[(int)Ups.Medium].isChecked = us.ToMedium;
                toTypes[(int)Ups.Highway].isChecked = us.ToHighway;
                toTypes[(int)Ups.Large].isChecked = us.ToLarge;
                toTypes[(int)Ups.Basic].isChecked = us.ToBasic;
                fromTypes[(int)Ups.Oneway].isChecked = us.Oneway;
                fromTypes[(int)Ups.Medium].isChecked = us.Medium;
                fromTypes[(int)Ups.Highway].isChecked = us.Highway;
                fromTypes[(int)Ups.Large].isChecked = us.Large;
                fromTypes[(int)Ups.Basic].isChecked = us.Basic;

                deletes[(int)p.Roads].isChecked = us.Roads;
                deletes[(int)p.Railroads].isChecked = us.Railroads;
                deletes[(int)p.Highways].isChecked = us.Highways;
                deletes[(int)p.PowerLines].isChecked = us.PowerLines;
                deletes[(int)p.WaterPipes].isChecked = us.WaterPipes;
                deletes[(int)p.HeatPipes].isChecked = us.HeatPipes;
                deletes[(int)p.Airplanes].isChecked = us.Airplanes;
                deletes[(int)p.Shipping].isChecked = us.Shipping;

                deletes[(int)p.Ground].isChecked = us.Ground;
                deletes[(int)p.Bridge].isChecked = us.Bridge;
                deletes[(int)p.Slope].isChecked = us.Slope;
                deletes[(int)p.Tunnel].isChecked = us.Tunnel;

                deletes[(int)p.Buildings].isChecked = us.Buildings;
                deletes[(int)p.Trees].isChecked = us.Trees;
                deletes[(int)p.Props].isChecked = us.Props;

                loc += 1;
                options[(int)ops.Services].isChecked = us.Services;
                loc += 1;
                options[(int)ops.Deletes].isChecked = us.Delete;
                loc += 1;
                options[(int)ops.Updates].isChecked = us.Update;
                loc += 1;
                deletes[(int)dl.Toggle].isChecked = us.Toggle;
            }
            catch (Exception ex)
            {
                WriteLog("GetSettings loc: " + loc + " deltypes.Count: " + deletes.Count + " Exception: " + ex.Message);
            }
            WriteLog("Leaving GetSettings deletes[1].isChecked: " + deletes[1].isChecked);
            m_settings = false;
        }

        #endregion

        #region "Event Process helpers"

        private void SetServicesEnabled()
        {
            WriteLog("Entering: SetServicesEnabled m_selectable: " + m_selectable);

            try
            {
                m_selectable = false;
                lSelectable.isVisible = m_selectable;

                //do we have at least one service type selected
                if (services.Any(o => o.isChecked == true)) { }
                else
                    return;

                //ok all checks complete
                m_selectable = true;
                lSelectable.isVisible = m_selectable;
            }
            catch (Exception ex)
            {
                WriteLog("SetServicesEnabled Exception: " + ex.Message + " Stack:: " + ex.StackTrace);
            }

            WriteLog("Leaving: SetServicesEnabled m_selectable: " + m_selectable);
        }

        private void SetUpdateEnabled()
        {
            WriteLog("Entering: SetUpdateEnabled m_selectable: " + m_selectable);

            try
            {
                m_selectable = false;
                lSelectable.isVisible = m_selectable;
                //is Updated enabled?
                if (options[(int)ops.Updates].isChecked == false)
                    return;

                //do we have a from road type
                if (fromTypes.Any(o => o.isChecked == true)) { }
                else
                    return;

                //do we have a to road type
                if (toTypes.Any(o => o.isChecked == true)) { }
                else
                    return;

                //do we have a from Road
                if (fromOneway.Any(o => o.isChecked == true)) { }
                else if (fromMedium.Any(o => o.isChecked == true)) { }
                else if (fromHighway.Any(o => o.isChecked == true)) { }
                else if (fromLarge.Any(o => o.isChecked == true)) { }
                else if (fromBasic.Any(o => o.isChecked == true)) { }
                else
                    return;

                //do we have a to Road
                if (toOneway.Any(o => o.isChecked == true)) { }
                else if (toMedium.Any(o => o.isChecked == true)) { }
                else if (toHighway.Any(o => o.isChecked == true)) { }
                else if (toLarge.Any(o => o.isChecked == true)) { }
                else if (toBasic.Any(o => o.isChecked == true)) { }
                else
                    return;

                //ok all checks complete
                m_selectable = true;
                lSelectable.isVisible = m_selectable;
            }
            catch (Exception ex)
            {
                WriteLog("SetUpdateEnabled Exception: " + ex.Message + " Stack:: " + ex.StackTrace);
            }

            WriteLog("Leaving: SetUpdateEnabled m_selectable: " + m_selectable);
        }

        private void SetDeleteEnabled()
        {
          WriteLog("Entering SetDeleteEnabled selectable is: " + m_selectable);
            try
            {
                WriteLog("(Ground || Bridge || Slope || Tunnel): " + (deletes[(int)p.Ground].isChecked || deletes[(int)p.Bridge].isChecked || deletes[(int)p.Slope].isChecked || deletes[(int)p.Tunnel].isChecked));

                WriteLog("(The Lines): " + (deletes[(int)p.Roads].isChecked ||
                                            deletes[(int)p.Railroads].isChecked ||
                                            deletes[(int)p.PowerLines].isChecked ||
                                            deletes[(int)p.WaterPipes].isChecked ||
                                            deletes[(int)p.HeatPipes].isChecked ||
                                            deletes[(int)p.Airplanes].isChecked ||
                                            deletes[(int)p.Shipping].isChecked));

                WriteLog("(The Properties): " + (deletes[(int)p.Buildings].isChecked ||
                                            deletes[(int)p.Trees].isChecked ||
                                            deletes[(int)p.Props].isChecked));

                m_selectable = (((options[(int)ops.Deletes].isChecked) &&
                                    (deletes[(int)p.Ground].isChecked ||
                                        deletes[(int)p.Bridge].isChecked ||
                                        deletes[(int)p.Slope].isChecked ||
                                        deletes[(int)p.Tunnel].isChecked) &&
                                    (deletes[(int)p.Roads].isChecked ||
                                        deletes[(int)p.Railroads].isChecked ||
                                        deletes[(int)p.PowerLines].isChecked ||
                                        deletes[(int)p.WaterPipes].isChecked ||
                                        deletes[(int)p.HeatPipes].isChecked ||
                                        deletes[(int)p.Airplanes].isChecked ||
                                        deletes[(int)p.Shipping].isChecked)) ||
                                    (deletes[(int)p.Buildings].isChecked ||
                                        deletes[(int)p.Trees].isChecked ||
                                        deletes[(int)p.Props].isChecked));

                lSelectable.isVisible = m_selectable;
            }
            catch (Exception ex)
            {
                WriteLog("SetDeleteEnabled Exception: " + ex.Message + " Stack:: " + ex.StackTrace);
            }
          WriteLog("Leaving SetDeleteEnabled selectable is: " + m_selectable);
        }

        private void ShowRoads()
        {
            //From
            //Make sure all road panels are hidden
            plBasic.isVisible = false;
            plHighway.isVisible = false;
            plLarge.isVisible = false;
            plMedium.isVisible = false;
            plOneway.isVisible = false;
            //now check for any that should be displayed
            foreach (UICheckBox c in fromTypes)
            {
                if (c.isChecked == true && options[(int)ops.Updates].isChecked)
                {
                    if (c.text == "Basic") { plBasic.isVisible = true; }
                    if (c.text == "Highway") { plHighway.isVisible = true; }
                    if (c.text == "Large") { plLarge.isVisible = true; }
                    if (c.text == "Medium") { plMedium.isVisible = true; }
                    if (c.text == "Oneway") { plOneway.isVisible = true; }
                }
            }
            //To
            //Make sure all road panels are hidden
            plToBasic.isVisible = false;
            plToHighway.isVisible = false;
            plToLarge.isVisible = false;
            plToMedium.isVisible = false;
            plToOneway.isVisible = false;
            //now check for any that should be displayed
            foreach (UICheckBox c in toTypes)
            {
                if (c.isChecked == true && options[(int)ops.Updates].isChecked)
                {
                    if (c.text == "Basic") { plToBasic.isVisible = true; }
                    if (c.text == "Highway") { plToHighway.isVisible = true; }
                    if (c.text == "Large") { plToLarge.isVisible = true; }
                    if (c.text == "Medium") { plToMedium.isVisible = true; }
                    if (c.text == "Oneway") { plToOneway.isVisible = true; }
                }
            }
            //Show if available
            SetUpdateEnabled();
        }

        private void UpdatePanels(bool show)
        {
            plRoads.isVisible = show;
            plDelete.isVisible = show;
            plServices.isVisible = show;
            plBasic.isVisible = show;
            plHighway.isVisible = show;
            plLarge.isVisible = show;
            plMedium.isVisible = show;
            plOneway.isVisible = show;
            plToBasic.isVisible = show;
            plToHighway.isVisible = show;
            plToLarge.isVisible = show;
            plToMedium.isVisible = show;
            plToOneway.isVisible = show;
            m_selectable = false;
            lSelectable.isVisible = m_selectable;
        }

        private void UpdateDisplayedRoads(List<UICheckBox> types, List<UICheckBox> roads, string text, bool show, int xPos)
        {
            WriteLog("Entering UpdateDisplayedRoads " + xPos);

            foreach (UICheckBox cb in types)
            {
                if (cb.text != text)
                {
                    cb.isChecked = false;
                }
            }

            if (text.Contains("Basic")) { DisplayCheckBoxes(roads, xPos, "Basic", show); }
            else if (text.Contains("Highway")) { DisplayCheckBoxes(roads, xPos, "Highway", show); }
            else if (text.Contains("Large")) { DisplayCheckBoxes(roads, xPos, "Large", show); }
            else if (text.Contains("Medium")) { DisplayCheckBoxes(roads, xPos, "Medium", show); }
            else if (text.Contains("Oneway")) { DisplayCheckBoxes(roads, xPos, "Oneway", show); }
            else
                WriteLog("Could not define this object type.");

            WriteLog("Leaving UpdateDisplayedRoads");
        }

        private void DisplayCheckBoxes(List<UICheckBox> roads, int xPos, string test, bool show)
        {
            WriteLog("Entering DisplayCheckBoxes");

            int y = 0;
            int x = xPos;

            foreach (UICheckBox cb in roads)
            {
                cb.isVisible = false;
                cb.label.isVisible = false;
                if (cb.text.Contains(test))
                {
                    cb.relativePosition = new Vector3(x, y);
                    cb.label.relativePosition = new Vector3(x + 25, y + 3);
                    cb.isVisible = show;
                    cb.label.isVisible = show;
                    y += 25;
                }
            }

            WriteLog("Leaving UpdateDisplayedRoads");
        }

        #endregion

        #region "Event Handlers"
        
        private void cbCurved_eventCheckChanged(UIComponent component, bool value)
        {
            WriteLog("Entering cbCurved_eventCheckChanged Checkbox: " + value);

            //not sure we need this it is a boolean

            WriteLog("Leaving cbCurved_eventCheckChanged Checkbox: " + value);
        }

        private void ServiceTypes_eventCheckChanged(UIComponent component, bool value)
        {
            WriteLog("Entering ServiceTypes_eventCheckChanged");

            SetServicesEnabled();

            WriteLog("Leaving ServiceTypes_eventCheckChanged");
        }

        private void btHelp_eventDoubleClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            WriteLog("Entering btHelp_eventDoubleClick");

            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream("AnotherRoadUpdate.ARUT.pdf");
                if (stream == null)
                {
                    WriteLog("Error loading embeded resource AnotherRoadUpdate.ARUT.pdf");
                    return;
                }
                BinaryReader br = new BinaryReader(stream);
                FileStream fs = new FileStream("ARUT.pdf", FileMode.Create);
                BinaryWriter bw = new BinaryWriter(fs);
                byte[] ba = new byte[stream.Length];
                stream.Read(ba, 0, ba.Length);
                bw.Write(ba);
                br.Close();
                bw.Close();
                Process p = new Process();
                p.StartInfo.FileName = "ARUT.pdf";
                p.Start();

            }
            catch (Exception ex)
            {
                WriteLog("btHelp_eventDoubleClick Exception: " + ex.Message + " Stack: " + ex.StackTrace);
            }

            WriteLog("Leaving btHelp_eventDoubleClick");
        }

        private void DeleteTypes_eventCheckChanged(UIComponent component, bool value)
        {
            WriteLog("Entering DeleteTypes_eventCheckChanged");
            UICheckBox c = (UICheckBox)component;
            //store the update
            if (c.text == deletes[(int)p.Roads].text) { us.Roads = value; }
            if (c.text == deletes[(int)p.Railroads].text) { us.Railroads = value; }
            if (c.text == deletes[(int)p.Highways].text) { us.Highways = value; }
            if (c.text == deletes[(int)p.PowerLines].text) { us.PowerLines = value; }
            if (c.text == deletes[(int)p.WaterPipes].text) { us.WaterPipes = value; }
            if (c.text == deletes[(int)p.HeatPipes].text) { us.HeatPipes = value; }
            if (c.text == deletes[(int)p.Airplanes].text) { us.Airplanes = value; }
            if (c.text == deletes[(int)p.Shipping].text) { us.Shipping = value; }

            if (c.text == deletes[(int)p.Buildings].text) { us.Buildings = value; }
            if (c.text == deletes[(int)p.Props].text) { us.Props = value; }
            if (c.text == deletes[(int)p.Trees].text) { us.Trees = value; }

            if (c.text == deletes[(int)p.Ground].text) { us.Ground = value; }
            if (c.text == deletes[(int)p.Bridge].text) { us.Bridge = value; }
            if (c.text == deletes[(int)p.Tunnel].text) { us.Tunnel = value; }

            //set enabled
            SetDeleteEnabled();
            WriteLog("Leaving DeleteTypes_eventCheckChange Roads: " + us.Roads + " Ground: " + us.Ground);
        }

        private void Options_eventCheckChanged(UIComponent component, bool value)
        {
            if (m_updates == true) { return; }
            m_updates = true;

            UICheckBox c = (UICheckBox)component;
            WriteLog("Entering: Options_eventCheckChanged: c.text: " + c.text + " Checked: " + c.isChecked);

            //Hide tham all
            UpdatePanels(false);

            //did we unclick tham all?
            if (c.isChecked == false) { }
            else
            {
                if (c.text == "Updates")
                {
                    options[(int)ops.Deletes].isChecked = false;
                    options[(int)ops.Services].isChecked = false;
                    plRoads.isVisible = true;
                    ShowRoads();
                }
                else if (c.text == "Deletes")
                {
                    options[(int)ops.Updates].isChecked = false;
                    options[(int)ops.Services].isChecked = false;
                    plDelete.isVisible = true;
                    SetDeleteEnabled();
                }
                else
                {
                    options[(int)ops.Updates].isChecked = false;
                    options[(int)ops.Deletes].isChecked = false;
                    plServices.isVisible = true;
                    SetServicesEnabled();
                }
            }
            m_updates = false;
            WriteLog("Leaving Options_eventCheckChanged");
        }

        private void FromTypes_eventCheckChanged(UIComponent component, bool value)
        {
            if (m_fromTypes == true) { return; }
            m_fromTypes = true;

            UICheckBox cb = (UICheckBox)component;

            WriteLog("Entering FromTypes_eventCheckChanged: Option: " + cb.text);

            int loc = 0;
            try
            {
                plBasic.isVisible = false;
                plHighway.isVisible = false;
                plLarge.isVisible = false;
                plMedium.isVisible = false;
                plOneway.isVisible = false;
                loc += 1;
            }
            catch (Exception ex)
            {
                WriteLog("Exception in FromTypes_eventCheckChanged loc: " + loc + " Message:: " + ex.Message + " Stack::: " + ex.StackTrace);
            }

            //uncheck any others
            foreach (UICheckBox c in fromTypes)
            {
                if (c.text != cb.text) { c.isChecked = false; }
            }

            if (cb.isChecked == true)
            {
                if (cb.text == "Basic") { plBasic.isVisible = true; }
                if (cb.text == "Highway") { plHighway.isVisible = true; }
                if (cb.text == "Large") { plLarge.isVisible = true; }
                if (cb.text == "Medium") { plMedium.isVisible = true; }
                if (cb.text == "Oneway") { plOneway.isVisible = true; }
            }

            m_fromTypes = false;

            WriteLog("Leaving FromTypes_eventCheckChanged: Option: " + cb.text);
        }

        private void ToTypes_eventCheckChanged(UIComponent component, bool value)
        {
            if (m_toTypes == true) { return; }
            m_toTypes = true;

            UICheckBox cb = (UICheckBox)component;

            WriteLog("Entering ToTypes_eventCheckChanged: Option: " + cb.text);

            int loc = 0;
            try
            {
                plToBasic.isVisible = false;
                plToHighway.isVisible = false;
                plToLarge.isVisible = false;
                plToMedium.isVisible = false;
                plToOneway.isVisible = false;
                loc += 1;
            }
            catch (Exception ex)
            {
                WriteLog("Exception in ToTypes_eventCheckChanged loc: " + loc + " Message:: " + ex.Message + " Stack::: " + ex.StackTrace);
            }

            //uncheck any others
            foreach (UICheckBox c in toTypes)
            {
                if (c.text != cb.text) { c.isChecked = false; }
            }

            if (cb.isChecked == true)
            {
                if (cb.text == "Basic") { plToBasic.isVisible = true; }
                if (cb.text == "Highway") { plToHighway.isVisible = true; }
                if (cb.text == "Large") { plToLarge.isVisible = true; }
                if (cb.text == "Medium") { plToMedium.isVisible = true; }
                if (cb.text == "Oneway") { plToOneway.isVisible = true; }
            }

            m_toTypes = false;

            WriteLog("Leaving ToTypes_eventCheckChanged: Option: " + cb.text);
        }

        private void FromRoad_eventCheckChanged(UIComponent component, bool value)
        {
            if (m_fromRoads == true) { return; }
            m_fromRoads = true;

            UICheckBox cb = (UICheckBox)component;

            WriteLog("Entering FromRoads_eventCheckChanged: Option: " + cb.text);

            //uncheck any others
            foreach (UICheckBox c in fromOneway) { if (c.text != cb.text) { c.isChecked = false; } }
            foreach (UICheckBox c in fromMedium) { if (c.text != cb.text) { c.isChecked = false; } }
            foreach (UICheckBox c in fromHighway) { if (c.text != cb.text) { c.isChecked = false; } }
            foreach (UICheckBox c in fromLarge) { if (c.text != cb.text) { c.isChecked = false; } }
            foreach (UICheckBox c in fromBasic) { if (c.text != cb.text) { c.isChecked = false; } }

            fromSelected = String.Empty;
            if (cb.isChecked) { fromSelected = cb.text; }

            SetUpdateEnabled();

            m_fromRoads = false;

            WriteLog("Leaving FromRoads_eventCheckChanged: Option: " + cb.text);
        }

        private void ToRoad_eventCheckChanged(UIComponent component, bool value)
        {
            WriteLog("Entering ToRoads_eventCheckChanged: m_toRoads: " + m_toRoads);
            if (m_toRoads == true) { return; }
            m_toRoads = true;

            UICheckBox cb = (UICheckBox)component;

            WriteLog("Entering ToRoads_eventCheckChanged: Option: " + cb.text);

            //uncheck any others
            foreach (UICheckBox c in toOneway) { if (c.text != cb.text) { c.isChecked = false; } }
            foreach (UICheckBox c in toMedium) { if (c.text != cb.text) { c.isChecked = false; } }
            foreach (UICheckBox c in toHighway) { if (c.text != cb.text) { c.isChecked = false; } }
            foreach (UICheckBox c in toLarge) { if (c.text != cb.text) { c.isChecked = false; } }
            foreach (UICheckBox c in toBasic) { if (c.text != cb.text) { c.isChecked = false; } }

            toSelected = String.Empty;
            if (cb.isChecked) { toSelected = cb.text; }
            SetUpdateEnabled();

            m_toRoads = false;

            WriteLog("Leaving ToRoads_eventCheckChanged: Option: " + cb.text);
        }
        
        private void Button_Clicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (plMain.isVisible == true)
            {
                this.enabled = false;
                plMain.isVisible = false;
            }
            else
            {
                this.enabled = true;
                plMain.isVisible = true;
            }
        }

        #endregion

        #region "Area Selection"

        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            while (!Monitor.TryEnter(this.m_dataLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
            }
            Vector3 startPosition;
            Vector3 mousePosition;
            Vector3 startDirection;
            Vector3 mouseDirection;
            bool active;

            try
            {
                active = this.m_active;

                startPosition = this.m_startPosition;
                mousePosition = this.m_mousePosition;
                startDirection = this.m_startDirection;
                mouseDirection = this.m_mouseDirection;
            }
            finally
            {
                Monitor.Exit(this.m_dataLock);
            }

            var color = Color.red;

            if (!active)
            {
                base.RenderOverlay(cameraInfo);
                return;
            }

            Vector3 a = (!active) ? mousePosition : startPosition;
            Vector3 vector = mousePosition;
            Vector3 a2 = (!active) ? mouseDirection : startDirection;
            Vector3 a3 = new Vector3(a2.z, 0f, -a2.x);

            float num = Mathf.Round(((vector.x - a.x) * a2.x + (vector.z - a.z) * a2.z) * 0.125f) * 8f;
            float num2 = Mathf.Round(((vector.x - a.x) * a3.x + (vector.z - a.z) * a3.z) * 0.125f) * 8f;

            float num3 = (num < 0f) ? -4f : 4f;
            float num4 = (num2 < 0f) ? -4f : 4f;

            Quad3 quad = default(Quad3);
            quad.a = a - a2 * num3 - a3 * num4;
            quad.b = a - a2 * num3 + a3 * (num2 + num4);
            quad.c = a + a2 * (num + num3) + a3 * (num2 + num4);
            quad.d = a + a2 * (num + num3) - a3 * num4;

            if (num3 != num4)
            {
                Vector3 b = quad.b;
                quad.b = quad.d;
                quad.d = b;
            }
            ToolManager toolManager = ToolManager.instance;
            toolManager.m_drawCallData.m_overlayCalls++;
            RenderManager.instance.OverlayEffect.DrawQuad(cameraInfo, color, quad, -1f, 1025f, false, true);
            base.RenderOverlay(cameraInfo);
            return;
        }

        /// <summary>
        /// Updated to set max range
        /// </summary>
        /// <param name="newMousePosition"></param>
        /// <returns></returns>
        private bool checkMaxArea(Vector3 newMousePosition)
        {
            if ((m_startPosition -newMousePosition).sqrMagnitude > m_maxArea * 100000)
            {
                return false;
            }
            return true;
        }

        public override void SimulationStep()
        {
            while (!Monitor.TryEnter(this.m_dataLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
            }
            Ray mouseRay;
            Vector3 cameraDirection;
            bool mouseRayValid;
            try
            {
                mouseRay = this.m_mouseRay;
                cameraDirection = this.m_cameraDirection;
                mouseRayValid = this.m_mouseRayValid;
            }
            finally
            {
                Monitor.Exit(this.m_dataLock);
            }

            ToolBase.RaycastInput input = new ToolBase.RaycastInput(mouseRay, m_mouseRayLength);
            ToolBase.RaycastOutput raycastOutput;
            if (mouseRayValid && ToolBase.RayCast(input, out raycastOutput))
            {
                if (!m_active)
                {
                    while (!Monitor.TryEnter(this.m_dataLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
                    {
                    }
                    try
                    {
                        this.m_mouseDirection = cameraDirection;
                        this.m_mousePosition = raycastOutput.m_hitPos;
                    }
                    finally
                    {
                        Monitor.Exit(this.m_dataLock);
                    }
                }
                else
                {
                    while (!Monitor.TryEnter(this.m_dataLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
                    {
                    }
                    try
                    {
                        if (checkMaxArea(raycastOutput.m_hitPos))
                        {
                            this.m_mousePosition = raycastOutput.m_hitPos;
                        }
                    }
                    finally
                    {
                        Monitor.Exit(this.m_dataLock);
                    }
                }

            }
        }
        
        int RebuildSegment(int segmentIndex, NetInfo newPrefab, bool roadDirectionMatters, Vector3 directionPoint, Vector3 direction, ref ToolBase.ToolErrors error)
        {
            NetManager net = Singleton<NetManager>.instance;

            if (segmentIndex >= net.m_segments.m_buffer.Length)
                return 0;

            NetInfo prefab = net.m_segments.m_buffer[segmentIndex].Info;

            NetTool.ControlPoint startPoint;
            NetTool.ControlPoint middlePoint;
            NetTool.ControlPoint endPoint;
            GetSegmentControlPoints(segmentIndex, out startPoint, out middlePoint, out endPoint);
            
            bool test = false;
            bool visualize = false;
            bool autoFix = true;
            bool needMoney = false;
            bool invert = false;
            
            ushort node = 0;
            ushort segment = 0;
            int cost = 0;
            int productionRate = 0;

            NetTool.CreateNode(newPrefab, startPoint, middlePoint, endPoint, NetTool.m_nodePositionsSimulation, 1000, test, visualize, autoFix, needMoney, invert, false, (ushort)0, out node, out segment, out cost, out productionRate);
            
            if (segment != 0)
            {
                if (newPrefab.m_class.m_service == ItemClass.Service.Road)
                {
                    Singleton<CoverageManager>.instance.CoverageUpdated(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Level.None);
                }
                
                return segment;
            }

            return 0;
        }

        #endregion

        #region "Updates, Deletes, Toggles, Oh my!"

        private int ConvertObjects(string convertTo, string convertFrom, bool test, out int totalCost, out ToolBase.ToolErrors errors)
        {
            int num = 0;
            totalCost = 0;
            errors = 0;

            StringWriter sw = new StringWriter();
            //sw.WriteLine(String.Format("Entering ConvertObjects at {0}.", DateTime.Now.TimeOfDay

            NetInfo info = PrefabCollection<NetInfo>.FindLoaded(convertTo);

            if (info == null)
            {
                sw.WriteLine("Could not find the object: " + convertTo + ", aborting.");
                return num;
            }
         
            NetSegment[] buffer = Singleton<NetManager>.instance.m_segments.m_buffer;

            //sw.WriteLine("Filling Singleton<NetManager>.instance.m_segments.m_buffer. Found: " + buffer.Length);

            for (int i = 0; i < buffer.Length - 1; i++)
            {
                NetSegment segment = buffer[i];

                // do not uncomment unless you know not to multi click
                //if (Array.Exists(m_roads, (s) => { return s == segment.Info.name; }))
                //{
                //    sw.WriteLine(segment.Info.name + " from: " + convertFrom + " to: " + convertTo);
                //}
                //sw.WriteLine(segment.Info.name + " from: " + convertFrom + " to: " + convertTo);

                //Validate in selected area
                if (segment.Info == null)
                {
                    //sw.WriteLine(String.Format("Segment {0} is Null.",segment.Info.name
                }
                else if (segment.Info.name != convertFrom)
                {
                    //if (segment.Info.name != "Pedestrian Gravel")
                    //    sw.WriteLine(String.Format("Segment {0} is not a converting item.", segment.Info.name
                }
                else if (ValidateSelectedArea(segment) == false)
                {
                    //sw.WriteLine(String.Format("Segment {0} is not in the selected area.", segment.Info.name
                }
                else
                {
                    try
                    {
                        NetTool.ControlPoint point;
                        NetTool.ControlPoint point2;
                        NetTool.ControlPoint point3;

                        //sw.WriteLine(segment.Info.name + " converting to " + convertTo + ".");
                        //sw.WriteLine("Segment is a corner: " + segment.m_cornerAngleStart + " X " + segment.m_cornerAngleEnd);
                        //sw.WriteLine("About to call GetSegmentControlPoints.\n");

                        this.GetSegmentControlPoints(i, out point, out point2, out point3);
                        bool flag = false;
                        bool flag2 = true;
                        bool flag3 = true;
                        bool flag4 = false;
                        ushort num3 = 0;
                        ushort num4 = 0;
                        int num5 = 0;
                        int num6 = 0;

                        //test for bad index
                        if ((point.m_position == new Vector3()) && (point.m_position == new Vector3()) && (point.m_position == new Vector3())) { }
                            else
                        {
                            //sw.WriteLine("About to call NetTool.Create test mode.\n");
                            //Validate in area and other errors (no money!)
                            errors = NetTool.CreateNode(info, point, point2, point3, NetTool.m_nodePositionsSimulation, 0x3e8, true, flag, flag2, flag3, flag4, false, 0, out num3, out num4, out num5, out num6);

                            //sw.WriteLine("Called NetTool.Create.\n");
                            if (errors == 0)
                            {
                                errors = NetTool.CreateNode(info, point, point2, point3, NetTool.m_nodePositionsSimulation, 0x3e8, false, flag, flag2, flag3, flag4, false, 0, out num3, out num4, out num5, out num6);
                                num++;
                                totalCost += num5;
                            }
                            else if (test == false)
                            {
                                sw.WriteLine("Could not convert: " + segment.Info.name + " to " + convertTo + ". Message: " + GetMessage(errors));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        sw.WriteLine("Error converting: " + segment.Info.name + " to " + convertTo + ". Message: " + ex.Message);
                    }
                }
            }
            //sw.WriteLine("Exiting ConvertObjects. num = " + num);
            //sw.WriteLine(String.Format("Exiting ConvertObjects at: {0} after converting: {1} items .", DateTime.Now.TimeOfDay, num
          WriteLog("" + sw);
            UIView.RefreshAll(true);
            return num;
        }

        private void ToggleServices(bool toggle, ItemClass.Service service)
        {
            FastList<ushort> fl = Singleton<BuildingManager>.instance.GetServiceBuildings(service);
            
            Building[] buffer =  Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i].Info.GetService() == service)
                {
                    Building bi = buffer[i];
                    string bn = bi.Info.gameObject.name;

                    Vector3 v3 = bi.CalculatePosition(new Vector3());

                  WriteLog("Settings before: " + bi.Info.GetAI().enabled, true);

                    //bi.Info.gameObject.SetActive(toggle);
                    //bi.Info.enabled = toggle;
                    bi.Info.GetAI().enabled = toggle;
                    //bi.m_flags.SetFlags(Building.Flags.Active, false);

                  WriteLog("Settings after: " + bi.Info.GetAI().enabled);
                }
            }
        }

        protected void DelateLanes()
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
                    //WriteLog("In the for loops ");
                    ushort num5 = NetManager.instance.m_segmentGrid[i * 270 + j];
                    int num6 = 0;
                    bool skip = false;
                    while (num5 != 0u)
                    {
                        //WriteLog("In the while ");
                        var segment = NetManager.instance.m_segments.m_buffer[(int)((UIntPtr)num5)];

                        WriteLog("Segment name: " + segment.Info.name + " Service :" + segment.Info.GetService());
                        Vector3 position = segment.m_middlePosition;
                        float positionDiff = Mathf.Max(Mathf.Max(minX - 16f - position.x, minZ - 16f - position.z), Mathf.Max(position.x - maxX - 16f, position.z - maxZ - 16f));

                        if (positionDiff < 0f)
                        {
                            string seg = segment.Info.name;
                            // we need to handle Bridges (Elevated, Slope), tunnels, railroads, Pipe, Power Lines, 
                            if (deletes[(int)p.Tunnel].isChecked == false && seg.Contains("Tunnel")) { skip = true; WriteLog("Skip Tunnel "); }                               
                            else if (deletes[(int)p.Bridge].isChecked == false && seg.Contains("Elevated")) { skip = true; WriteLog("Skip Elevated "); }
                            else if (deletes[(int)p.Slope].isChecked == false && seg.Contains("Slope")) { skip = true; WriteLog("Skip Slope "); }
                            else if (deletes[(int)p.Roads].isChecked == false && seg.Contains("Road")) { skip = true; WriteLog("Skip Road "); }
                            else if (deletes[(int)p.Railroads].isChecked == false && seg.Contains("Train")) { skip = true; WriteLog("Skip Train "); }
                            else if (deletes[(int)p.Highways].isChecked == false && seg.Contains("Highway")) { skip = true; WriteLog("Skip Highway "); }
                            else if (deletes[(int)p.PowerLines].isChecked == false && seg.Contains("Power")) { skip = true; WriteLog("Skip Power "); }
                            else if (deletes[(int)p.WaterPipes].isChecked == false && seg.Contains("Water Pipe")) { skip = true; WriteLog("Skip Water "); }
                            else if (deletes[(int)p.WaterPipes].isChecked == false && seg.Contains("Heat Pipe")) { skip = true; WriteLog("Skip Heat "); }
                            else if (deletes[(int)p.Airplanes].isChecked == false && seg.Contains("Airplane")) { skip = true; WriteLog("Skip Airplane "); }
                            else if (deletes[(int)p.Shipping].isChecked == false && seg.Contains("Ship")) { skip = true; WriteLog("Skip Ship "); }
                            else if (deletes[(int)p.Ground].isChecked == false &&
                                    (seg.Contains("Tunnel") == false &&
                                    seg.Contains("Slope") == false &&
                                    seg.Contains("Elevated") == false)) { skip = true; WriteLog("Skip Ground, Bridge, Slope or Tunnel");  }


                           WriteLog("Will the segment named, " + segment.Info.name + ", be deleted? That is " + !skip + ".");

                            if (skip == false)
                            {
                                segmentsToDelete.Add(num5);
                            }
                        }
                        num5 = NetManager.instance.m_segments.m_buffer[(int)((UIntPtr)num5)].m_nextGridSegment;
                        if (++num6 >= 262144)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }

            foreach (var segment in segmentsToDelete)
            {
                SimulationManager.instance.AddAction(this.ReleaseSegment(segment));
            }
            NetManager.instance.m_nodesUpdated = true;
        }

        private IEnumerator ReleaseSegment(ushort segment)
        {
            ToolBase.ToolErrors errors = ToolErrors.None;
            if (CheckSegment(segment, ref errors))
            {
                NetManager.instance.ReleaseSegment(segment, false);
            }
            yield return null;
        }

        protected void BulldozeBuildings()
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

        private IEnumerator ReleaseBuilding(ushort building)
        {
            BuildingManager.instance.ReleaseBuilding(building);
            yield return null;
        }

        protected void BulldozeTrees()
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

        protected void BulldozeProps()
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

        private string GetMessage(ToolErrors errors)
        {
            string text = "";
            if (errors == ToolErrors.None) text += "";
            if (errors == ToolErrors.OutOfArea) text += "Out of city limits!";
            else if (errors == ToolErrors.AlreadyExists) text += "Already exists";
            else if (errors == ToolErrors.CannotBuildOnWater) text += "Cannot build on water";
            else if (errors == ToolErrors.InvalidShape) text += "Invalid Shape";
            else if (errors == ToolErrors.NotEnoughMoney) text += "Not enough money";
            else if (errors == ToolErrors.OutOfArea) text += "Out of Area";
            else if (errors == ToolErrors.CannotUpgrade) text += "Cannot upgrade to this type";
            else text += "Ondefined error: " + errors;

            return text;
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

        void GetSegmentControlPoints(int segmentIndex, out NetTool.ControlPoint startPoint, out NetTool.ControlPoint middlePoint, out NetTool.ControlPoint endPoint)
        {
            NetManager net = Singleton<NetManager>.instance;
            startPoint = new NetTool.ControlPoint();
            middlePoint = new NetTool.ControlPoint();
            endPoint = new NetTool.ControlPoint();

            if (segmentIndex >= net.m_segments.m_buffer.Length)
                return;

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
        }

        private void ApplyServices()
        {

        }

        protected void ApplyUpdates()
        {
            //test our absolutes selected area for tiny selections and ignore them
            string xy = Math.Abs(m_startPosition.x - m_mousePosition.x) + " : " + Math.Abs(m_startPosition.y - m_mousePosition.y);
            if (Math.Abs(m_startPosition.x - m_mousePosition.x) < 20)
                {
                if (Math.Abs(m_startPosition.y - m_mousePosition.y) < 20)
                {
                    //WriteLog("Math.Abs(m_startPosition.x - m_mousePosition.x) + '' : '' + Math.Abs(m_startPosition.y - m_mousePosition.y) :" + xy);
                    //WriteLog("(Math.Abs(m_startPosition.y - m_mousePosition.y) < 20 :" + (Math.Abs(m_startPosition.y - m_mousePosition.y) < 20));
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
                ConvertObjects(toSelected, fromSelected, false, out totalCost, out errors);
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected void ApplyDeletes()
        {
            try
            {
                if (deletes[(int)p.Roads].isChecked || deletes[(int)p.Railroads].isChecked || deletes[(int)p.Highways].isChecked || deletes[(int)p.PowerLines].isChecked || deletes[(int)p.WaterPipes].isChecked || deletes[(int)p.HeatPipes].isChecked || deletes[(int)p.Airplanes].isChecked || deletes[(int)p.Shipping].isChecked)
                {
                    DelateLanes();
                }
            }
            catch (Exception)
            {
                throw;
            }

            if (deletes[(int)p.Buildings].isChecked)
                BulldozeBuildings();
            if (deletes[(int)p.Props].isChecked)
                BulldozeProps();
            if (deletes[(int)p.Trees].isChecked)
                BulldozeTrees();
        }

        #endregion

        #region "Logging"

        internal static void WriteLog(string data)
        {
            WriteLog(data, false);
        }

        internal static void WriteLog(string data, bool delete)
        {
            string filename = "AnotherRoadUpdater" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
            if (delete)
            {
                File.Delete(filename);
            }

            // Write the string to a file.
            System.IO.StreamWriter file = File.AppendText(filename);
            file.WriteLine(data);
            file.Close();
        }

        #endregion

        #endregion

    }
}
