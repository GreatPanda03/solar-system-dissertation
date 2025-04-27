// handles all data-related processes
// * all data is saved in JSON format - see the file paths below

using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System;
using Unity.VisualScripting;
using Mono.Cecil;
using System.Linq;

public class DataHandler
{

    #region Attributes

    private static List<Simulation> simulationSaves;
    public static List<Simulation> SimulationSaves { 
        get => simulationSaves; 
        private set => simulationSaves = value;
    }

    private static List<Simulation> simulationTemplates;
    public static List<Simulation> SimulationTemplates {
        get => simulationTemplates; 
        private set => simulationTemplates = value;
    }

    private static List<BodyTemplate> bodyTemplates;
    public static List<BodyTemplate> BodyTemplates {
        get => bodyTemplates;
        private set => bodyTemplates = value;
    }

    private static readonly string simulationSavesPath
        = Application.persistentDataPath + "/SimulationSaves.txt";
    public static readonly string simulationTemplatesPath
        = Application.persistentDataPath + "/SimulationTemplates.txt";

    public static readonly string bodyTemplatesPath
        = Application.persistentDataPath + "/BodyTemplates.txt";

    #endregion Attributes


    #region General Loading

    // retrieves all data stored in json files
    public static void LoadAllData()
    {
        Debug.Log("Loading data...");

        LoadSimulationSaves();
        LoadSimulationTemplates();
        LoadBodyTemplates();

        Debug.Log("Data loaded");
    }

    // returns any JSON data from the given file
    private static string ReadJsonFromFilePath(string filePath)
    {
        string data;

        try
        {
            data = File.ReadAllText(filePath);
            Debug.Log($"[DATA] SUCCESS: Data read from {filePath}");
        }
        catch(FileNotFoundException)
        {
            data = "[]";
            File.WriteAllText(filePath, "[]");
            Debug.Log($"[DATA] WARNING: No existing {filePath} file - creating a new one");
        }
        catch (Exception e)
        {
            data = "[]";
            Debug.Log($"[DATA] ERROR: Couldn't load {filePath} data: " + e.Message);
        }

        return data;
    }

    // loads all data from the file at the given file path 
    //   into a list of the given data type
    // * assumes the target data is a list of the target body in JSON format
    public static List<T> LoadJsonListDataFromFilePath<T>(string filePath)
    {
        List<T> list;
        string data = ReadJsonFromFilePath(filePath);

        // Debug.Log("[DATA] Simulations Saves to load: " + GetPrettyJson(data));

        list = JsonConvert.DeserializeObject<List<T>>(
            data,
            new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto
            }
        );

        if (list == null) 
        {
            list = new();
            Debug.Log($"[DATA] WARNING: No saves to load from {filePath} - prepared blank list");
        }

        return list;
    }

    #endregion General Loading


    #region Specific Loading

    // loads all of the user's simulations data
    public static void LoadSimulationSaves()
    {
        simulationSaves = LoadJsonListDataFromFilePath<Simulation>(
            simulationSavesPath);
    }

    // loads all of the user's simulation templates
    public static void LoadSimulationTemplates()
    {
        simulationTemplates = LoadJsonListDataFromFilePath<Simulation>(
            simulationTemplatesPath);
    }

    // loads all of the user's body templates
    public static void LoadBodyTemplates()
    {
        bodyTemplates = LoadJsonListDataFromFilePath<BodyTemplate>(
            bodyTemplatesPath);
    }

    #endregion Specific Loading


    #region Saving Data

    // saves the given data to the given file in JSON format
    // * COMPLETELY overwrites the file
    public static void SaveDataToFileAsJson<T>(string filePath, T dataToSave)
    {
        string data = JsonConvert.SerializeObject(
            dataToSave, 
            new JsonSerializerSettings()
            { 
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto
            }
        ); 
        
        Debug.Log($"[DATA] Data to save to {filePath}: "
            + GetPrettyJson(data));
        
        try 
        {
            File.WriteAllText(filePath, data);
            Debug.Log($"[DATA] SUCCESS: Data saved to {filePath}");
        }
        catch (Exception e)
        {
            Debug.Log($"[DATA] ERROR: Data couldn't be saved to {filePath}: "
                + e.Message);
        }
    }

    // writes all the current data to their respective json files
    public static void SaveAllData()
    {
        SaveDataToFileAsJson(simulationSavesPath, simulationSaves);
        SaveDataToFileAsJson(simulationTemplatesPath, simulationTemplates);
        SaveDataToFileAsJson(bodyTemplatesPath, bodyTemplates);
    }

    #endregion Saving Data


    #region CRUD Methods

    // CREATE

    // returns the next available ID from the list of items
    // * assumes the "item" has an ID attribute
    public static int GetNextId<T>(List<T> items)
    {
        int nextId = 1;
        List<int> usedIds = new();

        foreach (T item in items)
        {
            var idField = item.GetType().GetField("id");
            int id = (int) idField.GetValue(item);
            usedIds.Add(id);
        }

        while (usedIds.Contains(nextId)) nextId += 1;

        return nextId;
    }

    // returns the next available simulation save, save slot
    public static Simulation GetNextSimulationSaveSlot()
    {
        Simulation slot = new() { id = GetNextId(simulationSaves) };
        return slot;
    }

    // returns the next available simulation template save slot
    public static Simulation GetNextSimulationTemplateSlot()
    {
        Simulation slot = new() { id = GetNextId(simulationTemplates) };
        return slot;
    }

    // returns the next available body template save slot
    public static BodyTemplate GetNextBodyTemplateSlot()
    {
        BodyTemplate slot = new() { id = GetNextId(bodyTemplates) };
        return slot;
    }

    // READ

    // returns the item from the given list associated with the given id
    // * assumes all items have an ID attribute
    public static T GetSaveById<T>(List<T> items, int targetId)
    {
        foreach (T item in items)
        {
            var idField = item.GetType().GetField("id");
            int id = (int) idField.GetValue(item);
            if (id == targetId) return item;
        }

        return default;
    }

    public static Simulation GetSimulationSave(int id)
    {
        return GetSaveById(simulationSaves, id);
    }

    public static Simulation GetSimulationTemplateSave(int id)
    {
        return GetSaveById(simulationTemplates, id);
    }

    public static BodyTemplate GetBodyTemplateSave(int id)
    {
        return GetSaveById(bodyTemplates, id);
    }

    // UPDATE
    public static bool UpdateSimulationSave(int id, Simulation newData)
    {
        Simulation target = GetSimulationSave(id);
        
        if (target == null) return false;

        target.settings = newData.settings;
        target.bodies = newData.bodies;

        return true;
    }

    public static bool UpdateSavedTemplate(int id, Simulation newData)
    {
        Simulation target = GetSimulationTemplateSave(id);
        
        if (target == null) return false;

        target.settings = newData.settings;
        target.bodies = newData.bodies;

        return true;
    }

    public static bool UpdateBodyTemplate(int id, Body body)
    {
        BodyTemplate target = GetBodyTemplateSave(id);

        if (target == null) return false;

        target.Body = body;

        return true;
    }

    // DELETE
    public static T DeleteSaveSlot<T>(List<T> items, int id)
    {
        T target = GetSaveById(items, id);
        
        if (target == null) return default;

        items.Remove(target);

        return target;
    }

    public static Simulation DeleteSimulationSave(int id)
    {
        return DeleteSaveSlot(simulationSaves, id);
    }

    public static Simulation DeleteSimulationTemplateSave(int id)
    {
        return DeleteSaveSlot(simulationTemplates, id);
    }

    public static BodyTemplate DeleteBodyTemplateSave(int id)
    {
        return DeleteSaveSlot(bodyTemplates, id);
    }

    #endregion CRUD Methods


    #region Debugging

    public static string GetPrettyJson(object obj)
    {
        return JsonConvert.SerializeObject(
            obj, 
            new JsonSerializerSettings()
            { 
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }
        );
    }

    #endregion Debugging
}