using System.Linq;
using UnityEngine;
using Grid = Core.Grid<Tile>;

public sealed class BoardSetter : MonoBehaviour {
    [SerializeField] private Tile tilePf;
    [SerializeField] private Drop dropPf;

    [SerializeField] private DropSO[] dropSOs;

    public void SetBoard( Grid grid, int width, int height ) {
        Tile downTile = null;
        Tile leftTile = null;

        for ( int x = 0 ; x < width ; x++ ) {
            for ( int y = 0 ; y < height ; y++ ) {
                leftTile = grid.LeftNeighbour( x, y );
                downTile = grid.DownNeighbour( x, y );

                Vector3 spawnPosition = grid.GetWorldPosition( x, y );
                var tileInstance = Instantiate<Tile>( tilePf, spawnPosition, Quaternion.identity, this.transform );

                var dropInstance = Instantiate<Drop>( dropPf );
                dropInstance.transform.SetParent( tileInstance.transform, false );

                var availableDropSOs = dropSOs.Where( drop => drop != downTile?.Drop.DropData && drop != leftTile?.Drop.DropData );
                dropInstance.DropData = availableDropSOs.ElementAt( Random.Range( 0, availableDropSOs.Count() ) );
                tileInstance.Drop = dropInstance;

                grid.SetValue( x, y, tileInstance );
            }
        }
    }
}