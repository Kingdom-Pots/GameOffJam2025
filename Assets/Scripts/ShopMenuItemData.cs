using UnityEngine;
    
[CreateAssetMenu]
public class ShopMenuItemData : ScriptableObject
{
    public string Name;
    public string Description;
    public float Cost;
    public float DamageGain;
    public float SpeedGain;
    public float ZoomGain;
    public Sprite Sprite;
    public GameObject GameObject;
}
