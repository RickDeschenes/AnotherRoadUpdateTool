using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Text;

namespace AnotherRoadUpdate
{
    class UserSettings
    {
        private string[] settings = new string[] { "Basic", "Large", "Highway", "Medium", "Oneway", "ToBasic", "ToLarge", "ToHighway", "ToMedium", "ToOneway", "Curves", "Roads", "Railroads", "Highways", "PowerLines", "Pipes", "Buildings", "Trees", "Props", "Ground", "Bridge", "Toggle", "Tunnel", "Update", "Delete", "Services" };

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
        private bool _pipes;
        private bool _buildings;
        private bool _trees;
        private bool _props;
        private bool _ground;
        private bool _bridge;
        private bool _tunnel;
        private bool _toggle;
        private bool _update;
        private bool _delete;
        private bool _services;
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
        public bool Pipes { get { return _pipes; } set { _pipes = value; } }
        public bool Buildings { get { return _buildings; } set { _buildings = value; } }
        public bool Trees { get { return _trees; } set { _trees = value; } }
        public bool Props { get { return _props; } set { _props = value; } }
        public bool Ground { get { return _ground; } set { _ground = value; } }
        public bool Bridge { get { return _bridge; } set { _bridge = value; } }
        public bool Tunnel { get { return _tunnel; } set { _tunnel = value; } }
        public bool Toggle { get { return _toggle; } set { _toggle = value; } }
        public bool Update { get { return _update; } set { _update = value; } }
        public bool Delete { get { return _delete; } set { _delete = value; } }
        public bool Services { get { return _services; } set { _services = value; } }

        private const string fileName = "ARUTuserSettings.xml";
        private string us = fileName;
        private XmlDocument xml = new XmlDocument();

        public UserSettings()
        {
            if (File.Exists(us))
            {
                xml.Load(us);
            }
            else
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
                xml.SelectSingleNode("UserSettings/Pipes").InnerText = Pipes.ToString();
                xml.SelectSingleNode("UserSettings/Buildings").InnerText = Buildings.ToString();
                xml.SelectSingleNode("UserSettings/Trees").InnerText = Trees.ToString();
                xml.SelectSingleNode("UserSettings/Props").InnerText = Props.ToString();
                xml.SelectSingleNode("UserSettings/Ground").InnerText = Ground.ToString();
                xml.SelectSingleNode("UserSettings/Bridge").InnerText = Bridge.ToString();
                xml.SelectSingleNode("UserSettings/Toggle").InnerText = Toggle.ToString();
                xml.SelectSingleNode("UserSettings/Tunnel").InnerText = Tunnel.ToString();
                xml.SelectSingleNode("UserSettings/Update").InnerText = Update.ToString();
                xml.SelectSingleNode("UserSettings/Delete").InnerText = Delete.ToString();
                xml.SelectSingleNode("UserSettings/Services").InnerText = Services.ToString();
            }
            catch (Exception ex)
            {
                RoadUpdateTool.WriteLog("UserSettings.Save loc: " + loc + " error: " + ex.Message + ":: Stacktrace: " + ex.StackTrace);
            }
            xml.Save(us);
        }

        private void FillSettings()
        {
            XmlNode nd = xml.SelectSingleNode("UserSettings/Basic");

            _basic = (xml.SelectSingleNode("UserSettings/Basic").InnerText == "True");
            _large = (xml.SelectSingleNode("UserSettings/Large").InnerText == "True");
            _highway = (xml.SelectSingleNode("UserSettings/Highway").InnerText == "True");
            _medium = (xml.SelectSingleNode("UserSettings/Medium").InnerText == "True");
            _oneway = (xml.SelectSingleNode("UserSettings/Oneway").InnerText == "True");
            _toBasic = (xml.SelectSingleNode("UserSettings/ToBasic").InnerText == "True");
            _toLarge = (xml.SelectSingleNode("UserSettings/ToLarge").InnerText == "True");
            _toHighway = (xml.SelectSingleNode("UserSettings/ToHighway").InnerText == "True");
            _toMedium = (xml.SelectSingleNode("UserSettings/ToMedium").InnerText == "True");
            _toOneway = (xml.SelectSingleNode("UserSettings/ToOneway").InnerText == "True");
            _curves = (xml.SelectSingleNode("UserSettings/Curves").InnerText == "True");
            _roads = (xml.SelectSingleNode("UserSettings/Roads").InnerText == "True");
            _railroads = (xml.SelectSingleNode("UserSettings/Railroads").InnerText == "True");
            _highways = (xml.SelectSingleNode("UserSettings/Highways").InnerText == "True");
            _powerLines = (xml.SelectSingleNode("UserSettings/PowerLines").InnerText == "True");
            _pipes = (xml.SelectSingleNode("UserSettings/Pipes").InnerText == "True");
            _buildings = (xml.SelectSingleNode("UserSettings/Buildings").InnerText == "True");
            _trees = (xml.SelectSingleNode("UserSettings/Trees").InnerText == "True");
            _props = (xml.SelectSingleNode("UserSettings/Props").InnerText == "True");
            _ground = (xml.SelectSingleNode("UserSettings/Ground").InnerText == "True");
            _bridge = (xml.SelectSingleNode("UserSettings/Bridge").InnerText == "True");
            _toggle = (xml.SelectSingleNode("UserSettings/Toggle").InnerText == "True");
            _tunnel = (xml.SelectSingleNode("UserSettings/Tunnel").InnerText == "True");
            _update = (xml.SelectSingleNode("UserSettings/Update").InnerText == "True");
            _delete = (xml.SelectSingleNode("UserSettings/Delete").InnerText == "True");
            _services = (xml.SelectSingleNode("UserSettings/Services").InnerText == "True");
        }

        public void CreateSettings()
        {
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
        }
    }
}
