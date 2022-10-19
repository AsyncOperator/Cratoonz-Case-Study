using System.Collections;
using UnityEngine;
using DG.Tweening;

public sealed class DropMover : MonoBehaviour {
    [Tooltip( "Time it takes to swap tiles between each other" )]
    [SerializeField] private float swapDuration;
    [Tooltip( "Time it takes to drop tiles from above to below (this is fixed time, means no matter how high or low tile is dropped it will always take same amount of time)" )]
    [SerializeField] private float dropDuration;

    public IEnumerator SwapTiles( Tile t1, Tile t2 ) {
        Drop t1drop = t1.Drop;
        Drop t2drop = t2.Drop;

        Sequence sequence = DOTween.Sequence();

        sequence.Join( t1drop.transform.DOMove( t2.transform.position, swapDuration ) );
        sequence.Join( t2drop.transform.DOMove( t1.transform.position, swapDuration ) );

        yield return sequence.WaitForCompletion();

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