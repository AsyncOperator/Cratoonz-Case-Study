using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;

public sealed class Tile : MonoBehaviour {
    [Tooltip( "Defines duration of tween" )]
    [SerializeField] private float dropShrinkDuration;

    public Drop Drop { get; set; }

    public async Task DropShrinker() {
        if ( Drop != null ) {
            await Drop.transform.DOScale( Vector3.zero, dropShrinkDuration ).OnComplete( () => {
                Drop.DropData = null;
                Drop.transform.localScale = Vector3.one;
            } ).AsyncWaitForCompletion();
        }
    }
}