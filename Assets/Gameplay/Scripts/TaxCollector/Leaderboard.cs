using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;

public class Leaderboard : MonoBehaviour
{
    public GameObject rowPrefab;   // assign the Row prefab in Inspector
    public Transform panel;   
    public TMP_InputField donateField;
    public Transform mainPanel;
    public DonationValueValidator donationValueValidator; 

    int amountAdded = 0;

    CurrencyTracker cTracker;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void UpdatePanel(List<Faction> factions, CurrencyTracker currencyTracker)
    {
        cTracker = currencyTracker;
        // cTracker.currency = 20;
        var rowsCount = panel.transform.childCount;
        List<Faction> sorted = factions.OrderByDescending(f => f.total).ToList();
        if (rowsCount > 0)
        {
            var counter = 0;
            foreach (var faction in sorted)
            {
                var texts = panel.GetChild(counter++).gameObject.GetComponentsInChildren<TextMeshProUGUI>();
                texts[0].text = faction.factionname;
                texts[1].text = faction.total.ToString();
            }      
        }
        else
        {
            foreach (var faction in sorted)
            {
                GameObject row = Instantiate(rowPrefab, panel);
                var texts = row.GetComponentsInChildren<TextMeshProUGUI>();
                texts[0].text = faction.factionname;
                texts[1].text = faction.total.ToString();
            }
        }
        donateField.text = currencyTracker.currency.ToString();   
        donationValueValidator.maxValue = currencyTracker.currency;
    }

    public void Donate()
    {
        int amount = int.Parse(donateField.text);
        if (amount > cTracker.currency)
        {
            amount = cTracker.currency;
        }
        if (amount == 0) return;
        amountAdded = amount;
        cTracker.Use(amount);
        donateField.text = cTracker.currency.ToString();
        StartCoroutine(FactionService.AddToFactionTotal(FactionService.FactionSelected, amount, OnFactionUpdated));
    }
    

    void OnFactionUpdated(bool success)
    {
        var rowsCount = panel.childCount;
        string textAmount = "";
        if (rowsCount > 0)
        {
            for (int a = 0; a < rowsCount; a++)
            {
                if (panel.GetChild(a).gameObject.GetComponentsInChildren<TextMeshProUGUI>()[0].text == FactionService.FactionSelected)
                {
                    var texts = panel.GetChild(a).gameObject.GetComponentsInChildren<TextMeshProUGUI>();
                    texts[1].text = (int.Parse(texts[1].text)+amountAdded).ToString();
                    textAmount = texts[1].text;
                    break;
                }
            }      
        }
        var childCount = mainPanel.childCount;
        for (int a = 0; a < childCount; a++)
        {
            if (mainPanel.GetChild(a).transform.name == FactionService.FactionSelected && textAmount != "")
            {
                mainPanel.GetChild(a).transform.GetComponentInChildren<TextMeshProUGUI>().text = textAmount;
                break;
            }
        }
    }     
}
