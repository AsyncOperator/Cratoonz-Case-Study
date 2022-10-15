using UnityEngine;
using Grid = Core.Grid<Tile>;
using AsyncOperator.Extensions;

[RequireComponent( typeof( BoardSetter ) )]
public sealed class BoardCreator : MonoBehaviour {
    [SerializeField] private BoardSetter boardSetter;

    [Min( 1 ), SerializeField] private int gridWidth = 8;
    [Min( 1 ), SerializeField] private int gridHeight = 8;
    [SerializeField] private float gridTileSize = 1f;
    [SerializeField] private Vector3 gridOrigin = Vector3.zero;

    private Grid grid;

    private void OnValidate() => gameObject.GetComponent( ref boardSetter );

    private void Start() {
        grid = new Grid( gridWidth, gridHeight, gridTileSize, gridOrigin );
        boardSetter.SetBoard( grid, gridWidth, gridHeight );
    }
}