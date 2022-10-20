using System;
using System.Threading.Tasks;
using UnityEngine;
using Core;
using Core.Board;
using DG.Tweening;

public sealed class DropMover : MonoBehaviour {
    [SerializeField] private Swiper swiper;
    [SerializeField] private BoardCreator boardCreator;

    [Space( 20 )]

    [Tooltip( "Time it takes to swap tiles between each other" )]
    [SerializeField] private float swapDuration;
    [Tooltip( "Time it takes to drop tiles from above to below (this is fixed time, means no matter how high or low tile is dropped it will always take same amount of time)" )]
    [SerializeField] private float dropDuration;

    private Grid<Tile> grid;

    public event Func<Vector2Int, Vector2Int, bool> OnSwapped;

    private void OnEnable() {
        swiper.OnSwipeCalculated += Swiper_OnSwipeCalculated;

        boardCreator.OnBoardCreated += ( g ) => grid = g;
    }

    private void OnDisable() {
        swiper.OnSwipeCalculated -= Swiper_OnSwipeCalculated;
    }

    private async void Swiper_OnSwipeCalculated( Vector2 worldPosition, Swiper.DirectionType swipeDirection ) {
        Tile selectedTile = grid.GetValue( worldPosition );

        if ( selectedTile == null ) {
            return;
        }
        else {
            InputManager.Instance.Enabled = false;
        }

        Tile targetTile = null;
        Vector2Int selectedTileXY = grid.GetXY( worldPosition );

        switch ( swipeDirection ) {
            case Swiper.DirectionType.Right:
                targetTile = grid.RightNeighbour( selectedTileXY.x, selectedTileXY.y );
                break;
            case Swiper.DirectionType.Left:
                targetTile = grid.LeftNeighbour( selectedTileXY.x, selectedTileXY.y );
                break;
            case Swiper.DirectionType.Up:
                targetTile = grid.UpNeighbour( selectedTileXY.x, selectedTileXY.y );
                break;
            case Swiper.DirectionType.Down:
                targetTile = grid.DownNeighbour( selectedTileXY.x, selectedTileXY.y );
                break;
        }
        if ( targetTile == null ) {
            await FakeMoveTo( selectedTile, swipeDirection );
            InputManager.Instance.Enabled = true;
        }
        else if ( targetTile.Drop?.DropData != null ) {
            Sequence( selectedTile, targetTile );
        }

        async void Sequence( Tile t1, Tile t2 ) {
            await SwapDrops( t1, t2 );

            Vector2Int firstRowColumn = grid.GetXY( t1.transform.position );
            Vector2Int secondRowColumn = grid.GetXY( t2.transform.position );

            bool? valid = OnSwapped?.Invoke( firstRowColumn, secondRowColumn );

            if ( valid == false ) {
                await SwapDrops( t1, t2 );
                InputManager.Instance.Enabled = true;
            }
        }
    }

    private async Task SwapDrops( Tile t1, Tile t2 ) {
        Drop t1drop = t1.Drop;
        Drop t2drop = t2.Drop;

        Sequence sequence = DOTween.Sequence();

        sequence.Join( t1drop.transform.DOMove( t2.transform.position, swapDuration ) );
        sequence.Join( t2drop.transform.DOMove( t1.transform.position, swapDuration ) );

        await sequence.AsyncWaitForCompletion();

        t1drop.transform.SetParent( t2.transform, true );
        t2drop.transform.SetParent( t1.transform, true );

        t1.Drop = t2drop;
        t2.Drop = t1drop;
    }

    private async Task FakeMoveTo( Tile tile, Swiper.DirectionType direction ) {
        Drop tileDrop = tile.Drop;
        Vector2 destination = Vector2.zero;

        switch ( direction ) {
            case Swiper.DirectionType.Right:
                destination = new Vector2( grid.TileSize / 2, 0 );
                break;
            case Swiper.DirectionType.Left:
                destination = new Vector2( -grid.TileSize / 2, 0 );
                break;
            case Swiper.DirectionType.Up:
                destination = new Vector2( 0, grid.TileSize / 2 );
                break;
            case Swiper.DirectionType.Down:
                destination = new Vector2( 0, -grid.TileSize / 2 );
                break;
        }

        // Before moving drop outside of the grid make it mask interaction to none after then set it visibleInsideMask again
        await tileDrop.transform.DOLocalMove( destination, swapDuration ).SetLoops( 2, LoopType.Yoyo ).OnStart( () => tileDrop.AlwaysVisible() ).OnComplete( () => tileDrop.ConstraintVisible() ).AsyncWaitForCompletion();
    }

    public async Task MoveTo( Tile from, Tile to ) {
        Drop fromDrop = from.Drop;

        await fromDrop.transform.DOMove( to.transform.position, dropDuration ).SetEase( Ease.OutBack ).AsyncWaitForCompletion();
    }
}