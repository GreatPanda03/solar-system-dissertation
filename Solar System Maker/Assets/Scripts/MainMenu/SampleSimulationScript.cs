// Handles the orbit demo for the main menu

using System;
using System.Collections.Generic;
using UnityEngine;

public class SampleSimulationScript : MonoBehaviour
{   
    public GameObject StarTemplate;
    public GameObject PlanetTemplate;
    private List<GameObject> planets;
    public Vector3 systemCentre;
    private bool sampleRunning;

    // preparing variables
    private void Awake() {
        planets = new List<GameObject>();
        sampleRunning = false;
    }

    // creates a sample simulation when the scene is first loaded
    private void Start()
    {
        LoadSampleSimulation();
    }

    // prepares a sample simulation
    public void LoadSampleSimulation()
    {
        if (sampleRunning) return; // only one system at a time
        CreateSampleSimulation();
        RunSampleSimulation();
    }

    // preparing the initial demo with a single star and some number of planets
    private void CreateSampleSimulation()
    {
        var container = GameObject.Find("SampleSimulation");

        GameObject star = Instantiate(StarTemplate); // star at the centre
        star.transform.position = systemCentre;
        star.transform.name = "Star";

        Light starLight = star.AddComponent<Light>();
        starLight.transform.position = star.transform.position;
        starLight.color = Color.white;
        starLight.intensity = 25;
        starLight.range = 100;

        star.transform.parent = container.transform;

        // 1 to "planetCount" planets
        // * all at different starting positions in fixed orbits, with different speeds and colours
        int planetCount = UnityEngine.Random.Range(2,6);
        planets = new List<GameObject>();
        
        for (int p = 0; p < planetCount; p++) 
        {
            double angle = UnityEngine.Random.Range(0f,360f) * Math.PI / 180;
            float orbitDistance = star.transform.localScale.x + (p + 1) * 3f;
            
            Vector3 offset = new Vector3(
                orbitDistance * (float) Math.Cos(angle), 
                0, 
                orbitDistance * (float) Math.Sin(angle));
            
            Vector3 finalPosition = star.transform.position + offset;

            float sizeScale = UnityEngine.Random.Range(0.5f,1f);
            Vector3 finalSize = new Vector3(1,1,1) * sizeScale;
            
            Color colour = Color.HSVToRGB(
                UnityEngine.Random.Range(0f,1f), 0.25f, 1);

            GameObject planet = Instantiate(PlanetTemplate);
            planet.transform.localScale = finalSize;
            planet.transform.position = finalPosition;
            planet.GetComponent<MeshRenderer>().material.color = colour;
            planet.transform.name = "Planet " + (p + 1).ToString();
            planet.transform.parent = container.transform;

            planets.Add(planet);
        }

        Debug.Log("[SAMPLE SIM] SUCCESS: Sample System Loaded");
    }

    // runs / stops the sample simulation
    public void RunSampleSimulation()
    {
        sampleRunning = true;
    }

    public void StopSampleSimulation()
    {
        sampleRunning = false;
    }

    // clears the existing sample simulation (if any)
    // clearing the sample simulation to make way for the new sim
    public void ClearSampleSimulation()
    {
        StopSampleSimulation();

        GameObject container = GameObject.Find("SampleSimulation");

        for( int c = container.transform.childCount - 1; c >= 0; c--)
        {
            Destroy(container.transform.GetChild(c).gameObject);
        }
    }

    // making the planets orbit around the star indefinitely
    private void FixedUpdate()
    {
        if (!sampleRunning) return;

        foreach (GameObject planet in planets)
        {
            float distance = (planet.transform.position - systemCentre).magnitude;
            
            planet.transform.RotateAround(
                systemCentre,
                Vector3.up,
                (float) (25 / Math.Sqrt(distance)) * Time.deltaTime);
        }
    }
}