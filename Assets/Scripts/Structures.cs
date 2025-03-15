using UnityEngine;
using System;
using System.Collections.Generic;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class User
{
    public int Level { get; private set; }
    public int Exp { get; private set; }
    public Pet Pet { get; private set; }
    public List<Achievement> Achievements { get; private set; }
    public List<Rubbish> Rubbish { get; private set; }
    public Action<int, float> OnExpChanged { get; set; }

    public User()
    {
        Level = 1;
        Exp = 0;
        Pet = new Pet();
        Achievements = new List<Achievement>();
        for (int i = 0; i < GameManager.GameBalance.AchievementsData.Count; i++) Achievements.Add(new Achievement());
        Rubbish = new List<Rubbish>();
        for (int i = 0; i < 3; i++) Rubbish.Add(new Rubbish(i, 0));
    }

    public User(int level, int exp, int satiety)
    {
        Level = level;
        Exp = exp;
        Pet = new Pet(satiety);
    }

    public void AddExp(int count)
    {
        Exp += count;
        while (Exp >= GameManager.GetLevelData(Level).ExpCount)
        {
            Exp -= GameManager.GetLevelData(Level).ExpCount;
            Level++;
        }
        OnExpChanged?.Invoke(Level, (float)Exp / GameManager.GetLevelData(Level).ExpCount);
    }
}

public class Pet
{
    public float Satiety { get; private set; }
    public Action<float> OnSatietyChanged { get; set; }

    private const float HungrySpeed = 0.2f;

    public Pet()
    {
        Satiety = 100;
    }

    public Pet(int satiety)
    {
        Satiety = satiety;
    }

    public void UpdateSatiety()
    {
        Satiety = Mathf.Max(Mathf.Min(Satiety - Time.deltaTime * HungrySpeed, 100), 0);
        OnSatietyChanged?.Invoke(Satiety);
    }

    public bool Feed(List<Rubbish> rubbish)
    {
        for (int i = 0; i < rubbish.Count; i++)
        {
            if (rubbish[i].Count >= 0.4f)
            {
                Satiety += 10;
                rubbish[i].Use(0.4f);
                OnSatietyChanged?.Invoke(Satiety);
                return true;
            }
        }
        return false;
    }
}

public class Rubbish
{
    private int _id;
    public float Count { get; private set; }
    public Action<int, float> OnCountChanged { get; set; }

    public const int Max = 10;

    public Rubbish(int id, float count)
    {
        _id = id;
        Count = count;
    }

    public void Add(float count)
    {
        Count = Mathf.Min(Count + count, Max);
        OnCountChanged?.Invoke(_id, Count);
    }

    public void Use(float count)
    {
        Count = Mathf.Max(Count - count, 0);
        OnCountChanged?.Invoke(_id, Count);
    }
}

public class Achievement
{
    public bool Unlocked { get; private set; }

    public Achievement()
    {
        Unlocked = false;
    }

    public Achievement(bool unlocked)
    {
        Unlocked = unlocked;
    }

    public void Unlock()
    {
        Unlocked = true;
    }
}

