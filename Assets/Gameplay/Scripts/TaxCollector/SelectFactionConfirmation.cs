//using System.Collections.Generic;
//using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;
//using UnityEngine.UI;


public class SelectFactionConfirmation : MonoBehaviour
{
    public GameObject factionsMainPanel = null;
    public TextMeshProUGUI factionsTitle = null;   

    public TaxCollectorHelper taxHelper = null; 
    //public GameObject popup;

    void Start()
    {

    }

    public void CancelSelection()
    {
        FactionOnHover.selectionEnabled = true;
        var colorsToClean = factionsMainPanel.GetComponentsInChildren<UnityEngine.UI.Image>();
        foreach (var a in colorsToClean)
        {
            if (a.tag == "Faction")
            {
                a.color = Color.white;
            }
        }
    }

    public void SelectFaction()
    {
       FactionService.FactionSelected = FactionOnHover.lastSelectedFaction;
       gameObject.SetActive(false);
       FactionOnHover.selectionEnabled = true;
       factionsTitle.text = "Your Faction: " + FactionService.FactionSelected;
       taxHelper.SetFactionsValues();
    }
}
