using UnityEngine;
using AsyncOperator.Extensions;

[RequireComponent( typeof( SpriteRenderer ) )]
public sealed class Drop : MonoBehaviour {
    [SerializeField] private SpriteRenderer spriteRenderer;

    #region Properties
    private DropSO dropData;

    public DropSO DropData {
        get => dropData;
        set
        {
            dropData = value;
            spriteRenderer.sprite = dropData.dropSprite;
        }
    }
    #endregion

    private void OnValidate() {
        gameObject.GetComponent( ref spriteRenderer );
    }
}