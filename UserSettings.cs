using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Text;

namespace AnotherRoadUpdateTool
{
    internal class UserSettings
    {
        #region Declarations

        private string[] settings = new string[] { "Basic", "Large", "Highway", "Medium", "Oneway", "ToBasic", "ToLarge", "ToHighway", "ToMedium", "ToOneway", "Roads", "Railroads", "Highways", "PowerLines", "WaterPipes", "HeatPipes", "Airplanes", "Shipping", "Pedestrian", "Bicycle", "Tram", "Metro", "Buildings", "Trees", "Props", "Ground", "Bridge", "Slope", "Tunnel", "Curve", "Update", "Delete", "Services", "Toggle", "Terrain", "TerrainHeight", "Districts", "DistrictToggle" };

        private bool _basic;
        private bool _large;
        private bool _highway;
        private bool _medium;
        private bool _oneway;
        private bool _toBasic;
        private bool _toLarge;
        private bool _toHighway;
        private bool _toMedium;
        private bool _toOneway;
        private bool _roads;
        private bool _railroads;
        private bool _highways;
        private bool _powerLines;
        private bool _waterpipes;
        private bool _heatpipes;
        private bool _airplanes;
        private bool _shipping;
        private bool _pedestrian;
        private bool _bicycle;
        private bool _tram;
        private bool _metro;
        private bool _buildings;
        private bool _trees;
        private bool _props;
        private bool _ground;
        private bool _bridge;
        private bool _slope;
        private bool _tunnel;
        private bool _curve;
        private bool _update;
        private bool _delete;
        private bool _services;
        private bool _toggle;
        private bool _districts;
        private bool _districttoggle;
        private bool _terrain;
        private bool _HealthCare;
        private bool _PoliceDepartment;
        private bool _FireDepartment;
        private bool _PublicTransport;
        private bool _Education;
        private bool _Electricity;
        private bool _Water;
        private bool _Garbage;
        private bool _Beautification;
        private bool _Monument;
        private bool _abandoned;
        private bool _burned;
        private double _terrainheight;

        public bool Basic { get { return _basic; } set { _basic = value; } }
        public bool Large { get { return _large; } set { _large = value; } }
        public bool Highway { get { return _highway; } set { _highway = value; } }
        public bool Medium { get { return _medium; } set { _medium = value; } }
        public bool Oneway { get { return _oneway; } set { _oneway = value; } }
        public bool ToBasic { get { return _toBasic; } set { _toBasic = value; } }
        public bool ToLarge { get { return _toLarge; } set { _toLarge = value; } }
        public bool ToHighway { get { return _toHighway; } set { _toHighway = value; } }
        public bool ToMedium { get { return _toMedium; } set { _toMedium = value; } }
        public bool ToOneway { get { return _toOneway; } set { _toOneway = value; } }
        public bool Roads { get { return _roads; } set { _roads = value; } }
        public bool Railroads { get { return _railroads; } set { _railroads = value; } }
        public bool Highways { get { return _highways; } set { _highways = value; } }
        public bool PowerLines { get { return _powerLines; } set { _powerLines = value; } }
        public bool WaterPipes { get { return _waterpipes; } set { _waterpipes = value; } }
        public bool HeatPipes { get { return _heatpipes; } set { _heatpipes = value; } }
        public bool Airplanes { get { return _airplanes; } set { _airplanes = value; } }
        public bool Shipping { get { return _shipping; } set { _shipping = value; } }
        public bool Pedestrian { get { return _pedestrian; } set { _pedestrian = value; } }
        public bool Bicycle { get { return _bicycle; } set { _shipping = value; } }
        public bool Tram { get { return _tram; } set { _tram = value; } }
        public bool Metro { get { return _metro; } set { _metro = value; } }
        public bool Buildings { get { return _buildings; } set { _buildings = value; } }
        public bool Trees { get { return _trees; } set { _trees = value; } }
        public bool Props { get { return _props; } set { _props = value; } }
        public bool Ground { get { return _ground; } set { _ground = value; } }
        public bool Bridge { get { return _bridge; } set { _bridge = value; } }
        public bool Slope { get { return _slope; } set { _slope = value; } }
        public bool Tunnel { get { return _tunnel; } set { _tunnel = value; } }
        public bool Curve { get { return _curve; } set { _curve = value; } }
        public bool Update { get { return _update; } set { _update = value; } }
        public bool Delete { get { return _delete; } set { _delete = value; } }
        public bool Services { get { return _services; } set { _services = value; } }
        public bool Toggle { get { return _toggle; } set { _toggle = value; } }
        public bool Districts { get { return _districts; } set { _districts = value; } }
        public bool DistrictToggle { get { return _districttoggle; } set { _districttoggle = value; } }
        public bool Terrain { get { return _terrain; } set { _terrain = value; } }
        public bool HealthCare { get { return _HealthCare; } set { _HealthCare = value; } }
        public bool PoliceDepartment { get { return _PoliceDepartment; } set { _PoliceDepartment = value; } }
        public bool FireDepartment { get { return _FireDepartment; } set { _FireDepartment = value; } }
        public bool PublicTransport { get { return _PublicTransport; } set { _PublicTransport = value; } }
        public bool Education { get { return _Education; } set { _Education = value; } }
        public bool Electricity { get { return _Electricity; } set { _Electricity = value; } }
        public bool Water { get { return _Water; } set { _Water = value; } }
        public bool Garbage { get { return _Garbage; } set { _Garbage = value; } }
        public bool Beautification { get { return _Beautification; } set { _Beautification = value; } }
        public bool Monument { get { return _Monument; } set { _Monument = value; } }
        public bool Abandoned { get { return _abandoned; } set { _abandoned = value; } }
        public bool Burned { get { return _burned; } set { _burned = value; } }
        public double TerrainHeight { get { return _terrainheight; } set { _terrainheight = value; } }

        private const string fileName = "ARUTuserSettings.xml";
        private string us = fileName;
        private XmlDocument xml = new XmlDocument();

        #endregion

        public UserSettings()
        {
            bool create = true;

            //Do we have a file?
            if (File.Exists(us))
            {
                xml.Load(us);
                //was it any good?
                create = (xml.SelectSingleNode("UserSettings") == null);
            }
            //if we need to create a new file
            if (create)
            {
                CreateSettings();
            }
            FillSettings();
        }

        public void Save()
        {
            int loc = 0;
            try
            {
                xml.SelectSingleNode("UserSettings/Basic").InnerText = Basic.ToString();
                xml.SelectSingleNode("UserSettings/Large").InnerText = Large.ToString();
                xml.SelectSingleNode("UserSettings/Highway").InnerText = Highway.ToString();
                xml.SelectSingleNode("UserSettings/Medium").InnerText = Medium.ToString();
                xml.SelectSingleNode("UserSettings/Oneway").InnerText = Oneway.ToString();
                xml.SelectSingleNode("UserSettings/ToBasic").InnerText = ToBasic.ToString();
                xml.SelectSingleNode("UserSettings/ToLarge").InnerText = ToLarge.ToString();
                xml.SelectSingleNode("UserSettings/ToHighway").InnerText = ToHighway.ToString();
                xml.SelectSingleNode("UserSettings/ToMedium").InnerText = ToMedium.ToString();
                xml.SelectSingleNode("UserSettings/ToOneway").InnerText = ToOneway.ToString();
                xml.SelectSingleNode("UserSettings/Roads").InnerText = Roads.ToString();
                xml.SelectSingleNode("UserSettings/Railroads").InnerText = Railroads.ToString();
                xml.SelectSingleNode("UserSettings/Highways").InnerText = Highways.ToString();
                xml.SelectSingleNode("UserSettings/PowerLines").InnerText = PowerLines.ToString();
                xml.SelectSingleNode("UserSettings/WaterPipes").InnerText = WaterPipes.ToString();
                xml.SelectSingleNode("UserSettings/HeatPipes").InnerText = HeatPipes.ToString();
                xml.SelectSingleNode("UserSettings/Airplanes").InnerText = Airplanes.ToString();
                xml.SelectSingleNode("UserSettings/Shipping").InnerText = Shipping.ToString();
                xml.SelectSingleNode("UserSettings/Pedestrian").InnerText = Pedestrian.ToString();
                xml.SelectSingleNode("UserSettings/Bicycle").InnerText = Bicycle.ToString();
                xml.SelectSingleNode("UserSettings/Tram").InnerText = Tram.ToString();
                xml.SelectSingleNode("UserSettings/Metro").InnerText = Metro.ToString();
                xml.SelectSingleNode("UserSettings/Buildings").InnerText = Buildings.ToString();
                xml.SelectSingleNode("UserSettings/Trees").InnerText = Trees.ToString();
                xml.SelectSingleNode("UserSettings/Props").InnerText = Props.ToString();
                xml.SelectSingleNode("UserSettings/Ground").InnerText = Ground.ToString();
                xml.SelectSingleNode("UserSettings/Bridge").InnerText = Bridge.ToString();
                xml.SelectSingleNode("UserSettings/Slope").InnerText = Slope.ToString();
                xml.SelectSingleNode("UserSettings/Tunnel").InnerText = Tunnel.ToString();
                xml.SelectSingleNode("UserSettings/Curve").InnerText = Curve.ToString();
                xml.SelectSingleNode("UserSettings/Update").InnerText = Update.ToString();
                xml.SelectSingleNode("UserSettings/Delete").InnerText = Delete.ToString();
                xml.SelectSingleNode("UserSettings/Services").InnerText = Services.ToString();
                xml.SelectSingleNode("UserSettings/Toggle").InnerText = Toggle.ToString();
                xml.SelectSingleNode("UserSettings/Terrain").InnerText = Terrain.ToString();
                xml.SelectSingleNode("UserSettings/Districts").InnerText = Districts.ToString();
                xml.SelectSingleNode("UserSettings/DistrictToggle").InnerText = DistrictToggle.ToString();

                xml.SelectSingleNode("UserSettings/HealthCare").InnerText = HealthCare.ToString();
                xml.SelectSingleNode("UserSettings/PoliceDepartment").InnerText = PoliceDepartment.ToString();
                xml.SelectSingleNode("UserSettings/FireDepartment").InnerText = FireDepartment.ToString();
                xml.SelectSingleNode("UserSettings/PublicTransport").InnerText = PublicTransport.ToString();
                xml.SelectSingleNode("UserSettings/Education").InnerText = Education.ToString();
                xml.SelectSingleNode("UserSettings/Electricity").InnerText = Electricity.ToString();
                xml.SelectSingleNode("UserSettings/Water").InnerText = Water.ToString();
                xml.SelectSingleNode("UserSettings/Garbage").InnerText = Garbage.ToString();
                xml.SelectSingleNode("UserSettings/Beautification").InnerText = Beautification.ToString();
                xml.SelectSingleNode("UserSettings/Monument").InnerText = Monument.ToString();
                xml.SelectSingleNode("UserSettings/Abandoned").InnerText = Abandoned.ToString();
                xml.SelectSingleNode("UserSettings/Burned").InnerText = Burned.ToString();
                xml.SelectSingleNode("UserSettings/TerrainHeight").InnerText = TerrainHeight.ToString("0.00");
            }
            catch (Exception ex)
            {
                RoadUpdateTool.WriteError("Error in UserSettings.Save loc: " + loc + ".", ex);
            }
            xml.Save(us);
        }

        private void FillSettings()
        {
            _basic = ValidateSetting("Basic");
            _large = ValidateSetting("Large");
            _highway = ValidateSetting("Highway");
            _medium = ValidateSetting("Medium");
            _oneway = ValidateSetting("Oneway");
            _toBasic = ValidateSetting("ToBasic");
            _toLarge = ValidateSetting("ToLarge");
            _toHighway = ValidateSetting("ToHighway");
            _toMedium = ValidateSetting("ToMedium");
            _toOneway = ValidateSetting("ToOneway");

            _roads = ValidateSetting("Roads");
            _railroads = ValidateSetting("Railroads");
            _highways = ValidateSetting("Highways");
            _powerLines = ValidateSetting("PowerLines");
            _waterpipes = ValidateSetting("WaterPipes");
            _heatpipes = ValidateSetting("HeatPipes");
            _airplanes = ValidateSetting("Airplanes");
            _shipping = ValidateSetting("Shipping");
            _pedestrian = ValidateSetting("Pedestrian");
            _bicycle = ValidateSetting("Bicycle");
            _tram = ValidateSetting("Tram");
            _metro = ValidateSetting("Metro");

            _buildings = ValidateSetting("Buildings");
            _trees = ValidateSetting("Trees");
            _props = ValidateSetting("Props");
            _ground = ValidateSetting("Ground");
            _bridge = ValidateSetting("Bridge");
            _slope = ValidateSetting("Slope");
            _tunnel = ValidateSetting("Tunnel");
            _curve = ValidateSetting("Curve");
            _update = ValidateSetting("Update");
            _delete = ValidateSetting("Delete");
            _services = ValidateSetting("Services");
            _toggle = ValidateSetting("Toggle");
            _terrain = ValidateSetting("Terrain");
            _districts = ValidateSetting("Districts");
            _districttoggle = ValidateSetting("DistrictToggle");

            _HealthCare = ValidateSetting("HealthCare");
            _PoliceDepartment = ValidateSetting("PoliceDepartment");
            _FireDepartment = ValidateSetting("FireDepartment");
            _PublicTransport = ValidateSetting("PublicTransport");
            _Education = ValidateSetting("Education");
            _Electricity = ValidateSetting("Electricity");
            _Water = ValidateSetting("Water");
            _Garbage = ValidateSetting("Garbage");
            _Beautification = ValidateSetting("Beautification");
            _Monument = ValidateSetting("Monument");
            _abandoned = ValidateSetting("Abandoned");
            _burned = ValidateSetting("Burned");

            _terrainheight = ValidateSetting("TerrainHeight", 0.0);
        }

        private bool ValidateSetting(string node)
        {
            //we already validated the file exists and has our node "UserSettings"
            bool setting = false;
            //create a new node
            if (xml.SelectSingleNode("UserSettings/" + node) == null)
            {
                XmlNode tb = xml.SelectSingleNode("UserSettings");
                XmlNode nd = xml.CreateNode(XmlNodeType.Element, node, "");
                nd.InnerText = false.ToString();
                //RoadUpdateTool.WriteLog("creating a node: " + node);
                tb.AppendChild(nd);
            }
            //we have a node get the value
            setting = (xml.SelectSingleNode("UserSettings/" + node).InnerText == "True");
            return setting;
        }

        private double ValidateSetting(string node, double type)
        {
            //we already validated the file exists and has our node "UserSettings"
            double setting = 0.0;
            //create a new node
            if (xml.SelectSingleNode("UserSettings/" + node) == null)
            {
                XmlNode tb = xml.SelectSingleNode("UserSettings");
                XmlNode nd = xml.CreateNode(XmlNodeType.Element, node, "");
                nd.InnerText = false.ToString();
                //RoadUpdateTool.WriteLog("Creating a node: " + node);
                tb.AppendChild(nd);
            }
            //we have a node get the value
            string temp = xml.SelectSingleNode("UserSettings/" + node).InnerText;
            if (double.TryParse(temp, out setting) == false)
                setting = 0.0;
            return setting;
        }

        private int ValidateSetting(string node, int type)
        {
            //we already validated the file exists and has our node "UserSettings"
            int setting = 1;
            //create a new node
            if (xml.SelectSingleNode("UserSettings/" + node) == null)
            {
                XmlNode tb = xml.SelectSingleNode("UserSettings");
                XmlNode nd = xml.CreateNode(XmlNodeType.Element, node, "");
                nd.InnerText = false.ToString();
                //RoadUpdateTool.WriteLog("Creating a node: " + node);
                tb.AppendChild(nd);
            }
            //we have a node get the value
            string temp = xml.SelectSingleNode("UserSettings/" + node).InnerText;
            if (int.TryParse(temp, out setting) == false)
                setting = 1;
            return setting;
        }

        public void CreateSettings()
        {
            //RoadUpdateTool.WriteLog("Entring CreateSettings");
            xml = new XmlDocument();
            XmlDeclaration xmlDeclaration = xml.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = xml.DocumentElement;
            xml.InsertBefore(xmlDeclaration, root);
            //XmlElement temp = xml.CreateElement("UserSettings", "");
            XmlNode table = xml.CreateNode(XmlNodeType.Element, "UserSettings", "");
            xml.AppendChild(table);
            foreach (string s in settings)
            {
                XmlNode node = xml.CreateNode(XmlNodeType.Element, s, "");
                node.InnerText = false.ToString();
                table.AppendChild(node);
            }
            xml.Save(us);
            //RoadUpdateTool.WriteLog("Leaving CreateSettings");
        }
    }
}
