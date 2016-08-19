using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Text;
using System.Xml.Serialization;
using System.Reflection;

namespace AnotherRoadUpdate
{
    internal class Interface
    {
        #region Declarations

        internal struct ops
        {
            /// <summary>
            /// Name
            /// </summary>
            internal string name;
            /// <summary>
            /// Traslated Name
            /// </summary>
            internal string translation;

            internal ops(string name, string translation) : this()
            {
                this.name = name;
                this.translation = translation;
            }
        }

        internal struct del
        {
            internal string name;
            internal string translation;
            internal string section;
            internal string column;

            internal del(string name, string translation, string section, string column) : this()
            {
                this.name = name;
                this.translation = translation;
                this.section = section;
                this.column = column;
            }
        }

        internal struct _Options
        {
            internal ops Updates;
            internal ops Deletes;
            internal ops Services;
            internal ops Terrain;

            /// <summary>
            /// value is the Setting Name, Healthcare, Police Department, etc.
            /// name and translation are the values
            /// </summary>
            /// <param name="value"></param>
            /// <param name="name"></param>
            /// <param name="translation"></param>
            /// <returns></returns>
            internal bool Load(string value, string name, string translation)
            {
                bool results = true;
                if (value == "Updates")
                {
                    this.Updates.name = name;
                    this.Updates.translation = translation;
                }
                else if (value == "Deletes")
                {
                    this.Deletes.name = name;
                    this.Deletes.translation = translation;
                }
                else if (value == "Services")
                {
                    this.Services.name = name;
                    this.Services.translation = translation;
                }
                else if (value == "Terrain")
                {
                    this.Terrain.name = name;
                    this.Terrain.translation = translation;
                }
                else results = false;

                return results;
            }

            internal _Options(string Updaten, string Updatet, string Deleten, string Deletet, string Servicen, string Servicet, string Terrainn, string Terraint) : this()
            {
                this.Updates.name = Updaten;
                this.Updates.translation = Updatet;
                this.Deletes.name = Deleten;
                this.Deletes.translation = Deletet;
                this.Services.name = Servicen;
                this.Services.translation = Servicet;
                this.Terrain.name = Terrainn;
                this.Terrain.translation = Terraint;
            }
        }

        internal struct _Types
        {
            internal ops Ground;
            internal ops Bridge;
            internal ops Slope;
            internal ops Tunnel;
            internal ops Curve;

            /// <summary>
            /// value is the Setting Name, Healthcare, Police Department, etc.
            /// name and translation are the values
            /// </summary>
            /// <param name="value"></param>
            /// <param name="name"></param>
            /// <param name="translation"></param>
            /// <returns></returns>
            internal bool Load(string value, string name, string translation)
            {
                bool results = true;
                if (value == "Ground")
                {
                    this.Ground.name = name;
                    this.Ground.translation = translation;
                }
                else if (value == "Bridge")
                {
                    this.Bridge.name = name;
                    this.Bridge.translation = translation;
                }
                else if (value == "Slope")
                {
                    this.Slope.name = name;
                    this.Slope.translation = translation;
                }
                else if (value == "Tunnel")
                {
                    this.Tunnel.name = name;
                    this.Tunnel.translation = translation;
                }
                else if (value == "Curve")
                {
                    this.Curve.name = name;
                    this.Curve.translation = translation;
                }
                else results = false;

                return results;
            }

            internal _Types(string Groundn, string Groundt, string Bridgen, string Bridget, string Slopen, string Slopet, string Tunneln, string Tunnelt, string Curven, string Curvet) : this()
            {
                this.Ground.name = Groundn;
                this.Ground.translation = Groundt;
                this.Bridge.name = Bridgen;
                this.Bridge.translation = Bridget;
                this.Slope.name = Slopen;
                this.Slope.translation = Slopet;
                this.Tunnel.name = Tunneln;
                this.Tunnel.translation = Tunnelt;
                this.Curve.name = Curven;
                this.Curve.translation = Curvet;
            }
        }

        internal struct _Roads
        {
            internal ops Basic;
            internal ops Highway;
            internal ops Large;
            internal ops Medium;
            internal ops Oneway;

            /// <summary>
            /// value is the Setting Name, Healthcare, Police Department, etc.
            /// name and translation are the values
            /// </summary>
            /// <param name="value"></param>
            /// <param name="name"></param>
            /// <param name="translation"></param>
            /// <returns></returns>
            internal bool Load(string value, string name, string translation)
            {
                bool results = true;
                if (value == "Basic")
                {
                    this.Basic.name = name;
                    this.Basic.translation = translation;
                }
                else if (value == "Highway")
                {
                    this.Highway.name = name;
                    this.Highway.translation = translation;
                }
                else if (value == "Large")
                {
                    this.Large.name = name;
                    this.Large.translation = translation;
                }
                else if (value == "Medium")
                {
                    this.Medium.name = name;
                    this.Medium.translation = translation;
                }
                else if (value == "Oneway")
                {
                    this.Oneway.name = name;
                    this.Oneway.translation = translation;
                }
                else results = false;

                return results;
            }

            internal _Roads(string Basicn, string Basict, string Highwayn, string Highwayt, string Largen, string Larget, string Mediumn, string Mediumt, string Onewayn, string Onewayt) : this()
            {
                this.Basic.name = Basicn;
                this.Basic.translation = Basict;
                this.Highway.name = Highwayn;
                this.Highway.translation = Highwayt;
                this.Large.name = Largen;
                this.Large.translation = Larget;
                this.Medium.name = Mediumn;
                this.Medium.translation = Mediumt;
                this.Oneway.name = Onewayn;
                this.Oneway.translation = Onewayt;
            }
        }

        internal struct _BasicRoads
        {
            internal ops Basic;
            internal ops Grass;
            internal ops Trees;
            internal ops Bicycle;
            internal ops Tram;
            internal ops Bus;
            internal ops Gravel;

            /// <summary>
            /// value is the Setting Name, Healthcare, Police Department, etc.
            /// name and translation are the values
            /// </summary>
            /// <param name="value"></param>
            /// <param name="name"></param>
            /// <param name="translation"></param>
            /// <returns></returns>
            internal bool Load(string value, string name, string translation)
            {
                bool results = true;
                if (value == "Basic")
                {
                    this.Basic.name = name;
                    this.Basic.translation = translation;
                }
                else if (value == "Grass")
                {
                    this.Grass.name = name;
                    this.Grass.translation = translation;
                }
                else if (value == "Trees")
                {
                    this.Trees.name = name;
                    this.Trees.translation = translation;
                }
                else if (value == "Bicycle")
                {
                    this.Bicycle.name = name;
                    this.Bicycle.translation = translation;
                }
                else if (value == "Tram")
                {
                    this.Tram.name = name;
                    this.Tram.translation = translation;
                }
                else if (value == "Bus")
                {
                    this.Bus.name = name;
                    this.Bus.translation = translation;
                }
                else if (value == "Gravel")
                {
                    this.Gravel.name = name;
                    this.Gravel.translation = translation;
                }
                else results = false;

                return results;
            }

            internal _BasicRoads(string Basicn, string Basict, string Grassn, string Grasst, string Treen, string Treet, string Bicyclen, string Bicyclet, string Tramn, string Tramt, string Busn, string Bust, string Graveln, string Gravelt) : this()
            {
                this.Basic.name = Basicn;
                this.Basic.translation = Basict;
                this.Grass.name = Grassn;
                this.Grass.translation = Grasst;
                this.Trees.name = Treen;
                this.Trees.translation = Treet;
                this.Bicycle.name = Bicyclen;
                this.Bicycle.translation = Bicyclet;
                this.Tram.name = Tramn;
                this.Tram.translation = Tramt;
                this.Bus.name = Busn;
                this.Bus.translation = Bust;
                this.Gravel.name = Graveln;
                this.Gravel.translation = Gravelt;
            }
        }

        internal struct _Highways
        {
            internal ops Highway;
            internal ops Barrier;
            internal ops HighwayRamp;
            internal ops HighwayRampElevated;

            /// <summary>
            /// value is the Setting Name, Healthcare, Police Department, etc.
            /// name and translation are the values
            /// </summary>
            /// <param name="value"></param>
            /// <param name="name"></param>
            /// <param name="translation"></param>
            /// <returns></returns>
            internal bool Load(string value, string name, string translation)
            {
                bool results = true;
                if (value == "Highway")
                {
                    this.Highway.name = name;
                    this.Highway.translation = translation;
                }
                else if (value == "Barrier")
                {
                    this.Barrier.name = name;
                    this.Barrier.translation = translation;
                }
                else if (value == "HighwayRamp")
                {
                    this.HighwayRamp.name = name;
                    this.HighwayRamp.translation = translation;
                }
                else if (value == "HighwayRampElevated")
                {
                    this.HighwayRampElevated.name = name;
                    this.HighwayRampElevated.translation = translation;
                }
                else results = false;

                return results;
            }

            internal _Highways(string Basicn, string Basict, string Barriern, string Barriert, string Rampn, string Rampt, string RampElevatedn, string RampElevatedt) : this()
            {
                this.Highway.name = Basicn;
                this.Highway.translation = Basict;
                this.Barrier.name = Barriern;
                this.Barrier.translation = Barriert;
                this.HighwayRamp.name = Rampn;
                this.HighwayRamp.translation = Rampt;
                this.HighwayRampElevated.name = RampElevatedn;
                this.HighwayRampElevated.translation = RampElevatedt;
            }
        }

        internal struct _LargeRoads
        {
            internal ops Large;
            internal ops Grass;
            internal ops Trees;
            internal ops Bicycle;
            internal ops Tram;
            internal ops Bus;
            internal ops LargeOneway;
            internal ops OnewayGrass;
            internal ops OnewayTrees;

            /// <summary>
            /// value is the Setting Name, Healthcare, Police Department, etc.
            /// name and translation are the values
            /// </summary>
            /// <param name="value"></param>
            /// <param name="name"></param>
            /// <param name="translation"></param>
            /// <returns></returns>
            internal bool Load(string value, string name, string translation)
            {
                bool results = true;
                if (value == "Large")
                {
                    this.Large.name = name;
                    this.Large.translation = translation;
                }
                else if (value == "Grass")
                {
                    this.Grass.name = name;
                    this.Grass.translation = translation;
                }
                else if (value == "Trees")
                {
                    this.Trees.name = name;
                    this.Trees.translation = translation;
                }
                else if (value == "Bicycle")
                {
                    this.Bicycle.name = name;
                    this.Bicycle.translation = translation;
                }
                else if (value == "Tram")
                {
                    this.Tram.name = name;
                    this.Tram.translation = translation;
                }
                else if (value == "Bus")
                {
                    this.Bus.name = name;
                    this.Bus.translation = translation;
                }
                else if (value == "LargeOneway")
                {
                    this.LargeOneway.name = name;
                    this.LargeOneway.translation = translation;
                }
                else if (value == "OnewayGrass")
                {
                    this.OnewayGrass.name = name;
                    this.OnewayGrass.translation = translation;
                }
                else if (value == "OnewayTrees")
                {
                    this.OnewayTrees.name = name;
                    this.OnewayTrees.translation = translation;
                }
                else results = false;

                return results;
            }

            internal _LargeRoads(string Largen, string Larget, string Grassn, string Grasst, string Treesn, string Treest, string Bicyclen, string Bicyclet, string Tramn, string Tramt, string Busn, string Bust, string LargeOnewayn, string LargeOnewayt, string OnewayGrassn, string OnewayGrasst, string OnewayTreesn, string OnewayTreest) : this()
            {
                this.Large.name = Largen;
                this.Large.translation = Larget;
                this.Grass.name = Grassn;
                this.Grass.translation = Grasst;
                this.Trees.name = Treesn;
                this.Trees.translation = Treest;
                this.Bicycle.name = Bicyclen;
                this.Bicycle.translation = Bicyclet;
                this.Tram.name = Tramn;
                this.Tram.translation = Tramt;
                this.Bus.name = Busn;
                this.Bus.translation = Bust;
                this.LargeOneway.name = LargeOnewayn;
                this.LargeOneway.translation = LargeOnewayt;
                this.OnewayGrass.name = OnewayGrassn;
                this.OnewayGrass.translation = OnewayGrasst;
                this.OnewayTrees.name = OnewayTreesn;
                this.OnewayTrees.translation = OnewayTreest;
            }
        }

        internal struct _MediumRoads
        {
            internal ops Medium;
            internal ops Grass;
            internal ops Trees;
            internal ops Bicycle;
            internal ops Tram;
            internal ops Bus;

            /// <summary>
            /// value is the Setting Name, Healthcare, Police Department, etc.
            /// name and translation are the values
            /// </summary>
            /// <param name="value"></param>
            /// <param name="name"></param>
            /// <param name="translation"></param>
            /// <returns></returns>
            internal bool Load(string value, string name, string translation)
            {
                bool results = true;
                if (value == "Medium")
                {
                    this.Medium.name = name;
                    this.Medium.translation = translation;
                }
                else if (value == "Grass")
                {
                    this.Grass.name = name;
                    this.Grass.translation = translation;
                }
                else if (value == "Trees")
                {
                    this.Trees.name = name;
                    this.Trees.translation = translation;
                }
                else if (value == "Bicycle")
                {
                    this.Bicycle.name = name;
                    this.Bicycle.translation = translation;
                }
                else if (value == "Tram")
                {
                    this.Tram.name = name;
                    this.Tram.translation = translation;
                }
                else if (value == "Bus")
                {
                    this.Bus.name = name;
                    this.Bus.translation = translation;
                }
                else results = false;

                return results;
            }

            internal _MediumRoads(string Mediumn, string Mediumt, string Grassn, string Grasst, string Treesn, string Treest, string Bicyclen, string Bicyclet, string Tramn, string Tramt, string Busn, string Bust) : this()
            {
                this.Medium.name = Mediumn;
                this.Medium.translation = Mediumt;
                this.Grass.name = Grassn;
                this.Grass.translation = Grasst;
                this.Trees.name = Treesn;
                this.Trees.translation = Treest;
                this.Bicycle.name = Bicyclen;
                this.Bicycle.translation = Bicyclet;
                this.Tram.name = Tramn;
                this.Tram.translation = Tramt;
                this.Bus.name = Busn;
                this.Bus.translation = Bust;
            }
        }

        internal struct _OnewayRoads
        {
            internal ops Oneway;
            internal ops Grass;
            internal ops Trees;
            internal ops Tram;
            internal ops TramTrack;
            internal ops OnewayTramTrack;

            /// <summary>
            /// value is the Setting Name, Healthcare, Police Department, etc.
            /// name and translation are the values
            /// </summary>
            /// <param name="value"></param>
            /// <param name="name"></param>
            /// <param name="translation"></param>
            /// <returns></returns>
            internal bool Load(string value, string name, string translation)
            {
                bool results = true;
                if (value == "Oneway")
                {
                    this.Oneway.name = name;
                    this.Oneway.translation = translation;
                }
                else if (value == "Grass")
                {
                    this.Grass.name = name;
                    this.Grass.translation = translation;
                }
                else if (value == "Trees")
                {
                    this.Trees.name = name;
                    this.Trees.translation = translation;
                }
                else if (value == "Tram")
                {
                    this.Tram.name = name;
                    this.Tram.translation = translation;
                }
                else if (value == "TramTrack")
                {
                    this.TramTrack.name = name;
                    this.TramTrack.translation = translation;
                }
                else if (value == "OnewayTramTrack")
                {
                    this.OnewayTramTrack.name = name;
                    this.OnewayTramTrack.translation = translation;
                }
                else results = false;

                return results;
            }

            internal _OnewayRoads(string Onewayn, string Onewayt, string Grassn, string Grasst, string Treesn, string Treest, string Tramn, string Tramt, string TramTrackn, string TramTrackt, string OnewayTramTrackn, string OnewayTramTrackt) : this()
            {
                this.Oneway.name = Onewayn;
                this.Oneway.translation = Onewayt;
                this.Grass.name = Grassn;
                this.Grass.translation = Grasst;
                this.Trees.name = Treesn;
                this.Trees.translation = Treest;
                this.Tram.name = Tramn;
                this.Tram.translation = Tramt;
                this.TramTrack.name = TramTrackn;
                this.TramTrack.translation = TramTrackt;
                this.OnewayTramTrack.name = OnewayTramTrackn;
                this.OnewayTramTrack.translation = OnewayTramTrackt;
            }
        }

        internal struct _Deletes
        {
            internal del Roads;
            internal del RailRoads;
            internal del Highways;
            internal del PowerLines;
            internal del WaterPipes;
            internal del HeatPipes;
            internal del Airplane;
            internal del Shipping;
            internal del Pedestrian;
            internal del Bicycle;
            internal del Tram;
            internal del Metro;
            internal del Buildings;
            internal del Trees;
            internal del Props;

            /// <summary>
            /// value is the Setting Name, Healthcare, Police Department, etc.
            /// name and translation are the values
            /// </summary>
            /// <param name="value"></param>
            /// <param name="name"></param>
            /// <param name="translation"></param>
            /// <returns></returns>
            internal bool Load(string value, string name, string translation, string section, string column)
            {
                bool results = true;
                if (value == "Roads")
                {
                    this.Roads.name = name;
                    this.Roads.translation = translation;
                    this.Roads.section = section;
                    this.Roads.column = column;
                }
                else if (value == "Railroads")
                {
                    this.RailRoads.name = name;
                    this.RailRoads.translation = translation;
                    this.RailRoads.section = section;
                    this.RailRoads.column = column;
                }
                else if (value == "Highways")
                {
                    this.Highways.name = name;
                    this.Highways.translation = translation;
                    this.Highways.section = section;
                    this.Highways.column = column;
                }
                else if (value == "PowerLines")
                {
                    this.PowerLines.name = name;
                    this.PowerLines.translation = translation;
                    this.PowerLines.section = section;
                    this.PowerLines.column = column;
                }
                else if (value == "WaterPipes")
                {
                    this.WaterPipes.name = name;
                    this.WaterPipes.translation = translation;
                    this.WaterPipes.section = section;
                    this.WaterPipes.column = column;
                }
                else if (value == "HeatPipes")
                {
                    this.HeatPipes.name = name;
                    this.HeatPipes.translation = translation;
                    this.HeatPipes.section = section;
                    this.HeatPipes.column = column;
                }
                else if (value == "Airplane")
                {
                    this.Airplane.name = name;
                    this.Airplane.translation = translation;
                    this.Airplane.section = section;
                    this.Airplane.column = column;
                }
                else if (value == "Shipping")
                {
                    this.Shipping.name = name;
                    this.Shipping.translation = translation;
                    this.Shipping.section = section;
                    this.Shipping.column = column;
                }
                else if (value == "Pedestrian")
                {
                    this.Pedestrian.name = name;
                    this.Pedestrian.translation = translation;
                    this.Pedestrian.section = section;
                    this.Pedestrian.column = column;
                }
                else if (value == "Bicycle")
                {
                    this.Bicycle.name = name;
                    this.Bicycle.translation = translation;
                    this.Bicycle.section = section;
                    this.Bicycle.column = column;
                }
                else if (value == "Tram")
                {
                    this.Tram.name = name;
                    this.Tram.translation = translation;
                    this.Tram.section = section;
                    this.Tram.column = column;
                }
                else if (value == "Metro")
                {
                    this.Metro.name = name;
                    this.Metro.translation = translation;
                    this.Metro.section = section;
                    this.Metro.column = column;
                }
                else if (value == "Buildings")
                {
                    this.Buildings.name = name;
                    this.Buildings.translation = translation;
                    this.Buildings.section = section;
                    this.Buildings.column = column;
                }
                else if (value == "Trees")
                {
                    this.Trees.name = name;
                    this.Trees.translation = translation;
                    this.Trees.section = section;
                    this.Trees.column = column;
                }
                else if (value == "Props")
                {
                    this.Props.name = name;
                    this.Props.translation = translation;
                    this.Props.section = section;
                    this.Props.column = column;
                }
                else
                    results = false;

                return results;
            }
        }

        internal struct _Services
        {
            internal ops HealthCare;
            internal ops PoliceDepartment;
            internal ops FireDepartment;
            internal ops PublicTransport;
            internal ops Education;
            internal ops Electricity;
            internal ops Water;
            internal ops Garbage;
            internal ops Beautification;
            internal ops Monument;

            /// <summary>
            /// value is the Setting Name, Healthcare, Police Department, etc.
            /// name and translation are the values
            /// </summary>
            /// <param name="value"></param>
            /// <param name="name"></param>
            /// <param name="translation"></param>
            /// <returns></returns>
            internal bool Load(string value, string name, string translation)
            {
                bool results = true;
                if (value == "HealthCare")
                {
                    this.HealthCare.name = name;
                    this.HealthCare.translation = translation;
                }
                else if (value == "PoliceDepartment")
                {
                    this.PoliceDepartment.name = name;
                    this.PoliceDepartment.translation = translation;
                }
                else if (value == "FireDepartment")
                {
                    this.FireDepartment.name = name;
                    this.FireDepartment.translation = translation;
                }
                else if (value == "PublicTransport")
                {
                    this.PublicTransport.name = name;
                    this.PublicTransport.translation = translation;
                }
                else if (value == "Education")
                {
                    this.Education.name = name;
                    this.Education.translation = translation;
                }
                else if (value == "Electricity")
                {
                    this.Electricity.name = name;
                    this.Electricity.translation = translation;
                }
                else if (value == "Water")
                {
                    this.Water.name = name;
                    this.Water.translation = translation;
                }
                else if (value == "Garbage")
                {
                    this.Garbage.name = name;
                    this.Garbage.translation = translation;
                }
                else if (value == "Beautification")
                {
                    this.Beautification.name = name;
                    this.Beautification.translation = translation;
                }
                else if (value == "Monument")
                {
                    this.Monument.name = name;
                    this.Monument.translation = translation;
                }
                else results = false;

                return results;
            }

            internal _Services(string HealthCaren, string HealthCaret,
                                string PoliceDepartmentn, string PoliceDepartmentt,
                                string FireDepartmentn, string FireDepartmentt,
                                string PublicTransportn, string PublicTransportt,
                                string Educationn, string Educationt,
                                string Electricityn, string Electricityt,
                                string Watern, string Watert,
                                string Garbagen, string Garbaget,
                                string Beautificationn, string Beautificationt,
                                string Monumentn, string Monumentt) : this()
            {
                this.HealthCare.name = HealthCaren;
                this.HealthCare.translation = HealthCaret;
                this.PoliceDepartment.name = PoliceDepartmentn;
                this.PoliceDepartment.translation = PoliceDepartmentt;
                this.FireDepartment.name = FireDepartmentn;
                this.FireDepartment.translation = FireDepartmentt;
                this.PublicTransport.name = PublicTransportn;
                this.PublicTransport.translation = PublicTransportt;
                this.Education.name = Educationn;
                this.Education.translation = Educationt;
                this.Electricity.name = Electricityn;
                this.Electricity.translation = Electricityt;
                this.Water.name = Watern;
                this.Water.translation = Watert;
                this.Garbage.name = Garbagen;
                this.Garbage.translation = Garbaget;
                this.Beautification.name = Beautificationn;
                this.Beautification.translation = Beautificationt;
                this.Monument.name = Monumentn;
                this.Monument.translation = Monumentt;
            }
        }

        internal _Options Options = new _Options();
        internal _Types Types = new _Types();
        internal _Roads Roads = new _Roads();
        internal _BasicRoads BasicRoads = new _BasicRoads();
        internal _Highways Highways = new _Highways();
        internal _LargeRoads LargeRoads = new _LargeRoads();
        internal _MediumRoads MediumRoads = new _MediumRoads();
        internal _OnewayRoads OnewayRoads = new _OnewayRoads();
        internal _Deletes Deletes = new _Deletes();
        internal _Services Services = new _Services();

        private const string fileName = "ARUTInterface.xml";
        private string us = fileName;
        private XmlDocument xml = new XmlDocument();

        #endregion

        internal Interface()
        {
            bool create = true;

            //Do we have a file?
            if (File.Exists(us))
            {
                xml.Load(us);
                //was it any good?
                create = (xml.SelectSingleNode("Interface") == null);
            }
            //if we need to create a new file
            if (create)
            {
                CreateSettings();
            }

            create = (xml.SelectSingleNode("Interface") == null);
            if (create)
            {
                throw new Exception("Could not load interface XML");
            }
            else
                FillSettings();
        }

        internal void CreateSettings()
        {
            //RoadUpdateTool.WriteLog("Entering CreateSettings");
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream("AnotherRoadUpdateTest.ARUTInterface.xml");
            if (stream == null)
            {
                RoadUpdateTool.WriteLog("Error loading embeded resource AnotherRoadUpdateTest.ARUTInterface.xml");
                return;
            }
            BinaryReader br = new BinaryReader(stream);
            FileStream fs = new FileStream("ARUTInterface.xml", FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            byte[] ba = new byte[stream.Length];
            stream.Read(ba, 0, ba.Length);
            bw.Write(ba);
            br.Close();
            bw.Close();
            xml.Load("ARUTInterface.xml");
            //RoadUpdateTool.WriteLog("Leaving CreateSettings");
        }

        private void FillSettings()
        {
            //we need to fill each structure
            //Options
            Options = FillOptions(Options, xml.SelectSingleNode("Interface/Options"));
            //Types
            Types = FillTypes(Types, xml.SelectSingleNode("Interface/Types"));
            //Roads
            Roads = FillRoads(Roads, xml.SelectSingleNode("Interface/Roads"));
            //BasicRoads
            BasicRoads = FillBasicRoads(BasicRoads, xml.SelectSingleNode("Interface/Basic"));
            //Highways
            Highways = FillHighways(Highways, xml.SelectSingleNode("Interface/Highway"));
            //LargeRoads
            LargeRoads = FillLargeRoads(LargeRoads, xml.SelectSingleNode("Interface/Large"));
            //MediumRoads
            MediumRoads = FillMediumRoads(MediumRoads, xml.SelectSingleNode("Interface/Medium"));
            //OnewayRoads
            OnewayRoads = FillOnewayRoads(OnewayRoads, xml.SelectSingleNode("Interface/Oneway"));
            //Deletes
            Deletes = FillDeletes(Deletes, xml.SelectSingleNode("Interface/Deletes"));
            //Services
            Services = FillServices(Services, xml.SelectSingleNode("Interface/Services"));

        }

        private _Options FillOptions(_Options op, XmlNode ind)
        {
            foreach (XmlNode nd in ind)
            {
                string n = nd.SelectSingleNode("name").InnerText;
                string t = nd.SelectSingleNode("translation").InnerText;
                if (op.Load(nd.Name, n, t) == false)
                    RoadUpdateTool.WriteLog("FillOptions could not load: " + nd.Name);
            }
            return op;
        }

        private _Types FillTypes(_Types op, XmlNode ind)
        {
            foreach (XmlNode nd in ind)
            {
                string n = nd.SelectSingleNode("name").InnerText;
                string t = nd.SelectSingleNode("translation").InnerText;
                if (op.Load(nd.Name, n, t) == false)
                    RoadUpdateTool.WriteLog("FillTypes could not load: " + nd.Name);
            }
            return op;
        }

        private _Roads FillRoads(_Roads op, XmlNode ind)
        {
            foreach (XmlNode nd in ind)
            {
                string n = nd.SelectSingleNode("name").InnerText;
                string t = nd.SelectSingleNode("translation").InnerText;
                if (op.Load(nd.Name, n, t) == false)
                    RoadUpdateTool.WriteLog("FillRoads could not load: " + nd.Name);
            }
            return op;
        }

        private _BasicRoads FillBasicRoads(_BasicRoads op, XmlNode ind)
        {
            foreach (XmlNode nd in ind)
            {
                string n = nd.SelectSingleNode("name").InnerText;
                string t = nd.SelectSingleNode("translation").InnerText;
                if (op.Load(nd.Name, n, t) == false)
                    RoadUpdateTool.WriteLog("FillBasicRoads could not load: " + nd.Name);
            }
            return op;
        }

        private _Highways FillHighways(_Highways op, XmlNode ind)
        {
            foreach (XmlNode nd in ind)
            {
                string n = nd.SelectSingleNode("name").InnerText;
                string t = nd.SelectSingleNode("translation").InnerText;
                if (op.Load(nd.Name, n, t) == false)
                    RoadUpdateTool.WriteLog("FillHighways could not load: " + nd.Name);
            }
            return op;
        }

        private _LargeRoads FillLargeRoads(_LargeRoads op, XmlNode ind)
        {
            foreach (XmlNode nd in ind)
            {
                string n = nd.SelectSingleNode("name").InnerText;
                string t = nd.SelectSingleNode("translation").InnerText;
                if (op.Load(nd.Name, n, t) == false)
                    RoadUpdateTool.WriteLog("FillLargeRoads could not load: " + nd.Name);
            }
            return op;
        }

        private _MediumRoads FillMediumRoads(_MediumRoads op, XmlNode ind)
        {
            foreach (XmlNode nd in ind)
            {
                string n = nd.SelectSingleNode("name").InnerText;
                string t = nd.SelectSingleNode("translation").InnerText;
                if (op.Load(nd.Name, n, t) == false)
                    RoadUpdateTool.WriteLog("Could not load: " + nd.Name);
            }
            return op;
        }

        private _OnewayRoads FillOnewayRoads(_OnewayRoads op, XmlNode ind)
        {
            foreach (XmlNode nd in ind)
            {
                string n = nd.SelectSingleNode("name").InnerText;
                string t = nd.SelectSingleNode("translation").InnerText;
                if (op.Load(nd.Name, n, t) == false)
                    RoadUpdateTool.WriteLog("FillOnewayRoads could not load: " + nd.Name);
            }
            return op;
        }

        private _Deletes FillDeletes(_Deletes op, XmlNode ind)
        {
            foreach (XmlNode nd in ind)
            {
                string n = nd.SelectSingleNode("name").InnerText;
                string t = nd.SelectSingleNode("translation").InnerText;
                string s = nd.SelectSingleNode("section").InnerText;
                string c = nd.SelectSingleNode("column").InnerText;
                if (op.Load(nd.Name, n, t, s, c) == false)
                    RoadUpdateTool.WriteLog("FillDeletes could not load: " + nd.Name);
            }
            return op;
        }

        private _Services FillServices(_Services op, XmlNode ind)
        {
            foreach (XmlNode nd in ind)
            {
                string n = nd.SelectSingleNode("name").InnerText;
                string t = nd.SelectSingleNode("translation").InnerText;
                if (op.Load(nd.Name, n, t) == false)
                    RoadUpdateTool.WriteLog("FillServices could not load: " + nd.Name);
            }
            return op;
        }


        internal void Save()
        {
            int loc = 0;
            try
            {

            }
            catch (Exception ex)
            {
                RoadUpdateTool.WriteLog("Interface.Save loc: " + loc + " error: " + ex.Message + ":: Stacktrace: " + ex.StackTrace);;
            }
            xml.Save(us);
        }

        private ops ValidateSetting(string node)
        {
            //we already validated the file exists and has our node "UserSettings"
            ops setting;
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
            setting.name = xml.SelectSingleNode("UserSettings/" + node + "/name").InnerText;
            setting.translation = xml.SelectSingleNode("UserSettings/" + node + "/translation").InnerText;
            return setting;
        }
    }
}