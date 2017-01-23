using AnotherRoadUpdateTool.Helpers;
using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using System;
using UnityEngine;
using static UnityEngine.Object;

/// <summary>
/// Updated from SkylinesRoadUpdate added global preferences
/// added Additional selections
/// </summary>
namespace AnotherRoadUpdateTool
{
    public class Mod : IUserMod
    {
        private UICheckBox cbChirper;
        private UICheckBox cbAllRoads;
        private UICheckBox cbMaxAreas;
        private UICheckBox cbStartMoney;
        private UICheckBox cbAutoDistroy;
        private UISlider slMaxArea;
        private UITextField tfMaxArea;
        private UISlider slStartMoney;
        private UITextField tfStartMoney;

        private UserSettings us = new UserSettings();
        private UICheckBox cbDelete;
        private UICheckBox cbUpdate;
        private UICheckBox cbTerrain;
        private UICheckBox cbServices;
        private UICheckBox cbDistricts;
        
        public string Description
        {
            get { return "ARUT is used to take the best mods, according to me, and add them into one larger control."; }
        }

        public string Name
        {
            get { return "Another Road Update Tool - Version " + ProductVersion + ")"; }
        }

        private static string ProductVersion
        {
            get
            {
                Version ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string var = ver.Major + "." + ver.Minor + "." + ver.Build;
                return var;
            }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            UIHelperBase group = helper.AddGroup("ARUT Settings");

            group.AddGroup("ARUT Options");

            cbDelete = (UICheckBox)group.AddCheckbox("Delete panel", us.ShowDelete, ShowDelete_Checked);
            cbDelete.tooltip = "Toggle to show or hide Delete panel";

            cbUpdate = (UICheckBox)group.AddCheckbox("Update panel", us.ShowUpdate, ShowUpdate_Checked);
            cbUpdate.tooltip = "Toggle to show or hide Update panel";

            cbTerrain = (UICheckBox)group.AddCheckbox("Terrain panel", us.ShowTerrain, ShowTerrain_Checked);
            cbTerrain.tooltip = "Toggle to show or hide Terrain panel";

            cbServices = (UICheckBox)group.AddCheckbox("Services panel", us.ShowServices, ShowServices_Checked);
            cbServices.tooltip = "Toggle to show or hide Services panel";

            cbDistricts = (UICheckBox)group.AddCheckbox("Districts panel", us.ShowDistricts, ShowDistricts_Checked);
            cbDistricts.tooltip = "Toggle to show or hide Districts panel";

            cbChirper = (UICheckBox)group.AddCheckbox("Chirper panel ", us.Chirper, ShowChirper_Checked);
            cbChirper.tooltip = "Toggle to show or hide Chirper panel";

            cbAllRoads = (UICheckBox)group.AddCheckbox("All Roads Enabled", us.AllRoads, AllRoadsEnabled_Checked);
            cbAllRoads.tooltip = "Check to enable all road types.";

            cbAutoDistroy = (UICheckBox)group.AddCheckbox("Enable Auto Distroy", us.AutoDistroy, AutoDistroy_Checked);
            cbAutoDistroy.tooltip = "Check to enable Auto Distroy.";

            //set up the Max Area
            cbMaxAreas = (UICheckBox)group.AddCheckbox("Adjust Max Areas", us.AdjustAreas, AdjustAreas_Checked);
            cbMaxAreas.tooltip = " Check to adjust Games Max Areas.";
            slMaxArea = (UISlider)group.AddSlider("Max Areas (1 - 25)", 1, 25, 1f, us.MaxAreas, MaxArea_Changed);
            tfMaxArea = (UITextField)group.AddTextfield("Max Areas", us.MaxAreas.ToString(), MaxAreas_Changed);
            tfMaxArea.readOnly = true;
            tfMaxArea.width = 100;
            slMaxArea.enabled = cbMaxAreas.isChecked;
            tfMaxArea.enabled = cbMaxAreas.isChecked;

            //Set up Start Money
            cbStartMoney = (UICheckBox)group.AddCheckbox("Adjust Start up Money", us.AdjustMoney, AdjustMoney_Checked);
            cbStartMoney.tooltip = " Check to adjust new Games Start up money.";
            slStartMoney = (UISlider)group.AddSlider("Start up Amount", 100000, 750000, 50000, (int)us.StartMoney, StartMoney_Changed);
            tfStartMoney = (UITextField)group.AddTextfield("Start amount", us.StartMoney.ToString(), StartMoneys_Changed);
            tfStartMoney.readOnly = true;
            slStartMoney.enabled = cbStartMoney.isChecked;
            tfStartMoney.enabled = cbStartMoney.isChecked;
        }

        private void ShowDistricts_Checked(bool isChecked)
        {
            us.Districts = isChecked;
            us.Save();
        }

        private void ShowServices_Checked(bool isChecked)
        {
            us.Services = isChecked;
            us.Save();
        }

        private void ShowTerrain_Checked(bool isChecked)
        {
            us.Terrain = isChecked;
            us.Save();
        }

        private void ShowUpdate_Checked(bool isChecked)
        {
            us.Update = isChecked;
            us.Save();
        }

        private void ShowDelete_Checked(bool isChecked)
        {
            us.Delete = isChecked;
            us.Save();
        }

        private void AutoDistroy_Checked(bool isChecked)
        {
            us.AutoDistroy = isChecked;
            us.Save();
        }

        private void AdjustAreas_Checked(bool isChecked)
        {
            us.AdjustAreas = isChecked;
            tfMaxArea.enabled = isChecked;
            slMaxArea.enabled = isChecked;
            us.Save();
        }

        private void MaxAreas_Changed(string text)
        {
            //do nothing
            return;
        }

        private void MaxArea_Changed(float val)
        {
            us.MaxAreas = (int)val;
            tfMaxArea.text = us.MaxAreas.ToString();
            us.Save();
        }

        private void AdjustMoney_Checked(bool isChecked)
        {
            us.AdjustMoney = isChecked;
            tfStartMoney.enabled = isChecked;
            slStartMoney.enabled = isChecked;
            us.Save();
        }

        private void StartMoneys_Changed(string text)
        {
            //do nothing
            return;
        }

        private void StartMoney_Changed(float val)
        {
            us.StartMoney = (int)val;
            tfStartMoney.text = us.StartMoney.ToString();
            us.Save();
        }

        private void ShowChirper_Checked(bool isChecked)
        {
            us.Chirper = isChecked;
            us.Save();
        }

        private void AllRoadsEnabled_Checked(bool isChecked)
        {
            us.AllRoads = isChecked;
            us.Save();
        }
    }

    public class LoadingExtension : LoadingExtensionBase
    {
        public static Chirper chirp = new Chirper();
        public static UnlockRoads roads = new UnlockRoads();

        public ARUT updateTool;

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            updateTool = FindObjectOfType<ARUT>();
            if(updateTool == null)
            {
                GameObject gameController = GameObject.FindWithTag("GameController");
                updateTool = gameController.AddComponent<ARUT>();
                //ARUT.WriteLog("Setting Game Controller to updatetool object.");
            }

            if (mode == LoadMode.NewGame)
            {
                try
                {
                    var type = typeof(EconomyManager);
                    var cashAmountField = type.GetField("m_cashAmount", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                    cashAmountField.SetValue(EconomyManager.instance, ARUT.StartMoney * 100);
                    //ARUT.WriteLog("Set Cash Amount to " + ARUT.StartMoney.ToString("$0.00"));
                }
                catch (Exception ex)
                {
                    ARUT.WriteError("Error setting Cash Amount", ex);
                }
            }
            //ARUT.WriteLog("Calling InitGui.");
            updateTool.InitGui(mode, chirp, roads);
        }
    }

}
