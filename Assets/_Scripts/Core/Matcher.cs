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

    private List<Tile> tiles = new();

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
        for ( int r = 0 ; r < gridRowCount ; r++ ) {
            FindMatchesOnRow( tiles, r );
            tiles.AddRange( tiles );
        }

        for ( int c = 0 ; c < gridColumnCount ; c++ ) {
            FindMatchesOnColumn( tiles, c );
            tiles.AddRange( tiles );
        }

        tiles = tiles.Distinct().ToList();

        if ( tiles.Count != 0 ) {
            RemoveTilesDrop( tiles );
        }
    }

    private bool TryFindMatches( Vector2Int firstRowColumn, Vector2Int secondRowColumn ) {
        FindMatchesOnRow( tiles, firstRowColumn.x );
        FindMatchesOnColumn( tiles, firstRowColumn.y );
        FindMatchesOnRow( tiles, secondRowColumn.x );
        FindMatchesOnColumn( tiles, secondRowColumn.y );

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

        tiles.Clear();

        OnMatchHappened?.Invoke();
    }

    private void FindMatchesOnRow( List<Tile> tiles, int rowIndex ) {
        Tile headTile = null;
        Tile currentTile = null;

        int columnIndex = 0;
        int repetitionTimes = 1;

        for ( int c = 0 ; c < gridColumnCount ; c++ ) {
            currentTile = grid.GetValue( rowIndex, c );

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
                        tiles.Add( grid.GetValue( rowIndex, columnIndex + i ) );
                    }
                }
            }

            else {
                headTile = currentTile.Drop.DropData == null ? null : currentTile;
                columnIndex = c;
                repetitionTimes = 1;
            }
        }
    }

    private void FindMatchesOnColumn( List<Tile> tiles, int columnIndex ) {
        Tile headTile = null;
        Tile currentTile = null;

        int rowIndex = 0;
        int repetitionTimes = 1;

        for ( int r = 0 ; r < gridRowCount ; r++ ) {
            currentTile = grid.GetValue( r, columnIndex );

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
                        tiles.Add( grid.GetValue( rowIndex + i, columnIndex ) );
                    }
                }
            }

            else {
                headTile = currentTile.Drop.DropData == null ? null : currentTile;
                rowIndex = r;
                repetitionTimes = 1;
            }
        }
    }
}