using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameBalance", menuName = "ScriptableObjects/GameBalance")]
public class GameBalance : ScriptableObject
{
    public List<AchievementData> AchievementsData = new List<AchievementData>();
    public List<LevelData> Levels = new List<LevelData>();
    public List<string> Resources = new List<string>();

    [Serializable]
    public class AchievementData
    {
        public string Name;
        public string Description;
    }

    [Serializable]
    public class LevelData
    {
        public string Name;
        public int ExpCount;
    }
}