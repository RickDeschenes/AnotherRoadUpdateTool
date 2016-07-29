using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Text;

namespace AnotherRoadUpdate
{
    internal class UserSettings
    {
        #region Declarations

        private string[] settings = new string[] { "Basic", "Large", "Highway", "Medium", "Oneway", "ToBasic", "ToLarge", "ToHighway", "ToMedium", "ToOneway", "Curves", "Roads", "Railroads", "Highways", "PowerLines", "WaterPipes", "HeatPipes", "Airplanes", "Shipping", "Buildings", "Trees", "Props", "Ground", "Bridge", "Slope", "Tunnel", "Update", "Delete", "Services", "Toggle" };

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
        private bool _curves;
        private bool _roads;
        private bool _railroads;
        private bool _highways;
        private bool _powerLines;
        private bool _waterpipes;
        private bool _heatpipes;
        private bool _airplanes;
        private bool _shipping;
        private bool _buildings;
        private bool _trees;
        private bool _props;
        private bool _ground;
        private bool _bridge;
        private bool _slope;
        private bool _tunnel;
        private bool _update;
        private bool _delete;
        private bool _services;
        private bool _toggle;
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
        public bool Curves { get { return _curves; } set { _curves = value; } }
        public bool Roads { get { return _roads; } set { _roads = value; } }
        public bool Railroads { get { return _railroads; } set { _railroads = value; } }
        public bool Highways { get { return _highways; } set { _highways = value; } }
        public bool PowerLines { get { return _powerLines; } set { _powerLines = value; } }
        public bool WaterPipes { get { return _waterpipes; } set { _waterpipes = value; } }
        public bool HeatPipes { get { return _heatpipes; } set { _heatpipes = value; } }
        public bool Airplanes { get { return _airplanes; } set { _airplanes = value; } }
        public bool Shipping { get { return _shipping; } set { _shipping = value; } }
        public bool Buildings { get { return _buildings; } set { _buildings = value; } }
        public bool Trees { get { return _trees; } set { _trees = value; } }
        public bool Props { get { return _props; } set { _props = value; } }
        public bool Ground { get { return _ground; } set { _ground = value; } }
        public bool Bridge { get { return _bridge; } set { _bridge = value; } }
        public bool Slope { get { return _slope; } set { _slope = value; } }
        public bool Tunnel { get { return _tunnel; } set { _tunnel = value; } }
        public bool Update { get { return _update; } set { _update = value; } }
        public bool Delete { get { return _delete; } set { _delete = value; } }
        public bool Services { get { return _services; } set { _services = value; } }
        public bool Toggle { get { return _toggle; } set { _toggle = value; } }

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
                xml.SelectSingleNode("UserSettings/Curves").InnerText = Curves.ToString();
                xml.SelectSingleNode("UserSettings/Roads").InnerText = Roads.ToString();
                xml.SelectSingleNode("UserSettings/Railroads").InnerText = Railroads.ToString();
                xml.SelectSingleNode("UserSettings/Highways").InnerText = Highways.ToString();
                xml.SelectSingleNode("UserSettings/PowerLines").InnerText = PowerLines.ToString();
                xml.SelectSingleNode("UserSettings/WaterPipes").InnerText = WaterPipes.ToString();
                xml.SelectSingleNode("UserSettings/HeatPipes").InnerText = HeatPipes.ToString();
                xml.SelectSingleNode("UserSettings/Airplanes").InnerText = Airplanes.ToString();
                xml.SelectSingleNode("UserSettings/Shipping").InnerText = Shipping.ToString();
                xml.SelectSingleNode("UserSettings/Buildings").InnerText = Buildings.ToString();
                xml.SelectSingleNode("UserSettings/Trees").InnerText = Trees.ToString();
                xml.SelectSingleNode("UserSettings/Props").InnerText = Props.ToString();
                xml.SelectSingleNode("UserSettings/Ground").InnerText = Ground.ToString();
                xml.SelectSingleNode("UserSettings/Bridge").InnerText = Bridge.ToString();
                xml.SelectSingleNode("UserSettings/Slope").InnerText = Bridge.ToString();
                xml.SelectSingleNode("UserSettings/Tunnel").InnerText = Tunnel.ToString();
                xml.SelectSingleNode("UserSettings/Update").InnerText = Update.ToString();
                xml.SelectSingleNode("UserSettings/Delete").InnerText = Delete.ToString();
                xml.SelectSingleNode("UserSettings/Services").InnerText = Services.ToString();
                xml.SelectSingleNode("UserSettings/Toggle").InnerText = Toggle.ToString();
            }
            catch (Exception ex)
            {
                RoadUpdateTool.WriteLog("UserSettings.Save loc: " + loc + " error: " + ex.Message + ":: Stacktrace: " + ex.StackTrace);
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
            _curves = ValidateSetting("Curves");
            _roads = ValidateSetting("Roads");
            _railroads = ValidateSetting("Railroads");
            _highways = ValidateSetting("Highways");
            _powerLines = ValidateSetting("PowerLines");
            _waterpipes = ValidateSetting("WaterPipes");
            _heatpipes = ValidateSetting("HeatPipes");
            _airplanes = ValidateSetting("Airplanes");
            _shipping = ValidateSetting("Shipping");
            _buildings = ValidateSetting("Buildings");
            _trees = ValidateSetting("Trees");
            _props = ValidateSetting("Props");
            _ground = ValidateSetting("Ground");
            _bridge = ValidateSetting("Bridge");
            _bridge = ValidateSetting("Slope");
            _tunnel = ValidateSetting("Tunnel");
            _update = ValidateSetting("Update");
            _delete = ValidateSetting("Delete");
            _services = ValidateSetting("Services");
            _toggle = ValidateSetting("Toggle");
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
                RoadUpdateTool.WriteLog("creating a node: " + node);
                tb.AppendChild(nd);
            }
            //we have a node get the value
            setting = (xml.SelectSingleNode("UserSettings/" + node).InnerText == "True");
            return setting;
        }

        public void CreateSettings()
        {
            RoadUpdateTool.WriteLog("entring CreateSettings");
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
            RoadUpdateTool.WriteLog("leaving CreateSettings");
        }
    }
}
