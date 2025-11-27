using UnityEngine;

public class Damageable: MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool destroyOnDeath = false;

    [Header("TargetSettings")]
    [Tooltip("Only take damage from specific tags (leave empty for all)")]
    public string[] allowedDamageTags = new string[0];

    [Header("Events")]
    [Tooltip("Called when damage is taken")]
    public UnityEngine.Events.UnityEvent<float> onDamageTaken;

    [Tooltip("Called when health reaches 0")]
    public UnityEngine.Events.UnityEvent onDeath;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log($"{gameObject.name} took {damageAmount} damage. Health: {currentHealth}/{maxHealth}");

        // Invoke damage event
        onDamageTaken?.Invoke(damageAmount);

        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void TakeDamageFrom(float damageAmount, string attackerTag)
    {
        // Check if damage is allowed from this tag
        if (allowedDamageTags.Length > 0)
        {
            bool isAllowed = false;
            foreach (string tag in allowedDamageTags)
            {
                if (tag == attackerTag)
                {
                    isAllowed = true;
                    break;
                }
            }

            if (!isAllowed)
            {
                Debug.Log($"{gameObject.name} ignored damage from {attackerTag}");
                return;
            }
        }

        // Deal damage
        TakeDamage(damageAmount);
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log($"{gameObject.name} has been destroyed!");

        // Invoke death event
        onDeath?.Invoke();
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    public bool IsDead()
    {
        return isDead;
    }
    
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        gameObject.SetActive(true);
    }
}