using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FunctionalBuilding
{
    public float wattConsumption;
    public int maxEmployees;    // employees required for max output

    public FunctionalBuilding()
    {
        wattConsumption = 0;
        maxEmployees = 0;
    }
}
public class PowerStation : FunctionalBuilding
{
    //public float wattProduction;

    public PowerStation()
        : base()
    {
        //wattProduction = 20000;
        wattConsumption -= 20000;
        maxEmployees = 5;
    }
}
public class ShieldGenerator : FunctionalBuilding
{
    int structureCoverage;

    public ShieldGenerator()
        : base()
    {
        structureCoverage = 5;
        maxEmployees = 2;
    }
}

public class GreenHouse : FunctionalBuilding
{
    public int foodUnitsPerDay;
    public GreenHouse()
    {
        foodUnitsPerDay = 40;
        maxEmployees = 3;
    }
}

public class Residence : FunctionalBuilding
{
    public int capacity;
    public int population;
    List<Person> residents;

    public Residence()
    {
        population = 0;
        capacity = 30;
        residents = new List<Person>();
        maxEmployees = 2;
    }

    public Residence(int cap)
    {
        population = 0;
        capacity = cap;
        residents = new List<Person>();
        maxEmployees = 2;
    }

    public bool IsFull()
    {
        return population >= capacity;
    }

    public bool AddPerson(Person p)
    {
        if (IsFull())
            return false;
        residents.Add(p);
        return true;
    }
}

public class Person
{
    int age;
    int engineeringSkill; // for powerstations, shield generators, and residences upkeep
    int greenHouseSkill; // for green houses
    //int turretSkill;
    //int mineBeamSkill;
    public Person()
    {
        age = 18;
        Random r = new Random();
        engineeringSkill = Random.Range(1, 5);
        greenHouseSkill = Random.Range(1, 5);
    }
}