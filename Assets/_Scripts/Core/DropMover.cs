using UnityEngine;
using DG.Tweening;
using System.Collections;

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



}