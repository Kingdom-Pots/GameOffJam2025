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

    VisualElement m_CastleItem;
    ShopMenuItemController m_CastleItemController;
    List<ShopMenuCastleItemData> m_AllCastlesItems;

    Button m_BuyButton;
    Label m_TotalCostLabel;

    int m_TotalCost = 0;

    public void InitializeItems(VisualElement root)
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
        m_CastleItem = root.Q<VisualElement>("CastleItem");

        m_CastleItemController = new ShopMenuItemController();
        m_CastleItemController.SetVisualElement(m_CastleItem);

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

        m_AllCastlesItems = new List<ShopMenuCastleItemData>();
        m_AllCastlesItems.AddRange(Resources.LoadAll<ShopMenuCastleItemData>("ShopMenuItems"));
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

        if (m_AllCastlesItems.Count > 0) {
            m_CastleItemController.SetMenuItemData(m_AllCastlesItems[0] as ShopMenuItemData);
            m_CastleItem.visible = true;
        }
        else {
            m_CastleItem.visible = false;
        }

        m_BuyButton.enabledSelf = false;
    }

    public VisualElement GetGunItem() { return m_GunItem; }
    public VisualElement GetZoomItem() { return m_ZoomItem; }
    public VisualElement GetCastleItem() { return m_CastleItem; }
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

    public bool IsCastleItemSelected() 
    {
        return m_CastleItemController.IsSelected();
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

    public void ToggleCastleItemSelection() 
    {
        m_CastleItemController.ToggleSelection();
        ShopMenuCastleItemData item = m_AllCastlesItems[0];
        int cost = IsCastleItemSelected() ? item.Cost : -item.Cost;
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

    public ShopMenuCastleItemData GetSelectedCastleItem() 
    {
        return m_AllCastlesItems[0];
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

    public void RemoveSelectedCastleItem() 
    {
        m_AllCastlesItems.RemoveAt(0);
        m_CastleItemController.ToggleSelection();
    }

    public int GetTotalCost()
    {
        return m_TotalCost;
    }
}
