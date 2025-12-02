using System.Collections.Generic;
//using MoreMountains.Feedbacks;
using UnityEngine;
using TMPro;
//using UnityEngine.UIElements;
//using System.Xml;

public class TaxCollectorHelper : MonoBehaviour
{
    public GameObject factionsPanel = null;
    public GameObject donationPanel = null;
    GameManager gm = null;
    CurrencyTracker currencyTracker = null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gm = FindFirstObjectByType<GameManager>();
        currencyTracker = FindFirstObjectByType<CurrencyTracker>();
    }

    public void CursorLock()
    {
        if (gm)
        {
            gm.CursorLock();
        }
    }

    public void CursorUnlock()
    {
        if (gm)
        {
            gm.CursorUnlock();
        }
    }

    public void SetFactionsValues()
    {
        StartCoroutine(FactionService.GetFactions(OnFactionsLoaded));
    }

    void OnFactionsLoaded(List<Faction> factions)
    {
        foreach (var faction in factions)
        {
            var factionsToSet = factionsPanel.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var a in factionsToSet)
            {
                if (a.transform.parent.name == faction.factionname)
                {
                    a.text = faction.total.ToString();
                }
            }
        }
        donationPanel.SetActive(true);
        var script = donationPanel.GetComponent<Leaderboard>();   // get the script
        script.UpdatePanel(factions, currencyTracker);
    }  

    public void ShowDonationPanel()
    {
        if (FactionService.FactionSelected != null)
        {
            SetFactionsValues();
        }
    }        
}
