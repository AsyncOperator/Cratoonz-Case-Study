using UnityEngine;

[CreateAssetMenu( fileName = "NewDropData.asset", menuName = "Create new Drop Data" )]
public sealed class DropSO : ScriptableObject {
    public enum DropType {
        Red,
        Green,
        Blue,
        Yellow
    }

    [field: SerializeField] public Sprite DropSprite { get; private set; }
    [field: SerializeField] public DropType _DropType { get; private set; }
}