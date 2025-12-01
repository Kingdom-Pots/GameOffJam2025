using UnityEngine;
using UnityEngine.Events;
using TMPro;
using MoreMountains.Feedbacks;

public class CurrencyTracker : MonoBehaviour
{
    public MMF_Player TargetPlayer;

    // Currency Text UI
    public TMP_Text txt_Currency;

    // Default currency
    public int defaultCurrency;

    // current currency value
    public int currency;

    // Unity Events
    [System.Serializable]
    public class CurrencyEvent : UnityEvent<int> { }

    [Header("Events")]
    public CurrencyEvent OnCurrencyAdded;
    public CurrencyEvent OnCurrencyDeducted;

    public void Awake()
    {
        // Initialize events if they're null
        if (OnCurrencyAdded == null)
            OnCurrencyAdded = new CurrencyEvent();
        if (OnCurrencyDeducted == null)
            OnCurrencyDeducted = new CurrencyEvent();

        currency = defaultCurrency;
        UpdateUI();
    }

    // Gain currency
    public void Gain(int val)
    {
        MMF_FloatingText floatingTextFeedback = TargetPlayer.GetFeedbackOfType<MMF_FloatingText>();
        currency += val;
        floatingTextFeedback.Value = $"+{val}";
        UpdateUI();
        
        // Invoke the event with the amount added
        OnCurrencyAdded?.Invoke(val);
    }

    // Lose currency
    public bool Use(int val)
    {
        if (EnoughCurrency(val))
        {
            currency -= val;
            UpdateUI();
            
            // Invoke the event with the amount deducted
            OnCurrencyDeducted?.Invoke(val);
            return true;
        }
        else
        {
            return false;
        }
    }

    // Check availability of currency
    public bool EnoughCurrency(int val)
    {
        //Checks if value is equal to or lower than currency
        if (val <= currency)
            return true;
        else 
            return false;
    }
    
    // Update text UI
    void UpdateUI()
    {
        txt_Currency.text = currency.ToString();
    }
}