using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Blobcreate.ProjectileToolkit.Demo;
    
public class ShopMenuView : MonoBehaviour
{
    [SerializeField]
    VisualTreeAsset m_ListEntryTemplate;

    [SerializeField]
    GameObject m_TargetArtilleryGun;

    [SerializeField]
    GameObject m_Artillery;

    [SerializeField]
    GameObject m_CurrencySystem;

    [SerializeField]
    GameObject m_NPCDialog;

    // UI element references
    Button m_BuyButton;
    ShopMenuController m_ShopMenuController;
    CurrencyTracker m_CurrencyTracker;
    DialogView m_NPCDialogView;

    void Awake() {
        m_CurrencyTracker = m_CurrencySystem.GetComponent<CurrencyTracker>();
        Debug.Log($"Currency: {m_CurrencyTracker.currency}");

        m_NPCDialogView = m_NPCDialog.GetComponent<DialogView>();

        // Initialize the character list controller
        m_ShopMenuController = new ShopMenuController();
        m_ShopMenuController.SetCurrencyTracker(m_CurrencyTracker);
        m_ShopMenuController.EnumerateAllItems();
    }
    
    void OnEnable()
    {
        // The UXML is already instantiated by the UIDocument component
        var uiDocument = GetComponent<UIDocument>();
        m_ShopMenuController.InitializeItemList(uiDocument.rootVisualElement, m_ListEntryTemplate);

        var root = uiDocument.rootVisualElement;
        m_BuyButton = root.Q<Button>("BuyButton");
        m_BuyButton.clicked += OnBuyItemClicked;

        m_NPCDialogView.Talk("Mary", "Welcome to my store, Serenella !");
    }

    void OnDisable() {
        m_BuyButton.clicked -= OnBuyItemClicked;
    }

    void OnBuyItemClicked() 
    {
        ShopMenuItemData selectedItemData = m_ShopMenuController.GetItemSelected();
        if (selectedItemData)
        {
            if (m_CurrencyTracker.Use(selectedItemData.Cost)) {
                m_ShopMenuController.RemoveSelectedItem();

                UpgradeArtillery(selectedItemData);

                // send the cancel event to quit menu
                m_NPCDialogView.Talk("Mary", "Thanks a lot !!");
                InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(Key.Escape));
            }
            else
            {
                m_NPCDialogView.Talk("Mary", "You don't have enough coins, go kill monster !!");
            }
        }
    }

    void UpgradeArtillery(ShopMenuItemData item) {
        UpgradeArtilleryGun(item.GameObject);
        UpgradeArtilleryZoom(item.ZoomGain);
        UpgradeArtillerySpeed(item.SpeedGain);
        UpgradeArtilleryShellDamage(item.DamageGain);
    }

    void UpgradeArtilleryGun(GameObject prefab) {
        var oldGunTransform = m_TargetArtilleryGun.transform;
        var newGun = Instantiate(prefab, oldGunTransform.position, oldGunTransform.rotation, oldGunTransform.parent);
        foreach (Transform child in oldGunTransform)
        {
            child.SetParent(newGun.transform);
        }
        Destroy(m_TargetArtilleryGun);
        m_TargetArtilleryGun = newGun;
    }

    void UpgradeArtilleryZoom(float zoomGain) {
        CinemachineZoomController cineZoomCtl = m_Artillery.GetComponentInChildren<CinemachineZoomController>();
        cineZoomCtl.IncreaseZoom(zoomGain);
    }

    void UpgradeArtillerySpeed(float speedGain) {
        ArtilleryManager artilleryManager = m_Artillery.GetComponent<ArtilleryManager>();
        artilleryManager.IncreaseLaunchSpeed(speedGain);
        artilleryManager.IncreaseRotationSpeed(speedGain);
    }

    void UpgradeArtilleryShellDamage(float damageGain) {
        ArtilleryManager artilleryManager = m_Artillery.GetComponent<ArtilleryManager>();
        artilleryManager.IncreaseDamage(damageGain);
    }
}
