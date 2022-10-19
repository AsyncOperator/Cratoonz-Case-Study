using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Grid = Core.Grid<Tile>;
using AsyncOperator.Extensions;

namespace Core.Board {
    public sealed class BoardSetter : MonoBehaviour {
        [SerializeField] private Tile tilePf;
        [SerializeField] private Drop dropPf;

        [SerializeField] private DropSO[] dropSOs;

        [SerializeField] private DropMover dropMover;

        private Grid grid;
        private int gridRow, gridColumn;

        [SerializeField] private int[] spawnerColumns;

        private void OnEnable() {
            FindObjectOfType<Matcher>().OnMatchHappened += FindEmptyTiles;
            FindObjectOfType<BoardCreator>().OnSpawnerColumnsGenerated += ( spawnerArr ) => spawnerColumns = (int[])spawnerArr.Clone();
        }

        private void OnDisable() => FindObjectOfType<Matcher>().OnMatchHappened -= FindEmptyTiles;

        public void SetBoard( Grid<Tile> g ) {
            grid = g;
            gridRow = grid.Row;
            gridColumn = grid.Column;

            // local caching
            Tile downTile = null;
            Tile downDownTile = null;
            Tile leftTile = null;
            Tile leftLeftTile = null;

            List<DropSO> dropSOscannotBeUsed = new();

            for ( int r = 0 ; r < gridRow ; r++ ) {
                for ( int c = 0 ; c < gridColumn ; c++ ) {

                    // Since we are moving from left to right (row increment) and bottom to top (column increment),
                    // we can just check left and bottom neighbour of current tile to make decision of which dropData can be set for the current tile
                    downTile = grid.DownNeighbour( r, c );
                    downDownTile = grid.DownNeighbour( r - 1, c );
                    leftTile = grid.LeftNeighbour( r, c );
                    leftLeftTile = grid.LeftNeighbour( r, c - 1 );

                    if ( downTile?.Drop?.DropData == downDownTile?.Drop?.DropData ) {
                        dropSOscannotBeUsed.Add( downTile?.Drop.DropData );
                    }
                    if ( leftTile?.Drop?.DropData == leftLeftTile?.Drop?.DropData ) {
                        dropSOscannotBeUsed.Add( leftTile?.Drop.DropData );
                    }

                    Vector3 spawnPosition = grid.GetWorldPosition( r, c );
                    var tileInstance = Instantiate<Tile>( tilePf, spawnPosition, Quaternion.identity, this.transform );
                    tileInstance.name = $"Tile( {r}, {c})";

                    var dropInstance = Instantiate<Drop>( dropPf );
                    dropInstance.transform.SetParent( tileInstance.transform, false );

                    var availableDropSOs = dropSOs.Except( dropSOscannotBeUsed );
                    //var availableDropSOs = dropSOs.Where( drop => drop != downTile?.Drop?.DropData && drop != leftTile?.Drop?.DropData );
                    dropInstance.DropData = availableDropSOs.ElementAt( Random.Range( 0, availableDropSOs.Count() ) );
                    tileInstance.Drop = dropInstance;

                    // Finally, set grid value by generated tile
                    grid.SetValue( r, c, tileInstance );

                    dropSOscannotBeUsed.Clear();
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

            if ( emptyColumns.Count != 0 ) {
                for ( int i = 0 ; i < emptyColumns.Count ; i++ ) {
                    Debug.Log( $"Empty column {emptyColumns[ i ]}" );
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
                        var data = new EmptyTileData( r, emptyTileCount );
                        emptyTileDatas.Add( data );
                    }
                }
            }

            StartCoroutine( PushCO() );

            IEnumerator PushCO() {
                if ( emptyTileDatas.Count != 0 ) {
                    for ( int i = 0 ; i < emptyTileDatas.Count ; i++ ) {
                        Tile fromTile = grid.GetValue( emptyTileDatas[ i ].RowIndex, columnIndex );
                        Tile toTile = grid.GetValue( emptyTileDatas[ i ].RowIndex - emptyTileDatas[ i ].EmptyTileCount, columnIndex );

                        dropMover.MoveTo( fromTile, toTile );
                    }

                    // This must be greater than dropMover dropDuration time
                    yield return new WaitForSeconds( 0.2f );

                    for ( int i = 0 ; i < emptyTileDatas.Count ; i++ ) {
                        Tile fromTile = grid.GetValue( emptyTileDatas[ i ].RowIndex, columnIndex );
                        Tile toTile = grid.GetValue( emptyTileDatas[ i ].RowIndex - emptyTileDatas[ i ].EmptyTileCount, columnIndex );

                        Drop fromTileDrop = fromTile.Drop;
                        Drop toTileDrop = toTile.Drop;

                        fromTileDrop.transform.parent = toTile.transform;
                        toTileDrop.transform.parent = fromTile.transform;

                        toTileDrop.transform.ResetTransformation();
                        fromTileDrop.transform.ResetTransformation();

                        fromTile.Drop = toTileDrop;
                        toTile.Drop = fromTileDrop;
                    }

                    // Fill empty top tiles
                    for ( int i = 0 ; i < spawnerColumns.Length ; i++ ) {
                        for ( int r = gridRow - 1 ; r >= 0 ; r-- ) {
                            Tile check = grid.GetValue( r, spawnerColumns[ i ] );

                            if ( check.Drop.DropData == null ) {
                                check.Drop.DropData = dropSOs[ Random.Range( 0, dropSOs.Length ) ];
                            }
                            else {
                                break;
                            }
                        }

                    }
                }
            }
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