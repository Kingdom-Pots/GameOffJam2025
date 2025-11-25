using UnityEngine;
using UnityEngine.UIElements;
    
public class ShopMenuView : MonoBehaviour
{
    [SerializeField]
    VisualTreeAsset m_ListEntryTemplate;

    [SerializeField]
    GameObject m_TargetArtilleryGun;

    // UI element references
    Button m_BuyButton;
    ShopMenuController m_ShopMenuController;
    
    void OnEnable()
    {
        // The UXML is already instantiated by the UIDocument component
        var uiDocument = GetComponent<UIDocument>();

    
        // Initialize the character list controller
        m_ShopMenuController = new ShopMenuController();
        m_ShopMenuController.InitializeItemList(uiDocument.rootVisualElement, m_ListEntryTemplate);
        
        var root = uiDocument.rootVisualElement;
        m_BuyButton = root.Q<Button>("BuyButton");
        m_BuyButton.clicked += OnClick;
    }

    void OnDisable() {
        m_BuyButton.clicked -= OnClick;
    }

    void OnClick() 
    {
        Debug.Log("click");
        
        ShopMenuItemData selectedItemData = m_ShopMenuController.GetItemSelected();
        m_ShopMenuController.RemoveSelectedItem();

        m_TargetArtilleryGun = selectedItemData.GameObject;
    }
}
