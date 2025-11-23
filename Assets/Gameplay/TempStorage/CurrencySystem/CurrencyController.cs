using UnityEngine;
using UnityEngine.UI;

public class CurrencyTracker : MonoBehaviour
{
    // Currency Text UI
    public Text txt_Currency;

    // Default currency
    public int defaultCurrency;

    // current currency value
    public int currency;

    public void Init()
    {
        currency = defaultCurrency;
        UpdateUI();
    }

    // Gain currency
    public void Gain(int val)
    {
        currency += val;
        UpdateUI();
    }

    // Lose currency
    public bool Use(int val)
    {
        if (EnoughCurrency(val))
        {
            currency -= val;
            UpdateUI();
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
        //Checks if value is equal to or more than currency
        if (val >= currency)
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
