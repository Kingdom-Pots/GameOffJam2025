using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
    
public class ShopMenuView : MonoBehaviour
{
    [SerializeField]
    VisualTreeAsset m_ListEntryTemplate;

    [SerializeField]
    GameObject m_TargetArtilleryGun;

    // UI element references
    Button m_BuyButton;
    ShopMenuController m_ShopMenuController;

    void Awake() {
        // Initialize the character list controller
        m_ShopMenuController = new ShopMenuController();
        m_ShopMenuController.EnumerateAllItems();
    }
    
    void OnEnable()
    {
        // The UXML is already instantiated by the UIDocument component
        var uiDocument = GetComponent<UIDocument>();
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
        if (selectedItemData) 
        {
            m_ShopMenuController.RemoveSelectedItem();

            var oldGunTransform = m_TargetArtilleryGun.transform;
            var newGun = Instantiate(selectedItemData.GameObject, oldGunTransform.position, oldGunTransform.rotation, oldGunTransform.parent);
            foreach (Transform child in oldGunTransform)
            {
                child.SetParent(newGun.transform);
            }
            Destroy(m_TargetArtilleryGun);
            m_TargetArtilleryGun = newGun;

            /*
            GameObject shopNPC = GameObject.Find("ShopNPC");
            shopNPC.SetActive(false);
            */
            InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(Key.Escape));
        }
    }
}
