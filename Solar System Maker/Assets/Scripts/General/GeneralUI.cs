using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.UIElements;

public static class GeneralUI
{

    // returns true if the mouse is over any UI element of any UI document
    public static bool IsPointerOverAnyUI()
    {
        /*
        Vector2 mousePosition = Input.mousePosition;
        mousePosition.y = Screen.height - mousePosition.y;

        var uiDocuments = Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None);

        foreach (var document in uiDocuments)
        {
            if (!document.enabled || document.rootVisualElement == null)
                continue;

            var root = document.rootVisualElement;
            var picked = root.panel?.Pick(mousePosition);

            if (picked != null)
            {
                Debug.Log(picked);
                return true;
            }
        }
        */

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