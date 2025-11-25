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
    ListView m_MenuItemList;
    List<ShopMenuItemData> m_AllItems;
    
    public void InitializeItemList(VisualElement root, VisualTreeAsset listElementTemplate)
    {
        EnumerateAllItems();
    
        // Store a reference to the template for the list entries
        m_ListEntryTemplate = listElementTemplate;
    
        // Store a reference to the character list element
        m_root = root.Q<VisualElement>("ShopMenu");
        m_MenuItemList = root.Q<ListView>("ItemsToBuy");
        m_BuyButton = root.Q<Button>("BuyButton");

        m_BuyButton.enabledSelf = false;
    
        FillItemList();
    
        // Register to get a callback when an item is selected
        m_MenuItemList.selectionChanged += OnItemSelected;
        m_BuyButton.clicked += OnClick;
    }
    
    void EnumerateAllItems()
    {
        m_AllItems = new List<ShopMenuItemData>();
        m_AllItems.AddRange(Resources.LoadAll<ShopMenuItemData>("ShopMenuItems"));
    }
    
    void FillItemList()
    {
        // Set up a make item function for a list entry
        m_MenuItemList.makeItem = () =>
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
        m_MenuItemList.bindItem = (item, index) =>
        {
            (item.userData as ShopMenuItemController)?.SetMenuItemData(m_AllItems[index]);
        };
    
        // Set the actual item's source list/array
        m_MenuItemList.itemsSource = m_AllItems;
    }
    
    void OnItemSelected(IEnumerable<object> selectedItems)
    {
        // Get the currently selected item directly from the ListView
        var selectedItemData = m_MenuItemList.selectedItem as ShopMenuItemData;
        m_BuyButton.enabledSelf = true;
        Debug.Log($"Selected: {string.Join(", ", selectedItemData)}");
    }

    void OnClick() 
    {
        Debug.Log("click");
        var selectedIndex = m_MenuItemList.selectedIndex;
        Debug.Log($"remove index {selectedIndex}");
        if (selectedIndex >= 0) {
            m_AllItems.RemoveAt(selectedIndex);

            m_MenuItemList.selectedIndex = -1;
            m_MenuItemList.RefreshItems();
        }
    }
}
