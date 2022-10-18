using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Grid = Core.Grid<Tile>;

namespace Core.Board {
    public sealed class BoardSetter : MonoBehaviour {
        [SerializeField] private Tile tilePf;
        [SerializeField] private Drop dropPf;

        [SerializeField] private DropSO[] dropSOs;

        private Grid grid;
        private int gridWidth, gridHeight;

        private void OnEnable() => FindObjectOfType<Matcher>().OnMatchHappened += FindEmptyTiles;

        private void OnDisable() => FindObjectOfType<Matcher>().OnMatchHappened -= FindEmptyTiles;

        public void SetBoard( Grid<Tile> g ) {
            grid = g;
            gridWidth = grid.Width;
            gridHeight = grid.Height;

            // local caching
            Tile downTile = null;
            Tile leftTile = null;

            for ( int x = 0 ; x < gridWidth ; x++ ) {
                for ( int y = 0 ; y < gridHeight ; y++ ) {

                    // Since we are moving from left to right (row increment) and bottom to top (column increment),
                    // we can just check left and bottom neighbour of current tile to make decision of which dropData can be set for the current tile
                    leftTile = grid.LeftNeighbour( x, y );
                    downTile = grid.DownNeighbour( x, y );

                    Vector3 spawnPosition = grid.GetWorldPosition( x, y );
                    var tileInstance = Instantiate<Tile>( tilePf, spawnPosition, Quaternion.identity, this.transform );
                    tileInstance.name = $"Tile( {x}, {y})";

                    var dropInstance = Instantiate<Drop>( dropPf );
                    dropInstance.transform.SetParent( tileInstance.transform, false );

                    var availableDropSOs = dropSOs.Where( drop => drop != downTile?.Drop?.DropData && drop != leftTile?.Drop?.DropData );
                    dropInstance.DropData = availableDropSOs.ElementAt( Random.Range( 0, availableDropSOs.Count() ) );
                    tileInstance.Drop = dropInstance;

                    // Finally, set grid value by generated tile
                    grid.SetValue( x, y, tileInstance );
                }
            }
        }

        private void FindEmptyTiles() {
            Tile searchingTile = null;
            List<int> emptyTilesColumn = new();

            for ( int x = 0 ; x < gridWidth ; x++ ) {
                for ( int y = 0 ; y < gridHeight ; y++ ) {
                    searchingTile = grid.GetValue( x, y );
                    // Finding one empty tile enough, since we gonna check every tile in that column when pushing down, so breaking the inside for loop
                    if ( searchingTile != null && searchingTile.Drop?.DropData == null ) {
                        emptyTilesColumn.Add( x );
                        break;
                    }
                }
            }

            emptyTilesColumn = emptyTilesColumn.Distinct().ToList();

            if ( emptyTilesColumn.Count != 0 ) {
                for ( int i = 0 ; i < emptyTilesColumn.Count ; i++ ) {
                    PushTopTiles( emptyTilesColumn[ i ] );
                }
            }
        }

        private void PushTopTiles( int rowIndex ) {
            Tile searchingTile = null;
            int emptyTileCount = 0;

            List<Data> datas = new();

            for ( int y = 0 ; y < gridHeight ; y++ ) {
                searchingTile = grid.GetValue( rowIndex, y );
                if ( searchingTile.Drop.DropData == null ) {
                    emptyTileCount++;
                }
                else {
                    if ( emptyTileCount != 0 ) {
                        datas.Add( new Data( y, emptyTileCount ) );
                    }
                }
            }

            if ( datas.Count != 0 ) {
                for ( int i = 0 ; i < datas.Count ; i++ ) {
                    Tile tileToPush = grid.GetValue( rowIndex, datas[ i ].RowIndex );
                    Tile t = grid.GetValue( rowIndex, datas[ i ].RowIndex - datas[ i ].EmptyTileCount );

                    t.Drop.DropData = tileToPush.Drop.DropData;
                    tileToPush.Drop.DropData = null;
                }
            }

            //// Fill empty top tiles
            //for ( int x = 0 ; x < gridWidth ; x++ ) {
            //    Tile t = grid.GetValue( x, columnIndex );
            //    if ( t?.Drop?.DropData == null ) {
            //        Debug.Log( "Hakan" );
            //        t.Drop.DropData = dropSOs[ Random.Range( 0, dropSOs.Length ) ];
            //    }
            //}
        }

        private struct Data {
            public int RowIndex { get; private set; }
            public int EmptyTileCount { get; private set; }

            public Data( int rowIndex, int emptyTileCount ) {
                RowIndex = rowIndex;
                EmptyTileCount = emptyTileCount;
            }
        }
    }
}