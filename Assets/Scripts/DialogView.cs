using UnityEngine;
using UnityEngine.UIElements;
    
public class DialogView : MonoBehaviour
{
    // UI element references
    Label m_CharacterName;
    Label m_CharacterLine;
    
    void OnEnable()
    {
        // The UXML is already instantiated by the UIDocument component
        var uiDocument = GetComponent<UIDocument>();
        //m_DialogController.InitializeItemList(uiDocument.rootVisualElement, m_ListEntryTemplate);

        var root = uiDocument.rootVisualElement;
        m_CharacterName = root.Q<Label>("CharacterName");
        m_CharacterLine = root.Q<Label>("CharacterLine");

        m_CharacterName.text = "My Character";
        m_CharacterLine.text = "Hello !";
    }

    public void Talk(string character, string text)
    {
        m_CharacterName.text = character;
        m_CharacterLine.text = text;
    }
}
