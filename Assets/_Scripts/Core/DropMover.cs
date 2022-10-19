using System;
using System.Collections;
using UnityEngine;
using Core;
using Core.Board;
using DG.Tweening;
using System.Threading.Tasks;

public sealed class DropMover : MonoBehaviour {
    [SerializeField] private Swiper swiper;

    [Tooltip( "Time it takes to swap tiles between each other" )]
    [SerializeField] private float swapDuration;
    [Tooltip( "Time it takes to drop tiles from above to below (this is fixed time, means no matter how high or low tile is dropped it will always take same amount of time)" )]
    [SerializeField] private float dropDuration;

    private Grid<Tile> grid;

    public event Func<Vector2Int, Vector2Int, bool> OnSwapped;

    private void OnEnable() {
        FindObjectOfType<BoardCreator>().OnBoardCreated += ( g ) => grid = g;
        swiper.OnSwipeCalculated += Swiper_OnSwipeCalculated;
    }

    private void OnDisable() {
        FindObjectOfType<BoardCreator>().OnBoardCreated += ( g ) => grid = g;
        swiper.OnSwipeCalculated -= Swiper_OnSwipeCalculated;
    }

    private void Swiper_OnSwipeCalculated( Vector2 worldPosition, Swiper.DirectionType swipeDirection ) {
        Tile selectedTile = grid.GetValue( worldPosition );

        if ( selectedTile == null ) {
            return;
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
        if ( targetTile?.Drop?.DropData != null ) {
            Sequence( selectedTile, targetTile );
        }

        async void Sequence( Tile t1, Tile t2 ) {
            await SwapTiles( t1, t2 );

            Vector2Int firstRowColumn = grid.GetXY( t1.transform.position );
            Vector2Int secondRowColumn = grid.GetXY( t2.transform.position );

            bool? valid = OnSwapped?.Invoke( firstRowColumn, secondRowColumn );

            if ( valid == false ) {
                await SwapTiles( t1, t2 );
            }
        }
    }

    public async Task SwapTiles( Tile t1, Tile t2 ) {
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

    public void MoveTo( Tile from, Tile to ) {
        Drop fromDrop = from.Drop;

        fromDrop.transform.DOMove( to.transform.position, dropDuration ).SetEase( Ease.OutBack );
    }
}