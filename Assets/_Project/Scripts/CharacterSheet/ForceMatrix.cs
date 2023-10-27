using System;
using UnityEngine;

[Serializable]
public struct ForceMatrix
{
    public float physical;
    public float chemical;
    public float energy;
    public float biological;

    //Constructors
    public ForceMatrix(float value)
    {
        physical = value;
        chemical = value;
        energy = value;
        biological = value;
    }
    public ForceMatrix(float physical, float chemical, float energy, float biological)
    {
        this.physical = physical;
        this.chemical = chemical;
        this.energy = energy;
        this.biological = biological;
    }

    //Utility
    public float Sum()
    {
        return
            physical +
            chemical +
            energy +
            biological;
    }
    public ForceMatrix MinMax(float min, float max)
    {
        return new ForceMatrix(
            Mathf.Max(Mathf.Min(physical, max), min),
            Mathf.Max(Mathf.Min(chemical, max), min),
            Mathf.Max(Mathf.Min(energy, max), min),
            Mathf.Max(Mathf.Min(biological, max), min));
    }
    
    //Operators
    public static ForceMatrix operator +(ForceMatrix a, ForceMatrix b)
    {
        return new ForceMatrix(
            a.physical + b.physical,
            a.chemical + b.chemical,
            a.energy + b.energy,
            a.biological + b.biological);
    }
    public static ForceMatrix operator +(ForceMatrix a, float b)
    {
        return new ForceMatrix(
            a.physical + b,
            a.chemical + b,
            a.energy + b,
            a.biological + b);
    }
    public static ForceMatrix operator +(float a, ForceMatrix b)
    {
        return new ForceMatrix(
            a + b.physical,
            a + b.chemical,
            a + b.energy,
            a + b.biological);
    }
    public static ForceMatrix operator -(ForceMatrix a, ForceMatrix b)
    {
        return new ForceMatrix(
            a.physical - b.physical,
            a.chemical - b.chemical,
            a.energy - b.energy,
            a.biological - b.biological);
    }
    public static ForceMatrix operator -(ForceMatrix a, float b)
    {
        return new ForceMatrix(
            a.physical - b,
            a.chemical - b,
            a.energy - b,
            a.biological - b);
    }
    public static ForceMatrix operator -(float a, ForceMatrix b)
    {
        return new ForceMatrix(
            a - b.physical,
            a - b.chemical,
            a - b.energy,
            a - b.biological);
    }
    public static ForceMatrix operator *(ForceMatrix a, ForceMatrix b)
    {
        return new ForceMatrix(
            a.physical * b.physical,
            a.chemical * b.chemical,
            a.energy * b.energy,
            a.biological * b.biological);
    }
    public static ForceMatrix operator *(ForceMatrix a, float b)
    {
        return new ForceMatrix(
            a.physical * b,
            a.chemical * b,
            a.energy * b,
            a.biological * b);
    }

    //Quick Accessors
    public static ForceMatrix zero { get { return new ForceMatrix(0f, 0f, 0f, 0f); } }
    public static ForceMatrix one { get { return new ForceMatrix(1f, 1f, 1f, 1f); } }

    //Overrides
    public override string ToString()
    {
        return ToString('/');
    }
    public string ToString(char seperator = '/')
    {
        return String.Concat(
            physical.ToString(Common.defaultIntFormat), seperator,
            chemical.ToString(Common.defaultIntFormat), seperator,
            energy.ToString(Common.defaultIntFormat), seperator,
            biological.ToString(Common.defaultIntFormat));
    }
}