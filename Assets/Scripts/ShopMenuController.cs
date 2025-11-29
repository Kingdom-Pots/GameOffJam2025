using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
    
public class ShopMenuController
{
    // UI element references
    VisualElement m_GunItem;
    ShopMenuItemController m_GunItemController;
    List<ShopMenuGunItemData> m_AllGunsItems;

    VisualElement m_ZoomItem;
    ShopMenuItemController m_ZoomItemController;
    List<ShopMenuZoomItemData> m_AllZoomsItems;

    VisualElement m_CastleHealthItem;
    ShopMenuItemController m_CastleHealthItemController;
    List<ShopMenuCastleHealthItemData> m_AllCastlesHealthItems;
    int m_CastleHealthItemsCount; // temp

    VisualElement m_CastleDefenseItem;
    ShopMenuItemController m_CastleDefenseItemController;
    List<ShopMenuCastleDefenseItemData> m_AllCastlesDefenseItems;
    int m_CastleDefenseItemsCount; // temp

    Button m_BuyButton;
    Label m_TotalCostLabel;

    int m_TotalCost = 0;

    public void InitializeItems(VisualElement root, int nbCastleHealthItems, int nbCastleDefenseItems)
    {
        // bind controllers to visual elements and to callbacks
        // gun
        m_GunItem = root.Q<VisualElement>("GunItem");

        m_GunItemController = new ShopMenuItemController();
        m_GunItemController.SetVisualElement(m_GunItem);

        // zoom
        m_ZoomItem = root.Q<VisualElement>("ZoomItem");

        m_ZoomItemController = new ShopMenuItemController();
        m_ZoomItemController.SetVisualElement(m_ZoomItem);


        // castle
        m_CastleHealthItem = root.Q<VisualElement>("CastleHealthItem");

        m_CastleHealthItemController = new ShopMenuItemController();
        m_CastleHealthItemController.SetVisualElement(m_CastleHealthItem);
        m_CastleHealthItemsCount = nbCastleHealthItems;
        
        m_CastleDefenseItem = root.Q<VisualElement>("CastleDefenseItem");

        m_CastleDefenseItemController = new ShopMenuItemController();
        m_CastleDefenseItemController.SetVisualElement(m_CastleDefenseItem);
        m_CastleDefenseItemsCount = nbCastleDefenseItems;

        // total cost
        m_TotalCostLabel = root.Q<Label>("TotalCost");
        
        // disable button on start
        m_BuyButton = root.Q<Button>("BuyButton");
    
        FillStore();
    }

    public void EnumerateAllItems()
    {
        m_AllGunsItems = new List<ShopMenuGunItemData>();
        m_AllGunsItems.AddRange(Resources.LoadAll<ShopMenuGunItemData>("ShopMenuItems"));

        m_AllZoomsItems = new List<ShopMenuZoomItemData>();
        m_AllZoomsItems.AddRange(Resources.LoadAll<ShopMenuZoomItemData>("ShopMenuItems"));

        m_AllCastlesHealthItems = new List<ShopMenuCastleHealthItemData>();
        m_AllCastlesHealthItems.AddRange(Resources.LoadAll<ShopMenuCastleHealthItemData>("ShopMenuItems"));

        m_AllCastlesDefenseItems = new List<ShopMenuCastleDefenseItemData>();
        m_AllCastlesDefenseItems.AddRange(Resources.LoadAll<ShopMenuCastleDefenseItemData>("ShopMenuItems"));
    }

    public void FillStore()
    {
        m_TotalCost = 0;
        if (m_AllGunsItems.Count > 0) {
            m_GunItemController.SetMenuItemData(m_AllGunsItems[0] as ShopMenuItemData);
            m_GunItem.visible = true;
        }
        else {
            m_GunItem.visible = false;
        }

        if (m_AllZoomsItems.Count > 0) {
            m_ZoomItemController.SetMenuItemData(m_AllZoomsItems[0] as ShopMenuItemData);
            m_ZoomItem.visible = true;
        }
        else {
            m_ZoomItem.visible = false;
        }

        if (m_AllCastlesHealthItems.Count > 0) {
            m_CastleHealthItemController.SetMenuItemData(m_AllCastlesHealthItems[0] as ShopMenuItemData);
            m_CastleHealthItem.visible = true;
        }
        else {
            m_CastleHealthItem.visible = false;
        }

        if (m_AllCastlesDefenseItems.Count > 0) {
            m_CastleDefenseItemController.SetMenuItemData(m_AllCastlesDefenseItems[0] as ShopMenuItemData);
            m_CastleDefenseItem.visible = true;
        }
        else {
            m_CastleDefenseItem.visible = false;
        }

        m_BuyButton.enabledSelf = false;
    }

    public VisualElement GetGunItem() { return m_GunItem; }
    public VisualElement GetZoomItem() { return m_ZoomItem; }
    public VisualElement GetCastleHealthItem() { return m_CastleHealthItem; }
    public VisualElement GetCastleDefenseItem() { return m_CastleDefenseItem; }
    public Button GetBuyButton() { return m_BuyButton; }

    void UpdateCost(int amount) 
    {
        m_TotalCost += amount;
        m_TotalCostLabel.text = m_TotalCost.ToString();
    }

    public bool IsGunItemSelected() 
    {
        return m_GunItemController.IsSelected();
    }

    public bool IsZoomItemSelected() 
    {
        return m_ZoomItemController.IsSelected();
    }

    public bool IsCastleHealthItemSelected() 
    {
        return m_CastleHealthItemController.IsSelected();
    }

    public bool IsCastleDefenseItemSelected() 
    {
        return m_CastleDefenseItemController.IsSelected();
    }

    public void ToggleGunItemSelection() 
    {
        m_GunItemController.ToggleSelection();
        ShopMenuGunItemData item = m_AllGunsItems[0];
        int cost = IsGunItemSelected() ? item.Cost : -item.Cost;
        UpdateCost(cost);
    }

    public void ToggleZoomItemSelection() 
    {
        m_ZoomItemController.ToggleSelection();
        ShopMenuZoomItemData item = m_AllZoomsItems[0];
        int cost = IsZoomItemSelected() ? item.Cost : -item.Cost;
        UpdateCost(cost);
    }

    public void ToggleCastleHealthItemSelection() 
    {
        m_CastleHealthItemController.ToggleSelection();
        ShopMenuCastleHealthItemData item = m_AllCastlesHealthItems[0];
        int cost = IsCastleHealthItemSelected() ? item.Cost : -item.Cost;
        UpdateCost(cost);
    }

    public void ToggleCastleDefenseItemSelection() 
    {
        m_CastleDefenseItemController.ToggleSelection();
        ShopMenuCastleDefenseItemData item = m_AllCastlesDefenseItems[0];
        int cost = IsCastleDefenseItemSelected() ? item.Cost : -item.Cost;
        UpdateCost(cost);
    }

    public ShopMenuGunItemData GetSelectedGunItem() 
    {
        return m_AllGunsItems[0];
    }

    public ShopMenuZoomItemData GetSelectedZoomItem() 
    {
        return m_AllZoomsItems[0];
    }

    public ShopMenuCastleHealthItemData GetSelectedCastleHealthItem() 
    {
        return m_AllCastlesHealthItems[0];
    }

    public ShopMenuCastleDefenseItemData GetSelectedCastleDefenseItem() 
    {
        return m_AllCastlesDefenseItems[0];
    }

    public void RemoveSelectedGunItem() 
    {
        m_AllGunsItems.RemoveAt(0);
        m_GunItemController.ToggleSelection();
    }

    public void RemoveSelectedZoomItem() 
    {
        m_AllZoomsItems.RemoveAt(0);
        m_ZoomItemController.ToggleSelection();
    }

    public void RemoveSelectedCastleHealthItem() 
    {
        if (m_CastleHealthItemsCount > 1) {
            m_CastleHealthItemsCount -= 1;
        }
        else {
            m_AllCastlesHealthItems.RemoveAt(0);
        }
        m_CastleHealthItemController.ToggleSelection();
    }

    public void RemoveSelectedCastleDefenseItem() 
    {
        if (m_CastleDefenseItemsCount > 1) {
            m_CastleDefenseItemsCount -= 1;
        } else {
            m_AllCastlesDefenseItems.RemoveAt(0);
        }
        m_CastleDefenseItemController.ToggleSelection();
    }

    public int GetTotalCost()
    {
        return m_TotalCost;
    }
}
