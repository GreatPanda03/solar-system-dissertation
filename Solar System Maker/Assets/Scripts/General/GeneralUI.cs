using UnityEngine;
using UnityEngine.UIElements;

public static class GeneralUI
{

    // returns true if the mouse is over any UI element of any UI document
    public static bool IsPointerOverAnyUI()
    {
        Vector2 mousePosition = Input.mousePosition;
        mousePosition.y = Screen.height - mousePosition.y;

        var uiDocuments = Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None);

        foreach (var doc in uiDocuments)
        {
            if (!doc.enabled || doc.rootVisualElement == null)
                continue;

            var picked = doc.rootVisualElement.panel?.Pick(mousePosition);
            if (picked != null)
                return true;
        }

        return false;
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