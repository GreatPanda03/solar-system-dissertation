// Holds the settings options for a simulation

using System;
using UnityEngine;

[Serializable]
public class Settings
{

    #region Attributes

    [SerializeField]
    private string name;
    public string Name {
        get => name;
        set => name = value;
    }

    [SerializeField]
    private string description;
    public string Description {
        get => description;
        set => description = value;
    }

    [SerializeField]
    private double gravitationalConstant; // relative to unit G
    public double GravitationalConstant {
        get => gravitationalConstant;
        set => gravitationalConstant = value;
    }

    #endregion Attributes


    #region Constructor

    public Settings()
    {
        name = "New Simulation";
        description = "";
        gravitationalConstant = 1f;
    }

    #endregion Constructor


    #region Methods

    public Settings GetCopy()
    {
        return new()
        {
            Name = name,
            Description = description,
            GravitationalConstant = gravitationalConstant,
        };
    }

    public override string ToString()
    {
        return JsonUtility.ToJson(this, true);
    }

    #endregion Methods
}
