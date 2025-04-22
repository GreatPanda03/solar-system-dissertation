using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public static class GeneralUI
{

    // returns true if the mouse is over any UI element of any UI document
    public static bool IsMouseOverUI()
    {
        return EventSystem.current.currentSelectedGameObject != null;
    }

    // returns true if the user currently has a text box selected
    public static bool IsTextFieldFocused()
    {
        var uiDocuments = Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None);

        foreach (var doc in uiDocuments)
        {
            var panel = doc.rootVisualElement?.panel;
            var focused = panel?.focusController?.focusedElement;

            if (focused is TextField)
            {
                return true;
            }
        }

        return false;
    }
}