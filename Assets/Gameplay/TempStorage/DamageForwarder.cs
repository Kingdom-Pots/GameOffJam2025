using UnityEngine;

public class EnemyDamageForwarder : MonoBehaviour
{
    [HideInInspector]
    public EnemyController parentEnemy;

    void Start()
    {
        if (parentEnemy == null)
        {
            parentEnemy = GetComponentInParent<EnemyController>();
        }
    }
}