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
using System.ComponentModel;
using System.Text.RegularExpressions;

/// <summary>
/// Updated from SkylinesRoadUpdate added global preferences
/// added Additional selections
/// Added new to remove warning for reused m_
/// </summary>
namespace AnotherRoadUpdateTool
{
    public class RoadUpdateTool : DefaultTool
    {
        #region Declarations

        #region enums

        private enum tp
        {
            Ground = 0,
            Bridge = 1,
            Slope = 2,
            Tunnel = 3,
            Curve = 4
        }

        private enum Up
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
            Pedestrian = 8,
            Bicycle = 9,
            Tram = 10,
            Metro = 11,
            Buildings = 12,
            Trees = 13,
            Props = 14
        }

        private enum ops
        {
            Updates = 0,
            Deletes = 1,
            Services = 2,
            Terrain = 3,
            Districts = 4
        }

        private enum dl
        {
            HealthCare = 0,
            PoliceDepartment = 1,
            FireDepartment = 2,
            PublicTransport = 3,
            Education = 4,
            Electricity = 5,
            Water = 6,
            Garbage = 7,
            Beautification = 8,
            Monument = 9,
            Abandoned = 10,
            Burned = 11,
            Chirper = 12
        }

        //None = 0,
        //Residential = 1,
        //Commercial = 2,
        //Industrial = 3,
        //Unused1 = 4,
        //Unused2 = 5,
        //Citizen = 6,
        //Tourism = 7,
        //Office = 8,
        //Road = 9,
        //Electricity = 10,
        //Water = 11,
        //Beautification = 12,
        //Garbage = 13,
        //HealthCare = 14,
        //PoliceDepartment = 15,
        //Education = 16,
        //Monument = 17,
        //FireDepartment = 18,
        //PublicTransport = 19,
        //Government = 20

        public struct UndoStroke
        {
            public string name;
            public int minX;
            public int maxX;
            public int minZ;
            public int maxZ;
            public ushort[] originalHeights;
            public ushort[] backupHeights;
            public ushort[] rawHeights;
            public int pointer;
        }

        #endregion

        #region Variables

        #region Static

        //set to false once code is in production (Why not use "debug mode")
        private static bool logging = true;
        private static bool erroring = true;
        private static bool m_delete = true;
        private static bool m_deletelog = true;

        public readonly static SavedBool DemolishAbandoned = new SavedBool("ModDemolishAbandoned", Settings.gameSettingsFile, true, true);
        public readonly static SavedBool DemolishBurned = new SavedBool("ModDemolishBurned", Settings.gameSettingsFile, true, true);
        
        #endregion

        #region Objects

        UserSettings us = new UserSettings();

        private object m_dataLock = new object();

        private LoadMode mode;
        private Chirper Chirp;
        private UnlockRoads Roads;
        private DestroyMonitor Dozer;
        private MaxAreas Areas;

        #endregion
        
        #region Private

        private bool m_active;
        private bool m_selectable;
        private bool m_fromTypes;
        private bool m_toTypes;
        private bool m_toRoads;
        private bool m_fromRoads;
        private bool m_updates;
        private bool m_settings;

        private string m_available = "available.";
        private string m_unavailable = "unavailable.";
        private string m_updatetool = "Another Road Update Tool - Selection is ";
        private string fromSelected = string.Empty;
        private string toSelected = string.Empty;

        private Vector3 m_startPosition;
        private Vector3 m_endPosition;
        private Vector3 m_startDirection;
        private Vector3 m_mouseDirection;
        private Vector3 m_cameraDirection;
        private new Vector3 m_mousePosition;
        private new bool m_mouseRayValid;
        private new Ray m_mouseRay;
        private new float m_mouseRayLength;

        private float m_terrainHeight = 0.0f;

        private List<ushort> segmentsToDelete;

        readonly ushort[] m_undoBuffer = Singleton<TerrainManager>.instance.UndoBuffer;
        private ushort[] m_originalHeights;
        readonly ushort[] m_backupHeights = Singleton<TerrainManager>.instance.BackupHeights;
        readonly ushort[] m_rawHeights = Singleton<TerrainManager>.instance.RawHeights;

        SavedInputKey m_UndoKey = new SavedInputKey(Settings.mapEditorTerrainUndo, Settings.inputSettingsFile, DefaultSettings.mapEditorTerrainUndo, true);

        private int m_minX = 0;
        private int m_maxX = 0;
        private int m_minZ = 0;
        private int m_maxZ = 0;

        private BindingList<UndoStroke> UndoList;

        private float m_maxArea = 400f;

        private UIPanel plMain;
        private UIPanel plOptions;
        private UIPanel plTypes;
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
        private UIPanel plTerrain;
        private UIPanel plDistricts;

        private UILabel lLines;
        private UILabel lProperties;
        private UILabel lSelectable;
        private UILabel lInformation;

        private UIButton btHelp;
        private UIButton btHide;
        private UIButton mainButton;
        private UICheckBox cbToggle;
        private UICheckBox cbDistrictToggle;
        private UIDropDown ddHeights;
        private UITextField tfTerrainHeight;

        private List<UICheckBox> options = new List<UICheckBox>();
        private List<UICheckBox> deletes = new List<UICheckBox>();
        private List<UICheckBox> services = new List<UICheckBox>();
        private List<UICheckBox> types = new List<UICheckBox>();

        private List<UICheckBox> toTypes = new List<UICheckBox>();
        private List<UICheckBox> fromTypes = new List<UICheckBox>();

        private List<UICheckBox> toBasic = new List<UICheckBox>();
        private List<UICheckBox> fromBasic = new List<UICheckBox>();

        private List<UICheckBox> toLarge = new List<UICheckBox>();
        private List<UICheckBox> fromLarge = new List<UICheckBox>();

        private List<UICheckBox> toHighway = new List<UICheckBox>();
        private List<UICheckBox> fromHighway = new List<UICheckBox>();

        private List<UICheckBox> toMedium = new List<UICheckBox>();
        private List<UICheckBox> fromMedium = new List<UICheckBox>();

        private List<UICheckBox> toOneway = new List<UICheckBox>();
        private List<UICheckBox> fromOneway = new List<UICheckBox>();

        //These strings are importent in that they control the interface
        private string[] m_options = new string[] { "Updates", "Deletes", "Services", "Terrain", "Districts" };
        private string[] m_types = new string[] { "Ground", "Bridge", "Slope", "Tunnel", "Curve" };
        private string[] m_roads = new string[] { "Label ToFrom", "Basic", "Highway", "Large", "Medium", "Oneway" };
        private string[] m_basic = new string[] { "Basic Road", "Basic Road Decoration Grass", "Basic Road Decoration Trees",
            "Basic Road Bicycle", "Basic Road Tram", "Gravel Road" };

        private string[] m_highway = new string[] { "Highway Barrier", "Highway", "HighwayRamp" };

        private string[] m_large = new string[] { "Large Road", "Large Road Decoration Grass", "Large Road Decoration Trees",
            "Large Road Bicycle", "Large Road Bus", "Large Oneway", "Large Oneway Decoration Grass", "Large Oneway Decoration Trees" };

        private string[] m_medium = new string[] { "Medium Road", "Medium Road Decoration Grass", "Medium Road Decoration Trees",
            "Medium Road Bicycle", "Medium Road Bus", "Medium Road Tram" };

        private string[] m_oneway = new string[] { "Oneway Road", "Oneway Road Decoration Grass", "Oneway Road Decoration Trees",
            "Oneway Road Tram", "Tram Track", "Oneway Tram Track" };

        private string[] m_deletes = new string[] { "Label Lines", "Roads", "B Railroads", "Highways", "B PowerLines", "Water Pipes",
            "B Heat Pipes", "Airplanes", "B Shipping", "Pedestrian", "B Bicycle", "Tram", "B Metro", "Label Properties", "Buildings",
            "Trees", "Props" };

        private string[] m_services = new string[] {"Label Select Toggle Services", "HealthCare", "PoliceDepartment", "FireDepartment",
            "PublicTransport", "Education", "Electricity", "Water", "Garbage", "Beautification", "Monument", "Select Options",
            "Auto Bulldozer",  "Chirper" };

        private string[] m_heights = new string[] { "0.00", "5.00", "10.00", "15.00", "20.00", "25.00", "30.00", "35.00", "40.00",
            "45.00", "50.00", "55.00", "60.00", "65.00", "70.00", "75.00", "80.00", "85.00", "90.00", "95.00", "100.00", "150.00",
            "200.00", "250.00", "300.00", "350.00", "400.00", "450.00", "500.00", "550.00", "600.00", "650.00", "700.00", "750.00",
            "800.00", "850.00", "900.00", "950.00", "1000.00", "1500.00", "2000.00" };
        
        #endregion

        #endregion

        #endregion

        #region "Public Procedures"

        #region "Minor"

        protected override void Awake()
        {
            WriteLog("ARUT awake!");
            m_active = false;
            base.Awake();
        }

        protected override void OnToolUpdate()
        {
            //WriteLog("ARUT OnToolUpdate!");
            base.OnToolUpdate();
        }

        protected override void OnEnable()
        {
            UIView.GetAView().FindUIComponent<UITabstrip>("MainToolstrip").selectedIndex = -1;

            //setting up our backup
            m_minX = 0;
            m_maxX = 0;
            m_minZ = 0;
            m_maxZ = 0;
            m_originalHeights = new ushort[m_rawHeights.Length];

            for (int i = 0; i <= 1080; i++)
            {
                for (int j = 0; j <= 1080; j++)
                {
                    int num = i * 1081 + j;
                    m_backupHeights[num] = m_rawHeights[num];
                    m_originalHeights[num] = m_rawHeights[num];
                }
            }

            UndoList = new BindingList<UndoStroke>();
            Dozer = new DestroyMonitor();

            WriteLog("ARUT OnEnable!");
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            if (plMain != null)
                plMain.isVisible = false;

            WriteLog("ARUT OnDisable!");
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
            Event current = Event.current;

            if (!m_active && m_UndoKey.IsPressed(current) && UndoList.Count() >= 0)
            {
                ApplyUndo();
            }
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Escape)
                {
                    Event.current.Equals(null);
                    //e.keyCode = KeyCode.Asterisk;
                    this.enabled = false;
                    plMain.isVisible = false;
                }
            }

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
                    if (options[(int)ops.Updates].isChecked && plTypes.isVisible)
                    {
                        //WriteLog("Trying ApplyUpdates mouseXY: " + m_mousePosition);
                        //handle Updates
                        ApplyUpdates();
                        //WriteLog("Tried ApplyUpdates");
                    }
                    else if (options[(int)ops.Deletes].isChecked && plDelete.isVisible)
                    {
                        //WriteLog("Trying ApplyDeletes");
                        //handle Updates
                        ApplyDeletes();
                        //WriteLog("Tried ApplyDeletes");
                    }
                    else if (options[(int)ops.Services].isChecked && plServices.isVisible)
                    {
                        //WriteLog("Trying ApplyServices");
                        if (mode != LoadMode.LoadMap || mode != LoadMode.NewMap)
                            ApplyServices();
                        //WriteLog("Tried ApplyServices");
                    }
                    else if (options[(int)ops.Districts].isChecked && plDistricts.isVisible)
                    {
                        WriteLog("Trying ApplyDistrictsChange");
                        ApplyDistrictsChange();
                        WriteLog("Tried ApplyDistrictsChange");
                    }
                    else if (options[(int)ops.Terrain].isChecked && plTerrain.isVisible)
                    {
                        //WriteLog("Trying ApplyTerrainChange");
                        this.m_endPosition = this.m_mousePosition;
                        ////Handle Services on/off or Terrian Updates (Map mode)
                        if (mode == LoadMode.LoadMap || mode == LoadMode.NewMap)
                            ApplyTerrainChange();
                        //WriteLog("Tried ApplyDistrictsChange");
                    }
                    m_active = false;
                }
            }
        }

        protected override void OnToolLateUpdate()
        {
            //WriteLog("ARUT OnToolLateUpdate!");
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

        public void InitGui(LoadMode _mode, Chirper chirp, UnlockRoads roads)
        {
            //WriteLog("Entering InitGUI");
            mode = _mode;
            //store a local copy of each mod
            Chirp = chirp;
            Roads = roads;

            mainButton = UIView.GetAView().FindUIComponent<UIButton>("MarqueeBulldozer");

            if (mainButton == null)
            {
                var RoadUpdateButton = UIView.GetAView().FindUIComponent<UIMultiStateButton>("BulldozerButton");

                mainButton = RoadUpdateButton.parent.AddUIComponent<UIButton>();
                mainButton.name = "AnotherRoadUpdateTool";
                mainButton.size = new Vector2(40, 40);
                mainButton.tooltip = "Another Road Update tool";
                mainButton.relativePosition = new Vector2
                (
                    RoadUpdateButton.relativePosition.x + RoadUpdateButton.width / 2.0f - (mainButton.width * 2) - RoadUpdateButton.width,
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
                plMain.name = "AnotherRoadUpdateTool";
                //Create the panels (Little like a tab view)
                int height = CreatePanels(plMain);

                plMain.size = new Vector2(575, height);

                string tooltip = "I will try to open a pdf file, if you do not have a viewer.... do not click.";
                btHelp = addButton(plMain, "¿", tooltip, 765, 260, 25, 25);
                btHelp.isVisible = true;
                //btHelp.zOrder = 0;
                btHelp.eventDoubleClick += btHelp_eventDoubleClick;

                tooltip = "I will try to close the option panel.";
                btHide = addButton(plMain, "Close", tooltip, 765, -285, 75, 25);
                btHide.isVisible = true;
                //btHide.zOrder = 0;
                btHide.eventClick += btHide_eventClick;

                plMain.relativePosition = new Vector2
                (
                    RoadUpdateButton.relativePosition.x + RoadUpdateButton.width / 2.0f - plMain.width,
                    RoadUpdateButton.relativePosition.y - (plMain.height + 3)
                );

                //We can load the users last session
                GetSettings(true);

                //About to set unlockable tiles
                Areas = new MaxAreas();
                //WriteLog("Leaving InitGUI");
            }
        }

        #endregion

        #region "Private procedures"

        #region "GUI Layout"

        private int CreatePanels(UIPanel panel)
        {
            //WriteLog("Entering CreatePanels");
            int plx = 1;
            int ply = 1;
            int srv = 75;
            int del = 75;
            int ter = 75;
            int typ = 100;

            srv = GenerateOptions(panel, ply, plx);

            if (mode == LoadMode.LoadMap || mode == LoadMode.NewMap)
            {
                options[(int)ops.Services].Disable();
                options[(int)ops.Terrain].Enable();
            }
            else
            {
                options[(int)ops.Services].Enable();
                options[(int)ops.Terrain].Disable();
            }

            typ = GenerateplTypes(panel, srv, plx);
            del = srv + 40;
            ter = (int)plOptions.height;
            //WriteLog("typ = " +  typ + " srv = " +  srv + " del = " +  del);

            typ = GenerateplRoads(panel, typ, plx); //Updates
            GenerateplDelete(panel, del, plx);      //Deletes
            GenerateplServices(panel, srv, plx);    //Services
            GenerateplTerrain(panel, ter, plx);     //Terrain
            GenerateplDistricts(panel, ter, plx);     //Districts

            GenerateRoadPanels(panel, ref plBasic, ref plToBasic, fromBasic, toBasic, m_basic, "Basic", typ, plx);
            GenerateRoadPanels(panel, ref plHighway, ref plToHighway, fromHighway, toHighway, m_highway, "Highway", typ, plx);
            GenerateRoadPanels(panel, ref plLarge, ref plToLarge, fromLarge, toLarge, m_large, "Large", typ, plx);
            GenerateRoadPanels(panel, ref plMedium, ref plToMedium, fromMedium, toMedium, m_medium, "Medium", typ, plx);
            GenerateRoadPanels(panel, ref plOneway, ref plToOneway, fromOneway, toOneway, m_oneway, "Oneway", typ, plx);
            
            //WriteLog("Leaving CreatePanels");
            //three rows in options, five in Road Types, 15 in Large roads
            return 25 * (3 + 6 + 12);
        }

        private int GenerateOptions(UIPanel panel, int ply, int plx)
        {
            //WriteLog("Entering GenerateOptions");
            //Show the road type option
            plOptions = panel.AddUIComponent<UIPanel>();
            plOptions.relativePosition = new Vector3(1, 1);
            plOptions.isVisible = true;
            plOptions.tooltip = "Select the type of updates to perform.";

            //This was the title, indent the rest
            lSelectable = addLabel(plOptions, ply, plx, m_updatetool + m_unavailable, true);

            int cb = 0;
            foreach (string s in m_options)
            {
                string t = String.Format("Select to display the {0} options", s);
                options.Add(addCheckbox(plOptions, ply + 20, plx, s, t, true));
                //Space out the options (We may ad building, tress, and props)
                plx += 100;
                options[cb].eventCheckChanged += Options_eventCheckChanged;
                cb += 1;
            }

            lInformation = addLabel(plOptions,  ply + 45, 1, "Details from changes.", true);

            //set the panal size (two rows, 50)
            plOptions.size = new Vector2(panel.width, ply + 65);

            //return the top of the Road Types and Services panels
            //WriteLog("Leaving GenerateOptions");
            return (int)plOptions.height;
        }

        private int GenerateplTypes(UIPanel panel, int ply, int plx)
        {
            //WriteLog("Entering GenerateplTypes");
            //Show the road type option
            plTypes = panel.AddUIComponent<UIPanel>();
            plTypes.relativePosition = new Vector3(1, ply);
            plTypes.isVisible = true;
            plTypes.tooltip = "Select the line types to modify";

            addLabel(plTypes, 1, 1, "Select the line types to modify", true);
            int x = 5;
            int y = 20;
            int step = 0;
            int cb = 0;

            //load the update types
            foreach (string s in m_types)
            {
                string t = "These types will be updated.";
                types.Add(addCheckbox(plTypes, y, x + step, s, t, true));
                types[cb].eventCheckChanged += Types_eventCheckChanged;
                step += 100;
                cb += 1;
            }
            //set the panel size (two rows, 50)
            plTypes.size = new Vector2(panel.width, 50);
            //return the top of the Road Types, Services, and Deletes panels (add to plOptions (50 + 50))
            return ply + (int)plTypes.height;
        }

        private int GenerateplRoads(UIPanel panel, int ply, int plx)
        {
            //WriteLog("Entering GenerateplTypes");
            //Show the road type option
            plRoads = panel.AddUIComponent<UIPanel>();
            plRoads.relativePosition = new Vector3(1, ply);
            plRoads.isVisible = false;
            plRoads.tooltip = "Select the Road types to convert from and to";

            int x1 = 10;
            int x2 = 280;
            int y = 1;
            int cb = 0;
            int step = 0;
            //load the update types but hide them
            foreach (string s in m_roads)
            {
                if (s.StartsWith("Label "))
                {
                    try
                    {
                        addLabel(plRoads, y, x1 - 5, "From ==> ", true);
                        addLabel(plRoads, y, x2 - 5, "To ==> ", true);
                        y += 20;
                    }
                    catch (Exception ex)
                    {
                        WriteError("Error in GenerateplTypes: ", ex);
                    }
                }
                else
                {
                    string t = "";
                    if (s == "Ground" || s == "Bridge" || s == "Tunnel" || s == "Slope" || s == "Curve")
                    {
                        t = "These types will be updated.";
                        deletes.Add(addCheckbox(plDelete, y, x1 + step, s, t, true));
                        step += 110;
                        if (s == "Curve")
                            y += 25;
                    }
                    else
                    {
                        t = String.Format("If checked {0} sections will be displayed.", s);
                        fromTypes.Add(addCheckbox(plRoads, y, x1, s, t, true));
                        toTypes.Add(addCheckbox(plRoads, y, x2, s, t, true));
                        fromTypes[cb].eventCheckChanged += FromTypes_eventCheckChanged;
                        toTypes[cb].eventCheckChanged += ToTypes_eventCheckChanged;
                    }
                    y += 25;
                    cb++;
                }
            }
            plRoads.size = new Vector2(panel.width, 200);
            //WriteLog("Leaving GenerateplTypes");
            //return the top of the roads panals
            return ply + y;
        }

        private void GenerateplDelete(UIPanel panel, int ply, int plx)
        {
            //WriteLog("Entering GenerateplDelete");
            //Show the road type option
            plDelete = panel.AddUIComponent<UIPanel>();
            plDelete.relativePosition = new Vector3(1, ply);
            plDelete.isVisible = false;
            plDelete.tooltip = "Select the type of items to delete.";

            int cb = 0;
            int x = 15;
            int y = 6;

            //load the bulldoze road type options
            addLabel(plDelete, y - 5, 5, "Select your delete options.", true);

            y += 15;
            foreach (string s in m_deletes)
            {
                if (s == "Label Lines")
                {
                    lLines = addLabel(plDelete, y, 10, "Lines", true);
                    y += 20;
                }
                else if (s == "Label Properties")
                {
                    lProperties = addLabel(plDelete, y, 10, "Properties", true);
                    y += 20;
                }
                else
                {
                    string t = "These items will be deleted.";
                    if (s.StartsWith("B "))
                    {
                        deletes.Add(addCheckbox(plDelete, y, x + 210, s.Replace("B ", ""), t, true));
                        y += 25;
                    }
                    else
                    {
                        deletes.Add(addCheckbox(plDelete, y, x, s, t, true));
                        if (s == "Buildings" || s == "Trees" || s == "Props") { y += 25; }
                    }
                    deletes[cb].eventCheckChanged += DeleteTypes_eventCheckChanged;
                    cb += 1;
                }
            }

            plDelete.size = new Vector2(panel.width, y + 25);
            //WriteLog("Leaving GenerateplDelete: " + deletes.Count);
        }

        private void GenerateplServices(UIPanel panel, int ply, int plx)
        {
            //WriteLog("Entering GenerateplServices");
            //Show the road type option
            plServices = panel.AddUIComponent<UIPanel>();
            plServices.relativePosition = new Vector3(1, ply);
            plServices.isVisible = false;
            plServices.tooltip = "Select the type of services to toggle on and off.";

            int cb = 0;
            int y = 1;

            for ( int i = 0; i < m_services.Length; i++)
            {
                string s = m_services[i];

                string t = "These items will be toggled On or Off.";
                
                if (s == "Auto Bulldozer")
                {
                    addLabel(plServices, y, 10, s, true);
                    y += 25;
                    services.Add(addCheckbox(plServices, y, 15, "Abandoned", "Check to delete abandoned buildings.", true));
                    services[cb].eventCheckChanged += ServiceTypes_eventCheckChanged;
                    cb += 1;
                    services.Add(addCheckbox(plServices, y, 150, "Burned", "Check to delete burned buildings.", true));
                    services[cb].eventCheckChanged += ServiceTypes_eventCheckChanged;
                    cb += 1;
                }
                else if (s == "Select Options")
                {
                    addLabel(plServices, y, 5, s, true);
                }
                else if (s == "Chirper")
                {
                    addLabel(plServices, y, 10, s, true);
                    y += 25;
                    services.Add(addCheckbox(plServices, y, 15, s, "Checked to Show, unchecked to Hide Chirper.", true));
                    services[cb].eventCheckChanged += ServiceTypes_eventCheckChanged;
                    cb += 1;
                }
                else if (s.StartsWith("Label "))
                {
                    addLabel(plServices, y, 5, s.Replace("Label ", ""), true);
                    t = "Check to turn on services, unchecked to turn them off.";
                    y += 25;
                    cbToggle = addCheckbox(plServices, y, 10, "Check to turn Sevices On, Uncheck for Off.", t, true);
                }
                else
                {
                    s = m_services[i];
                    services.Add(addCheckbox(plServices, y, 15, s, t, true));
                    services[cb].eventCheckChanged += ServiceTypes_eventCheckChanged;
                    i++;
                    cb += 1;
                    if (i < m_services.Length)
                    {
                        s = m_services[i];
                        services.Add(addCheckbox(plServices, y, 285, s, t, true));
                        services[cb].eventCheckChanged += ServiceTypes_eventCheckChanged;
                        cb += 1;
                    }
                }
                //WriteLog("Value and Index: " + s + ", " + cb);
                y += 25;
            }
            //no need for this dropdown
            //ServiceDropdown(plServices);
            plServices.size = new Vector2(panel.width, y + 25);
            //WriteLog("Leaving GenerateplServices");
        }

        private void GenerateplTerrain(UIPanel panel, int ply, int plx)
        {
            //WriteLog("Entering GenerateplTerrain");
            
            //Show the road type option
            plTerrain = panel.AddUIComponent<UIPanel>();
            plTerrain.relativePosition = new Vector3(1, ply);
            plTerrain.isVisible = false;
            plTerrain.tooltip = "Select or enter the height desired.";

            addLabel(plTerrain, 1, 1, "Enter a value into the text box to set the height", true);

            tfTerrainHeight = addTextBox(plTerrain, "TerrainHeight", "0,00", 20, 1, 120, 25, "Use values between 2000 and 0.0", true, true);
            tfTerrainHeight.eventTextChanged += TerrainHeight_eventTextChanged;

            //Add the dropdown
            ddHeights = addDropDown(plTerrain, 45, 1, 120, 25, "", "Select default values for Terrain Height changes.", 350);

            foreach (string vl in m_heights)
            {
                ddHeights.AddItem(vl);
            }
            ddHeights.selectedIndex = 0;
            ddHeights.eventSelectedIndexChanged += ddHeights_eventSelectedIndexChanged;

            //WriteLog("Leaving GenerateplTerrain");
        }

        private void GenerateplDistricts(UIPanel panel, int ply, int plx)
        {
            //WriteLog("Entering GenerateplDistricts");

            //Show the road type option
            plDistricts = panel.AddUIComponent<UIPanel>();
            plDistricts.relativePosition = new Vector3(1, ply);
            plDistricts.isVisible = false;
            plDistricts.tooltip = "Select an area to update the district.";

            addLabel(plDistricts, 1, 1, "Toggle the check box to create or delete the district. (Undo is not available)", true);
            addLabel(plDistricts, 20, 1, "Add to an existing District by starting the selection within the district.", true);

            cbDistrictToggle = addCheckbox(plDistricts, 40, 15, "Checked for Create (Update), unchecked for delete", "Check to create a district, uncheck to delete", true);

           // WriteLog("Leaving GenerateplDistricts");
        }

        private void GenerateRoadPanels(UIPanel panel, ref UIPanel pFrom, ref UIPanel pTo, List<UICheckBox> lFrom, List<UICheckBox> lTo, string[] road, string name, int ply, int plx)
        {
            //WriteLog("Entering GenerateRoadPanels");
            //Show the road from option
            pFrom = panel.AddUIComponent<UIPanel>();
            pFrom.relativePosition = new Vector3(1, ply);
            pFrom.size = new Vector2(25 * 12, panel.width / 2);
            pFrom.isVisible = false;
            pFrom.tooltip = "Select the type of road to convert.";
            //Show the road to option
            pTo = panel.AddUIComponent<UIPanel>();
            pTo.relativePosition = new Vector3(285, ply);
            pTo.size = new Vector2(25 * 12, panel.width / 2);
            pTo.isVisible = false;
            pTo.tooltip = "Select the type of road to convert to.";

            addLabel(pFrom, 1, 5, pFrom.tooltip, true);
            addLabel(pTo, 1, 5, pTo.tooltip, true);

            int cb = 0;
            int y = 22;
            //load the update roads but hide them
            foreach (string s in road)
            {
                string t = String.Format("If checked {0} sections will be converted.", s);
                lFrom.Add(addCheckbox(pFrom, y, 11, s, t, true));
                t = String.Format("If checked sections will convert to {0}.", s);
                lTo.Add(addCheckbox(pTo, y, 11, s, t, true));
                lFrom[cb].eventCheckChanged += FromRoad_eventCheckChanged;
                lTo[cb].eventCheckChanged += ToRoad_eventCheckChanged;
                cb++;
                y += 25;
            }
            //WriteLog("Leaving GenerateRoadPanels");
        }


        #region "Adding Controls"

        private UILabel addLabel(UIPanel panel, int yPos, int xPos, string text, bool hidden)
        {
            //WriteLog("Entering addUILabel");
            UILabel lb = panel.AddUIComponent<UILabel>();
            lb.relativePosition = new Vector3(xPos, yPos);
            lb.height = 0;
            lb.width = 80;
            lb.text = text;
            lb.isVisible = hidden;
            //WriteLog("Leaving addUILabel");
            return lb;
        }

        private UICheckBox addCheckbox(UIPanel panel, int yPos, int xPos, string text, string tooltip, bool hidden)
        {
            //WriteLog("Entering addCheckbox");
            var cb = panel.AddUIComponent<UICheckBox>();
            cb.relativePosition = new Vector3(xPos, yPos);
            cb.height = 0;
            cb.width = 80;

            var label = panel.AddUIComponent<UILabel>();
            label.relativePosition = new Vector3(xPos + 25, yPos + 3);
            cb.label = label;
            cb.label.tabIndex = cb.GetInstanceID();
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
            cb.disabledColor = new Color(127.0f / 255.0f, 127.0f / 255.0f, 127.0f / 255.0f, 1.0f);
            label.disabledColor = new Color(127.0f / 255.0f, 127.0f / 255.0f, 127.0f / 255.0f, 1.0f);
            cb.tooltip = tooltip;
            label.tooltip = cb.tooltip;
            cb.isChecked = false;
            cb.label.isVisible = hidden;
            cb.isVisible = hidden;

            //WriteLog("Leaving addCheckbox");
            return cb;
        }

        private UIDropDown addDropDown(UIPanel panel, int y, int x, int w, int h, string text, string tooltip, int lh)
        {
            UIDropDown dd = panel.AddUIComponent<UIDropDown>();

            dd.size = new Vector2(w, h);
            dd.relativePosition = new Vector3(x, y);
            dd.listBackground = "GenericPanelLight";
            dd.itemHeight = 32;
            dd.itemHover = "ListItemHover";
            dd.itemHighlight = "ListItemHighlight";
            dd.normalBgSprite = "ButtonMenu";
            dd.listWidth = x;
            dd.listHeight = lh;
            dd.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
            dd.popupColor = new Color32(45, 52, 61, 255);
            dd.popupTextColor = new Color32(170, 170, 170, 255);
            dd.zOrder = 1;
            dd.textScale = 0.8f;
            dd.verticalAlignment = UIVerticalAlignment.Middle;
            dd.horizontalAlignment = UIHorizontalAlignment.Left;
            dd.selectedIndex = 0;
            dd.textFieldPadding = new RectOffset(8, 0, 8, 0);
            dd.itemPadding = new RectOffset(14, 0, 0, 0);

            var dropdownButton = dd.AddUIComponent<UIButton>();
            dd.triggerButton = dropdownButton;

            dropdownButton.text = "";
            dropdownButton.size = dd.size;
            dropdownButton.relativePosition = new Vector3(0.0f, 0.0f);
            dropdownButton.textVerticalAlignment = UIVerticalAlignment.Middle;
            dropdownButton.textHorizontalAlignment = UIHorizontalAlignment.Left;
            dropdownButton.normalFgSprite = "IconDownArrow";
            dropdownButton.hoveredFgSprite = "IconDownArrowHovered";
            dropdownButton.pressedFgSprite = "IconDownArrowPressed";
            dropdownButton.focusedFgSprite = "IconDownArrowFocused";
            dropdownButton.disabledFgSprite = "IconDownArrowDisabled";
            dropdownButton.foregroundSpriteMode = UIForegroundSpriteMode.Fill;
            dropdownButton.horizontalAlignment = UIHorizontalAlignment.Right;
            dropdownButton.verticalAlignment = UIVerticalAlignment.Middle;
            dropdownButton.zOrder = 0;
            dropdownButton.textScale = 0.8f;
            return dd;
        }

        private UITextField addTextBox(UIPanel panel, string name, string text, int y, int x, int width, int height, string tooltip, bool numeric, bool allowFloats)
        {
            UITextField tf = panel.AddUIComponent<UITextField>();

            tf.relativePosition = new Vector3(x, y);
            tf.size = new Vector3(width, height);
            tf.name = name;
            tf.text = text;
            tf.width = width;
            tf.height = height;
            tf.tooltip = tooltip;
            tf.numericalOnly = numeric;
            tf.allowFloats = allowFloats;
            tf.textScale = 0.8f;
            tf.color = Color.black;
            tf.cursorBlinkTime = 0.45f;
            tf.cursorWidth = 1;
            tf.horizontalAlignment = UIHorizontalAlignment.Left;
            tf.selectionBackgroundColor = new Color(233, 201, 148, 255);
            tf.selectionSprite = "EmptySprite";
            tf.verticalAlignment = UIVerticalAlignment.Middle;
            tf.padding = new RectOffset(5, 0, 5, 0);
            tf.foregroundSpriteMode = UIForegroundSpriteMode.Fill;
            tf.normalBgSprite = "TextFieldPanel";
            tf.hoveredBgSprite = "TextFieldPanelHovered";
            tf.focusedBgSprite = "TextFieldPanel";
            tf.isInteractive = true;
            tf.enabled = true;
            tf.readOnly = false;
            tf.builtinKeyNavigation = true;

            return tf;
        }
        
        private UIButton addButton(UIPanel panel, string text, string tooltip, int y, int x, int w, int h)
        {
            UIButton bt = panel.AddUIComponent<UIButton>();
            bt.relativePosition += new Vector3(x, y);
            bt.name = text.Replace(" ", "_");
            bt.text = text;
            bt.tooltip = tooltip;
            bt.textScale = 0.8f;
            bt.width = w;
            bt.height = h;
            bt.normalBgSprite = "ButtonMenu";
            bt.disabledBgSprite = "ButtonMenuDisabled";
            bt.hoveredBgSprite = "ButtonMenuHovered";
            bt.focusedBgSprite = "ButtonMenu";
            bt.pressedBgSprite = "ButtonMenuPressed";
            bt.textColor = new Color32(255, 255, 255, 255);
            bt.disabledTextColor = new Color32(7, 7, 7, 255);
            bt.hoveredTextColor = new Color32(255, 255, 255, 255);
            bt.focusedTextColor = new Color32(255, 255, 255, 255);
            bt.pressedTextColor = new Color32(30, 30, 44, 255);

            //WriteLog("Leaving addButton: " + text + " location: " + bt.relativePosition + "main HeightxWidth: " + panel.height + "x" + panel.width);
            return bt;
        }

        #endregion

        #endregion

        #region "Settings"

        private void SetSettings()
        {
            if (m_settings == true) { return; }
            m_settings = true;

            //WriteLog("Leaving SetSettings types[(int)tp.Ground].isChecked: " + types[(int)tp.Ground].isChecked);

            us.ToOneway = toTypes[(int)Up.Oneway].isChecked;
            us.ToMedium = toTypes[(int)Up.Medium].isChecked;
            us.ToHighway = toTypes[(int)Up.Highway].isChecked;
            us.ToLarge = toTypes[(int)Up.Large].isChecked;
            us.ToBasic = toTypes[(int)Up.Basic].isChecked;
            us.Oneway = fromTypes[(int)Up.Oneway].isChecked;
            us.Medium = fromTypes[(int)Up.Medium].isChecked;
            us.Highway = fromTypes[(int)Up.Highway].isChecked;
            us.Large = fromTypes[(int)Up.Large].isChecked;
            us.Basic = fromTypes[(int)Up.Basic].isChecked;

            us.Roads = deletes[(int)p.Roads].isChecked;
            us.Railroads = deletes[(int)p.Railroads].isChecked;
            us.Highways = deletes[(int)p.Highways].isChecked;
            us.PowerLines = deletes[(int)p.PowerLines].isChecked;
            us.WaterPipes = deletes[(int)p.WaterPipes].isChecked;
            us.HeatPipes = deletes[(int)p.HeatPipes].isChecked;
            us.Airplanes = deletes[(int)p.Airplanes].isChecked;
            us.Shipping = deletes[(int)p.Shipping].isChecked;
            us.Pedestrian = deletes[(int)p.Pedestrian].isChecked;
            us.Bicycle = deletes[(int)p.Bicycle].isChecked;
            us.Tram = deletes[(int)p.Tram].isChecked;
            us.Metro = deletes[(int)p.Metro].isChecked;

            us.Ground = types[(int)tp.Ground].isChecked;
            us.Bridge = types[(int)tp.Bridge].isChecked;
            us.Slope = types[(int)tp.Slope].isChecked;
            us.Tunnel = types[(int)tp.Tunnel].isChecked;
            us.Curve = types[(int)tp.Curve].isChecked;

            us.Buildings = deletes[(int)p.Buildings].isChecked;
            us.Trees = deletes[(int)p.Trees].isChecked;
            us.Props = deletes[(int)p.Props].isChecked;

            us.Update = options[(int)ops.Updates].isChecked;
            us.Delete = options[(int)ops.Deletes].isChecked;
            us.Services = options[(int)ops.Services].isChecked;
            us.Terrain = options[(int)ops.Terrain].isChecked;
            us.Toggle = cbToggle.isChecked;
            us.Districts = options[(int)ops.Districts].isChecked;
            us.DistrictToggle = cbDistrictToggle.isChecked;
            us.TerrainHeight = m_terrainHeight;

            us.HealthCare = services[(int)dl.HealthCare].isChecked;
            us.PoliceDepartment = services[(int)dl.PoliceDepartment].isChecked;
            us.FireDepartment = services[(int)dl.FireDepartment].isChecked;
            us.PublicTransport = services[(int)dl.PublicTransport].isChecked;
            us.Education = services[(int)dl.Education].isChecked;
            us.Electricity = services[(int)dl.Electricity].isChecked;
            us.Water = services[(int)dl.Water].isChecked;
            us.Garbage = services[(int)dl.Garbage].isChecked;
            us.Beautification = services[(int)dl.Beautification].isChecked;
            us.Monument = services[(int)dl.Monument].isChecked;
            us.Abandoned = services[(int)dl.Abandoned].isChecked;
            us.Burned = services[(int)dl.Burned].isChecked;

            Properties.Settings.Default.Chirper = services[(int)dl.Chirper].isChecked;

          //WriteLog("On Exit us.Abandoned & us.Burned are: " + us.Abandoned + " & " + us.Burned);

            us.Save();

            //WriteLog("Leaving SetSettings types[(int)tp.Ground].isChecked: " + types[(int)tp.Ground].isChecked);
            m_settings = false;
        }

        private void GetSettings(bool toBasic)
        {
            if (m_settings == true) { return; }
            m_settings = true;

            string loc = "Starting";

            //WriteLog("Entering GetSettings types[(int)tp.Ground].isChecked: " + types[(int)tp.Ground].isChecked);

            try
            {
                toTypes[(int)Up.Oneway].isChecked = us.ToOneway;
                toTypes[(int)Up.Medium].isChecked = us.ToMedium;
                toTypes[(int)Up.Highway].isChecked = us.ToHighway;
                toTypes[(int)Up.Large].isChecked = us.ToLarge;
                toTypes[(int)Up.Basic].isChecked = us.ToBasic;
                fromTypes[(int)Up.Oneway].isChecked = us.Oneway;
                fromTypes[(int)Up.Medium].isChecked = us.Medium;
                fromTypes[(int)Up.Highway].isChecked = us.Highway;
                fromTypes[(int)Up.Large].isChecked = us.Large;
                fromTypes[(int)Up.Basic].isChecked = us.Basic;

                deletes[(int)p.Roads].isChecked = us.Roads;
                deletes[(int)p.Railroads].isChecked = us.Railroads;
                deletes[(int)p.Highways].isChecked = us.Highways;
                deletes[(int)p.PowerLines].isChecked = us.PowerLines;
                deletes[(int)p.WaterPipes].isChecked = us.WaterPipes;
                deletes[(int)p.HeatPipes].isChecked = us.HeatPipes;
                deletes[(int)p.Airplanes].isChecked = us.Airplanes;
                deletes[(int)p.Shipping].isChecked = us.Shipping;
                deletes[(int)p.Pedestrian].isChecked = us.Pedestrian;
                deletes[(int)p.Bicycle].isChecked = us.Bicycle;
                deletes[(int)p.Tram].isChecked = us.Tram;
                deletes[(int)p.Metro].isChecked = us.Metro;

                types[(int)tp.Ground].isChecked = us.Ground;
                types[(int)tp.Bridge].isChecked = us.Bridge;
                types[(int)tp.Slope].isChecked = us.Slope;
                types[(int)tp.Tunnel].isChecked = us.Tunnel;
                types[(int)tp.Curve].isChecked = us.Curve;

                deletes[(int)p.Buildings].isChecked = us.Buildings;
                deletes[(int)p.Trees].isChecked = us.Trees;
                deletes[(int)p.Props].isChecked = us.Props;

                if (mode == LoadMode.LoadMap || mode == LoadMode.NewMap)
                {
                    if (us.Services == true)
                    {
                        us.Terrain = true;
                        us.Services = false;
                    }
                }
                else
                {
                    if (us.Terrain == true)
                    {
                        us.Terrain = false;
                        us.Services = true;
                    }
                }

                loc  = "Services";
                options[(int)ops.Services].isChecked = us.Services;
                loc = "Deletes";
                options[(int)ops.Deletes].isChecked = us.Delete;
                loc = "Updates";
                options[(int)ops.Updates].isChecked = us.Update;
                loc = "Terrain";
                options[(int)ops.Terrain].isChecked = us.Terrain;
                loc = "Toggle";
                cbToggle.isChecked = us.Toggle;
                options[(int)ops.Districts].isChecked = us.Districts;
                cbDistrictToggle.isChecked = us.DistrictToggle;
                loc = "TerrainHeight";
                m_terrainHeight = (float)us.TerrainHeight;

                services[(int)dl.HealthCare].isChecked = us.HealthCare;
                services[(int)dl.PoliceDepartment].isChecked = us.PoliceDepartment;
                services[(int)dl.FireDepartment].isChecked = us.FireDepartment;
                services[(int)dl.PublicTransport].isChecked = us.PublicTransport;
                services[(int)dl.Education].isChecked = us.Education;
                services[(int)dl.Electricity].isChecked = us.Electricity;
                services[(int)dl.Water].isChecked = us.Water;
                services[(int)dl.Garbage].isChecked = us.Garbage;
                services[(int)dl.Beautification].isChecked = us.Beautification;
                services[(int)dl.Monument].isChecked = us.Monument;
                services[(int)dl.Abandoned].isChecked = us.Abandoned;
                services[(int)dl.Burned].isChecked = us.Burned;

                services[(int)dl.Chirper].isChecked = Properties.Settings.Default.Chirper;

              //WriteLog("On entering us.Abandoned & us.Burned are: " + us.Abandoned + " & " + us.Burned);

                //we need to toggle shown or not
                Chirp.Toggle(services[(int)dl.Chirper].isChecked);
            
                try
                {
                    ZoneTool zt = Singleton<ZoneTool>.instance;
                  //WriteLog("About to try setting Zone Tool Mode to Select. zt.mode: " + zt.m_mode);
                    zt.m_mode = ZoneTool.Mode.Select;
                  //WriteLog("Value  of Zone Tool Mode. zt.m_mode: " + zt.m_mode);
                }
                catch (Exception ex)
                {
                    WriteError("Error setting ZoneTool Mode to Select.", ex);
                }

            }
            catch (Exception ex)
            {
                WriteError("Error in GetSettings loc: " + loc + " deltypes.Count: " + deletes.Count, ex);
            }
            //WriteLog("Leaving GetSettings types[(int)tp.Ground].isChecked: " + types[(int)tp.Ground].isChecked);
            m_settings = false;
        }

        #endregion

        #region "Event Process helpers"

        private void SetServicesEnabled()
        {
            //WriteLog("Entering: SetServicesEnabled m_selectable: " + m_selectable);

            try
            {
                m_selectable = false;
                lSelectable.text = m_updatetool + m_unavailable;

                //If we are not in a city mode, exit
                if (mode == LoadMode.LoadMap || mode == LoadMode.NewMap)
                {
                    plServices.Disable();
                    return;
                }

                //do we have at least one service type selected
                if (services.Any(o => o.isChecked == true)) { }
                else
                    return;

                //ok all checks complete
                plServices.Enable();
                m_selectable = true;
                lSelectable.text = m_updatetool + m_available;
            }
            catch (Exception ex)
            {
                WriteError("Error in SetServicesEnabled Exception: ", ex);
            }

            //WriteLog("Leaving: SetServicesEnabled m_selectable: " + m_selectable);
        }

        private void SetTerrainEnabled()
        {
            //WriteLog("Entering: SetTerrainEnabled m_selectable: " + m_selectable);

            try
            {
                if (mode == LoadMode.LoadMap || mode == LoadMode.NewMap)
                {
                    //ok all checks complete
                    plTerrain.Enable();
                    m_selectable = true;
                    lSelectable.text = m_updatetool + m_available;
                    tfTerrainHeight.text = m_terrainHeight.RoundToNearest(2f).ToString("0.00");
                }
                else
                {
                    plTerrain.Disable();
                    m_selectable = false;
                    lSelectable.text = m_updatetool + m_unavailable;
                }
            }
            catch (Exception ex)
            {
                WriteError("Error in SetTerrainEnabled Exception: ", ex);
            }

            //WriteLog("Leaving: SetServicesEnabled m_selectable: " + m_selectable);
        }

        private void SetDistrictsEnabled()
        {
          //WriteLog("Entering: SetDistrictsEnabled m_selectable: " + m_selectable);

            try
            {
                if (mode == LoadMode.LoadMap || mode == LoadMode.NewMap)
                {
                    plDistricts.Disable();
                    m_selectable = false;
                    lSelectable.text = m_updatetool + m_unavailable;
                }
                else
                {
                    plDistricts.Enable();
                    m_selectable = true;
                    lSelectable.text = m_updatetool + m_available;
                }
            }
            catch (Exception ex)
            {
                WriteError("Error in SetDistrictsEnabled Exception: ", ex);
            }

          //WriteLog("Leaving: SetDistrictsEnabled m_selectable: " + m_selectable);
        }

        private void SetUpdateEnabled()
        {
            //WriteLog("Entering: SetUpdateEnabled m_selectable: " + m_selectable);

            try
            {
                m_selectable = false;
                lSelectable.text = m_updatetool + m_unavailable;

                //is Updated enabled?
                if (options[(int)ops.Updates].isChecked == false)
                {
                    //WriteLog("Leaving: SetUpdateEnabled 'Update not checked' m_selectable: " + m_selectable);
                    return;
                }

                //do we have a to road type
                if (types.Any(o => o.isChecked == true)) { }
                else
                {
                    //WriteLog("Leaving: SetUpdateEnabled ' No types selected' m_selectable: " + m_selectable);
                    return;
                }

                //do we have a from road type
                if (fromTypes.Any(o => o.isChecked == true)) { }
                else
                {
                    //WriteLog("Leaving: SetUpdateEnabled 'No to road type selected' m_selectable: " + m_selectable);
                    return;
                }

                //do we have a to road type
                if (toTypes.Any(o => o.isChecked == true)) { }
                else
                {
                    //WriteLog("Leaving: SetUpdateEnabled 'No from road type selected' m_selectable: " + m_selectable);
                    return;
                }

                //do we have a from Road
                if (fromOneway.Any(o => o.isChecked == true)) { }
                else if (fromMedium.Any(o => o.isChecked == true)) { }
                else if (fromHighway.Any(o => o.isChecked == true)) { }
                else if (fromLarge.Any(o => o.isChecked == true)) { }
                else if (fromBasic.Any(o => o.isChecked == true)) { }
                else
                {
                    //WriteLog("Leaving: SetUpdateEnabled 'No to road selected'm_selectable: " + m_selectable);
                    return;
                }

                //do we have a to Road
                if (toOneway.Any(o => o.isChecked == true)) { }
                else if (toMedium.Any(o => o.isChecked == true)) { }
                else if (toHighway.Any(o => o.isChecked == true)) { }
                else if (toLarge.Any(o => o.isChecked == true)) { }
                else if (toBasic.Any(o => o.isChecked == true)) { }
                else
                {
                    //WriteLog("Leaving: SetUpdateEnabled 'No from road selected' m_selectable: " + m_selectable);
                    return;
                }

                //ok all checks complete
                m_selectable = true;
                lSelectable.text = m_updatetool + m_available;
            }
            catch (Exception ex)
            {
                WriteError("Error in Error in SetUpdateEnabled ", ex);
            }

            //WriteLog("Leaving: SetUpdateEnabled m_selectable: " + m_selectable);
        }

        private void SetDeleteEnabled()
        {
            //WriteLog("Entering SetDeleteEnabled selectable is: " + m_selectable);
            try
            {                
                m_selectable = false;
                lSelectable.text = m_updatetool + m_unavailable;

                //is delete enabled?
                if (options[(int)ops.Deletes].isChecked == false)
                {
                    //WriteLog("Leaving: SetDeleteEnabled 'Deletes not checked' m_selectable: " + m_selectable);
                    return;
                }

                //do we have any types
                if (types.Any(o => o.isChecked == true)) { }
                else
                {
                    //WriteLog("Leaving: SetDeleteEnabled 'No types selected' m_selectable: " + m_selectable);
                    return;
                }
                //do we have anyhting checked to delete?
                if (deletes.Any(o => o.isChecked == true)) { }
                else
                {
                    //WriteLog("Leaving: SetDeleteEnabled 'No item selected to delete' m_selectable: " + m_selectable);
                    return;
                }
                m_selectable = true;
                lSelectable.text = m_updatetool + m_available;
            }
            catch (Exception ex)
            {
                WriteError("Error in SetDeleteEnabled Exception: ", ex);
            }
            //WriteLog("Leaving SetDeleteEnabled selectable is: " + m_selectable);
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
            plTerrain.isVisible = show;
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
            plDistricts.isVisible = show;

            SetUpdateEnabled();
        }

        private void UpdateDisplayedRoads(List<UICheckBox> types, List<UICheckBox> roads, string text, bool show, int xPos)
        {
            //WriteLog("Entering UpdateDisplayedRoads " + xPos);

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

            //WriteLog("Leaving UpdateDisplayedRoads");
        }

        private void DisplayCheckBoxes(List<UICheckBox> roads, int xPos, string test, bool show)
        {
            //WriteLog("Entering DisplayCheckBoxes");

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

            //WriteLog("Leaving UpdateDisplayedRoads");
        }

        private int ReturnInteger(string value, int min, int max)
        {
            int tmp = min;
            try
            {
                tmp = Convert.ToInt32(value);
            }
            catch (Exception)
            {
                tmp = 0;
            }
            return Math.Min(Math.Max(tmp, min), max);
        }

        private double ReturnDouble(string value, double min, double max)
        {
            double tmp = min;
            try
            {
                tmp = Convert.ToDouble(value);
            }
            catch (Exception)
            {
                tmp = 0;
            }
            return Math.Min(Math.Max(tmp, min), max);
        }

        private bool isNumeric(string value)
        {
            bool results = false;
            try
            {
                System.Convert.ToInt32(value);
                results = true;
            }
            catch (FormatException e)
            {
                WriteError("Error in isNumeric: ", e);
            }
            return results;
        }

        #endregion

        #region "Event Handlers"

        private void btHide_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            //WriteLog("Entering btHide_eventClick: " + plMain.isVisible);
            this.OnDisable();
            plMain.isVisible = false;
            this.enabled = false;
            //WriteLog("Leaving btHide_eventClick: " + plMain.isVisible);
        }

        private void TerrainHeight_eventTextChanged(UIComponent component, string value)
        {
            //WriteLog("Entering Settings Panel eventTextChanged");

            UITextField tf = (UITextField)component;
            tf.text = Regex.Replace(tf.text, "[^0-9\\.]", "");
            double val = 0;

            if (tf.text.Length == 0)
            {
                tf.text = "0.0";
                return;
            }
            if (isNumeric(value) == false)
                return;
            if (double.TryParse(value, out val) == false)
                return;
            if ( val < 0) { val = 0; }
            if (val > 2000) { val = 2000; }
            
            //WriteLog("Settings Panel eventTextChanged - setting val: " + val);
            try
            {
                m_terrainHeight = (float)val;
                tfTerrainHeight.text = val.ToString("#.##");
            }
            catch (Exception e)
            {
                WriteError(e.Message, e);
            }

            //WriteLog("Leaving SettingsPanel eventTextChanged - val: : " + val + ", Value: " + tf.text + ": " + m_terrainHeight);
        }

        private void ddHeights_eventSelectedIndexChanged(UIComponent component, int value)
        {
            //WriteLog("ddHeights.items[value]: " + ddHeights.items[value]);
            tfTerrainHeight.text = ddHeights.items[value];
            //WriteLog("tfTerrainHeight.text: " + tfTerrainHeight.text);
        }

        private void serviceDropdown_eventSelectedIndexChanged(UIComponent component, int value)
        {

        }

        private void Types_eventCheckChanged(UIComponent component, bool value)
        {
            if (plRoads.isVisible)
            {
                SetUpdateEnabled();
            }
            else
            {
                SetDeleteEnabled();
            }
        }

        private void ServiceTypes_eventCheckChanged(UIComponent component, bool value)
        {

            //do we reenble or disable chirper
            UICheckBox cb = (UICheckBox)component;
            
          //WriteLog("Entering ServiceTypes_eventCheckChanged.Name, Value: " + cb.text + ", Value: " + value);

            if (cb.text == "Chirper")
            {
                //we need to toggle shown or not
                if (Chirp.ToggleState != value)
                    Chirp.Toggle(value);
                return;
            }

            if (cb.text == "Abandoned")
            {
                //Toggle tha value as needed
                if (DemolishAbandoned.@value != value)
                    DemolishAbandoned.@value = !DemolishAbandoned.@value;
                return;
            }

            if (cb.text == "Burned")
            {
                //Toggle tha value as needed
                if (DemolishBurned.@value != value)
                    DemolishBurned.@value = !DemolishBurned.@value;
                return;
            }

            SetServicesEnabled();
            //WriteLog("Leaving ServiceTypes_eventCheckChanged");
        }

        private void btHelp_eventDoubleClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            //WriteLog("Entering btHelp_eventDoubleClick");

            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream("AnotherRoadUpdate.ARUT.pdf");
                if (stream == null)
                {
                    //WriteLog("Error loading embeded resource AnotherRoadUpdate.ARUT.pdf");
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
                WriteError("Error in btHelp_eventDoubleClick ", ex);
            }

            //WriteLog("Leaving btHelp_eventDoubleClick");
        }

        private void DeleteTypes_eventCheckChanged(UIComponent component, bool value)
        {
            if (m_updates == true) { return; }
            m_updates = true;

            //WriteLog("Entering DeleteTypes_eventCheckChanged");
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

            if (c.text == types[(int)tp.Ground].text) { us.Ground = value; }
            if (c.text == types[(int)tp.Bridge].text) { us.Bridge = value; }
            if (c.text == types[(int)tp.Tunnel].text) { us.Tunnel = value; }
            if (c.text == types[(int)tp.Slope].text) { us.Tunnel = value; }
            if (c.text == types[(int)tp.Curve].text) { us.Tunnel = value; }

            //set enabled
            SetDeleteEnabled();
            m_updates = false;
            //WriteLog("Leaving DeleteTypes_eventCheckChange Roads: " + us.Roads + " Ground: " + us.Ground);
        }

        private void Options_eventCheckChanged(UIComponent component, bool value)
        {
            if (m_updates == true) { return; }
            m_updates = true;

            UICheckBox c = (UICheckBox)component;
            //WriteLog("Entering: Options_eventCheckChanged: c.text: " + c.text + " Checked: " + c.isChecked);

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
                    options[(int)ops.Terrain].isChecked = false;
                    options[(int)ops.Districts].isChecked = false;
                    plTypes.isVisible = true;
                    plRoads.isVisible = true;
                    ShowRoads();
                }
                else if (c.text == "Deletes")
                {
                    options[(int)ops.Updates].isChecked = false;
                    options[(int)ops.Services].isChecked = false;
                    options[(int)ops.Terrain].isChecked = false;
                    options[(int)ops.Districts].isChecked = false;
                    plTypes.isVisible = true;
                    plDelete.isVisible = true;
                    SetDeleteEnabled();
                }
                else if (c.text == "Services")
                {
                    options[(int)ops.Updates].isChecked = false;
                    options[(int)ops.Deletes].isChecked = false;
                    options[(int)ops.Terrain].isChecked = false;
                    options[(int)ops.Districts].isChecked = false;
                    plTypes.isVisible = false;
                    plServices.isVisible = true;
                    SetServicesEnabled();
                }
                else if (c.text == "Terrain")
                {
                    options[(int)ops.Updates].isChecked = false;
                    options[(int)ops.Deletes].isChecked = false;
                    options[(int)ops.Services].isChecked = false;
                    options[(int)ops.Districts].isChecked = false;
                    plTypes.isVisible = false;
                    plTerrain.isVisible = true;
                    SetTerrainEnabled();
                }
                else if (c.text == "Districts")
                {
                    options[(int)ops.Updates].isChecked = false;
                    options[(int)ops.Deletes].isChecked = false;
                    options[(int)ops.Services].isChecked = false;
                    options[(int)ops.Terrain].isChecked = false;
                    plTypes.isVisible = false;
                    plDistricts.isVisible = true;
                    SetDistrictsEnabled();
                }
            }
            m_updates = false;
            //WriteLog("Leaving Options_eventCheckChanged");
        }

        private void FromTypes_eventCheckChanged(UIComponent component, bool value)
        {
            if (m_fromTypes == true) { return; }
            m_fromTypes = true;

            UICheckBox cb = (UICheckBox)component;

            //WriteLog("Entering FromTypes_eventCheckChanged: Option: " + cb.text);

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
                WriteError("Error in FromTypes_eventCheckChanged loc: " + loc + ".", ex);
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

            //WriteLog("Leaving FromTypes_eventCheckChanged: Option: " + cb.text);
        }

        private void ToTypes_eventCheckChanged(UIComponent component, bool value)
        {
            if (m_toTypes == true) { return; }
            m_toTypes = true;

            UICheckBox cb = (UICheckBox)component;

            //WriteLog("Entering ToTypes_eventCheckChanged: Option: " + cb.text);

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
                WriteError("Error in ToTypes_eventCheckChanged loc: " + loc + ".", ex);
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

            //WriteLog("Leaving ToTypes_eventCheckChanged: Option: " + cb.text);
        }

        private void FromRoad_eventCheckChanged(UIComponent component, bool value)
        {
            if (m_fromRoads == true) { return; }
            m_fromRoads = true;

            UICheckBox cb = (UICheckBox)component;

            //WriteLog("Entering FromRoads_eventCheckChanged: Option: " + cb.text);

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

            //WriteLog("Leaving FromRoads_eventCheckChanged: Option: " + cb.text);
        }

        private void ToRoad_eventCheckChanged(UIComponent component, bool value)
        {
            //WriteLog("Entering ToRoads_eventCheckChanged: m_toRoads: " + m_toRoads);
            if (m_toRoads == true) { return; }
            m_toRoads = true;

            UICheckBox cb = (UICheckBox)component;

            //WriteLog("Entering ToRoads_eventCheckChanged: Option: " + cb.text);

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

            //WriteLog("Leaving ToRoads_eventCheckChanged: Option: " + cb.text);
        }

        private void Button_Clicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            //WriteLog("Entering Button_Clicked: " + plMain.isVisible);
            if (plMain.isVisible == true)
            {
                this.OnDisable();
                plMain.isVisible = false;
                this.enabled = false;
            }
            else
            {
                this.enabled = true;
                plMain.isVisible = true;
                mainButton.enabled = true;
            }
            //WriteLog("Leaving Button_Clicked: " + plMain.isVisible);
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

            var color = Color.green;

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

        #endregion

        #region "Updates, Deletes, Toggles, Oh my!"

        #region "Apply Helpers"

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
            if (errors == ToolErrors.None) text = "None.";
            else if (errors == ToolErrors.OutOfArea) text += " Out of city limits!";
            else if (errors == ToolErrors.AlreadyExists) text += " Already exists";
            else if (errors == ToolErrors.CannotBuildOnWater) text += " Cannot build on water.";
            else if (errors == ToolErrors.InvalidShape) text += " Invalid Shape.";
            else if (errors == ToolErrors.NotEnoughMoney) text += " Not enough money.";
            else if (errors == ToolErrors.TooShort) text += " Segment to short.";
            else if (errors == ToolErrors.CannotUpgrade) text += " Cannot upgrade to this type.";
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
                WriteLog("GetSegmentControlPoints:: segmentIndex >= net.m_segments.m_buffer.Length: segmentIndex: " + segmentIndex + " net.m_segments.m_buffer.Length: " + net.m_segments.m_buffer.Length);
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

        private string GetName(Building bd)
        {
            InstanceID id = new InstanceID();
            id.Building = (ushort)bd.Info.GetInstanceID();
            return Singleton<InstanceManager>.instance.GetName(id);
        }

        #endregion

        #region "Apply Changes"

        private void ApplyDistrictsChange()
        {
            //Set up the max area
            int minX;
            int minZ;
            int maxX;
            int maxZ;

            GetMinMax(out minX, out minZ, out maxX, out maxZ);
            string log = "ApplyDistrictsChange.GetMinMax = (minX, minZ) : (maxX, maxZ) (" + minX + ", " + minZ + ") : (" + maxX + ", " + maxZ + ")";
            WriteLog(log);

            try
            {
                //load the districtManager
                DistrictManager dm = Singleton<DistrictManager>.instance;
                District[] buffer = Singleton<DistrictManager>.instance.m_districts.m_buffer;

                byte id;
                dm.CreateDistrict(out id);
                WriteLog("id; " + id);
                dm.SetDistrictName((int)id, "New District");
                dm.AreaModified(minX, minZ, maxX, maxZ, true);
                dm.GetDistrictArea(id, out minX, out minZ, out maxX, out maxZ);
                string name = "District Name: " + dm.GetDistrictName((int)id) +"; id: " + id + "; DistrictsArea = (minX, minZ) : (maxX, maxZ) (" + minX + ", " + minZ + ") : (" + maxX + ", " + maxZ + ")";
                WriteLog(name);
                id = 0;
                foreach (District d in buffer)
                {
                    District.Flags df = d.m_flags;
                    dm.GetDistrictArea(id, out minX, out minZ, out maxX, out maxZ);
                    name = "District Name: " + dm.GetDistrictName((int)id) + "; id: " + id + "; DistrictsArea = (minX, minZ) : (maxX, maxZ) (" + minX + ", " + minZ + ") : (" + maxX + ", " + maxZ + ")";
                    WriteLog(name);
                    id += 1;
                }
            }
            catch (Exception ex)
            {
                WriteError("Error in ApplyDistrictsChange: ", ex);
            }
            ////load the district buffer
            //DistrictManager dm = Singleton<DistrictManager>.instance;
            //District[] buffer = Singleton<DistrictManager>.instance.m_districts.m_buffer;

            //foreach (District d in buffer)
            //{
            //    //we need to see if we are in any existing districts
            //    //we need to make sure that this was not a mouse click event
            //    if (maxZ - minZ >= 1 && maxX - minX >= 1)
            //    {
            //        //we need to update the area in 120 point sections
            //        for (int i = minZ; i <= maxZ; i++)
            //        {
            //            for (int j = minX; j <= maxX; j++)
            //            {
            //                byte id;
            //                Singleton<DistrictManager>.instance.CreateDistrict(out id);
            //                Singleton<DistrictManager>.instance.AreaModified(minX, minZ, maxX, maxZ, true);

            //            }
            //            //make sure we exit the loop
            //            if (i + 1 >= maxZ)
            //                break;
            //            i += 119;
            //            if (i > maxZ)
            //                i = maxZ - 1;
            //        }

            //        string coords = minX + ", " + minZ + ") : (" + maxX + ", " + maxZ + ") diff = (" + (maxX - minX) + ", " + (maxZ - minZ) + ")";
            //        log = "Exiting ApplyBrush: (minX, minZ) : (maxX, maxZ) = (" + coords;
            //        //WriteLog(log);
            //    }

            //}
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
                            WriteError("Error testing convert of: " + segment.Info.name + " to " + convertTo + ".", ex);
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
                                WriteError(message, ex);
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
            //UIView.RefreshAll(true);
            base.RenderOverlay(RenderManager.instance.CurrentCameraInfo);
            return num;
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
                    try
                    {
                        //WriteLog("In the for loops ");
                        ushort num5 = NetManager.instance.m_segmentGrid[i * 270 + j];
                        int num6 = 0;
                        bool skip = false;
                        while (num5 != 0u)
                        {
                            //WriteLog("In the while ");
                            var segment = NetManager.instance.m_segments.m_buffer[(int)((UIntPtr)num5)];

                            bool curved = (segment.m_cornerAngleEnd != segment.m_cornerAngleStart);

                            //WriteLog("Segment name: " + segment.Info.name + " Service :" + segment.Info.GetService());
                            //WriteLog("m_startDirection: " + segment.m_startDirection + " m_endDirection: " + segment.m_endDirection);
                            //WriteLog("m_cornerAngleEnd: " + segment.m_cornerAngleEnd + " m_cornerAngleStart: " + segment.m_cornerAngleStart);
                            //WriteLog("m_startLeftSegment: " + segment.m_startLeftSegment + " m_startRightSegment: " + segment.m_startRightSegment);
                            //WriteLog("m_endLeftSegment: " + segment.m_endRightSegment + " m_endRightSegment: " + segment.m_endRightSegment);

                            Vector3 position = segment.m_middlePosition;
                            float positionDiff = Mathf.Max(Mathf.Max(minX - 16f - position.x, minZ - 16f - position.z), Mathf.Max(position.x - maxX - 16f, position.z - maxZ - 16f));

                            if (positionDiff < 0f)
                            {
                                string seg = segment.Info.name;
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

                                if (skip == false)
                                {
                                    segmentsToDelete.Add(num5);
                                    //WriteLog("The segment named, " + segment.Info.name + ", be deleted? " + !skip);
                                }
                                else { }
                                //WriteLog("The segment named, " + segment.Info.name + ", be deleted? " + !skip);
                            }
                            num5 = NetManager.instance.m_segments.m_buffer[(int)((UIntPtr)num5)].m_nextGridSegment;
                            if (++num6 >= 262144)
                            {
                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                break;
                            }
                        }   //while (num5 != 0u)
                    }
                    catch (Exception ex)
                    {
                        WriteError("Error in DeleteLanes segmentsToDelete (segment loop) ", ex);
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
                    WriteError("Error in DeleteLanes segmentsToDelete (Delete loop).", ex); ;
                }
            }
            NetManager.instance.m_nodesUpdated = true;
        }

        private void ApplyServices()
        {
            //WriteLog("Entering ApplyServices: Code not yet implamented.");
            Building[] buffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;

            bool toggle = cbToggle.isChecked;
            //string onoff = "Off";
            byte rate = 0;


            for (int i = 0; i < buffer.Length; i++)
            {
                Building bd = buffer[i];

                if (services[(int)dl.Beautification].isChecked == false && bd.Info.m_class.m_service == ItemClass.Service.Beautification) { }
                else if (services[(int)dl.Education].isChecked == false && bd.Info.m_class.m_service == ItemClass.Service.Education) { }
                else if (services[(int)dl.Electricity].isChecked == false && bd.Info.m_class.m_service == ItemClass.Service.Electricity) { }
                else if (services[(int)dl.FireDepartment].isChecked == false && bd.Info.m_class.m_service == ItemClass.Service.FireDepartment) { }
                else if (services[(int)dl.Garbage].isChecked == false && bd.Info.m_class.m_service == ItemClass.Service.Garbage) { }
                else if (services[(int)dl.HealthCare].isChecked == false && bd.Info.m_class.m_service == ItemClass.Service.HealthCare) { }
                else if (services[(int)dl.Monument].isChecked == false && bd.Info.m_class.m_service == ItemClass.Service.Monument) { }
                else if (services[(int)dl.PoliceDepartment].isChecked == false && bd.Info.m_class.m_service == ItemClass.Service.PoliceDepartment) { }
                else if (services[(int)dl.PublicTransport].isChecked == false && bd.Info.m_class.m_service == ItemClass.Service.PublicTransport) { }
                else if (services[(int)dl.Water].isChecked == false && bd.Info.m_class.m_service == ItemClass.Service.Water) { }
                else if (ValidateSelectedArea(bd) == false)
                {
                    //sw.WriteLine(String.Format("Segment {0} is not in the selected area.", segment.Info.name));
                }
                else
                {
                    //WriteLog("Building Name: " + bd.Info.gameObject.name + " GetName: " + GetName(bd));
                    //WriteLog("BuildingAI Name: " + bd.Info.m_buildingAI.name);
                    //WriteLog("Building.Info.GetLocalizedTitle : " + bd.Info.GetLocalizedTitle());
                    
                    //onoff = "Off";
                    rate = 0;
                    if (toggle == true)
                    {
                        //onoff = "On";
                        rate = 100;
                    }

                    //WriteLog("Setting services to " + onoff + ". bd.m_flags was: " + bd.m_flags.IsFlagSet(Building.Flags.Active));

                    if (bd.m_flags.IsFlagSet(Building.Flags.Active) != toggle)
                    {
                        //WriteLog("About to toggle: ");
                        bd.m_flags ^= Building.Flags.Active;
                        bd.m_productionRate = rate;
                    }

                    //WriteLog("Setting services to " + onoff + ". bd.m_flags is: " + bd.m_flags.IsFlagSet(Building.Flags.Active));
                }
                buffer[i] = bd;
            }
            //WriteLog("Leaving ApplyServices: Code not yet implamented.");
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
                ConvertSegments(toSelected, fromSelected, false, out totalCost, out errors);
            }
            catch (Exception ex )
            {
                WriteError("Error in ApplyUpdates ", ex);
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
            catch (Exception ex)
            {
                WriteError("Error in ApplyDeletes unknown Option: ", ex);
            }

            if (deletes[(int)p.Buildings].isChecked)
                BulldozeBuildings();
            if (deletes[(int)p.Props].isChecked)
                BulldozeProps();
            if (deletes[(int)p.Trees].isChecked)
                BulldozeTrees();
        }
        
        private void ApplyTerrainChange()
        {
            //Make the call to update the entire area with the new height
            ApplyBrush();
        }

        private void GetMinMax(out int minX, out int minZ, out int maxX, out int maxZ)
        {
            //get the terrain coords
            Vector3 startm = ConvertCoords(m_startPosition, true);
            Vector3 endm = ConvertCoords(m_endPosition, true);

            //Load the values
            float startx = startm.x;
            float startz = startm.z;
            float endx = endm.x;
            float endz = endm.z;

            //we need the min and max coordinates
            float min = 0;
            float max = 1080;

            //Get the smaller X into startx and larger into endx
            //Also not less than 0 or more then 1080
            if (startx > endx)
            {
                minX = (int)Mathf.Min(Mathf.Max(endx, min), max);
                maxX = (int)Mathf.Max(Mathf.Min(startx, max), min);
            }
            else
            {
                minX = (int)Mathf.Min(Mathf.Max(startx, min), max);
                maxX = (int)Mathf.Max(Mathf.Min(endx, max), min);
            }
            //Get the smaller Z into startz and larger into endz
            if (startz > endz)
            {
                minZ = (int)Mathf.Min(Mathf.Max(endz, min), max);
                maxZ = (int)Mathf.Max(Mathf.Min(startz, max), min);
            }
            else
            {
                minZ = (int)Mathf.Min(Mathf.Max(startz, min), max);
                maxZ = (int)Mathf.Max(Mathf.Min(endz, max), min);
            }
        }

        private void ApplyBrush()
        {
            ushort finalHeight = 500;
            MyITerrain mTerrain = new MyITerrain();
            //WriteLog("m_terrainHeight: " + m_terrainHeight);
            finalHeight = mTerrain.HeightToRaw((float)m_terrainHeight);
            //WriteLog("finalHeight: " + finalHeight);

            int minX;
            int minZ;
            int maxX;
            int maxZ;

            GetMinMax(out minX, out minZ, out maxX, out maxZ);

            string log = "ApplyBrush - GetMinMax = (minX, minZ) : (maxX, maxZ) (" + minX + ", " + minZ + ") : (" + maxX + ", " + maxZ + ") - finalHeight: " + finalHeight;
            //WriteLog(log);

            //we need to make sure that this was not a mouse click event
            if (maxZ - minZ >= 1 && maxX - minX >= 1)
            {
                for (int i = minZ; i <= maxZ; i++)
                {
                    for (int j = minX; j <= maxX; j++)
                    {
                        int num = i * 1081 + j;
                        //We want the prior backup in the 'original'
                        m_originalHeights[num] = m_backupHeights[num];
                        //We want the current in the back up
                        m_backupHeights[num] = m_rawHeights[num];
                        //We want the new height in the new/raw
                        m_rawHeights[num] = finalHeight;
                    }
                }
                //we need to update the area in 120 point sections
                for (int i = minZ; i <= maxZ; i++)
                {
                    for (int j = minX; j <= maxX; j++)
                    {
                        int x1 = j;
                        int x2 = Math.Max(i + 119, maxX);
                        int z1 = i;
                        int z2 = Math.Max(j + 119, maxZ);
                        TerrainModify.UpdateArea(x1, z1, x2, z2, true, false, false);

                        //log = "(x1, z1) : ( x2, z2): (" + x1 + ", " + z1 + ") : (" + x2 + ", " + z2 + ")";
                        //WriteLog("ApplyBrush: " + log);
                        //make sure we exit the loop
                        if (j + 1 >= maxX)
                            break;
                        j += 119;
                        if (j > maxX)
                            j = maxX - 1;
                    }
                    //make sure we exit the loop
                    if (i + 1 >= maxZ)
                        break;
                    i += 119;
                    if (i > maxZ)
                        i = maxZ - 1;
                }

                m_minX = minX;
                m_maxX = maxX;
                m_minZ = minZ;
                m_maxZ = maxZ;

                //Store the change
                EndStroke();

                //does this redraw the screen
                transform.Translate(new Vector3(0, 0, 0));

                string coords = minX + ", " + minZ + ") : (" + maxX + ", " + maxZ + ") diff = (" + (maxX - minX) + ", " + (maxZ - minZ) + ")";
                log = "Exiting ApplyBrush: (minX, minZ) : (maxX, maxZ) = (" + coords;
                //WriteLog(log);
            }
        }

        private static Vector3 SnapToTerrain(Vector3 mouse)
        {
            return new Vector3(Mathf.RoundToInt(mouse.x / 16f), 0f, Mathf.RoundToInt(mouse.z / 16f)) * 16f;
        }

        private static float ConvertCoords(float coords, bool ScreenToTerrain = true)
        {
            return ScreenToTerrain ? coords / 16f + 1080 / 2 : (coords - 1080 / 2) * 16f;
        }

        private Vector3 ConvertCoords(Vector3 Pos, bool ScreenToTerrain = true)
        {
            return new Vector3
            {
                x = ConvertCoords(Pos.x, ScreenToTerrain),
                z = ConvertCoords(Pos.z, ScreenToTerrain)
            };
        }

        private void EndStroke()
        {
            // LoadingExtension.WriteLog("Entering EndStroke");
            //creating the undo stroke
            UndoStroke item = default(UndoStroke);
            item.name = "undo: " + UndoList.Count;
            item.minX = m_minX;
            item.maxX = m_maxX;
            item.minZ = m_minZ;
            item.maxZ = m_maxZ;
            item.pointer = UndoList.Count;
            item.rawHeights = m_rawHeights;
            item.backupHeights = m_backupHeights;
            item.originalHeights = m_originalHeights;

            UndoList.Add(item);

            m_minX = 0;
            m_maxX = 0;
            m_minZ = 0;
            m_maxZ = 0;

            // LoadingExtension.WriteLog("Exiting EndStroke");
        }

        private void ApplyUndo()
        {
            if (UndoList.Count < 0)
            {
                return;
            }
            //remove the current changes from the list (there are none)
            UndoStroke undoStroke = UndoList[UndoList.Count - 1];
            UndoList.RemoveAt(UndoList.Count - 1);

            int minX = undoStroke.minX;
            int maxX = undoStroke.maxX;
            int minZ = undoStroke.minZ;
            int maxZ = undoStroke.maxZ;
            int pointer = undoStroke.pointer;

            //log = "ApplyUndo = (minX, minZ) : (maxX, maxZ) (" + minX + ", " + minZ + ") : (" + maxX + ", " + maxZ + ")";
            //LoadingExtension.WriteLog(log);

            for (int i = minZ; i <= maxZ; i++)
            {
                for (int j = minX; j <= maxX; j++)
                {
                    int num = i * 1081 + j;
                    //we want the new/raw to be the back up (un do)
                    m_rawHeights[num] = undoStroke.backupHeights[num];
                    //we want the prior backup to match the original (original as in one step back)
                    m_backupHeights[num] = undoStroke.originalHeights[num];
                }
            }

            m_minX = 0;
            m_maxX = 0;
            m_minZ = 0;
            m_maxZ = 0;

            ////Apply Undo
            //TerrainModify.UpdateArea(minX, minZ, maxX, maxZ, true, false, false);
            string log = "(minX, minZ) : ( maxX, maxZ): (" + minX + ", " + minZ + ") : (" + maxX + ", " + maxZ + ")";
            //LoadingExtension.WriteLog("ApplyBrush: " + log);

            //we need to update the area in 120 point sections
            for (int i = minZ; i <= maxZ; i++)
            {
                for (int j = minX; j <= maxX; j++)
                {
                    int x1 = j;
                    int x2 = Math.Max(i + 119, maxX);
                    int z1 = i;
                    int z2 = Math.Max(j + 119, maxZ);
                    TerrainModify.UpdateArea(x1, z1, x2, z2, true, false, false);
                    log = "(x1, z1) : ( x2, z2): (" + x1 + ", " + z1 + ") : (" + x2 + ", " + z2 + ")";
                    //LoadingExtension.WriteLog("ApplyUndo: " + log);
                    //make sure we exit the loop
                    if (j + 1 >= maxX)
                        break;
                    j += 119;
                    if (j > maxX)
                        j = maxX - 1;
                }
                //make sure we exit the loop
                if (i + 1 >= maxZ)
                    break;
                i += 119;
                if (i > maxZ)
                    i = maxZ - 1;
            }

            //does this redraw the screen
            transform.Translate(new Vector3(0, 0, 0));
        }

        #endregion

        #endregion

        #region "Logging"

        internal static void WriteLog(string data)
        {
            if (logging == false) { return; }
            string filename = "ARUT_Logging" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
            if (m_deletelog)
            {
                m_deletelog = false;
                File.Delete(filename);
            }

            // Write the string to a file.
            System.IO.StreamWriter file = File.AppendText(filename);
            file.WriteLine(data);
            file.Close();
        }

        internal static void WriteError(string data, Exception ex)
        {
            if (erroring == false) { return; }
            string filename = "ARUT_Error" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
            if (m_delete)
            {
                m_delete = false;
                File.Delete(filename);
            }

            // Write the string to a file.
            System.IO.StreamWriter file = File.AppendText(filename);
            file.WriteLine(data + " Exception: " + ex.Message + " Stack: " + ex.StackTrace);
            file.Close();
        }

        #endregion

        #endregion

    }
}