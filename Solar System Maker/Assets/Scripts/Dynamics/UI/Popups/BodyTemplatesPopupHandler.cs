// creates instances of the "Saved Simulations" interface
// * the interface displays the user's saved simulations which can be opened and run


using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class BodyTemplatesPopupHandler
{

    #region Attributes

    public VisualElement popup;
    public UIController uiController;
    public VisualTreeAsset templateEntry;
    public VisualElement parent;

    public VisualElement selectedEntry;
    public BodyTemplate selectedTemplate;
    public event Action<Body> OnTemplateSelected;

    #endregion Attributes


    #region Constructor

    // prepares the given UI as a "Saved Simulations" interface
    public BodyTemplatesPopupHandler(
        VisualElement popup,
        UIController uiController, 
        VisualTreeAsset templateEntry,
        VisualElement parent)
    {
        this.popup = popup;
        this.uiController = uiController;
        this.templateEntry = templateEntry;
        this.parent = parent;

        if (parent.Q<VisualElement>("BodyTemplates") != null) return;

        popup.name = "BodyTemplates";
        parent.Add(popup);

        ScrollView list = popup.Q<ScrollView>("List");
        list.contentContainer.style.flexDirection = FlexDirection.Row;
        list.contentContainer.style.flexWrap = Wrap.Wrap;

        popup.Q<Button>("Select").clicked += OnSelectClicked;
        popup.Q<Button>("Delete").clicked += OnDeleteClicked;
        popup.Q<Button>("Close").clicked += OnCloseClicked;

        LoadEntries();
    }

    #endregion Constructor


    #region UI Methods

    // closes the popup and communicates the selected body
    public void OnSelectClicked()
    {
        if (selectedTemplate != null)
        {
            popup.RemoveFromHierarchy();
            OnTemplateSelected?.Invoke(selectedTemplate.Body);
        }
    }

    // deletes the selected simulation on click
    private void OnDeleteClicked()
    {
        DeleteEntry(selectedTemplate);
    }

    // returns to the "Main Menu" interface on click
    private void OnCloseClicked()
    {
        popup.RemoveFromHierarchy();
    }

    #endregion UI Methods


    #region Entry Management

    // load all saves of the user as entries
    public void LoadEntries()
    {
        List<BodyTemplate> templates = DataHandler.BodyTemplates;

        foreach (BodyTemplate template in templates)
        {
            VisualElement entry = CreateEntry(template);
            EnableEntry(entry, template);
            popup.Q<ScrollView>("List").Add(entry);
        }
    }

    // creates an entry for the given save
    public VisualElement CreateEntry(BodyTemplate template)
    {
        VisualElement entry = templateEntry.Instantiate();
        entry.Q<Label>("Title").text = template.Name;

        return entry;
    }

    // allows the save associated with the given entry to be interacted with
    public void EnableEntry(VisualElement entry, BodyTemplate template)
    {
        Button button = entry.Q<Button>(); // the entry button itself

        button.clicked += () => {
            SelectEntry(entry);
            UpdateViewer(template);
        };
    }

    // selects the given Save Entry
    public void SelectEntry(VisualElement entry)
    {
        if (selectedEntry == null)
        {
            selectedEntry = entry;
            selectedEntry.AddToClassList("selectedEntry");
        }
        else
        {
            if (selectedEntry == entry)
            {
                selectedEntry.RemoveFromClassList("selectedEntry");
                selectedEntry = null;
            }
            else
            {
                selectedEntry.RemoveFromClassList("selectedEntry");
                selectedEntry = entry;
                selectedEntry.AddToClassList("selectedEntry");
            }
        }
    }

    // updates the save viewer with the given save's details
    public void UpdateViewer(BodyTemplate template)
    {
        // updating the save viewer panel accordingly
        VisualElement viewer = popup.Q<VisualElement>("Viewer");
        VisualElement details = popup.Q<VisualElement>("Details");
        Label title = details.Q<Label>("Title");
        Label description = details.Q<Label>("Description");
        Label noSelection = viewer.Q<Label>("NoSelection");

        if (template != null) 
        {
            title.text = template.Name;
            description.text = template.Description;

            noSelection.style.display = DisplayStyle.None;
            details.style.display = DisplayStyle.Flex;

            selectedTemplate = template;
        }
        else
        {
            details.style.display = DisplayStyle.None;
            noSelection.style.display = DisplayStyle.Flex;

            selectedTemplate = null;
        }
    }

    // deletes the given save using its ID
    public void DeleteEntry(BodyTemplate template)
    { 
        DataHandler.DeleteBodyTemplateSave(template.id);

        UpdateViewer(null);
        ClearList();
        LoadEntries();
    }

    // removes all entries from the saves list
    public void ClearList()
    {
        popup.Q<ScrollView>("List").Clear();
    }

    #endregion Entry Management
}


