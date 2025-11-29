using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FactionOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject popup; // Assign your popup text GameObject in Inspector

    public void OnPointerEnter(PointerEventData eventData)
    {
        gameObject.GetComponent<UnityEngine.UI.Image>().color = Color.green;
        popup.GetComponents<TextMeshProUGUI>()[0].text = gameObject.name;
        popup.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        gameObject.GetComponent<UnityEngine.UI.Image>().color = Color.white;
        popup.SetActive(false);
    }
}
