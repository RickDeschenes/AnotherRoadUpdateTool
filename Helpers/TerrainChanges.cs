using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AnotherRoadUpdateTool.Helpers
{
    internal class TerrainChanges
    {

        private int m_minX = 0;
        private int m_maxX = 0;
        private int m_minZ = 0;
        private int m_maxZ = 0;
        private Vector3 m_startPosition;
        private Vector3 m_endPosition;
        private float m_terrainHeight;
        private ushort[] m_originalHeights;
        private ushort[] m_backupHeights;
        private ushort[] m_rawHeights;

        //private BindingList<ARUT.UndoStroke> undoList;

        public TerrainChanges()
        {

        }

        public TerrainChanges(Vector3 _startPosition, Vector3 _endPosition, float _terrainHeight, ushort[] _originalHeights, ushort[] _backupHeights, ushort[] _rawHeights)
        {
            this.m_startPosition = _startPosition;
            this.m_endPosition = _endPosition;
            this.m_terrainHeight = _terrainHeight;
            this.m_originalHeights = _originalHeights;
            this.m_backupHeights = _backupHeights;
            this.m_rawHeights = _rawHeights;
        }

        public void ApplyTerrainChange()
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
                        TerrainModify.BeginUpdateArea();
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

                ////does this redraw the screen
                //transform.Translate(new Vector3(0, 0, 0));

                string coords = minX + ", " + minZ + ") : (" + maxX + ", " + maxZ + ") diff = (" + (maxX - minX) + ", " + (maxZ - minZ) + ")";
                log = "Exiting ApplyBrush: (minX, minZ) : (maxX, maxZ) = (" + coords;
                //WriteLog(log);
            }
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
            //ARUT.WriteLog("Entering EndStroke");
            //creating the undo stroke
            ARUT.UndoStroke item = default(ARUT.UndoStroke);
            item.name = "undo: " + ARUT.UndoList.Count;
            item.minX = m_minX;
            item.maxX = m_maxX;
            item.minZ = m_minZ;
            item.maxZ = m_maxZ;
            item.pointer = ARUT.UndoList.Count;
            item.rawHeights = m_rawHeights;
            item.backupHeights = m_backupHeights;
            item.originalHeights = m_originalHeights;
            
            ARUT.UndoList.Add(item);
            //ARUT.WriteLog("Exiting EndStroke");
        }

    }
}
