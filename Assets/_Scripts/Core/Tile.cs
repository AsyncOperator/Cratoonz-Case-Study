using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

// Tile needs spriteRenderer to display sprite
[RequireComponent( typeof( SpriteRenderer ) )]
public sealed class Tile : MonoBehaviour {
    [Tooltip( "Defines duration of tween" )]
    [SerializeField] private float dropShrinkDuration;

    public Drop Drop { get; set; }

    /// <summary>
    /// Scale down Drop property transform to zero then set Drop property's DropData to null
    /// </summary>
    public async Task DropShrinker() {
        if ( Drop != null ) {
            await Drop.transform.DOScale( Vector3.zero, dropShrinkDuration ).OnComplete( () => {
                Drop.DropData = null;
                Drop.transform.localScale = Vector3.one;
            } ).AsyncWaitForCompletion();
        }
    }
}