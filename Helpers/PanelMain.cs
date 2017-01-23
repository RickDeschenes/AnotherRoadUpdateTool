using ColossalFramework.UI;
using ICities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace AnotherRoadUpdateTool.Helpers
{
    class PanelMain : UIPanel
    {

        #region Declarations

        #region enums

        internal enum tp
        {
            Ground = 0,
            Bridge = 1,
            Slope = 2,
            Tunnel = 3,
            Curve = 4
        }

        internal enum up
        {
            Basic = 0,
            Highway = 1,
            Large = 2,
            Medium = 3,
            Oneway = 4
        }

        internal enum dl
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

        internal enum ops
        {
            Updates = 0,
            Deletes = 1,
            Services = 2,
            Terrain = 3,
            Districts = 4
        }

        internal enum sr
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

        #endregion //enumerations

        #region private Variables

        private string m_available = "available.";
        private string m_unavailable = "unavailable.";
        private string m_updatetool = "Another Road Update Tool - Selection is ";
        private string m_defaultInfo = "Watch for additional information.";
        private string fromSelected = string.Empty;
        private string toSelected = string.Empty;

        private LoadMode mode;

        private bool mouseDown;
        private bool m_updates;
        private bool m_fromTypes;
        private bool m_toTypes;
        private bool m_toRoads;
        private bool m_fromRoads;

        private bool m_selectable;

        private double m_terrainHeight;


        #region String Arrays

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

        private string[] m_incraments = new string[] { "1", "10", "50", "100", "250", " 500" };

        #endregion //String Arrays

        #region Lists

        private List<UICheckBox> panels = new List<UICheckBox>();
        private List<UICheckBox> options = new List<UICheckBox>();
        private List<UICheckBox> services = new List<UICheckBox>();

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

        #endregion //Lists

        #endregion // private Variables

        #region internal

        internal LoadMode Mode { get { return mode; } set { mode = value; } }

        internal bool Selectable { get { return m_selectable; } set { m_selectable = value; } }

        internal string FromSelected { get { return fromSelected; } set { fromSelected = value; } }

        internal string ToSelected { get { return toSelected; } set { toSelected = value; } }

        internal double TerrainHeight
        {
            get { return m_terrainHeight; }
            set
            {
                if (lHeight != null)
                {
                    lHeight.text = value.ToString("0.00");
                    if (tfTerrainHeight != null)
                        tfTerrainHeight.text = lHeight.text;
                }
                m_terrainHeight = value;
            }
        }

        internal int Height;
        internal int Width;

        internal UIPanel plOptions;
        internal UIPanel plTypes;
        internal UIPanel plRoads;
        internal UIPanel plDelete;
        internal UIPanel plServices;
        internal UIPanel plBasic;
        internal UIPanel plHighway;
        internal UIPanel plLarge;
        internal UIPanel plMedium;
        internal UIPanel plOneway;
        internal UIPanel plToBasic;
        internal UIPanel plToHighway;
        internal UIPanel plToLarge;
        internal UIPanel plToMedium;
        internal UIPanel plToOneway;
        internal UIPanel plTerrain;
        internal UIPanel plDistricts;

        internal UILabel lLines;
        internal UILabel lProperties;
        internal UILabel lSelectable;
        internal UILabel lInformation;
        internal UILabel lHeight;
        internal UILabel lbTitle;

        internal UIButton btHelp;
        internal UIButton btHide;
        internal UIButton btValidate;
        internal UICheckBox cbToggle;
        internal UICheckBox cbDistrictToggle;
        internal UITextField tfTerrainHeight;

        internal List<UICheckBox> deletes = new List<UICheckBox>();
        internal List<UICheckBox> types = new List<UICheckBox>();

        #endregion //Internal Variables

        #endregion "Declarations"

        #region "Internal/Public Code"

        #region Class

        public override void Awake()
        {
            ARUT.WriteLog("Main Awake!");
            base.Awake();
        }

        public override void OnDestroy()
        {
            ARUT.WriteLog("Main OnDestroy!");
            base.OnDestroy();
        }

        public override void OnDisable()
        {
            ARUT.WriteLog("Main OnDisable!");
            base.OnDisable();
        }

        public override void OnEnable()
        {
            ARUT.WriteLog("Main OnEnable!");
            base.OnEnable();
        }

        public override void Start()
        {
            ARUT.WriteLog("Main Start!");
            base.Start();
        }
        
        public PanelMain()
        {

        }

        #endregion //Class

        #region CreateMain

        internal void CreateObjects()
        {
            //ARUT.WriteLog("setting anchor style");

            //if (anchor != null)
            //    this.anchor = UIAnchorStyle.None;

            ARUT.WriteLog("setting mode");
            if (mode == LoadMode.LoadMap || mode == LoadMode.NewMap)
                this.backgroundSprite = "GenericPanel";
            else
                this.backgroundSprite = "SubcategoriesPanel";

            int height = 525;

            this.size = new Vector2(575, height);

            //ARUT.WriteLog("setting lbTitle");
            lbTitle = addLabel(this, 1, 1, "ARUT - Another 'Road' Update Tool", "All your Mods in one control", false);
            lbTitle.autoSize = false;
            lbTitle.isVisible = true;
            lbTitle.outlineColor = Color.red;
            lbTitle.outlineSize = 2;
            lbTitle.bottomColor = Color.yellow;
            lbTitle.width = this.width;
            lbTitle.height = 20;
            lbTitle.textScale = 0.9f;
            lbTitle.textAlignment = UIHorizontalAlignment.Center;
            lbTitle.verticalAlignment = UIVerticalAlignment.Middle;
            lbTitle.eventMouseDown += Title_eventMouseDown;
            lbTitle.eventMouseMove += Title_eventMouseMove;
            lbTitle.eventMouseUp += Title_eventMouseUp;

            //ARUT.WriteLog("setting btHelp");
            string tooltip = "I will try to open a pdf file, if you do not have a viewer.... do not click.";
            btHelp = addButton(this, "¿", tooltip, (int)this.height - 25, (int)this.width - 25, 25, 25);
            btHelp.isVisible = true;
            //btHelp.zOrder = 0;
            btHelp.eventDoubleClick += btHelp_eventDoubleClick;

            //ARUT.WriteLog("setting btHide");
            tooltip = "I will try to close the option panel.";
            btHide = addButton(this, "Close", tooltip, (int)this.height - 25, 1, 75, 25);
            btHide.isVisible = true;
            //btHide.zOrder = 0;
            btHide.eventClick += btHide_eventClick;

            //ARUT.WriteLog("setting Mouse and Size events");
            this.eventSizeChanged += Main_eventSizeChanged;

            //this.relativePosition = new Vector2(575, 525);

            ARUT.WriteLog("Going to create panels");
            //Create the panels (Little like a tab view)
            height = CreatePanels(this);
            ARUT.WriteLog("Created panels! Be good");
        }

        #endregion //CreateMain

        #region CheckBox values

        internal bool Options(ops option, bool set = false, bool value = false)
        {
            if (set == false) { return options[(int)option].isChecked; }
            else { options[(int)option].isChecked = value; }
            return options[(int)option].isChecked;
        }

        internal bool Types(tp type, bool set = false, bool value = false)
        {
            if (set == false) { return types[(int)type].isChecked; }
            else { types[(int)type].isChecked = value; }
            return types[(int)type].isChecked;
        }

        internal bool Services(sr service, bool set = false, bool value = false)
        {
            if (set == false) { return services[(int)service].isChecked; }
            else { services[(int)service].isChecked = value; }
            return services[(int)service].isChecked;
        }
        
        internal bool Deletes(dl delete, bool set = false, bool value = false)
        {
            if (set == false) { return deletes[(int)delete].isChecked; }
            else { deletes[(int)delete].isChecked = value; }
            return deletes[(int)delete].isChecked;
        }

        internal bool From(up from, bool set = false, bool value = false)
        {
            if (set == false) { return fromTypes[(int)from].isChecked; }
            else { fromTypes[(int)from].isChecked = value; }
            return fromTypes[(int)from].isChecked;
        }

        internal bool To(up to, bool set = false, bool value = false)
        {
            if (set == false) { return toTypes[(int)to].isChecked; }
            else { toTypes[(int)to].isChecked = value; }
            return toTypes[(int)to].isChecked;
        }

        #endregion //CheckBoxes

        #endregion "Internal/Public Code"

        #region "Private Code"

        #region "Form Move Events"

        private void Main_eventSizeChanged(UIComponent component, Vector2 value)
        { lbTitle.width = this.width; }

        private void Title_eventMouseUp(UIComponent component, UIMouseEventParameter eventParam)
        { mouseDown = false; }

        private void Title_eventMouseMove(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (mouseDown == true)
            {
                try
                {
                    // Move the top and left according to the delta amount
                    Vector3 delta = new Vector3(eventParam.moveDelta.x, eventParam.moveDelta.y);
                    //Just move the Panel
                    this.position += delta;

                    //record the new left and top
                    
                }
                catch (Exception ex)
                {
                    ARUT.WriteError("Error in MouseMove: ", ex);
                }
            }
        }

        private void Title_eventMouseDown(UIComponent component, UIMouseEventParameter eventParam)
        { mouseDown = true; }

        #endregion "Form Move Events"
        
        #region "GUI Layout"

        #region "Create Panels"

        private int CreatePanels(UIPanel panel)
        {
            int plx = 1;
            int options = 20;
            int services = 0;
            int types = 0;
            int roads = 0;

            services = GenerateOptions(panel, options, plx);    //Options
            types = GenerateplTypes(panel, services, plx);      //Types
            roads = GenerateplRoads(panel, types, plx);         //Updates
            GenerateplDelete(panel, types, plx);                //Deletes
            GenerateplServices(panel, services, plx);           //Services
            GenerateplTerrain(panel, services, plx);            //Terrain
            GenerateplDistricts(panel, services, plx);          //Districts

            GenerateRoadPanels(panel, ref plBasic, ref plToBasic, fromBasic, toBasic, m_basic, "Basic", roads, plx);
            GenerateRoadPanels(panel, ref plHighway, ref plToHighway, fromHighway, toHighway, m_highway, "Highway", roads, plx);
            GenerateRoadPanels(panel, ref plLarge, ref plToLarge, fromLarge, toLarge, m_large, "Large", roads, plx);
            GenerateRoadPanels(panel, ref plMedium, ref plToMedium, fromMedium, toMedium, m_medium, "Medium", roads, plx);
            GenerateRoadPanels(panel, ref plOneway, ref plToOneway, fromOneway, toOneway, m_oneway, "Oneway", roads, plx);

            return 525;
        }

        private int GenerateOptions(UIPanel panel, int ply, int plx)
        {
            ARUT.WriteLog("Entering GenerateOptions");
            plOptions = panel.AddUIComponent<UIPanel>();
            plOptions.relativePosition = new Vector3(plx, ply);
            plOptions.isVisible = true;
            plOptions.tooltip = "Select the type of updates and options to perform.";

            int y = 1;
            lSelectable = addLabel(plOptions, y, plx, m_updatetool + m_unavailable, true);
            y += 25;
            lInformation = addLabel(plOptions, y, 1, m_defaultInfo, true);
            y += 25;

            int cb = 0;
            foreach (string s in m_options)
            {
                bool enable = true;
                string t = String.Format("Select to display the {0} options", s);
                options.Add(addCheckbox(plOptions, y, plx, s, t, true));
                //Space out the options (We may add building, trees, and props)
                plx += 100;
                switch (s)
                {
                    case "Update":
                        enable = ARUT.ShowUpdate;
                        break;
                    case "Delete":
                        enable = ARUT.ShowDelete;
                        break;
                    case "Districts":
                        enable = (ARUT.ShowDistricts == (mode != LoadMode.LoadMap && mode != LoadMode.NewMap));
                        break;
                    case "Terrain":
                        enable = (ARUT.ShowTerrain == (mode == LoadMode.LoadMap || mode == LoadMode.NewMap));
                        break;
                    case "Services":
                        enable = (ARUT.ShowServices == (mode != LoadMode.LoadMap && mode != LoadMode.NewMap));
                        break;
                    default:
                        break;
                }
                options[cb].enabled = enable;
                options[cb].eventCheckChanged += Options_eventCheckChanged;
                cb += 1;
            }
            y += 25;

            //set the panal size (two rows, 50)
            plOptions.size = new Vector2(panel.width, y);

            return (int)plOptions.height + 20;
        }

        private int GenerateplTypes(UIPanel panel, int ply, int plx)
        {
            plTypes = panel.AddUIComponent<UIPanel>();
            plTypes.relativePosition = new Vector3(1, ply);
            plTypes.isVisible = true;
            plTypes.tooltip = "Select the line types to modify";

            int y = 1;
            addLabel(plTypes, y, 1, "Select the line types to modify", true);
            y += 25;
            int x = 5;
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
            y += 20;
            plTypes.size = new Vector2(panel.width, y);

            return ply + y;
        }

        private int GenerateplRoads(UIPanel panel, int ply, int plx)
        {
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
                        ARUT.WriteError("Error in GenerateplTypes: ", ex);
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
            //return the top of the roads panels
            return ply + y;
        }

        private void GenerateplDelete(UIPanel panel, int ply, int plx)
        {
            //Show the road type option
            plDelete = panel.AddUIComponent<UIPanel>();
            plDelete.relativePosition = new Vector3(1, ply);
            plDelete.isVisible = false;
            plDelete.tooltip = "Select the type of items to delete.";

            int cb = 0;
            int x = 15;
            int y = 1;

            //load the bulldoze road type options
            addLabel(plDelete, y, 5, "Select your delete options.", true);

            y += 20;
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
        }

        private void GenerateplServices(UIPanel panel, int ply, int plx)
        {
            //Show the road type option
            plServices = panel.AddUIComponent<UIPanel>();
            plServices.relativePosition = new Vector3(1, ply);
            plServices.isVisible = false;
            plServices.tooltip = "Select the type of services to toggle on and off.";

            int cb = 0;
            int y = 1;

            for (int i = 0; i < m_services.Length; i++)
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
                y += 25;
            }
            //no need for this dropdown
            plServices.size = new Vector2(panel.width, y + 25);
        }

        private void GenerateplTerrain(UIPanel panel, int ply, int plx)
        {
            //Show the road type option
            plTerrain = panel.AddUIComponent<UIPanel>();
            plTerrain.relativePosition = new Vector3(1, ply);
            plTerrain.isVisible = false;
            plTerrain.tooltip = "Select or enter the height desired.";

            int x = 1;
            int y = 1;
            int inset = 65;

            string s = "Enter a value for the new terrain height";
            string t = "Enter the height desired.";
            addLabel(plTerrain, y, x, s, t, true);

            y += 25;
            s = "Current: ";
            t = "Look here for current new Terrain height.";
            addLabel(plTerrain, y, x, s, t, true);

            s = "0.00";
            lHeight = addLabel(plTerrain, y, x + inset, s, t, true);
            lHeight.autoSize = false;
            lHeight.width = 100;
            lHeight.height = 50;
            lHeight.textAlignment = UIHorizontalAlignment.Right;

            y += 25;
            s = "Height";
            addLabel(plTerrain, y, x, s, t, true);

            tfTerrainHeight = addTextBox(plTerrain, "TerrainHeight", "0,00", y, x + inset, 100, 25, "Use values between 2000 and 0.0", true, true);
            tfTerrainHeight.horizontalAlignment = UIHorizontalAlignment.Right;
            tfTerrainHeight.eventKeyDown += TerrainHeight_eventKeyDown;
            tfTerrainHeight.eventTextChanged += TerrainHeight_eventTextChanged;

            btValidate = addButton(plTerrain, "Validate", "Validate the number in the text field.", y, x + inset + 105, 125, 25);
            btValidate.isVisible = true;
            btValidate.state = UIButton.ButtonState.Disabled;
            btValidate.eventClick += Validate_eventClick;

            y += 25;
            s = "Game Height";
            t = "Click to load in the current Games set Terrain Height";
            UIButton GameHeight = addButton(plTerrain, s, t, y, x, 100, 25);
            GameHeight.eventClick += GameHeight_eventClick;
        }

        private void GenerateplDistricts(UIPanel panel, int ply, int plx)
        {
            //ARUT.WriteLog("Entering GenerateplDistricts");

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
        }
        
        #region "Adding Controls"

        private UILabel addLabel(UIPanel panel, int yPos, int xPos, string text, bool hidden)
        {
            return addLabel(panel, yPos, xPos, text, "", hidden);
        }

        private UILabel addLabel(UIPanel panel, int yPos, int xPos, string text, string t, bool hidden)
        {
            UILabel lb = panel.AddUIComponent<UILabel>();
            lb.relativePosition = new Vector3(xPos, yPos);
            lb.height = 0;
            lb.width = 80;
            lb.text = text;
            lb.tooltip = t;
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
            
            return cb;
        }

        private UIDropDown addDropDown(UIPanel panel, int y, int x, int w, int h, string text, string tooltip)
        {
            UIDropDown dd = panel.AddUIComponent<UIDropDown>();

            dd.size = new Vector2(w, h);
            dd.relativePosition = new Vector3(x, y);
            dd.listBackground = "GenericPanelLight";
            dd.itemHeight = 15;
            dd.itemHover = "ListItemHover";
            dd.itemHighlight = "ListItemHighlight";
            dd.normalBgSprite = "ListItemHover";
            dd.listWidth = 100;
            dd.listHeight = 350;
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

        private UISlider addSlider(UIPanel panel, string name, int y, int x, int w, int h, float min, float max, float step, float defaultValue, string tooltip)
        {
            UISlider sl = panel.AddUIComponent<UISlider>();

            sl.relativePosition = new Vector3(x, y);
            sl.name = name;
            sl.width = w;
            sl.height = h;
            sl.tooltip = tooltip;
            sl.minValue = min;
            sl.maxValue = max;
            sl.stepSize = step;
            sl.value = defaultValue;
            sl.isVisible = true;
            sl.color = Color.blue;
            sl.BringToFront();
            if (mode == LoadMode.LoadMap || mode == LoadMode.NewMap)
                sl.backgroundSprite = "SubcategoriesPanel";
            else
                sl.backgroundSprite = "GenericPanel";

            return sl;
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
            bt.relativePosition = new Vector3(x, y);
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

            //ARUT.WriteLog("Leaving addButton: " + text + " location: " + bt.relativePosition + "main HeightxWidth: " + panel.height + "x" + panel.width);
            return bt;
        }

        #endregion

        #endregion "Create Panels"

        #endregion "GUI Layout"

        #region Event Handlers
        
        private void GameHeight_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            string found = "TSBar";
            try
            {
                UISlicedSprite TSBar = (UISlicedSprite)UIView.GetAView().FindUIComponent("TSBar");
                found = "OptionsBar";
                UIPanel OptionsBar = (UIPanel)TSBar.Find("OptionsBar");
                found = "LevelHeight";
                UIPanel LevelHeight = (UIPanel)OptionsBar.Find("LevelHeightPanel");
                found = "Settings";
                UIPanel Settings = (UIPanel)LevelHeight.Find("Settings");
                found = "Height";
                UISlider Height = (UISlider)Settings.Find("Height");
                found = "Parsing " + Height.value;
                float zero = Height.value;
                ARUT.WriteLog("zero: " + zero.ToString());
                if (zero > 0) { zero = 0; }

                tfTerrainHeight.text = zero.ToString("0.00");
                ARUT.WriteLog("tfTerrainHeight.text: " + zero.ToString("0.00"));
            }
            catch (Exception ex)
            {
                ARUT.WriteError("Object Not found in MyITerrain.TerrainHeight. " + found, ex);
            }
        }

        private void btHide_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            //we need to end all options, turn off select (Updates, Deletes, Terrain, Services)
            this.isVisible = !this.isVisible;
        }

        private void TerrainHeight_eventTextChanged(UIComponent component, string value)
        {
            SetTerrainEnabled();
        }

        private void TerrainHeight_eventKeyDown(UIComponent component, UIKeyEventParameter eventParam)
        {
            if ((eventParam.keycode >= KeyCode.Alpha0 && eventParam.keycode <= KeyCode.Alpha9) || eventParam.keycode <= KeyCode.Period) { }
            else
            { eventParam = null; }
        }

        private void Validate_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (btValidate.state == UIButton.ButtonState.Disabled) { return; }

            string raw = tfTerrainHeight.text;
            double val;
            if (double.TryParse(raw, out val) == true)
            {
                if (val > 1024)
                {
                    tfTerrainHeight.text = "1024.00";
                }
                else if (val < 0)
                {
                    tfTerrainHeight.text = "0.00";
                }
                else
                {
                    tfTerrainHeight.text = val.ToString("0.00");
                }
                m_terrainHeight = (float)val;
                lHeight.text = val.ToString("0.00");
                SetTerrainEnabled();
            }
        }

        private void Types_eventCheckChanged(UIComponent component, bool value)
        {
            if (plRoads.isVisible)
            {
                SetUpdateEnabled();
            }
            else
            {
                SetSelectionEnabled();
            }
        }

        private void ServiceTypes_eventCheckChanged(UIComponent component, bool value)
        {
            //do we reenble or disable chirper
            UICheckBox cb = (UICheckBox)component;

            if (cb.text == "Chirper")
            {
                //we need to toggle shown or not
                ARUT.SetChirper(value);
                return;
            }

            if (cb.text == "Abandoned")
            {
                //Toggle tha value as needed
                if (ARUT.DemolishAbandoned.@value != value)
                    ARUT.DemolishAbandoned.@value = !ARUT.DemolishAbandoned.@value;
                return;
            }

            if (cb.text == "Burned")
            {
                //Toggle tha value as needed
                if (ARUT.DemolishBurned.@value != value)
                    ARUT.DemolishBurned.@value = !ARUT.DemolishBurned.@value;
                return;
            }

            SetServicesEnabled();
        }

        private void btHelp_eventDoubleClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream("AnotherRoadUpdate.ARUT.pdf");
                if (stream == null)
                {
                    //ARUT.WriteLog("Error loading embeded resource AnotherRoadUpdate.ARUT.pdf");
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
                ARUT.WriteError("Error in btHelp_eventDoubleClick ", ex);
            }
        }

        private void DeleteTypes_eventCheckChanged(UIComponent component, bool value)
        {
            if (m_updates == true) { return; }
            m_updates = true;
            
            UICheckBox c = (UICheckBox)component;
            //store the update
            if (c.text == deletes[(int)dl.Roads].text) { ARUT.us.Roads = value; }
            if (c.text == deletes[(int)dl.Railroads].text) { ARUT.us.Railroads = value; }
            if (c.text == deletes[(int)dl.Highways].text) { ARUT.us.Highways = value; }
            if (c.text == deletes[(int)dl.PowerLines].text) { ARUT.us.PowerLines = value; }
            if (c.text == deletes[(int)dl.WaterPipes].text) { ARUT.us.WaterPipes = value; }
            if (c.text == deletes[(int)dl.HeatPipes].text) { ARUT.us.HeatPipes = value; }
            if (c.text == deletes[(int)dl.Airplanes].text) { ARUT.us.Airplanes = value; }
            if (c.text == deletes[(int)dl.Shipping].text) { ARUT.us.Shipping = value; }

            if (c.text == deletes[(int)dl.Buildings].text) { ARUT.us.Buildings = value; }
            if (c.text == deletes[(int)dl.Props].text) { ARUT.us.Props = value; }
            if (c.text == deletes[(int)dl.Trees].text) { ARUT.us.Trees = value; }

            if (c.text == types[(int)tp.Ground].text) { ARUT.us.Ground = value; }
            if (c.text == types[(int)tp.Bridge].text) { ARUT.us.Bridge = value; }
            if (c.text == types[(int)tp.Tunnel].text) { ARUT.us.Tunnel = value; }
            if (c.text == types[(int)tp.Slope].text) { ARUT.us.Tunnel = value; }
            if (c.text == types[(int)tp.Curve].text) { ARUT.us.Tunnel = value; }

            //set enabled
            SetSelectionEnabled();
            m_updates = false;
        }

        private void Options_eventCheckChanged(UIComponent component, bool value)
        {
            if (m_updates == true) { return; }
            m_updates = true;

            UICheckBox c = (UICheckBox)component;

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
                    SetSelectionEnabled();
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
        }

        private void FromTypes_eventCheckChanged(UIComponent component, bool value)
        {
            if (m_fromTypes == true) { return; }
            m_fromTypes = true;

            UICheckBox cb = (UICheckBox)component;

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
                ARUT.WriteError("Error in FromTypes_eventCheckChanged loc: " + loc + ".", ex);
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
        }

        private void ToTypes_eventCheckChanged(UIComponent component, bool value)
        {
            if (m_toTypes == true) { return; }
            m_toTypes = true;

            UICheckBox cb = (UICheckBox)component;

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
                ARUT.WriteError("Error in ToTypes_eventCheckChanged loc: " + loc + ".", ex);
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
        }

        private void FromRoad_eventCheckChanged(UIComponent component, bool value)
        {
            if (m_fromRoads == true) { return; }
            m_fromRoads = true;

            UICheckBox cb = (UICheckBox)component;

            //ARUT.WriteLog("Entering FromRoads_eventCheckChanged: Option: " + cb.text);

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

            //ARUT.WriteLog("Leaving FromRoads_eventCheckChanged: Option: " + cb.text);
        }

        private void ToRoad_eventCheckChanged(UIComponent component, bool value)
        {
            //ARUT.WriteLog("Entering ToRoads_eventCheckChanged: m_toRoads: " + m_toRoads);
            if (m_toRoads == true) { return; }
            m_toRoads = true;

            UICheckBox cb = (UICheckBox)component;

            //ARUT.WriteLog("Entering ToRoads_eventCheckChanged: Option: " + cb.text);

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
            //ARUT.WriteLog("Leaving ToRoads_eventCheckChanged: Option: " + cb.text);
        }

        #endregion "Event Handlers"

        #region "Event Process helpers"

        internal void RefreshView()
        {
            int cb = 0;
            foreach (string s in m_options)
            {
                bool enable = true;
                switch (s)
                {
                    case "Update":
                        enable = ARUT.ShowUpdate;
                        break;
                    case "Delete":
                        enable = ARUT.ShowDelete;
                        break;
                    case "Districts":
                        enable = (ARUT.ShowDistricts == (mode != LoadMode.LoadMap && mode != LoadMode.NewMap));
                        break;
                    case "Terrain":
                        enable = (ARUT.ShowTerrain == (mode == LoadMode.LoadMap || mode == LoadMode.NewMap));
                        break;
                    case "Services":
                        enable = (ARUT.ShowServices == (mode != LoadMode.LoadMap && mode != LoadMode.NewMap));
                        break;
                    default:
                        break;
                }
                options[cb].enabled = enable;
                cb += 1;
            }
        }

        private void SetServicesEnabled()
        {
            //ARUT.WriteLog("Entering: SetServicesEnabled m_selectable: " + m_selectable);
            try
            {
                m_selectable = false;
                lSelectable.text = m_updatetool + m_unavailable;

                //do we have at least one service type selected
                if (services.Any(o => o.isChecked == true)) { }
                else
                    return;

                //ok all checks complete
                m_selectable = true;
                lSelectable.text = m_updatetool + m_available;
            }
            catch (Exception ex)
            {
                ARUT.WriteError("Error in SetServicesEnabled Exception: ", ex);
            }
            //ARUT.WriteLog("Leaving: SetServicesEnabled m_selectable: " + m_selectable);
        }

        private void SetTerrainEnabled()
        {
            //ARUT.WriteLog("Entering SetTerrainEnabled m_selectable: " + m_selectable);
            if (lHeight.text == tfTerrainHeight.text)
            {
                m_selectable = true;
                lSelectable.text = m_updatetool + m_available;
                lInformation.text = m_defaultInfo;
            }
            else
            {
                m_selectable = false;
                lSelectable.text = m_updatetool + m_unavailable;
                lInformation.text = "Use the Validate button to validate your input";
            }
            //ARUT.WriteLog("Leaving SetTerrainEnabled m_selectable: " + m_selectable);
        }

        private void SetDistrictsEnabled()
        {
            //ARUT.WriteLog("Entering: SetDistrictsEnabled m_selectable: " + m_selectable);
            try
            {
                m_selectable = true;
                lSelectable.text = m_updatetool + m_available;
            }
            catch (Exception ex)
            {
                ARUT.WriteError("Error in SetDistrictsEnabled Exception: ", ex);
            }
            //ARUT.WriteLog("Leaving: SetDistrictsEnabled m_selectable: " + m_selectable);
        }

        private void SetUpdateEnabled()
        {
            //ARUT.WriteLog("Entering: SetUpdateEnabled m_selectable: " + m_selectable);
            try
            {
                m_selectable = false;
                lSelectable.text = m_updatetool + m_unavailable;

                //is Updated enabled?
                if (options[(int)ops.Updates].isChecked == false)
                {
                    //ARUT.WriteLog("Leaving: SetUpdateEnabled 'Update not checked' m_selectable: " + m_selectable);
                    return;
                }

                //do we have a to road type
                if (types.Any(o => o.isChecked == true)) { }
                else
                {
                    //ARUT.WriteLog("Leaving: SetUpdateEnabled ' No types selected' m_selectable: " + m_selectable);
                    return;
                }

                //do we have a from road type
                if (fromTypes.Any(o => o.isChecked == true)) { }
                else
                {
                    //ARUT.WriteLog("Leaving: SetUpdateEnabled 'No to road type selected' m_selectable: " + m_selectable);
                    return;
                }

                //do we have a to road type
                if (toTypes.Any(o => o.isChecked == true)) { }
                else
                {
                    //ARUT.WriteLog("Leaving: SetUpdateEnabled 'No from road type selected' m_selectable: " + m_selectable);
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
                    //ARUT.WriteLog("Leaving: SetUpdateEnabled 'No to road selected'm_selectable: " + m_selectable);
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
                    //ARUT.WriteLog("Leaving: SetUpdateEnabled 'No from road selected' m_selectable: " + m_selectable);
                    return;
                }

                //ok all checks complete
                m_selectable = true;
                lSelectable.text = m_updatetool + m_available;
            }
            catch (Exception ex)
            {
                ARUT.WriteError("Error in Error in SetUpdateEnabled ", ex);
            }
            //ARUT.WriteLog("Leaving: SetUpdateEnabled m_selectable: " + m_selectable);
        }

        private void SetSelectionEnabled()
        {
            //ARUT.WriteLog("Entering SetSelectionEnabled selectable is: " + m_selectable);
            try
            {
                m_selectable = false;
                lSelectable.text = m_updatetool + m_unavailable;

                //is delete enabled?
                if (options[(int)ops.Deletes].isChecked == false)
                {
                    //ARUT.WriteLog("Leaving: SetSelectionEnabled 'Deletes not checked' m_selectable: " + m_selectable);
                    return;
                }

                //do we have any types
                if (types.Any(o => o.isChecked == true)) { }
                else
                {
                    //ARUT.WriteLog("Leaving: SetSelectionEnabled 'No types selected' m_selectable: " + m_selectable);
                    return;
                }
                //do we have anything checked to delete?
                if (deletes.Any(o => o.isChecked == true)) { }
                else
                {
                    ARUT.WriteLog("Leaving: SetSelectionEnabled 'No item selected to delete' m_selectable: " + m_selectable);
                    return;
                }
                m_selectable = true;
                lSelectable.text = m_updatetool + m_available;
            }
            catch (Exception ex)
            {
                ARUT.WriteError("Error in SetSelectionEnabled Exception: ", ex);
            }
            //ARUT.WriteLog("Leaving SetSelectionEnabled selectable is: " + m_selectable);
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
            //ARUT.WriteLog("Entering UpdateDisplayedRoads " + xPos);
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
                ARUT.WriteLog("Could not define this object type.");
            //ARUT.WriteLog("Leaving UpdateDisplayedRoads");
        }

        private void DisplayCheckBoxes(List<UICheckBox> roads, int xPos, string test, bool show)
        {
            //ARUT.WriteLog("Entering DisplayCheckBoxes");

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
            //ARUT.WriteLog("Leaving UpdateDisplayedRoads");
        }

        #endregion

        #endregion "Private Code"
    }
}
