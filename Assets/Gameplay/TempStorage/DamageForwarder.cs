using UnityEngine;

public class EnemyDamageForwarder : MonoBehaviour
{
    [HideInInspector]
    public EnemyController parentEnemy;

    void Start()
    {
        if (parentEnemy == null)
            parentEnemy = GetComponentInParent<EnemyController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        ArtilleryShellBehavior shell = other.GetComponent<ArtilleryShellBehavior>();
        if (shell != null)
        {
            parentEnemy.TakeDamage(shell.damage);
        }
    }
}