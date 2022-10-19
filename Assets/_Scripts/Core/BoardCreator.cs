using System;
using UnityEngine;

namespace Core.Board {
    public sealed class BoardCreator : MonoBehaviour {
        #region Grid Settings
        [Header( "GRID SETTINGS" )]
        [Space( 10 )]
        [Tooltip( "How many rows will be in the grid" )]
        [Min( 1 ), SerializeField] private int gridRowCount = 8;
        [Tooltip( "How many columns will be in the grid" )]
        [Min( 1 ), SerializeField] private int gridColumnCount = 8;
        [Tooltip( "Scale of the tile (result value will be => Vector3.one * gridTileSize)" )]
        [Min( 1f ), SerializeField] private float gridTileSize = 1f;
        [Tooltip( "Set the grid pivot position (remember grid pivot is on bottom left corner)" )]
        [SerializeField] private Vector3 gridOrigin = Vector3.zero;
        [Delayed, SerializeField] private int[] spawnerColumns;
        #endregion

        private Grid<Tile> grid;

        public event Action<Grid<Tile>> OnBoardCreated;
        public event Action<int[]> OnSpawnerColumnsGenerated;

        private void OnValidate() {
            // Spawner columns validation
            for ( int i = 0 ; i < spawnerColumns.Length ; i++ ) {
                if ( spawnerColumns[ i ] >= gridColumnCount ) {
                    Debug.LogError( $"Element at ({i}) You exceed grid column count, given value <color=red> ( {spawnerColumns[ i ]} ) </color> should be smaller than <color=red> ( {gridColumnCount} ) </color>" );
                }
                else if ( spawnerColumns[ i ] < 0 ) {
                    Debug.LogError( $"Element at ({i})<color=yellow><b> You cannot give a negative number </b></color>" );
                }
                else {
                    Debug.Log( $"Element at ({i}) Seems valid huh" );
                }
            }
        }

        private void Start() {
            grid = new Grid<Tile>( gridRowCount, gridColumnCount, gridTileSize, gridOrigin );

            OnBoardCreated?.Invoke( grid );
            if ( spawnerColumns.Length != 0 ) {
                OnSpawnerColumnsGenerated?.Invoke( spawnerColumns );
            }
        }
    }
}