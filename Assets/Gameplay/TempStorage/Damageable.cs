using UnityEngine;

public class Damageable : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool destroyOnDeath = false;

    [Header("Visual Feedback")]
    public GameObject deathEffectPrefab;
    public GameObject damageEffectPrefab;
    public AudioClip deathSound;
    public AudioClip damageSound;

    [Header("Events")]
    [Tooltip("Called when damage is taken")]
    public UnityEngine.Events.UnityEvent<float> onDamageTaken;

    [Tooltip("Called when health reaches 0")]
    public UnityEngine.Events.UnityEvent onDeath;

    [Header("Debug")]
    public bool showHealthBar = true;

    private bool isDead = false;
    private AudioSource audioSource;

    void Start()
    {
        currentHealth = maxHealth;

        // Get or add audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (deathSound != null || damageSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log($"{gameObject.name} took {damageAmount} damage. Health: {currentHealth}/{maxHealth}");

        // Invoke damage event
        onDamageTaken?.Invoke(damageAmount);

        // Play damage effect
        if (damageEffectPrefab != null)
        {
            Instantiate(damageEffectPrefab, transform.position, Quaternion.identity);
        }

        // Play damage sound
        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }

        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log($"{gameObject.name} has been destroyed!");

        // Invoke death event
        onDeath?.Invoke();

        // Play death effect
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        // Play death sound
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // Destroy or disable
        if (destroyOnDeath)
        {
            Destroy(gameObject, 0.5f); // Delay to allow sound/effects to play
        }
        else
        {
            gameObject.SetActive(false);
        }
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