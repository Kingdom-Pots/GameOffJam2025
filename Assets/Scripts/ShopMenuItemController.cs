using UnityEngine.UIElements;
using UnityEngine;
    
public class ShopMenuItemController
{
    Label m_NameLabel;
    Label m_DescriptionLabel;
    Label m_CostLabel;
    VisualElement m_Sprite;
    VisualElement m_Background;
    
    // This function retrieves a reference to the 
    // character name label inside the UI element.
    public void SetVisualElement(VisualElement visualElement)
    {
        m_NameLabel = visualElement.Q<Label>("ItemName");
        m_DescriptionLabel = visualElement.Q<Label>("ItemDescription");
        m_CostLabel = visualElement.Q<Label>("ItemCost");
        m_Sprite = visualElement.Q<VisualElement>("ItemSprite");
        m_Background = visualElement.Q<VisualElement>("Background");

        // callback for highlight
        m_Background.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
        m_Background.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
    }
    
    // This function receives the item whose name this list 
    // element is supposed to display. Since the elements list 
    // in a `ListView` are pooled and reused, it's necessary to 
    // have a `Set` function to change which character's data to display.
    public void SetMenuItemData(ShopMenuItemData itemData)
    {
        m_NameLabel.text = itemData.Name;
        m_DescriptionLabel.text = itemData.Description;
        m_CostLabel.text = itemData.Cost;
        m_Sprite.style.backgroundImage = new StyleBackground(itemData.Sprite);
    }

    private void OnPointerEnter(PointerEnterEvent evt)
    {
        m_Background.AddToClassList("highlight");
        m_Background.RemoveFromClassList("normal");
    }

    private void OnPointerLeave(PointerLeaveEvent evt)
    {
        m_Background.AddToClassList("normal");
        m_Background.RemoveFromClassList("highlight");
    }
}
