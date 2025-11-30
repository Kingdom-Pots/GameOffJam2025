using TMPro;
using UnityEngine;

public class DonationValueValidator : MonoBehaviour
{
    TMP_InputField inputField;
    public int maxValue = 100; // set your max here

    void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        // Subscribe to input validation
        inputField.onValueChanged.AddListener(ValidateInput);
        inputField.onEndEdit.AddListener(FinalizeInput);
    }

    public void AddValue()
    {
        inputField.text = (int.Parse(inputField.text)+5).ToString();
    }

    public void SubtractValue()
    {
        inputField.text = (int.Parse(inputField.text)-5).ToString();
    }


    void ValidateInput(string text)
    {
        // Allow empty input while typing
        if (string.IsNullOrEmpty(text)) return;

        // Try parse
        if (int.TryParse(text, out int value))
        {
            // Clamp to positive and max
            if (value < 0)
                inputField.text = "0";
            else if (value > maxValue)
                inputField.text = maxValue.ToString();
        }
        else
        {
            // Remove invalid characters
            inputField.text = "";
        }
    }

    void FinalizeInput(string text)
    {
        // Ensure final value is valid
        if (string.IsNullOrEmpty(text))
            inputField.text = "0";
    }
}
