using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Blobcreate.ProjectileToolkit.Demo;
using UnityEngine.Events;
    
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

    // Unity Events
    [System.Serializable]
    public class ShopMenuEvent : UnityEvent {}

    [Header("Events")]
    public ShopMenuEvent OnShopItemBought;
    public ShopMenuEvent OnShopItemClicked;
    public ShopMenuEvent OnShopItemHovered;

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
        // Initialize events if they're null
        if (OnShopItemBought == null)
            OnShopItemBought = new ShopMenuEvent();

        if (OnShopItemClicked == null)
            OnShopItemClicked = new ShopMenuEvent();

        if (OnShopItemHovered == null)
            OnShopItemHovered = new ShopMenuEvent();

        // The UXML is already instantiated by the UIDocument component
        var uiDocument = GetComponent<UIDocument>();
        m_ShopMenuController.InitializeItems(uiDocument.rootVisualElement, m_CastleHealthItemRecycling, m_CastleDefenseItemRecycling);

        m_GunItem = m_ShopMenuController.GetGunItem();
        m_GunItem.RegisterCallback<ClickEvent>(OnGunItemSelected);
        m_GunItem.RegisterCallback<MouseEnterEvent>(OnItemHovered);

        m_ZoomItem = m_ShopMenuController.GetZoomItem();
        m_ZoomItem.RegisterCallback<ClickEvent>(OnZoomItemSelected);
        m_ZoomItem.RegisterCallback<MouseEnterEvent>(OnItemHovered);

        m_CastleHealthItem = m_ShopMenuController.GetCastleHealthItem();
        m_CastleHealthItem.RegisterCallback<ClickEvent>(OnCastleHealthItemSelected);
        m_CastleHealthItem.RegisterCallback<MouseEnterEvent>(OnItemHovered);

        m_CastleDefenseItem = m_ShopMenuController.GetCastleDefenseItem();
        m_CastleDefenseItem.RegisterCallback<ClickEvent>(OnCastleDefenseItemSelected);
        m_CastleDefenseItem.RegisterCallback<MouseEnterEvent>(OnItemHovered);

        m_BuyButton = m_ShopMenuController.GetBuyButton();
        m_BuyButton.clicked += OnBuyItemClicked;

        m_NPCDialogView.Talk(Character.Mary, $"Welcome back, {Character.Ella}...");
    }

    void OnDisable() {
        m_GunItem.UnregisterCallback<ClickEvent>(OnGunItemSelected);
        m_GunItem.UnregisterCallback<MouseEnterEvent>(OnItemHovered);

        m_ZoomItem.UnregisterCallback<ClickEvent>(OnZoomItemSelected);
        m_ZoomItem.UnregisterCallback<MouseEnterEvent>(OnItemHovered);

        m_CastleHealthItem.UnregisterCallback<ClickEvent>(OnCastleHealthItemSelected);
        m_CastleHealthItem.UnregisterCallback<MouseEnterEvent>(OnItemHovered);

        m_CastleDefenseItem.UnregisterCallback<ClickEvent>(OnCastleDefenseItemSelected);
        m_CastleDefenseItem.UnregisterCallback<MouseEnterEvent>(OnItemHovered);

        m_BuyButton.clicked -= OnBuyItemClicked;
    }

    void OnItemHovered(MouseEnterEvent evt) {
        OnShopItemHovered?.Invoke();
    }

    void CheckBuyButton() 
    {
        int total = m_ShopMenuController.GetTotalCost();
        if (total == 0)
        {
            m_BuyButton.enabledSelf = false;
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
            m_NPCDialogView.Talk(Character.Mary, "You don't have enough coins, better luck next time.");
        }
    }

    void OnGunItemSelected(ClickEvent evt)
    {
        m_ShopMenuController.ToggleGunItemSelection();
        CheckBuyButton();
        OnShopItemClicked?.Invoke();
    }

    void OnZoomItemSelected(ClickEvent evt)
    {
        m_ShopMenuController.ToggleZoomItemSelection();
        CheckBuyButton();
        OnShopItemClicked?.Invoke();
    }

    void OnCastleHealthItemSelected(ClickEvent evt)
    {
        m_ShopMenuController.ToggleCastleHealthItemSelection();
        CheckBuyButton();
        OnShopItemClicked?.Invoke();
    }

    void OnCastleDefenseItemSelected(ClickEvent evt)
    {
        m_ShopMenuController.ToggleCastleDefenseItemSelection();
        CheckBuyButton();
        OnShopItemClicked?.Invoke();
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
                UpgradeArtillerySpeed(selectedItemData.LaunchSpeedGain, selectedItemData.RotationSpeedGain);
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

        OnShopItemBought?.Invoke();

        // message from NPC
        m_NPCDialogView.Talk(Character.Mary, "Thanks a lot !!");
        
        // update the elements
        m_ShopMenuController.FillStore();
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

    void UpgradeArtillerySpeed(float launchSpeedGain, float rotationSpeedGain) {
        ArtilleryManager artilleryManager = m_Artillery.GetComponent<ArtilleryManager>();
        artilleryManager.IncreaseLaunchSpeed(launchSpeedGain);
        artilleryManager.IncreaseRotationSpeed(rotationSpeedGain);
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
