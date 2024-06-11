using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[CreateAssetMenu(fileName = "RichLevelSettings", menuName = "ScriptableObjects/RichLevelSettings", order = 1)]
public class RichLevelSettings : ScriptableObject
{
    public RichnessLevelMoney[] levels;
    public RichLevels maxLevel = RichLevels.Rich;

    public RichLevels GetLevelByMoney(int m)
    {
        if(m <= 0)
        {
            return levels[0].level;
        }

        RichnessLevelMoney[] reverseLevels = new RichnessLevelMoney[levels.Length];
        Array.Copy(levels, reverseLevels, levels.Length);
        Array.Reverse(reverseLevels);
        foreach(var l in reverseLevels)
        {
            if(m >= l.money)
            {
                return l.level;
            }
        }
        return levels[0].level;
    }
    public RichnessLevelMoney GetLevelSettings(RichLevels level)
    {
        foreach(var l in levels)
        {
            if(level == l.level)
            {
                return l;
            }
        }
        return levels[0];
    }

}

public enum RichLevels
{
    Hobo, Poor, Decent, Rich, 
    Unreachable //unreachable is needed for ingame logic
}

[System.Serializable]
public struct RichnessLevelMoney
{
    public RichLevels level;
    public string inGameName;
    public Color nameColor;
    public int money;
}


[System.Serializable]
public struct RichnessLevelMesh
{
    public RichLevels name;
    public GameObject mesh;
}
