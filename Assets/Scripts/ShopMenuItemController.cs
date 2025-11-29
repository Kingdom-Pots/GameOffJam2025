using UnityEngine.UIElements;
using UnityEngine;
    
public class ShopMenuItemController
{
    Label m_NameLabel;
    Label m_DescriptionLabel;
    Label m_CostLabel;
    VisualElement m_Sprite;
    VisualElement m_Background;

    bool m_Selected = false;

    // This function retrieves a reference to the 
    // root visual element containg all the other elements
    public void SetVisualElement(VisualElement visualElement)
    {
        m_NameLabel = visualElement.Q<Label>("ItemName");
        m_DescriptionLabel = visualElement.Q<Label>("ItemDescription");
        m_CostLabel = visualElement.Q<Label>("ItemCost");
        m_Sprite = visualElement.Q<VisualElement>("ItemSprite");
        m_Background = visualElement.Q<VisualElement>("Background");
    }
    
    // This function receives the item with all infos to display
    public void SetMenuItemData(ShopMenuItemData itemData)
    {
        m_NameLabel.text = itemData.Name;
        m_DescriptionLabel.text = itemData.Description;
        m_CostLabel.text = itemData.Cost.ToString();
        m_Sprite.style.backgroundImage = new StyleBackground(itemData.Sprite);
    }

    public void ToggleSelection()
    {
        m_Selected = !m_Selected;
        if (m_Selected) 
        {
            m_Background.AddToClassList("selected");
        }
        else 
        {
            m_Background.RemoveFromClassList("selected");
        }
    }

    public bool IsSelected() 
    {
        return m_Selected;
    }
}

