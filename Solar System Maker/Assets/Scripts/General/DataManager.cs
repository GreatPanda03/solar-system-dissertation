
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System;

public static class DataManager
{
    public static List<Simulation> simulationSaves { get; private set; }
    public static List<Simulation> templateSaves { get; private set; }

    public static string simulationSavesPath = Application.persistentDataPath + "/SimulationSaves.txt";
    public static string templateSavesPath = Application.persistentDataPath + "/TemplateSaves.txt";


    #region Loading Data

    // retrieves all data stored in json files
    public static void LoadAllData()
    {
        simulationSaves = new List<Simulation>();
        templateSaves = new List<Simulation>();

        string simulationsData;

        // TO DO: add some kind of system to disable saving if
        //   the file can't be read from
        // * "There was a problem loading your data"
        // * "Saving will be disabled for this session to avoid overwriting potentially existing data"
        // * "Please restart the application to re-enable saving"
        try
        {
            simulationsData = File.ReadAllText(simulationSavesPath);
            Debug.Log("[DATA] SUCCESS: Data Loaded");
        }
        catch(FileNotFoundException)
        {
            simulationsData = "[]";
            File.WriteAllText(simulationSavesPath, "[]");
            Debug.Log("[DATA] No existing 'SaveSimulations.txt' file, creating a new one");
        }
        catch (Exception e)
        {
            simulationsData = "[]";
            Debug.Log("[DATA] ERROR: " + e.Message);
        }

        Debug.Log("Simulations Data to Load: "
            + GetPrettyJson(simulationsData));

        simulationSaves = JsonConvert.DeserializeObject<List<Simulation>>(
            simulationsData,
            new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto
            }
        );

        foreach (Simulation save in simulationSaves)
        {
            for (int b = 0; b < save.bodies.Count; b++)
            {
                Body body = save.bodies[b];
                     if (body.Type == "Star")   { body = (Star) body;   }
                else if (body.Type == "Planet") { body = (Planet) body; };
            }
        }
    }

    #endregion Loading Data


    #region Saving Data

    // writes all the current data to their respective json files
    public static void SaveAllData()
    {
        string simulationsData = JsonConvert.SerializeObject(
            simulationSaves, 
            new JsonSerializerSettings()
            { 
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto
            }
        ); 
        
        Debug.Log("Simulations Data To Save: "
            + GetPrettyJson(simulationSaves));
        
        try 
        {
            File.WriteAllText(simulationSavesPath, simulationsData);
            Debug.Log("[DATA] SUCCESS: Data Saved");
        }
        catch (Exception e)
        {
            Debug.Log("[DATA] ERROR: The simulation data could not be saved, " + e.Message);
        }
    }

    #endregion Saving Data


    #region CRUD Methods

    // CREATE

    // returns an empty simulation with the next available ID
    public static Simulation CreateSimulationSave()
    {
        Simulation simulation = new();
        List<int> usedIds = new();

        foreach (Simulation save in simulationSaves)
            usedIds.Add(save.id);

        int nextId = 0;
        do nextId += 1; while (usedIds.Contains(nextId));

        simulation.id = nextId;

        return simulation;
    }

    public static void AddSimulationSave(Simulation simulation)
    {
        simulationSaves.Add(simulation);
    }

    public static void AddTemplateSave(Simulation simulation)
    {
        templateSaves.Add(simulation);
    }

    // READ
    public static Simulation GetSimulationSave(int id)
    {
        foreach (Simulation simulation in simulationSaves)
        {
            if (simulation.id == id) return simulation;
        }
        return null;
    }

    public static Simulation GetTemplateSave(int id)
    {
        foreach (Simulation template in templateSaves)
        {
            if (template.id == id) return template;
        }
        return null;
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
        Simulation target = GetTemplateSave(id);
        
        if (target == null) return false;

        target.settings = newData.settings;
        target.bodies = newData.bodies;

        return true;
    }

    // DELETE
    public static Simulation DeleteSimulationSave(int id)
    {
        Simulation target = GetSimulationSave(id);
        
        if (target == null) return null;
        simulationSaves.Remove(target);

        return target;
    }

    public static Simulation DeleteTemplateSave(int id)
    {
        Simulation target = GetTemplateSave(id);
        
        if (target == null) return null;
        templateSaves.Remove(target);

        return target;
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