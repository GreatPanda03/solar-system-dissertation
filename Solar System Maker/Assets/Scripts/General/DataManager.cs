
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

public static class DataManager
{
    public static List<Simulation> simulationSaves { get; private set; }
    public static List<Simulation> templateSaves { get; private set; }


    #region Loading & Saving Data

    // retrieves all data stored in json files
    public static void LoadAllData()
    {
        simulationSaves = new List<Simulation>();
        templateSaves = new List<Simulation>();

        /*

        // Read
        StreamReader reader = new StreamReader("MyPath.txt");
        string lineA = reader.ReadLine();
        string[] splitA = lineA.Split(',');
        scoreA = int.Parse(splitA[1]);

        string lineB = reader.ReadLine();
        string[] splitB = lineB.Split(',');
        scoreB = int.Parse(splitB[1]);

        */
    }

    // writes all the current data to their respective json files
    public static void SaveAllData()
    {
        JsonSerializerSettings jsonSettings = new()
        { 
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented
        };

        string simulationsData = JsonConvert.SerializeObject(
            simulationSaves, jsonSettings); 
        
        Debug.Log("Simulation Data To Save: " + simulationsData);

        string filePath = Application.persistentDataPath + "SimulationSaves.txt";

        File.WriteAllText(filePath, simulationsData);
    }

    #endregion Loading & Saving Data


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
}