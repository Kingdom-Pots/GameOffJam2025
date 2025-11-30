//using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//using System.Linq;
//using System.Collections.Generic;

public class FactionOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject popup; // Assign your popup text GameObject in Inspector

    public static bool selectionEnabled = true;

    public GameObject confirmSelection = null;
    public TextMeshProUGUI textConfirmSelection = null;
    public static string lastSelectedFaction = "";

    public void Start()
    {
        //Button btn = confirmSelection.transform.Find("ButtonOK")?.GetComponent<Button>();
        var selfObject = gameObject;
        Button btn = GetComponents<Button>()[0];
        btn.onClick.AddListener(() => {
            SelectionRequest();
        });       
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!FactionOnHover.selectionEnabled) return;
        if (FactionService.FactionSelected == null)
        {
            gameObject.GetComponent<UnityEngine.UI.Image>().color = Color.green;
            lastSelectedFaction = gameObject.name;
            textConfirmSelection.text = textConfirmSelection.text.Replace("<chosen faction>", gameObject.name);        
            
        }
        popup.SetActive(true);
        popup.GetComponents<TextMeshProUGUI>()[0].text = gameObject.name;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!FactionOnHover.selectionEnabled) return;
        if (FactionService.FactionSelected == null)
        {
            gameObject.GetComponent<UnityEngine.UI.Image>().color = Color.white;
        }
        popup.SetActive(false);        
    }

    void SelectionRequest()
    {
        if (FactionService.FactionSelected == null)
        {
            confirmSelection.SetActive(true);
            FactionOnHover.selectionEnabled = false;
        }
    }

    
}
