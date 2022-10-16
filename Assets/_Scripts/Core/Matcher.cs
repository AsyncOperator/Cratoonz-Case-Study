using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Grid = Core.Grid<Tile>;
using Core.Board;

public sealed class Matcher : MonoBehaviour {
    [Min( 3 ), SerializeField] private int repeatTimesToCountAsMatch;

    private Grid grid;

    private int gridWidth;
    private int gridHeight;

    private void OnEnable() {
        FindObjectOfType<BoardCreator>().OnBoardCreated += ( g ) => {
            grid = g;
            gridWidth = grid.Width;
            gridHeight = grid.Height;
        };
    }

    public void Match( Vector2Int firstRowColumn, Vector2Int secondRowColumn ) {
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

        // Delete dropData inside tile
        for ( int i = 0 ; i < tiles.Count ; i++ ) {
            tiles[ i ].Drop.DropData = null;
        }
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