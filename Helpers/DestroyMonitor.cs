using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework;
using ICities;
using UnityEngine;

namespace AnotherRoadUpdateTool.Helpers
{
    public class DestroyMonitor : ThreadingExtensionBase
    {
        private BuildingManager _buildingManager;

        private SimulationManager _simulationManager;

        private EffectManager _effectManager;

        private EconomyManager _economyManager;

        private CoverageManager _coverageManager;

        private AudioGroup _nullAudioGroup;

        public DestroyMonitor()
        {
            Initialize();
        }

        private void Initialize()
        {
            this._buildingManager = Singleton<BuildingManager>.instance;
            this._simulationManager = Singleton<SimulationManager>.instance;
            this._effectManager = Singleton<EffectManager>.instance;
            this._economyManager = Singleton<EconomyManager>.instance;
            this._coverageManager = Singleton<CoverageManager>.instance;
            this._nullAudioGroup = new AudioGroup(0, new SavedFloat("NOTEXISTINGELEMENT", Settings.gameSettingsFile, 0f, false));
        }

        public override void OnAfterSimulationTick()
        {
            //if we have it disabled exit
            if (ARUT.AutoDistroy == false)
                return;
            try
            {
                for (ushort i = (ushort)(this._simulationManager.m_currentTickIndex % 1000); i < (int)this._buildingManager.m_buildings.m_buffer.Length; i = (ushort)(i + 1000))
                {
                    if (this._buildingManager.m_buildings.m_buffer[i].m_flags != Building.Flags.None && (ARUT.DemolishAbandoned && (this._buildingManager.m_buildings.m_buffer[i].m_flags & Building.Flags.Abandoned) != Building.Flags.None || ARUT.DemolishBurned && (this._buildingManager.m_buildings.m_buffer[i].m_flags & Building.Flags.BurnedDown) != Building.Flags.None))
                    {
                        this.DeleteBuildingImpl(ref i, ref this._buildingManager.m_buildings.m_buffer[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                ARUT.WriteError("Looping Error in Bulldozer? ", ex);
            }
        }

        private void DeleteBuildingImpl(ref ushort buildingId, ref Building building)
        {
            BuildingInfo info = building.Info;
            if (info.m_buildingAI.CheckBulldozing(buildingId, ref building) != ToolBase.ToolErrors.None)
            {
                return;
            }
            int buildingRefundAmount = this.GetBuildingRefundAmount(ref buildingId, ref building);
            if (buildingRefundAmount != 0)
            {
                this._economyManager.AddResource(EconomyManager.Resource.RefundAmount, buildingRefundAmount, info.m_class);
            }
            this.DispatchAutobulldozeEffect(info, ref building.m_position, ref building.m_angle, building.Length);
            this._buildingManager.ReleaseBuilding(buildingId);
            if (ItemClass.GetPublicServiceIndex(info.m_class.m_service) != -1)
            {
                this._coverageManager.CoverageUpdated(info.m_class.m_service, info.m_class.m_subService, info.m_class.m_level);
            }
        }

        private void DispatchAutobulldozeEffect(BuildingInfo info, ref Vector3 pos, ref float angle, int length)
        {
            EffectInfo mBulldozeEffect = this._buildingManager.m_properties.m_bulldozeEffect;
            if (mBulldozeEffect == null)
            {
                return;
            }
            InstanceID instanceID = new InstanceID();
            EffectInfo.SpawnArea spawnArea = new EffectInfo.SpawnArea(Matrix4x4.TRS(Building.CalculateMeshPosition(info, pos, angle, length), Building.CalculateMeshRotation(angle), Vector3.one), info.m_lodMeshData);
            this._effectManager.DispatchEffect(mBulldozeEffect, instanceID, spawnArea, Vector3.zero, 0f, 1f, this._nullAudioGroup);
        }

        private int GetBuildingRefundAmount(ref ushort id, ref Building building)
        {
            if (!this._simulationManager.IsRecentBuildIndex(building.m_buildIndex))
            {
                return 0;
            }
            return building.Info.m_buildingAI.GetRefundAmount(id, ref building);
        }
    }
}
