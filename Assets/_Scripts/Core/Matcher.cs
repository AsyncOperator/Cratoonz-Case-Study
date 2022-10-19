using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Grid = Core.Grid<Tile>;
using Core.Board;

public sealed class Matcher : MonoBehaviour {
    private const int REPEAT_TIMES_TO_COUNT_AS_MATCH = 3;

    [SerializeField] private BoardCreator boardCreator;
    [SerializeField] private BoardSetter boardSetter;
    [SerializeField] private DropMover dropMover;

    private Grid grid;
    private int gridRowCount, gridColumnCount;

    public event Action OnMatchHappened;

    private void OnEnable() {
        boardSetter.OnBoardUpdated += TryFindMatchesOnWholeGrid;
        dropMover.OnSwapped += TryFindMatches;

        boardCreator.OnBoardCreated += ( g ) => {
            grid = g;
            gridRowCount = grid.Row;
            gridColumnCount = grid.Column;
        };
    }

    private void TryFindMatchesOnWholeGrid() {
        List<Tile> tiles = new();

        for ( int r = 0 ; r < gridRowCount ; r++ ) {
            var tmpList = FindMatchesOnRow( r );
            if ( tmpList.Count != 0 ) {
                tiles.AddRange( tmpList );
            }
        }

        for ( int c = 0 ; c < gridColumnCount ; c++ ) {
            var tmpList = FindMatchesOnColumn( c );
            if ( tmpList.Count != 0 ) {
                tiles.AddRange( tmpList );
            }
        }

        tiles = tiles.Distinct().ToList();

        if ( tiles.Count != 0 ) {
            RemoveTilesDrop( tiles );
        }
    }

    private bool TryFindMatches( Vector2Int firstRowColumn, Vector2Int secondRowColumn ) {
        List<Tile> tiles = new();
        List<Tile> returnedTile = new();

        returnedTile = FindMatchesOnRow( firstRowColumn.x );

        for ( int i = 0 ; i < returnedTile.Count ; i++ ) {
            tiles.Add( returnedTile[ i ] );
        }

        returnedTile.Clear();
        returnedTile = FindMatchesOnColumn( firstRowColumn.y );

        for ( int i = 0 ; i < returnedTile.Count ; i++ ) {
            tiles.Add( returnedTile[ i ] );
        }

        returnedTile.Clear();
        returnedTile = FindMatchesOnRow( secondRowColumn.x );

        for ( int i = 0 ; i < returnedTile.Count ; i++ ) {
            tiles.Add( returnedTile[ i ] );
        }

        returnedTile.Clear();
        returnedTile = FindMatchesOnColumn( secondRowColumn.y );

        for ( int i = 0 ; i < returnedTile.Count ; i++ ) {
            tiles.Add( returnedTile[ i ] );
        }

        tiles = tiles.Distinct().ToList();

        // Remove dropData inside tiles list
        if ( tiles.Count != 0 ) {
            RemoveTilesDrop( tiles );
            return true;
        }
        else {
            return false;
        }
    }

    private async void RemoveTilesDrop( List<Tile> tiles ) {
        Task[] tasks = new Task[ tiles.Count ];

        for ( int i = 0 ; i < tiles.Count ; i++ ) {
            tasks[ i ] = tiles[ i ].DropShrinker();
        }

        await Task.WhenAll( tasks );

        OnMatchHappened?.Invoke();
    }

    private List<Tile> FindMatchesOnRow( int rowIndex ) {
        List<Tile> matchingTiles = new();

        Tile headTile = null;
        int columnIndex = 0;
        int repetitionTimes = 1;

        for ( int c = 0 ; c < gridColumnCount ; c++ ) {
            Tile currentTile = grid.GetValue( rowIndex, c );

            // If head tile is not null and current tile we are searching has a valid dropData
            if ( headTile == null && currentTile?.Drop?.DropData != null ) {
                headTile = currentTile;
                columnIndex = c;
                repetitionTimes = 1;
            }

            else if ( headTile != null && headTile.Drop?.DropData == currentTile.Drop.DropData ) {
                repetitionTimes++;

                if ( repetitionTimes >= REPEAT_TIMES_TO_COUNT_AS_MATCH ) {
                    for ( int i = 0 ; i < repetitionTimes ; i++ ) {
                        matchingTiles.Add( grid.GetValue( rowIndex, columnIndex + i ) );
                    }
                }
            }

            else {
                headTile = currentTile.Drop.DropData == null ? null : currentTile;
                columnIndex = c;
                repetitionTimes = 1;
            }
        }

        matchingTiles = matchingTiles.Distinct().ToList();
        return matchingTiles;
    }

    private List<Tile> FindMatchesOnColumn( int columnIndex ) {
        List<Tile> matchingTiles = new();

        Tile headTile = null;
        int rowIndex = 0;
        int repetitionTimes = 1;

        for ( int r = 0 ; r < gridRowCount ; r++ ) {
            Tile currentTile = grid.GetValue( r, columnIndex );

            // If head tile is not null and current tile we are searching has a valid dropData
            if ( headTile == null && currentTile?.Drop?.DropData != null ) {
                headTile = currentTile;
                rowIndex = r;
                repetitionTimes = 1;
            }

            else if ( headTile != null && headTile.Drop?.DropData == currentTile.Drop.DropData ) {
                repetitionTimes++;

                if ( repetitionTimes >= REPEAT_TIMES_TO_COUNT_AS_MATCH ) {
                    for ( int i = 0 ; i < repetitionTimes ; i++ ) {
                        matchingTiles.Add( grid.GetValue( rowIndex + i, columnIndex ) );
                    }
                }
            }

            else {
                headTile = currentTile.Drop.DropData == null ? null : currentTile;
                rowIndex = r;
                repetitionTimes = 1;
            }
        }

        matchingTiles = matchingTiles.Distinct().ToList();
        return matchingTiles;
    }
}