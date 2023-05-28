using UnityEngine;

[CreateAssetMenu(fileName = "TileSetting", menuName = "Wemade 2048/ Tile Settings", order = 0)]

public class TileSetting : ScriptableObject
{
    public float AnimationTime = .3f;

    public AnimationCurve AnimationCurve;

    public TileColor[] TileColors;
}
