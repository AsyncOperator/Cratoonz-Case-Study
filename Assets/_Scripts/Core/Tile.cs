using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;

// Tile needs collider to be detected by raycast
// Tile needs spriteRenderer to display sprite, make sense huh?
[RequireComponent( typeof( BoxCollider2D ), typeof( SpriteRenderer ) )]
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