using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum Character {
    Mary,
    Daigo,
    Ella,
    Karen,
}

public class DialogView : MonoBehaviour
{
    // UI element references
    VisualElement m_Root;
    Label m_CharacterName;
    Label m_CharacterLine;
    Button m_CloseButton;
    
    void OnEnable()
    {
        // The UXML is already instantiated by the UIDocument component
        var uiDocument = GetComponent<UIDocument>();

        m_Root = uiDocument.rootVisualElement;

        m_CharacterName = m_Root.Q<Label>("CharacterName");
        m_CharacterLine = m_Root.Q<Label>("CharacterLine");

        m_CloseButton = m_Root.Q<Button>("CloseButton");
        m_CloseButton.clicked += OnCloseButtonClicked;

        m_CharacterName.text = "";
        m_CharacterLine.text = "";
    }

    void OnDisable()
    {
        m_CloseButton.clicked -= OnCloseButtonClicked;
    }

    void OnCloseButtonClicked() {
        Hide();
    }

    public void Hide() {
        m_Root.visible = false;
    }

    public void Talk(Character character, string text)
    {
        m_Root.visible = true;
        // StartCoroutine(FactionService.GetFactions(OnFactionsLoaded));
        // StartCoroutine(FactionService.AddToFactionTotal("United States of South America", 5, OnFactionUpdated));
        m_CharacterName.text = character.ToString();
        m_CharacterLine.text = text;
    }

    // void OnFactionsLoaded(List<Faction> factions)
    // {
    //     foreach (var faction in factions)
    //     {
    //         Debug.Log($"Faction: {faction.factionname}, Total: {faction.total}");
    //     }
    // }   
    // void OnFactionUpdated(bool success)
    // {
    //     if (success)
    //         Debug.Log("Faction total updated successfully!");
    //     else
    //         Debug.Log("Faction update failed.");
    // } 
}
