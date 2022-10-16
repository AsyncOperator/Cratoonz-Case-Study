using UnityEngine;

[CreateAssetMenu( fileName = "NewDropData.asset", menuName = "Create new Drop Data" )]
public sealed class DropSO : ScriptableObject {
    public enum DropType {
        Red,
        Green,
        Blue,
        Yellow
    }

    public Sprite dropSprite;
    public DropType dropType;
}