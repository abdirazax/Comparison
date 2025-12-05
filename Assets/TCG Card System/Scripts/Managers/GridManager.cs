using System;
using UnityEngine;

namespace TCG_Card_System.Scripts.Managers
{
    public class GridManager : MonoBehaviour
    {
        private Grid _grid;
        public Vector3 CellSize
        {
            get
            {
                return _grid.cellSize;
            }
        }
        
        private void Start()
        {
            _grid = GetComponent<Grid>();
            if (_grid == null)
            {
                Debug.LogError("Grid component not found on the GameObject.");
            }
        }

        public Vector3 GetWorldPosition(Vector3Int position)
        {
            return _grid.GetCellCenterWorld(position);
        }
        public Vector3Int GetCellPosition(Vector3 worldPosition)
        {
            return _grid.WorldToCell(worldPosition);
        }


        private int GetCenteredPositionInt(int cardSlotIndex, int totalSlotCount, int cardSlotSize)
        {
            // Remember that one slot equals two grid cells in the grid system.
            // This is because it is easier to center the one-slot card if it consists of two grid cells.
            if (totalSlotCount <= 0)
                throw new ArgumentException("Total slot count must be greater than zero.");
            int xPosition = (cardSlotIndex * 2 - totalSlotCount) + cardSlotSize;
            
            return xPosition;
        }

        private Vector3Int GetXCenteredPositionInt(int cardSlotIndex, int totalSlotCount, int cardSlotSize, Vector3Int gridOffset)
        {
            int xPosition = GetCenteredPositionInt(cardSlotIndex, totalSlotCount, cardSlotSize);
            return new Vector3Int(xPosition, 0, 0) + gridOffset;
        }

        public Vector3 GetXCenteredPosition(int cardSlotIndex, int totalSlotCount, int cardSlotSize, Vector3Int gridOffset)
        {
            return GetWorldPosition(GetXCenteredPositionInt(cardSlotIndex, totalSlotCount, cardSlotSize, gridOffset));
        }
        
    }
}