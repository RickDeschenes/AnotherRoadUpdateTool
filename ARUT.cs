using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading;
using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using ICities;
using UnityEngine;
using AnotherRoadUpdateTool.Helpers;

/// <summary>
/// Updated from SkylinesRoadUpdate added global preferences
/// added Additional selections
/// Added new to remove warning for reused m_
/// </summary>
namespace AnotherRoadUpdateTool
{
    public class ARUT : DefaultTool
    {
        #region Declarations

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

        #region Variables

        internal static UserSettings us = new UserSettings();

        internal static int segcount = 0;

        #region Static

        //set to false once code is in production (Why not use "debug mode")
        private static bool logging = true;
        private static bool erroring = true;
        private static bool m_delete = true;
        private static bool m_deletelog = true;

        internal static bool AdjustAreas;
        internal static bool AdjustMoney;
        internal static bool AllRoads;
        internal static bool AutoDistroy;

        internal static bool ShowDelete;
        internal static bool ShowUpdate;
        internal static bool ShowTerrain;
        internal static bool ShowServices;
        internal static bool ShowDistricts;

        internal static int MaxAreas;
        //internal static int StartMoney;
        internal static int StartMoney { get { return us.StartMoney; } private set { value = us.StartMoney; }  }
        
        internal readonly static SavedBool DemolishAbandoned = new SavedBool("ModDemolishAbandoned", Settings.gameSettingsFile, true, true);
        internal readonly static SavedBool DemolishBurned = new SavedBool("ModDemolishBurned", Settings.gameSettingsFile, true, true);

        internal static BindingList<UndoStroke> UndoList;

        #endregion

        #region Objects

        private object m_dataLock = new object();

        private LoadMode mode;
        private static Chirper Chirp;
        private UnlockRoads Roads;
        private DestroyMonitor Dozer;
        private MaxAreas Areas;
        private Zones Zones;

        #endregion

        #region Private

        #region not Arrays

        private bool m_active;
        private bool m_settings;
        
        private Vector3 m_startPosition;
        private Vector3 m_endPosition;
        private Vector3 m_startDirection;
        private Vector3 m_mouseDirection;
        private Vector3 m_cameraDirection;
        private new Vector3 m_mousePosition;
        private new bool m_mouseRayValid;
        private new Ray m_mouseRay;
        private new float m_mouseRayLength;

        readonly ushort[] m_undoBuffer = Singleton<TerrainManager>.instance.UndoBuffer;
        private ushort[] m_originalHeights;
        readonly ushort[] m_backupHeights = Singleton<TerrainManager>.instance.BackupHeights;
        readonly ushort[] m_rawHeights = Singleton<TerrainManager>.instance.RawHeights;

        SavedInputKey m_UndoKey = new SavedInputKey(Settings.mapEditorTerrainUndo, Settings.inputSettingsFile, DefaultSettings.mapEditorTerrainUndo, true);
        
        private float m_maxArea = 400f;

        private PanelMain plMain;
        internal UIButton mainButton;

        #endregion

        #endregion

        #endregion

        #endregion

        #region "Public Procedures"

        #region "Minor"

        protected override void Awake()
        {
            WriteLog("ARUT awake! + m_active: " + m_active);
            m_active = false;

            base.Awake();
        }
        
        protected override void OnEnable()
        {
            WriteLog("ARUT OnEnable!");
            UIView.GetAView().FindUIComponent<UITabstrip>("MainToolstrip").selectedIndex = -1;
            plMain.isVisible = true;
            base.OnEnable();
        }

        protected override void OnDestroy()
        {
            SetSettings();
            WriteLog("ARUT OnDestroy!");
            base.OnDestroy();
        }

        protected override void OnDisable()
        {
            //WriteLog("ARUT OnDisable Stack: " + new System.Diagnostics.StackTrace(true).ToString());
            WriteLog("ARUT OnDisable!");
            plMain.isVisible = false;
            base.OnDisable();
        }

        protected override void OnToolUpdate()
        {
            //WriteLog("ARUT OnToolUpdate!");
            base.OnToolUpdate();
        }

        protected override void OnToolGUI(Event e)
        {
            //WriteLog("ARUT OnToolGUI! plMain.Selectable: " + plMain.Selectable);
            //Event current = Event.current;

            if (plMain.isVisible == false)
                return;

            if (!m_active && m_UndoKey.IsPressed(e) && UndoList.Count() >= 0)
            {
                ApplyUndo();
            }

            if (e.type == EventType.MouseDown && m_mouseRayValid && plMain.Selectable)
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
            else if (e.type == EventType.MouseUp && m_active && plMain.Selectable)
            {
                if (e.button == 0)
                {
                    //handle Updates
                    if (plMain.Options(PanelMain.ops.Updates) && plMain.plTypes.isVisible)
                    {
                        UpdateObjects uo = new UpdateObjects(plMain.lInformation, m_startPosition, m_mousePosition, plMain.deletes, plMain.types, plMain.FromSelected, plMain.ToSelected);
                        Thread t = new Thread(new ThreadStart(uo.ApplyUpdates));
                        t.Start();
                        while (!t.IsAlive);
                    }
                    //handle Deletes
                    else if (plMain.Options(PanelMain.ops.Deletes) && plMain.plDelete.isVisible)
                    {
                        DeleteObjects rd = new DeleteObjects(m_startPosition, m_mousePosition, plMain.deletes, plMain.types);
                        Thread t = new Thread(new ThreadStart(rd.ApplyDeletes));
                        t.Start();
                        while (!t.IsAlive);
                    }
                    //handle Services
                    else if (plMain.Options(PanelMain.ops.Services) && plMain.plServices.isVisible)
                    {
                        ToggleServices ts = new ToggleServices(plMain, m_startPosition, m_mousePosition);
                        Thread t = new Thread(new ThreadStart(ts.ApplyServices));
                        t.Start();
                        while (!t.IsAlive);
                    }
                    //handle Districts
                    else if (plMain.Options(PanelMain.ops.Districts) && plMain.plDistricts.isVisible)
                    {
                        WriteLog("Trying ApplyDistrictsChange");
                        ApplyDistrictsChange();
                        WriteLog("Tried ApplyDistrictsChange");
                    }
                    //handle Terrain
                    else if (plMain.Options(PanelMain.ops.Terrain) && plMain.plTerrain.isVisible)
                    {
                        this.m_endPosition = this.m_mousePosition;
                        if (mode == LoadMode.LoadMap || mode == LoadMode.NewMap)
                        {
                            TerrainChanges tc = new TerrainChanges(m_startPosition, m_endPosition, (float)plMain.TerrainHeight, m_originalHeights, m_backupHeights, m_rawHeights);
                            Thread t = new Thread(new ThreadStart(tc.ApplyTerrainChange));
                            t.Start();
                            while (!t.IsAlive) ;
                        }
                        //ApplyTerrainChange();
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
            WriteLog("Entering InitGUI");
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

                WriteLog("About to set PanelMain in InitGUI");
                GameObject go = new GameObject("buildingWindowObject");

                plMain = go.AddComponent<PanelMain>();

                var view = UIView.GetAView();

                plMain.Mode = mode;
                plMain.transform.parent = view.transform;
                plMain.isVisible = true;
                //plMain.canFocus = true;
                //plMain.isInteractive = true;
                //plMain.relativePosition = new Vector3(572, 525);

                plMain.backgroundSprite = "MenuPanel2";

                plMain.eventPositionChanged += PlMain_eventPositionChanged;
                plMain.CreateObjects();

                //WriteLog("About to set GetSettings in InitGUI");
                //We can load the users last session
                GetSettings();
                //we can set to top, left based on the last position
                WriteLog("plMain us.Left, us.Top: " + us.Left + "x" + us.Top);
                plMain.relativePosition = new Vector3(us.Top, us.Left);
                WriteLog("plMain relativePosition: " + plMain.relativePosition);
                WriteLog("plMain Position: " + plMain.position);

                plMain.RefreshView();

                //WriteLog("About to set Areas and Zones in InitGUI");

                //About to set unlockable tiles
                Areas = new MaxAreas();
                Zones = new Zones();
                UndoList = new BindingList<UndoStroke>();
                Dozer = new DestroyMonitor();
                //WriteLog("Leaving InitGUI");
            }
        }

        private void PlMain_eventPositionChanged(UIComponent component, Vector2 value)
        {
            //recoird new top and left
            us.Top = (int)plMain.relativePosition.x;
            us.Left = (int)plMain.relativePosition.y;
            //WriteLog("plMain relativePosition: " + plMain.relativePosition);
            //WriteLog("plMain Position: " + plMain.position);
        }

        internal static void SetChirper(bool value)
        {
            if (Chirp.ToggleState != value)
                Chirp.Toggle(value);
        }

        #endregion

        #region "Private procedures"

        #region "Settings"

        private void SetSettings()
        {
            if (m_settings == true) { return; }
            m_settings = true;

            //WriteLog("Leaving SetSettings plMain.Types(PanelMain.tp.Ground): " + plMain.Types(PanelMain.tp.Ground));

            us.ToOneway = plMain.To(PanelMain.up.Oneway);
            us.ToMedium = plMain.To(PanelMain.up.Medium);
            us.ToHighway = plMain.To(PanelMain.up.Highway);
            us.ToLarge = plMain.To(PanelMain.up.Large);
            us.ToBasic = plMain.To(PanelMain.up.Basic);
            us.Oneway = plMain.From(PanelMain.up.Oneway);
            us.Medium = plMain.From(PanelMain.up.Medium);
            us.Highway = plMain.From(PanelMain.up.Highway);
            us.Large = plMain.From(PanelMain.up.Large);
            us.Basic = plMain.From(PanelMain.up.Basic);

            us.Roads = plMain.Deletes(PanelMain.dl.Roads);
            us.Railroads = plMain.Deletes(PanelMain.dl.Railroads);
            us.Highways = plMain.Deletes(PanelMain.dl.Highways);
            us.PowerLines = plMain.Deletes(PanelMain.dl.PowerLines);
            us.WaterPipes = plMain.Deletes(PanelMain.dl.WaterPipes);
            us.HeatPipes = plMain.Deletes(PanelMain.dl.HeatPipes);
            us.Airplanes = plMain.Deletes(PanelMain.dl.Airplanes);
            us.Shipping = plMain.Deletes(PanelMain.dl.Shipping);
            us.Pedestrian = plMain.Deletes(PanelMain.dl.Pedestrian);
            us.Bicycle = plMain.Deletes(PanelMain.dl.Bicycle);
            us.Tram = plMain.Deletes(PanelMain.dl.Tram);
            us.Metro = plMain.Deletes(PanelMain.dl.Metro);

            us.Ground = plMain.Types(PanelMain.tp.Ground);
            us.Bridge = plMain.Types(PanelMain.tp.Bridge);
            us.Slope = plMain.Types(PanelMain.tp.Slope);
            us.Tunnel = plMain.Types(PanelMain.tp.Tunnel);
            us.Curve = plMain.Types(PanelMain.tp.Curve);

            us.Buildings = plMain.Deletes(PanelMain.dl.Buildings);
            us.Trees = plMain.Deletes(PanelMain.dl.Trees);
            us.Props = plMain.Deletes(PanelMain.dl.Props);

            us.Update = plMain.Options(PanelMain.ops.Updates);
            us.Delete = plMain.Options(PanelMain.ops.Deletes);
            us.Services = plMain.Options(PanelMain.ops.Services);
            us.Terrain = plMain.Options(PanelMain.ops.Terrain);
            us.Toggle = plMain.cbToggle.isChecked;
            us.Districts = plMain.Options(PanelMain.ops.Districts);
            us.DistrictToggle = plMain.cbDistrictToggle.isChecked;
            us.TerrainHeight = plMain.TerrainHeight;

            us.HealthCare = plMain.Services(PanelMain.sr.HealthCare);
            us.PoliceDepartment = plMain.Services(PanelMain.sr.PoliceDepartment);
            us.FireDepartment = plMain.Services(PanelMain.sr.FireDepartment);
            us.PublicTransport = plMain.Services(PanelMain.sr.PublicTransport);
            us.Education = plMain.Services(PanelMain.sr.Education);
            us.Electricity = plMain.Services(PanelMain.sr.Electricity);
            us.Water = plMain.Services(PanelMain.sr.Water);
            us.Garbage = plMain.Services(PanelMain.sr.Garbage);
            us.Beautification = plMain.Services(PanelMain.sr.Beautification);
            us.Monument = plMain.Services(PanelMain.sr.Monument);
            us.Abandoned = plMain.Services(PanelMain.sr.Abandoned);
            us.Burned = plMain.Services(PanelMain.sr.Burned);

            us.Chirper = plMain.Services(PanelMain.sr.Chirper);
            us.ShowDelete = ShowDelete;
            us.ShowUpdate = ShowUpdate;
            us.ShowTerrain = ShowTerrain;
            us.ShowServices = ShowServices;
            us.ShowDistricts = ShowDistricts;
            us.AdjustAreas = AdjustAreas;
            us.AdjustMoney = AdjustMoney;
            us.AllRoads = AllRoads;
            us.AutoDistroy = AutoDistroy;

            us.MaxAreas = MaxAreas;
            us.StartMoney = StartMoney;
            us.Save();
            
            m_settings = false;
        }

        private void GetSettings()
        {
            if (m_settings == true) { return; }
            m_settings = true;
            
            string loc = "Starting";

            //setting up our backup array
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

            try
            {
                plMain.To(PanelMain.up.Oneway, true, us.ToOneway);
                plMain.To(PanelMain.up.Medium, true, us.ToMedium);
                plMain.To(PanelMain.up.Highway, true, us.ToHighway);
                plMain.To(PanelMain.up.Large, true, us.ToLarge);
                plMain.To(PanelMain.up.Basic, true, us.ToBasic);
                plMain.From(PanelMain.up.Oneway, true, us.Oneway);
                plMain.From(PanelMain.up.Medium, true, us.Medium);
                plMain.From(PanelMain.up.Highway, true, us.Highway);
                plMain.From(PanelMain.up.Large, true, us.Large);
                plMain.From(PanelMain.up.Basic, true, us.Basic);

                plMain.Deletes(PanelMain.dl.Roads, true, us.Roads);
                plMain.Deletes(PanelMain.dl.Railroads, true, us.Railroads);
                plMain.Deletes(PanelMain.dl.Highways, true, us.Highways);
                plMain.Deletes(PanelMain.dl.PowerLines, true, us.PowerLines);
                plMain.Deletes(PanelMain.dl.WaterPipes, true, us.WaterPipes);
                plMain.Deletes(PanelMain.dl.HeatPipes, true, us.HeatPipes);
                plMain.Deletes(PanelMain.dl.Airplanes, true, us.Airplanes);
                plMain.Deletes(PanelMain.dl.Shipping, true, us.Shipping);
                plMain.Deletes(PanelMain.dl.Pedestrian, true, us.Pedestrian);
                plMain.Deletes(PanelMain.dl.Bicycle, true, us.Bicycle);
                plMain.Deletes(PanelMain.dl.Tram, true, us.Tram);
                plMain.Deletes(PanelMain.dl.Metro, true, us.Metro);

                plMain.Types(PanelMain.tp.Ground, true, us.Ground);
                plMain.Types(PanelMain.tp.Bridge, true, us.Bridge);
                plMain.Types(PanelMain.tp.Slope, true, us.Slope);
                plMain.Types(PanelMain.tp.Tunnel, true, us.Tunnel);
                plMain.Types(PanelMain.tp.Curve, true, us.Curve);

                plMain.Deletes(PanelMain.dl.Buildings, true, us.Buildings);
                plMain.Deletes(PanelMain.dl.Trees, true, us.Trees);
                plMain.Deletes(PanelMain.dl.Props, true, us.Props);

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
                plMain.Options(PanelMain.ops.Services, true, us.Services);
                loc = "Deletes";
                plMain.Options(PanelMain.ops.Deletes, true, us.Delete);
                loc = "Updates";
                plMain.Options(PanelMain.ops.Updates, true, us.Update);
                loc = "Terrain";
                plMain.Options(PanelMain.ops.Terrain, true, us.Terrain);
                loc = "Toggle";
                plMain.cbToggle.isChecked = us.Toggle;
                plMain.Options(PanelMain.ops.Districts, true, us.Districts);
                plMain.cbDistrictToggle.isChecked = us.DistrictToggle;
                loc = "TerrainHeight";
                plMain.TerrainHeight = us.TerrainHeight;

                plMain.Services(PanelMain.sr.HealthCare, true, us.HealthCare);
                plMain.Services(PanelMain.sr.PoliceDepartment, true, us.PoliceDepartment);
                plMain.Services(PanelMain.sr.FireDepartment, true, us.FireDepartment);
                plMain.Services(PanelMain.sr.PublicTransport, true, us.PublicTransport);
                plMain.Services(PanelMain.sr.Education, true, us.Education);
                plMain.Services(PanelMain.sr.Electricity, true, us.Electricity);
                plMain.Services(PanelMain.sr.Water, true, us.Water);
                plMain.Services(PanelMain.sr.Garbage, true, us.Garbage);
                plMain.Services(PanelMain.sr.Beautification, true, us.Beautification);
                plMain.Services(PanelMain.sr.Monument, true, us.Monument);
                plMain.Services(PanelMain.sr.Abandoned, true, us.Abandoned);
                plMain.Services(PanelMain.sr.Burned, true, us.Burned);
                loc = "Chirper";
                plMain.Services(PanelMain.sr.Chirper, true, us.Chirper);

                ShowDelete = us.ShowDelete;
                ShowUpdate = us.ShowUpdate;
                ShowTerrain = us.ShowTerrain;
                ShowServices = us.ShowServices;
                ShowDistricts = us.ShowDistricts;

                AdjustAreas = us.AdjustAreas;
                AdjustMoney = us.AdjustMoney;
                AllRoads = us.AllRoads;
                AutoDistroy = us.AutoDistroy;

                MaxAreas = us.MaxAreas;
                StartMoney = us.StartMoney;
                
                loc = "Chirp";
                //we need to toggle shown or not
                if (mode == LoadMode.NewGame || mode == LoadMode.LoadGame || mode == LoadMode.NewGameFromScenario)
                {
                    Chirp.Toggle(plMain.Services(PanelMain.sr.Chirper));
                }
                loc = "ZoneTool";
            }
            catch (Exception ex)
            {
                WriteError("Error in GetSettings loc: " + loc + " deltypes.Count: " + plMain.deletes.Count, ex);
            }
            //WriteLog("Leaving GetSettings plMain.Types(PanelMain.tp.Ground): " + plMain.Types(PanelMain.tp.Ground));
            m_settings = false;
        }
                       
        #endregion

        #region "Event Handlers"
        
        private void Button_Clicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            WriteLog("Setting plMain: " + !plMain.isVisible);
            plMain.isVisible = !plMain.isVisible;
            this.enabled = plMain.isVisible;
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

        #region "Services Toggles and Districts, Oh my!"

        #region "Apply Helpers"

        private bool checkMaxArea(Vector3 newMousePosition)
        {
            if ((m_startPosition - newMousePosition).sqrMagnitude > m_maxArea * 100000)
            {
                return false;
            }
            return true;
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

            DistrictManager dm = Singleton<DistrictManager>.instance;

            DistrictProperties dp = new DistrictProperties();
            try
            {
                byte id;
                bool result = dm.CreateDistrict(out id);

                dm.AreaModified(minX, minZ, maxX, maxZ, true);
            }
            catch (Exception ex)
            {
                WriteError("Error in ApplyDistrictsChange: ", ex);
            }
        }
        
        private void ApplyUndo()
        {
            if (UndoList.Count <= 0) { return; }
            WriteLog("Entring ApplyUndo");
            //remove the current changes from the list (there are none)
            UndoStroke undoStroke = UndoList[UndoList.Count - 1];
            UndoList.RemoveAt(UndoList.Count - 1);

            int minX = undoStroke.minX;
            int maxX = undoStroke.maxX;
            int minZ = undoStroke.minZ;
            int maxZ = undoStroke.maxZ;
            int pointer = undoStroke.pointer;

            for (int i = minZ; i <= maxZ; i++)
            {
                for (int j = minX; j <= maxX; j++)
                {
                    int num = i * 1081 + j;            
                    m_rawHeights[num] = undoStroke.backupHeights[num];      //we want the new/raw to be the back up (un do)                   
                    m_backupHeights[num] = undoStroke.originalHeights[num]; //we want the prior backup to match the original (original as in one step back)
                }
            }

            //Apply Undo in 120 point sections
            for (int i = minZ; i <= maxZ; i++)
            {
                for (int j = minX; j <= maxX; j++)
                {
                    int x1 = j;
                    int x2 = Math.Max(i + 119, maxX);
                    int z1 = i;
                    int z2 = Math.Max(j + 119, maxZ);
                    TerrainModify.UpdateArea(x1, z1, x2, z2, true, false, false);
                    TerrainModify.BeginUpdateArea();

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
            WriteLog("Leaving ApplyUndo");
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