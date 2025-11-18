using UnityEngine;
using UnityEngine.UIElements;
    
public class ShopMenuView : MonoBehaviour
{
    [SerializeField]
    VisualTreeAsset m_ListEntryTemplate;
    
    void OnEnable()
    {
        // The UXML is already instantiated by the UIDocument component
        var uiDocument = GetComponent<UIDocument>();
    
        // Initialize the character list controller
        var shopMenuController = new ShopMenuController();
        shopMenuController.InitializeItemList(uiDocument.rootVisualElement, m_ListEntryTemplate);
    }
}
