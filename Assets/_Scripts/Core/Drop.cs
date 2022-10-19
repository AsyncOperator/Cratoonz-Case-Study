using UnityEngine;
using AsyncOperator.Extensions;

[RequireComponent( typeof( SpriteRenderer ) )]
public sealed class Drop : MonoBehaviour {
    [SerializeField] private SpriteRenderer spriteRenderer;

    #region Properties
    private DropSO dropData;

    /// <summary>
    /// Set dropData field, if it set to null then set spriteRenderer sprite to null otherwise set it to given value's sprite field
    /// </summary>
    public DropSO DropData {
        get => dropData;
        set
        {
            dropData = value;
            spriteRenderer.sprite = dropData != null ? dropData.DropSprite : null;
        }
    }
    #endregion

    private void OnValidate() => gameObject.GetComponent( ref spriteRenderer );

    public void AlwaysVisible() => spriteRenderer.maskInteraction = SpriteMaskInteraction.None;

    public void ConstraintVisible() => spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
}