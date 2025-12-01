using UnityEngine;

/// <summary>
/// Simple helper for displaying dialogue. Set text in Inspector and call Say() to show it.
/// </summary>
public class DialogueHelper : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField]
    DialogView m_DialogView;

    [Header("Default Dialogue")]
    [SerializeField]
    Character m_Character = Character.Mary;

    [SerializeField]
    [TextArea(3, 10)]
    string m_Text = "";

    void Awake()
    {
        if (m_DialogView == null)
        {
            Debug.LogError("DialogueHelper: DialogView not assigned!");
        }
    }

    /// <summary>
    /// Show dialogue using Inspector settings
    /// </summary>
    public void Say()
    {
        if (m_DialogView != null)
        {
            m_DialogView.Talk(m_Character, m_Text);
        }
    }

    /// <summary>
    /// Show dialogue with custom text
    /// </summary>
    public void Say(string text)
    {
        if (m_DialogView != null)
        {
            m_DialogView.Talk(m_Character, text);
        }
    }

    /// <summary>
    /// Show dialogue with custom character and text
    /// </summary>
    public void Say(Character character, string text)
    {
        if (m_DialogView != null)
        {
            m_DialogView.Talk(character, text);
        }
    }

    /// <summary>
    /// Hide the dialogue
    /// </summary>
    public void Hide()
    {
        if (m_DialogView != null)
        {
            m_DialogView.Hide();
        }
    }
}