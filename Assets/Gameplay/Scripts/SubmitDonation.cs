using UnityEngine;
using UnityEngine.UI;

public class FactionButtonHandler : MonoBehaviour
{
    public GameObject dropdownPanel;
    public void ShowDropdownPanel()
    {
        dropdownPanel.SetActive(true);
    }
}
