using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Grid = Core.Grid<Tile>;
using Core.Board;
using System;
using System.Threading.Tasks;

public sealed class Matcher : MonoBehaviour {
    [Min( 3 ), SerializeField] private int repeatTimesToCountAsMatch;

    private Grid grid;
    private int gridWidth, gridHeight;

    public event Action OnMatchHappened;

    private void OnEnable() {
        FindObjectOfType<BoardCreator>().OnBoardCreated += ( g ) => {
            grid = g;
            gridWidth = grid.Width;
            gridHeight = grid.Height;
        };
    }

    public bool Match( Vector2Int firstRowColumn, Vector2Int secondRowColumn ) {
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

        Debug.Log( tiles.Count );
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
        List<Tile> tiles = new();

        Tile tileIterator = grid.GetValue( rowIndex, 0 );
        int indexIterator = 0;
        int repetationIterator = 1;

        for ( int y = 1 ; y < gridHeight ; y++ ) {
            Tile t = grid.GetValue( rowIndex, y );

            if ( tileIterator.Drop.DropData == t.Drop.DropData ) {
                repetationIterator++;

                if ( repetationIterator >= repeatTimesToCountAsMatch ) {
                    for ( int i = 0 ; i < repetationIterator ; i++ ) {
                        tiles.Add( grid.GetValue( rowIndex, indexIterator + i ) );
                    }
                }
            }

            else {
                indexIterator = y;
                repetationIterator = 1;
                tileIterator = grid.GetValue( rowIndex, y );
            }
        }

        tiles = tiles.Distinct().ToList();
        return tiles;
    }

    private List<Tile> FindMatchesOnColumn( int columnIndex ) {
        List<Tile> tiles = new();

        Tile tileIterator = grid.GetValue( 0, columnIndex );
        int indexIterator = 0;
        int repetationIterator = 1;

        for ( int x = 1 ; x < gridWidth ; x++ ) {
            Tile t = grid.GetValue( x, columnIndex );

            if ( tileIterator.Drop.DropData == t.Drop.DropData ) {
                repetationIterator++;

                if ( repetationIterator >= repeatTimesToCountAsMatch ) {
                    for ( int i = 0 ; i < repetationIterator ; i++ ) {
                        tiles.Add( grid.GetValue( indexIterator + i, columnIndex ) );
                    }
                }
            }

            else {
                indexIterator = x;
                repetationIterator = 1;
                tileIterator = grid.GetValue( x, columnIndex );
            }
        }

        tiles = tiles.Distinct().ToList();
        return tiles;
    }
}