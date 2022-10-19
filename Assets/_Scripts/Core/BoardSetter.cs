using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Grid = Core.Grid<Tile>;
using AsyncOperator.Extensions;

namespace Core.Board {
    public sealed class BoardSetter : MonoBehaviour {
        [SerializeField] private BoardCreator boardCreator;
        [SerializeField] private Matcher matcher;
        [SerializeField] private DropMover dropMover;

        [Space( 20 )]

        [SerializeField] private Tile tilePf;
        [SerializeField] private Drop dropPf;

        [SerializeField] private DropSO[] dropSOs;

        private Grid grid;
        private int gridRowCount, gridColumnCount;

        private int[] spawnerColumns;

        private void OnEnable() {
            matcher.OnMatchHappened += FindColumnsContainsEmptyTile;
            boardCreator.OnBoardCreated += SetBoard;

            boardCreator.OnSpawnerColumnsGenerated += ( spawnerArr ) => {
                var modifiedSpawnerArr = spawnerArr.Distinct().ToArray();
                spawnerColumns = (int[])modifiedSpawnerArr.Clone();
            };
        }

        private void OnDisable() => matcher.OnMatchHappened -= FindColumnsContainsEmptyTile;

        private void SetBoard( Grid<Tile> g ) {
            grid = g;
            gridRowCount = grid.Row;
            gridColumnCount = grid.Column;

            // local caching
            Tile downTile = null;
            Tile downDownTile = null;
            Tile leftTile = null;
            Tile leftLeftTile = null;

            List<DropSO> dropSOscannotBeUsed = new();

            for ( int r = 0 ; r < gridRowCount ; r++ ) {
                for ( int c = 0 ; c < gridColumnCount ; c++ ) {

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

        private async void FindColumnsContainsEmptyTile() {
            Tile searchingTile = null;
            List<int> emptyColumns = new(); // Store columns indeces which contains some empty tiles

            for ( int c = 0 ; c < gridColumnCount ; c++ ) {
                for ( int r = 0 ; r < gridRowCount ; r++ ) {

                    searchingTile = grid.GetValue( r, c );
                    // Finding one empty tile enough, since we gonna check every tile in that column when pushing down, so breaking the inside for loop
                    if ( searchingTile?.Drop?.DropData == null ) {
                        emptyColumns.Add( c );
                        break;
                    }
                }
            }

            if ( emptyColumns.Count != 0 ) {
                Task[] tasks = new Task[ emptyColumns.Count ];

                for ( int i = 0 ; i < emptyColumns.Count ; i++ ) {
                    tasks[ i ] = FindTilesWillBePushed( emptyColumns[ i ] );
                }

                await Task.WhenAll( tasks );

                Spawn();
            }
        }

        private async Task FindTilesWillBePushed( int columnIndex ) {
            Tile searchingTile = null;
            int emptyTileCount = 0;

            List<EmptyTileData> emptyTileDatas = new();

            for ( int r = 0 ; r < gridRowCount ; r++ ) {
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

            await PushTiles( emptyTileDatas, columnIndex );
        }

        private async Task PushTiles( List<EmptyTileData> emptyTileDatas, int columnIndex ) {
            if ( emptyTileDatas.Count != 0 ) {
                for ( int i = 0 ; i < emptyTileDatas.Count ; i++ ) {
                    Tile fromTile = grid.GetValue( emptyTileDatas[ i ].RowIndex, columnIndex );
                    Tile toTile = grid.GetValue( emptyTileDatas[ i ].RowIndex - emptyTileDatas[ i ].EmptyTileCount, columnIndex );

                    dropMover.MoveTo( fromTile, toTile );
                }

                // This must be greater than dropMover dropDuration time
                await Task.Delay( 200 );

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
            }
        }

        private void Spawn() {
            for ( int i = 0 ; i < spawnerColumns.Length ; i++ ) {
                for ( int r = gridRowCount - 1 ; r >= 0 ; r-- ) {
                    Tile check = grid.GetValue( r, spawnerColumns[ i ] );

                    if ( check.Drop.DropData == null ) {
                        check.Drop.DropData = dropSOs[ Random.Range( 0, dropSOs.Length ) ];
                    }
                    else {
                        // Since we pushed tiles before spawning new ones we now that when we are start searching from top to bottom the first not null tile is the indication of that all the others in below that are also full so no need to check return early
                        break;
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