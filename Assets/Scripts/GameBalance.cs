using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameBalance", menuName = "ScriptableObjects/GameBalance")]
public class GameBalance : ScriptableObject
{
    public AnimationCurve FeedCurve;
    public float FoodPerFeed;
    public int ExpPerFeed;
    public int PointsPerFeed;
    public List<AchievementData> AchievementsData = new List<AchievementData>();
    public List<LevelData> Levels = new List<LevelData>();
    public List<SkinData> SkinsData = new List<SkinData>();
    public List<CodeData> CodesData = new List<CodeData>();

    [Serializable]
    public class AchievementData
    {
        public Sprite Image;
        public string Name;
        public string Description;
    }

    [Serializable]
    public class LevelData
    {
        public string Name;
        public int ExpCount;
    }

    [Serializable]
    public class SkinData
    {
        public Sprite Image;
        public string Name;
        public string Description;
    }

    [Serializable]
    public class CodeData
    {
        public Sprite Image;
        public int Cost;
        public string Name;
    }
}