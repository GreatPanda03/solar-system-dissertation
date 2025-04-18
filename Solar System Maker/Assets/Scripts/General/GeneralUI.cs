using UnityEngine;
using UnityEngine.UIElements;

public static class GeneralUI
{
    public static bool IsPointerOverAnyUI()
    {
        // Get the current mouse position and flip Y for UI Toolkit
        Vector2 mousePosition = Input.mousePosition;
        mousePosition.y = Screen.height - mousePosition.y;

        // Find all active UIDocuments in the scene
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
}