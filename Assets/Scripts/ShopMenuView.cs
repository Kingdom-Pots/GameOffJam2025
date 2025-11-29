using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Blobcreate.ProjectileToolkit.Demo;
    
public class ShopMenuView : MonoBehaviour
{
    [SerializeField]
    GameObject m_TargetArtilleryGun;

    [SerializeField]
    GameObject m_Artillery;

    [SerializeField]
    GameObject m_CurrencySystem;

    [SerializeField]
    GameObject m_NPCDialog;

    [SerializeField]
    GameObject m_Castle;

    /// number of times the castle health item can be used
    [SerializeField]
    int m_CastleHealthItemRecycling;

    /// number of times the castle defense item can be used
    [SerializeField]
    int m_CastleDefenseItemRecycling;

    // UI element references
    VisualElement m_GunItem;
    VisualElement m_ZoomItem;
    VisualElement m_CastleHealthItem;
    VisualElement m_CastleDefenseItem;
    Button m_BuyButton;

    ShopMenuController m_ShopMenuController;
    CurrencyTracker m_CurrencyTracker;
    Damageable m_CastleDamageable;

    DialogView m_NPCDialogView;

    void Awake() {
        // get currency tracker
        m_CurrencyTracker = m_CurrencySystem.GetComponent<CurrencyTracker>();
        Debug.Log($"Currency: {m_CurrencyTracker.currency}");

        // get NPC dialog view
        m_NPCDialogView = m_NPCDialog.GetComponent<DialogView>();

        // get castle damageable script
        m_CastleDamageable = m_Castle.GetComponent<Damageable>();

        // Initialize the shop controller
        m_ShopMenuController = new ShopMenuController();
        m_ShopMenuController.EnumerateAllItems();
    }
    
    void OnEnable()
    {
        // The UXML is already instantiated by the UIDocument component
        var uiDocument = GetComponent<UIDocument>();
        m_ShopMenuController.InitializeItems(uiDocument.rootVisualElement, m_CastleHealthItemRecycling, m_CastleDefenseItemRecycling);

        m_GunItem = m_ShopMenuController.GetGunItem();
        m_GunItem.RegisterCallback<ClickEvent>(OnGunItemSelected);

        m_ZoomItem = m_ShopMenuController.GetZoomItem();
        m_ZoomItem.RegisterCallback<ClickEvent>(OnZoomItemSelected);

        m_CastleHealthItem = m_ShopMenuController.GetCastleHealthItem();
        m_CastleHealthItem.RegisterCallback<ClickEvent>(OnCastleHealthItemSelected);

        m_CastleDefenseItem = m_ShopMenuController.GetCastleDefenseItem();
        m_CastleDefenseItem.RegisterCallback<ClickEvent>(OnCastleDefenseItemSelected);

        m_BuyButton = m_ShopMenuController.GetBuyButton();
        m_BuyButton.clicked += OnBuyItemClicked;

        m_NPCDialogView.Talk(Character.Mary, $"Welcome to my store, {Character.Serenella} !");
    }

    void OnDisable() {
        m_GunItem.UnregisterCallback<ClickEvent>(OnGunItemSelected);
        m_ZoomItem.UnregisterCallback<ClickEvent>(OnZoomItemSelected);
        m_CastleHealthItem.UnregisterCallback<ClickEvent>(OnCastleHealthItemSelected);
        m_CastleDefenseItem.UnregisterCallback<ClickEvent>(OnCastleDefenseItemSelected);
        m_BuyButton.clicked -= OnBuyItemClicked;
    }

    void CheckBuyButton() 
    {
        int total = m_ShopMenuController.GetTotalCost();
        if (total == 0)
        {
            return;
        }

        if (m_CurrencyTracker.EnoughCurrency(total)) 
        {
            m_BuyButton.enabledSelf = true;
            m_NPCDialogView.Talk(Character.Mary, "Do you need anything else ?");
        }
        else 
        {
            m_BuyButton.enabledSelf = false;
            m_NPCDialogView.Talk(Character.Mary, "You don't have enough coins, go kill monster !!");
        }
    }

    void OnGunItemSelected(ClickEvent evt)
    {
        m_ShopMenuController.ToggleGunItemSelection();
        CheckBuyButton();
    }

    void OnZoomItemSelected(ClickEvent evt)
    {
        m_ShopMenuController.ToggleZoomItemSelection();
        CheckBuyButton();
    }

    void OnCastleHealthItemSelected(ClickEvent evt)
    {
        m_ShopMenuController.ToggleCastleHealthItemSelection();
        CheckBuyButton();
    }

    void OnCastleDefenseItemSelected(ClickEvent evt)
    {
        m_ShopMenuController.ToggleCastleDefenseItemSelection();
        CheckBuyButton();
    }

    void OnBuyItemClicked() 
    {
        // check for gun
        if (m_ShopMenuController.IsGunItemSelected()) {
            ShopMenuGunItemData selectedItemData = m_ShopMenuController.GetSelectedGunItem();
            if (m_CurrencyTracker.Use(selectedItemData.Cost)) {
                m_ShopMenuController.RemoveSelectedGunItem();
                UpgradeArtilleryGun(selectedItemData.GameObject);
                UpgradeArtilleryShellDamage(selectedItemData.DamageGain);
                UpgradeArtillerySpeed(selectedItemData.SpeedGain);
            }
        }

        // check for zoom
        if (m_ShopMenuController.IsZoomItemSelected()) {
            ShopMenuZoomItemData selectedItemData = m_ShopMenuController.GetSelectedZoomItem();
            if (m_CurrencyTracker.Use(selectedItemData.Cost)) {
                m_ShopMenuController.RemoveSelectedZoomItem();
                UpgradeArtilleryZoom(selectedItemData.ZoomGain);
            }
        }

        // check for castle health
        if (m_ShopMenuController.IsCastleHealthItemSelected()) {
            ShopMenuCastleHealthItemData selectedItemData = m_ShopMenuController.GetSelectedCastleHealthItem();
            if (m_CurrencyTracker.Use(selectedItemData.Cost)) {
                m_ShopMenuController.RemoveSelectedCastleHealthItem();
                HealCastle(selectedItemData.HealthGain);
            }
        }

        // check for castle
        if (m_ShopMenuController.IsCastleDefenseItemSelected()) {
            ShopMenuCastleDefenseItemData selectedItemData = m_ShopMenuController.GetSelectedCastleDefenseItem();
            if (m_CurrencyTracker.Use(selectedItemData.Cost)) {
                m_ShopMenuController.RemoveSelectedCastleDefenseItem();
                UpgradeCastleDefense(selectedItemData.DefenseGain);
            }
        }

        // message from NPC
        m_NPCDialogView.Talk(Character.Mary, "Thanks a lot !!");
        
        // update the elements
        m_ShopMenuController.FillStore();
        
        // send the cancel event to quit menu
        //InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(Key.Escape));
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

    void UpgradeCastleDefense(float defenseGain) {
        m_CastleDamageable.IncreaseMaxHealth(defenseGain);
    }

    void HealCastle(float healthGain) {
        m_CastleDamageable.Heal(healthGain);
    }
}
