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
        private int gridRow, gridColumn;

        private void OnEnable() => FindObjectOfType<Matcher>().OnMatchHappened += FindEmptyTiles;

        private void OnDisable() => FindObjectOfType<Matcher>().OnMatchHappened -= FindEmptyTiles;

        public void SetBoard( Grid<Tile> g ) {
            grid = g;
            gridRow = grid.Row;
            gridColumn = grid.Column;

            // local caching
            Tile downTile = null;
            Tile leftTile = null;

            for ( int r = 0 ; r < gridRow ; r++ ) {
                for ( int c = 0 ; c < gridColumn ; c++ ) {

                    // Since we are moving from left to right (row increment) and bottom to top (column increment),
                    // we can just check left and bottom neighbour of current tile to make decision of which dropData can be set for the current tile
                    leftTile = grid.LeftNeighbour( r, c );
                    downTile = grid.DownNeighbour( r, c );

                    Vector3 spawnPosition = grid.GetWorldPosition( r, c );
                    var tileInstance = Instantiate<Tile>( tilePf, spawnPosition, Quaternion.identity, this.transform );
                    tileInstance.name = $"Tile( {r}, {c})";

                    var dropInstance = Instantiate<Drop>( dropPf );
                    dropInstance.transform.SetParent( tileInstance.transform, false );

                    var availableDropSOs = dropSOs.Where( drop => drop != downTile?.Drop?.DropData && drop != leftTile?.Drop?.DropData );
                    dropInstance.DropData = availableDropSOs.ElementAt( Random.Range( 0, availableDropSOs.Count() ) );
                    tileInstance.Drop = dropInstance;

                    // Finally, set grid value by generated tile
                    grid.SetValue( r, c, tileInstance );
                }
            }
        }

        private void FindEmptyTiles() {
            Tile searchingTile = null;
            List<int> emptyColumns = new(); // Store columns indeces which contains some empty tiles

            for ( int c = 0 ; c < gridColumn ; c++ ) {
                for ( int r = 0 ; r < gridRow ; r++ ) {

                    searchingTile = grid.GetValue( r, c );
                    // Finding one empty tile enough, since we gonna check every tile in that column when pushing down, so breaking the inside for loop
                    if ( searchingTile?.Drop?.DropData == null ) {
                        emptyColumns.Add( c );
                        break;
                    }
                }
            }

            emptyColumns = emptyColumns.Distinct().ToList();

            if ( emptyColumns.Count != 0 ) {
                for ( int i = 0 ; i < emptyColumns.Count ; i++ ) {
                    PushTopTiles( emptyColumns[ i ] );
                }
            }
        }

        private void PushTopTiles( int columnIndex ) {
            Tile searchingTile = null;
            int emptyTileCount = 0;

            List<EmptyTileData> emptyTileDatas = new();

            for ( int r = 0 ; r < gridRow ; r++ ) {
                searchingTile = grid.GetValue( r, columnIndex );
                if ( searchingTile.Drop.DropData == null ) {
                    emptyTileCount++;
                }
                else {
                    if ( emptyTileCount != 0 ) {
                        emptyTileDatas.Add( new EmptyTileData( r, emptyTileCount ) );
                    }
                }
            }

            if ( emptyTileDatas.Count != 0 ) {
                for ( int i = 0 ; i < emptyTileDatas.Count ; i++ ) {
                    Tile tileToPush = grid.GetValue( emptyTileDatas[ i ].RowIndex, columnIndex );
                    Tile t = grid.GetValue( emptyTileDatas[ i ].RowIndex - emptyTileDatas[ i ].EmptyTileCount, columnIndex );

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

        private struct EmptyTileData {
            public int RowIndex { get; private set; }
            public int EmptyTileCount { get; private set; }

            public EmptyTileData( int rowIndex, int emptyTileCount ) {
                RowIndex = rowIndex;
                EmptyTileCount = emptyTileCount;
            }
        }
    }
}