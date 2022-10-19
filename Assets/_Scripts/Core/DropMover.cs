using System.Collections;
using UnityEngine;
using DG.Tweening;

public sealed class DropMover : MonoBehaviour {
    [SerializeField] private float swapDuration;

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
        Drop toDrop = to.Drop;

        fromDrop.transform.parent = to.transform;
        toDrop.transform.parent = from.transform;

        fromDrop.transform.DOLocalMove( Vector3.zero, swapDuration ).SetEase( Ease.OutBack ).OnComplete( () => {
            from.Drop = toDrop;
            to.Drop = fromDrop;
        } );
    }
}