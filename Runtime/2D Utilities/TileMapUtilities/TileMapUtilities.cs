using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SF.Utilities
{
    public static class TileMapUtilities
    {
        public static void GetUsedTiles<TTileType>(this Tilemap tilemap, out List<TTileType> usedTiles) 
            where TTileType : TileBase
        {
            usedTiles = new();

            if (tilemap == null)
                return;
            
            tilemap.CompressBounds();
            BoundsInt bounds = tilemap.cellBounds;
            var positionEnumerator = bounds.allPositionsWithin;
            
            foreach (var position in positionEnumerator)
            {
                if(tilemap.HasTile(position))
                    usedTiles.Add(tilemap.GetTile<TTileType>(position));
            }
        }

        [BurstCompile]
        public static NativeList<Vector3Int> GetTileCellPositions(this Tilemap tilemap) 
        {
            NativeList<Vector3Int> tilePositions = new NativeList<Vector3Int>(Allocator.Persistent);
            
            if (tilemap == null)
                return tilePositions;
            
            tilemap.CompressBounds();
            BoundsInt bounds = tilemap.cellBounds;
            var positionEnumerator = bounds.allPositionsWithin;
            
            foreach (var position in positionEnumerator)
            {
                if(tilemap.HasTile(position))
                    tilePositions.Add(position);
            }

            return tilePositions;
        }

        [BurstCompile]
        public static void GetUsedTileData(this Tilemap tilemap, out List<TileData> usedTileData)
        {
            usedTileData = new();

            if (tilemap == null)
                return;
            
            tilemap.CompressBounds();
            BoundsInt bounds = tilemap.cellBounds;
            var positionEnumerator = bounds.allPositionsWithin;
            
            foreach (var position in positionEnumerator)
            {
                if (tilemap.HasTile(position))
                {
                    var tileData = new TileData();
                    tilemap.GetTile(position).GetTileData(position,tilemap, ref tileData);
                    usedTileData.Add(tileData);
                }
            }
        }
#if UNITY_6000_4_OR_NEWER
        [BurstCompile]
        public static int GetUsedTileData(this Tilemap tilemap, out Tilemap.PositionArray tilePositions ,out Tilemap.TileArray tileArray)
        {
            tileArray = new();
            tilePositions = new();
            
            if (tilemap == null)
                return 0;
            
            tilemap.CompressBounds();
            BoundsInt bounds = tilemap.cellBounds;
            
            tilemap.GetTiles(bounds, out tilePositions,out tileArray, allocator:Allocator.TempJob);
            return tileArray.Length;
        }
    #endif
    }
}
