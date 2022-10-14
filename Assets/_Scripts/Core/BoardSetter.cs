using UnityEngine;
using Grid = Core.Grid<Tile>;

public sealed class BoardSetter : MonoBehaviour {
    [SerializeField] private Tile tilePf;
    [SerializeField] private Drop dropPf;

    [SerializeField] private DropSO[] dropSOs;

    public void SetBoard( Grid grid, int width, int height ) {

        for ( int w = 0 ; w < width ; w++ ) {
            for ( int h = 0 ; h < height ; h++ ) {
                Vector3 worldPosition = grid.GetWorldPosition( w, h );

                var tileInstance = Instantiate<Tile>( tilePf, worldPosition, Quaternion.identity, this.transform );
                var dropInstance = Instantiate<Drop>( dropPf );

                dropInstance.transform.SetParent( tileInstance.transform, false );
                dropInstance.DropData = dropSOs[ Random.Range( 0, dropSOs.Length ) ];
            }
        }
    }
}