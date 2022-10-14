using UnityEngine;
using Grid = Core.Grid<Tile>;

public sealed class BoardCreator : MonoBehaviour {
    [SerializeField] private Tile tilePf;

    [Min( 1 ), SerializeField] private int gridWidth;
    [Min( 1 ), SerializeField] private int gridHeight;
    [SerializeField] private int gridTileSize;
    [SerializeField] private Vector3 gridOrigin;

    private Grid grid;

    private void Start() {
        grid = new Grid( gridWidth, gridHeight, gridTileSize, gridOrigin );
        CreateTiles();
    }

    private void CreateTiles() {
        for ( int w = 0 ; w < gridWidth ; w++ ) {
            for ( int h = 0 ; h < gridHeight ; h++ ) {
                Instantiate<Tile>( tilePf, grid.GetWorldPosition( w, h ), Quaternion.identity, transform );
            }
        }
    }
}