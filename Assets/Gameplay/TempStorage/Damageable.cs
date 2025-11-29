using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Damageable : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool destroyOnDeath = false;

    [Header("Target Settings")]
    [Tooltip("Only take damage from specific tags (leave empty for all)")]
    public string[] allowedDamageTags = new string[0];

    [Header("UI References")]
    [Tooltip("TextMeshPro component to display health percentage")]
    public TextMeshProUGUI healthText;
    
    [Tooltip("Slider component to display health bar")]
    public Slider healthSlider;
    
    [Tooltip("Image component that changes when damaged")]
    public Image healthStatusImage;
    
    [Tooltip("Sprite to show when alive/healthy")]
    public Sprite normalSprite;
    
    [Tooltip("Sprite to show when damaged")]
    public Sprite damagedSprite;
    
    [Tooltip("Sprite to show when dead")]
    public Sprite deadSprite;
    
    [Tooltip("Health percentage threshold to show damaged sprite (0-1)")]
    [Range(0f, 1f)]
    public float damagedThreshold = 0.5f;
    
    [Tooltip("Base width of the health slider (will scale with max health)")]
    public float baseSliderWidth = 200f;
    
    [Tooltip("Base max health value for slider width calculation")]
    public float baseMaxHealth = 100f;

    [Header("Events")]
    [Tooltip("Called when damage is taken")]
    public UnityEngine.Events.UnityEvent<float> onDamageTaken;
    
    [Tooltip("Called when health is recovered")]
    public UnityEngine.Events.UnityEvent<float> onHealthRecovered;

    [Tooltip("Called when health reaches 0")]
    public UnityEngine.Events.UnityEvent onDeath;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        
        // Set the base max health for slider scaling if not manually set
        if (baseMaxHealth == 0)
        {
            baseMaxHealth = maxHealth;
        }
        
        UpdateUI();
    }

    #region Damage Methods
    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log($"{gameObject.name} took {damageAmount} damage. Health: {currentHealth}/{maxHealth}");

        // Invoke damage event
        onDamageTaken?.Invoke(damageAmount);
        
        // Update UI
        UpdateUI();

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
    #endregion

    #region Health Recovery Methods
    /// <summary>
    /// Heals the entity by a specified amount (cannot exceed max health)
    /// </summary>
    public void Heal(float healAmount)
    {
        if (isDead) return;

        float previousHealth = currentHealth;
        currentHealth += healAmount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        float actualHealAmount = currentHealth - previousHealth;

        Debug.Log($"{gameObject.name} healed {actualHealAmount}. Health: {currentHealth}/{maxHealth}");

        // Invoke health recovered event
        onHealthRecovered?.Invoke(actualHealAmount);
        
        // Update UI
        UpdateUI();
    }

    /// <summary>
    /// Instantly restores health to maximum
    /// </summary>
    public void HealToFull()
    {
        if (isDead) return;

        float healAmount = maxHealth - currentHealth;
        currentHealth = maxHealth;

        Debug.Log($"{gameObject.name} fully healed. Health: {currentHealth}/{maxHealth}");

        // Invoke health recovered event
        onHealthRecovered?.Invoke(healAmount);
        
        // Update UI
        UpdateUI();
    }
    #endregion

    #region Max Health Methods
    /// <summary>
    /// Increases maximum health by a specified amount
    /// </summary>
    public void IncreaseMaxHealth(float amount)
    {
        maxHealth += amount;
        Debug.Log($"{gameObject.name} max health increased by {amount}. New max: {maxHealth}");
        
        // Update UI to reflect new percentage
        UpdateUI();
    }

    /// <summary>
    /// Increases maximum health and heals by the same amount
    /// </summary>
    public void IncreaseMaxHealthAndHeal(float amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        
        Debug.Log($"{gameObject.name} max health increased and healed by {amount}. Health: {currentHealth}/{maxHealth}");
        
        // Update UI
        UpdateUI();
    }

    /// <summary>
    /// Sets a new maximum health value (adjusts current health proportionally)
    /// </summary>
    public void SetMaxHealth(float newMaxHealth, bool maintainPercentage = true)
    {
        if (maintainPercentage)
        {
            float healthPercentage = GetHealthPercentage();
            maxHealth = newMaxHealth;
            currentHealth = maxHealth * healthPercentage;
        }
        else
        {
            maxHealth = newMaxHealth;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }
        
        Debug.Log($"{gameObject.name} max health set to {maxHealth}. Current health: {currentHealth}");
        
        // Update UI
        UpdateUI();
    }
    #endregion

    #region UI Methods
    /// <summary>
    /// Updates all UI elements based on current health status
    /// </summary>
    private void UpdateUI()
    {
        UpdateHealthText();
        UpdateHealthSlider();
        UpdateHealthImage();
    }

    /// <summary>
    /// Updates the health text display with current percentage
    /// </summary>
    private void UpdateHealthText()
    {
        if (healthText != null)
        {
            float percentage = GetHealthPercentage() * 100f;
            healthText.text = $"{percentage:F0}%";
        }
    }

    /// <summary>
    /// Updates the health slider value and scales width based on max health
    /// </summary>
    private void UpdateHealthSlider()
    {
        if (healthSlider != null)
        {
            // Update slider value
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
            
            // Scale slider width based on max health increase
            RectTransform sliderRect = healthSlider.GetComponent<RectTransform>();
            if (sliderRect != null)
            {
                float healthRatio = maxHealth / baseMaxHealth;
                float newWidth = baseSliderWidth * healthRatio;
                sliderRect.sizeDelta = new Vector2(newWidth, sliderRect.sizeDelta.y);
            }
        }
    }

    /// <summary>
    /// Updates the health status image based on current health
    /// </summary>
    private void UpdateHealthImage()
    {
        if (healthStatusImage == null) return;

        // Store the current sprite to check if it changed
        Sprite newSprite = null;

        if (isDead && deadSprite != null)
        {
            // Show dead sprite
            newSprite = deadSprite;
        }
        else if (GetHealthPercentage() <= damagedThreshold && damagedSprite != null)
        {
            // Show damaged sprite
            newSprite = damagedSprite;
        }
        else if (normalSprite != null)
        {
            // Show normal sprite
            newSprite = normalSprite;
        }

        // Only update if sprite changed
        if (newSprite != null && healthStatusImage.sprite != newSprite)
        {
            healthStatusImage.sprite = newSprite;
            
            // Set image to native size to prevent scaling issues
            healthStatusImage.SetNativeSize();
        }
    }
    #endregion

    #region Death and Reset Methods
    void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log($"{gameObject.name} has been destroyed!");

        // Update UI to show death state
        UpdateUI();

        // Invoke death event
        onDeath?.Invoke();
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        gameObject.SetActive(true);
        
        // Update UI to show reset state
        UpdateUI();
    }
    #endregion

    #region Utility Methods
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    public bool IsDead()
    {
        return isDead;
    }
    #endregion
}