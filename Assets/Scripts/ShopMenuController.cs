using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
    
public class ShopMenuController
{
    // UXML template for list entries
    VisualElement m_root;
    VisualTreeAsset m_ListEntryTemplate;
    Button m_BuyButton;
    
    // UI element references
    ListView m_MenuGunsList;
    List<ShopMenuGunItemData> m_AllGunsItems;
    ShopMenuGunItemData m_CurrentGunSelection;

    ListView m_MenuZoomsList;
    List<ShopMenuZoomItemData> m_AllZoomsItems;
    ShopMenuZoomItemData m_CurrentZoomSelection;

    ListView m_MenuCastlesList;
    List<ShopMenuCastleItemData> m_AllCastlesItems;
    ShopMenuCastleItemData m_CurrentCastleSelection;
    
    CurrencyTracker m_CurrencyTracker;

    public void SetCurrencyTracker(CurrencyTracker currencyTracker) => m_CurrencyTracker = currencyTracker;
    
    public void InitializeItemList(VisualElement root, VisualTreeAsset listElementTemplate)
    {
        // Store a reference to the template for the list entries
        m_ListEntryTemplate = listElementTemplate;
    
        // Store a reference to the character list element
        m_root = root.Q<VisualElement>("ShopMenu");

        m_MenuGunsList = root.Q<ListView>("GunsToBuy");
        m_MenuZoomsList = root.Q<ListView>("ZoomsToBuy");
        m_MenuCastlesList = root.Q<ListView>("CastlesToBuy");
        
        m_BuyButton = root.Q<Button>("BuyButton");
        m_BuyButton.enabledSelf = false;
    
        FillItemsLists();
        ReduceItemsLists();
    
        // Register to get a callback when an item is selected
        m_MenuGunsList.selectionChanged += OnGunItemSelected;
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

    void FillItemsLists() {
        FillGunsItemsList();
        FillZoomsItemsList();
        FillCastlesItemsList();
    }
    
    void FillGunsItemsList()
    {
        // Set up a make item function for a list entry
        m_MenuGunsList.makeItem = () =>
        {
            // Instantiate the UXML template for the entry
            var newListEntry = m_ListEntryTemplate.Instantiate();
    
            // Instantiate a controller for the data
            var newListEntryLogic = new ShopMenuItemController();
    
            // Assign the controller script to the visual element
            newListEntry.userData = newListEntryLogic;
    
            // Initialize the controller script
            newListEntryLogic.SetVisualElement(newListEntry);
    
            // Return the root of the instantiated visual tree
            return newListEntry;
        };
    
        // Set up bind function for a specific list entry
        m_MenuGunsList.bindItem = (item, index) =>
        {
            if (m_AllGunsItems.Count > 0) 
            {
                (item.userData as ShopMenuItemController)?.SetMenuGunItemData(m_AllGunsItems[index]);
            }
            else {
                m_MenuGunsList.Clear();
            }
        };
    }

    void FillZoomsItemsList()
    {
        // Set up a make item function for a list entry
        m_MenuZoomsList.makeItem = () =>
        {
            // Instantiate the UXML template for the entry
            var newListEntry = m_ListEntryTemplate.Instantiate();
    
            // Instantiate a controller for the data
            var newListEntryLogic = new ShopMenuItemController();
    
            // Assign the controller script to the visual element
            newListEntry.userData = newListEntryLogic;
    
            // Initialize the controller script
            newListEntryLogic.SetVisualElement(newListEntry);
    
            // Return the root of the instantiated visual tree
            return newListEntry;
        };
    
        // Set up bind function for a specific list entry
        m_MenuZoomsList.bindItem = (item, index) =>
        {
            if (m_AllZoomsItems.Count > 0) 
            {
                (item.userData as ShopMenuItemController)?.SetMenuZoomItemData(m_AllZoomsItems[index]);
            }
            else {
                m_MenuZoomsList.Clear();
            }
        };
    }

    void FillCastlesItemsList()
    {
        // Set up a make item function for a list entry
        m_MenuCastlesList.makeItem = () =>
        {
            // Instantiate the UXML template for the entry
            var newListEntry = m_ListEntryTemplate.Instantiate();
    
            // Instantiate a controller for the data
            var newListEntryLogic = new ShopMenuItemController();
    
            // Assign the controller script to the visual element
            newListEntry.userData = newListEntryLogic;
    
            // Initialize the controller script
            newListEntryLogic.SetVisualElement(newListEntry);
    
            // Return the root of the instantiated visual tree
            return newListEntry;
        };
    
        // Set up bind function for a specific list entry
        m_MenuCastlesList.bindItem = (item, index) =>
        {
            if (m_AllCastlesItems.Count > 0) 
            {
                (item.userData as ShopMenuItemController)?.SetMenuCastleItemData(m_AllCastlesItems[index]);
            }
            else {
                m_MenuCastlesList.Clear();
            }
        };
    }

    void ReduceItemsLists() {
        // Set the actual item's source list/array
        var subGunListSource = new List<ShopMenuGunItemData>();
        if (m_AllGunsItems.Count > 0)
        {
            subGunListSource = m_AllGunsItems.GetRange(0, 1);
        }
        m_MenuGunsList.itemsSource = subGunListSource;

        // Set the actual item's source list/array
        var subZoomListSource = new List<ShopMenuZoomItemData>();
        if (m_AllZoomsItems.Count > 0)
        {
            subZoomListSource = m_AllZoomsItems.GetRange(0, 1);
        }
        m_MenuZoomsList.itemsSource = subZoomListSource;

        // Set the actual item's source list/array
        var subCastleListSource = new List<ShopMenuCastleItemData>();
        if (m_AllCastlesItems.Count > 0)
        {
            subCastleListSource = m_AllCastlesItems.GetRange(0, 1);
        }
        m_MenuCastlesList.itemsSource = subCastleListSource;
    }

    public void RemoveSelectedGunItem() {
        var selectedIndex = m_MenuGunsList.selectedIndex;
        if (selectedIndex >= 0) {
            m_AllGunsItems.RemoveAt(0/*selectedIndex*/);

            m_MenuGunsList.selectedIndex = -1;
            m_MenuGunsList.RefreshItems();
            m_CurrentGunSelection = null;
        }
    }

    public ShopMenuGunItemData GetGunItemSelected()
    {
        return m_CurrentGunSelection;
    }
    
    void OnGunItemSelected(IEnumerable<object> selectedItems)
    {
        // Get the currently selected item directly from the ListView
        m_CurrentGunSelection = m_MenuGunsList.selectedItem as ShopMenuGunItemData;
        if (m_CurrentGunSelection && m_CurrencyTracker.EnoughCurrency(m_CurrentGunSelection.Cost)) 
        {
            Debug.Log($"Selected: {string.Join(", ", m_CurrentGunSelection)}");
            m_BuyButton.enabledSelf = true;
        }
    }
}
