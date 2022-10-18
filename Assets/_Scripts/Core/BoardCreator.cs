using System;
using UnityEngine;
using AsyncOperator.Extensions;

namespace Core.Board {
    [RequireComponent( typeof( BoardSetter ) )]
    public sealed class BoardCreator : MonoBehaviour {
        [SerializeField] private BoardSetter boardSetter;

        #region Grid Settings
        [Space( 10 )]
        [Header( "GRID SETTINGS" )]
        [Space( 15 )]

        [Tooltip( "How many rows will be in the grid" )]
        [Min( 1 ), SerializeField] private int gridRowCount = 8;
        [Tooltip( "How many columns will be in the grid" )]
        [Min( 1 ), SerializeField] private int gridColumnCount = 8;
        [Tooltip( "Scale of the tile (result value will be => Vector3.one * gridTileSize)" )]
        [Min( 1f ), SerializeField] private float gridTileSize = 1f;
        [Tooltip( "Set the grid pivot position (remember grid pivot is on bottom left corner)" )]
        [SerializeField] private Vector3 gridOrigin = Vector3.zero;
        #endregion

        private Grid<Tile> grid;

        public event Action<Grid<Tile>> OnBoardCreated;

        private void OnValidate() => gameObject.GetComponent( ref boardSetter );

        private void Start() {
            grid = new Grid<Tile>( gridRowCount, gridColumnCount, gridTileSize, gridOrigin );

            boardSetter.SetBoard( grid );
            OnBoardCreated?.Invoke( grid );
        }
    }
}