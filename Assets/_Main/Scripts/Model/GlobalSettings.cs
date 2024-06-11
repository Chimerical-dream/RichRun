using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GlobalSettings", menuName = "ScriptableObjects/GlobalSettings", order = 1)]
public class GlobalSettings : ScriptableObject
{
    public RichLevelSettings RichLevelSettings;
}
