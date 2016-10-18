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
        private UICheckBox cbChirperEnabled;
        private UICheckBox cbAllRoadsEnabled;
        private UICheckBox cbMaxAreasEnabled;
        private UICheckBox cbStartMoneyEnabled;
        private UISlider slMaxArea;
        private UITextField tfMaxArea;
        private UISlider slStartMoney;
        private UITextField tfStartMoney;

        public string Description
        {
            get { return "Another Road Update tool is used to take the best mods, according to me, and add them into one larger control. I also added settings and other options to allow for more persoanl flavors"; }
        }

        public string Name
        {
            get { return "Another Road Update Tool (Version 1.2.1500"; }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            UIHelperBase group = helper.AddGroup("ARUT Settings");

            group.AddGroup("Options");

            cbChirperEnabled = (UICheckBox)group.AddCheckbox("Chirper Toggle to Show or Hide", Properties.Settings.Default.Chirper, ShowChirper_Checked);

            group.AddCheckbox("All Roads Enabled", Properties.Settings.Default.AllRoads, AllRoadsEnabled_Checked);

            //set up the MAx Area
            group.AddCheckbox("Adjust Max Areas", Properties.Settings.Default.AdjustMoney, AdjustAreas_Checked);
            slMaxArea = (UISlider)group.AddSlider("Max Areas (1 - 25)", 1, 25, 1f, Properties.Settings.Default.MaxAreas, MaxArea_Changed);
            tfMaxArea = (UITextField)group.AddTextfield("Max Areas", Properties.Settings.Default.MaxAreas.ToString(), MaxAreas_Changed);
            tfMaxArea.readOnly = true;
            tfMaxArea.width = 100;

            group.AddCheckbox("Adjust Start up Money", Properties.Settings.Default.AdjustMoney, AdjustMoney_Checked);
            slStartMoney = (UISlider)group.AddSlider("Start up Amount", 100000, 750000, 50000, (int)Properties.Settings.Default.StartMoney, StartMoney_Changed);
            tfStartMoney = (UITextField)group.AddTextfield("Start amount", Properties.Settings.Default.StartMoney.ToString(), StartMoneys_Changed);
            tfMaxArea.readOnly = true;
        }

        private void AdjustAreas_Checked(bool isChecked)
        {
            Properties.Settings.Default.AdjustAreas = isChecked;
            tfMaxArea.enabled = isChecked;
            slMaxArea.enabled = isChecked;
        }

        private void MaxAreas_Changed(string text)
        {
            //do nothing
            return;
        }

        private void MaxArea_Changed(float val)
        {
            Properties.Settings.Default.MaxAreas = (int)val;
            tfMaxArea.text = Properties.Settings.Default.MaxAreas.ToString();
        }

        private void AdjustMoney_Checked(bool isChecked)
        {
            Properties.Settings.Default.AdjustAreas = isChecked;
            tfStartMoney.enabled = isChecked;
            slStartMoney.enabled = isChecked;
        }

        private void StartMoneys_Changed(string text)
        {
            //do nothing
            return;
        }

        private void StartMoney_Changed(float val)
        {
            Properties.Settings.Default.StartMoney = (int)val;
            tfStartMoney.text = Properties.Settings.Default.StartMoney.ToString();
        }

        private void ShowChirper_Checked(bool isChecked)
        {
            Properties.Settings.Default.Chirper = isChecked;
        }

        private void AllRoadsEnabled_Checked(bool c)
        {
            Properties.Settings.Default.AllRoads = c;
        }
    }

    public class LoadingExtension : LoadingExtensionBase
    {
        public static Chirper chirp = new Chirper();
        public static UnlockRoads roads = new UnlockRoads();

        public RoadUpdateTool updateTool;

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            updateTool = FindObjectOfType<RoadUpdateTool>();
            if(updateTool == null)
            {
                GameObject gameController = GameObject.FindWithTag("GameController");
                updateTool = gameController.AddComponent<RoadUpdateTool>();
            }

            if (mode == LoadMode.NewGame)
            {
                try
                {
                    var type = typeof(EconomyManager);
                    var cashAmountField = type.GetField("m_cashAmount", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                    cashAmountField.SetValue(EconomyManager.instance, 50000000);
                    RoadUpdateTool.WriteLog("Set Cash Amount to $500,000.00");
                }
                catch (Exception ex)
                {
                    RoadUpdateTool.WriteError("Error setting Cash Amount", ex);
                }
            }
            updateTool.InitGui(mode, chirp, roads);
        }
    }

}
