using UnityEngine;
using AsyncOperator.Extensions;
using System;

namespace Core.Board {
    [RequireComponent( typeof( BoardSetter ) )]
    public sealed class BoardCreator : MonoBehaviour {
        [SerializeField] private BoardSetter boardSetter;

        #region Grid Settings
        [Space( 10 )]
        [Header( "GRID SETTINGS" )]
        [Space( 15 )]

        [Tooltip( "How many tiles will be on the grid row" )]
        [Min( 1 ), SerializeField] private int gridWidth = 8;
        [Tooltip( "How many tiles will be on the grid column" )]
        [Min( 1 ), SerializeField] private int gridHeight = 8;
        [Tooltip( "Scale of the tile (result value will be => Vector3.one * gridTileSize)" )]
        [Min( 1f ), SerializeField] private float gridTileSize = 1f;
        [Tooltip( "Set the grid pivot position (remember grid pivot is on bottom left corner)" )]
        [SerializeField] private Vector3 gridOrigin = Vector3.zero;
        #endregion

        private Grid<Tile> grid;

        public event Action<Grid<Tile>> OnBoardCreated;

        private void OnValidate() => gameObject.GetComponent( ref boardSetter );

        private void Start() {
            grid = new Grid<Tile>( gridWidth, gridHeight, gridTileSize, gridOrigin );
            boardSetter.SetBoard( grid, gridWidth, gridHeight );

            OnBoardCreated?.Invoke( grid );
        }
    }
}