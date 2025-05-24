using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine.Analytics;

[FirestoreData]
public class User
{
    public readonly List<Achievement> Achievements = new List<Achievement>()
    {
        new ResourceScanAchievement(ResourceType.Plastic, 1f),
        new ResourceScanAchievement(ResourceType.Paper, 3f, new SkinReward(Skin.Shredder)),
        new ResourceUseAchievement(ResourceType.Glass, 2f, new SkinReward(Skin.Oven)),
        new ResourceUseAchievement(ResourceType.Paper, 5f, new SkinReward(Skin.Container))
    };

    public Action<int, int> OnExpChanged { get; set; }
    public Action<int> OnPointsChanged { get; set; }
    public Action<Skin, Skin> OnSkinChanged { get; set; }
    public Action<Skin> OnSkinUnlocked { get; set; }

    [FirestoreProperty] public SerializableDateTime LastSaveDate { get; set; } = new SerializableDateTime();
    [FirestoreProperty] public List<SerializableAchievement> SerializedAchievements { get; set; } = new List<SerializableAchievement>();
    [FirestoreProperty] public int _level { get; set; }
    [FirestoreProperty] public int _exp { get; set; }
    [FirestoreProperty] public List<Resource> _resources { get; set; }
    public List<Achievement> _achievements { get; set; }
    [FirestoreProperty] public Pet _pet { get; set; }
    [FirestoreProperty] public List<bool> _unlockedSkins { get; set; }
    [FirestoreProperty] public int _points { get; set; }

    public int Points => _points;
    public int Level => _level;
    public int Exp => _exp;
    public Pet Pet => _pet;

    public static GameBalance.LevelData GetLevelData(int level)
    {
        if (level < GameManager.GameBalance.Levels.Count - 1)
        {
            return GameManager.GameBalance.Levels[level];
        }
        else return GameManager.GameBalance.Levels[GameManager.GameBalance.Levels.Count - 1];
    }

    public User()
    {
        _level = 1;
        _exp = 0;
        _pet = new Pet();
    }

    public void Load()
    {
        _achievements = new List<Achievement>();
        for (int i = 0; i < SerializedAchievements.Count; i++)
        {
            _achievements.Add(SerializedAchievements[i].DeserializedAchievement);
            _achievements[i]._reward = Achievements[i]._reward;
        }
    }

    public void Save()
    {
        SerializedAchievements = new List<SerializableAchievement>();
        for (int i = 0; i < _achievements.Count; i++) SerializedAchievements.Add(new SerializableAchievement(_achievements[i]));
    }

    public void AddPoints(int count)
    {
        _points += count;
        OnPointsChanged?.Invoke(_points);
    }

    public bool WastePoints(int count)
    {
        if (_points - count >= 0)
        {
            _points -= count;
            OnPointsChanged?.Invoke(_points);
            return true;
        }
        return false;
    }

    public void AddExp(int count)
    {
        _exp += count;
        while (Exp >= GetLevelData(Level).ExpCount)
        {
            _exp -= GetLevelData(Level).ExpCount;
            _level++;
            GameManager.AudioManager.PlaySound(Sound.LevelUp);
        }
        OnExpChanged?.Invoke(Exp, Level);
    }

    public Resource GetResource(ResourceType resource)
    {
        CheckRecources();
        return _resources[(int)resource];
    }

    public Achievement GetAchievement(int id)
    {
        CheckAchievements();
        return _achievements[id];
    }

    public void SetSkin(Skin skin)
    {
        CheckSkins();
        if (_unlockedSkins[(int)skin])
        {
            Skin previousSkin = _pet.CurrentSkin;
            _pet.SetSkin(skin);
            OnSkinChanged?.Invoke(previousSkin, _pet.CurrentSkin);
        }
    }

    public bool IsSkinUnlocked(Skin skin)
    {
        CheckSkins();
        return _unlockedSkins[(int)skin];
    }

    public void UnlockSkin(Skin skin)
    {
        CheckSkins();
        _unlockedSkins[(int)skin] = true;
        OnSkinUnlocked?.Invoke(skin);
    }

    public Skin CurrentSkin
    {
        get
        {
            CheckSkins();
            return _pet.CurrentSkin;
        }
    }

    private void CheckRecources()
    {
        int count = Enum.GetValues(typeof(ResourceType)).Length;
        if (_resources is null)
        {
            _resources = new List<Resource>();
        }
        if (_resources.Count < count)
        {
            for (int i = _resources.Count; i < count; i++) _resources.Add(new Resource());
        }
    }

    private void CheckAchievements()
    {
        if(_achievements is null)
        {
            _achievements = new List<Achievement>();
        }
        if (_achievements.Count < Achievements.Count)
        {
            for (int i = _achievements.Count; i < Achievements.Count; i++)
            {
                _achievements.Add(Achievements[i]);
                _achievements[i]._id = i;
            }
        }
    }

    private void CheckSkins()
    {
        if (_unlockedSkins is null)
        {
            _unlockedSkins = new List<bool>();
            _unlockedSkins.Add(true);
        }
        int count = Enum.GetValues(typeof(Skin)).Length;
        if (_unlockedSkins.Count < count)
        {
            for (int i = _unlockedSkins.Count; i < count; i++) _unlockedSkins.Add(false);
        }
    }
}

[FirestoreData]
public class Pet
{
    //private const float HungrySpeed = 0.01f;
    private const float HungrySpeed = 1f;
    private const int HungryPeriod = 5;

    public Action<float> OnSatietyChanged { get; set; }

    [FirestoreProperty] public float _satiety { get; set; }
    [FirestoreProperty] public Skin _currentSkin { get; set; }

    public float Satiety => _satiety;
    public Skin CurrentSkin => _currentSkin;

    private CancellationTokenSource _updateSatietyCancellationTokenSource;

    public Pet()
    {
        _satiety = 100;
        _updateSatietyCancellationTokenSource = new CancellationTokenSource();
        UpdateSatiety(_updateSatietyCancellationTokenSource.Token);
    }

    public float Feed(Resource resource)
    {
        float used = resource.Use(GameManager.GameBalance.FoodPerFeed);
        _satiety = Mathf.Min(_satiety + used * 100, 100);
        OnSatietyChanged?.Invoke(Satiety);
        return used;
    }

    public void WasteTime(float seconds)
    {
        float hungryCoefficient = 1f;
        if (_currentSkin == Skin.Container)
        {
            hungryCoefficient *= 0.8f;
        }
        _satiety = Mathf.Max(Mathf.Min(Satiety - seconds / HungryPeriod * HungrySpeed * hungryCoefficient, 100), 0);
    }

    public void SetSkin(Skin skin)
    {
        _currentSkin = skin;
    }

    private async void UpdateSatiety(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            _satiety = Mathf.Max(Mathf.Min(Satiety - HungrySpeed, 100), 0);
            OnSatietyChanged?.Invoke(Satiety);
            await Task.Delay(1000 * HungryPeriod);
        }
    }
}

[FirestoreData]
public class Resource
{
    [FirestoreProperty] public float _count { get; set; }

    public Action<float> OnCountChanged { get; set; }
    public Action<float> OnUsed { get; set; }

    public float Count => _count;

    public const int Max = 10;

    public Resource()
    {
        _count = 0;
    }

    public void Add(float count)
    {
        _count = Mathf.Min(Count + count, Max);
        OnCountChanged?.Invoke(Count);
    }

    public float Use(float count)
    {
        float used = Count > count ? count : Count;
        _count = Mathf.Max(Count - count, 0);
        OnUsed?.Invoke(Count);
        return used;
    }
}

[FirestoreData]
public class SerializableDateTime
{
    [FirestoreProperty] public long ticks { get; set; }

    public DateTime Value
    {
        get => new DateTime(ticks);
        set => ticks = value.Ticks;
    }

    public SerializableDateTime(DateTime dateTime)
    {
        Value = dateTime;
    }

    public SerializableDateTime() { }
}

public enum ResourceType { Paper, Plastic, Glass }

public enum Skin { Dafault, Container, Oven, Shredder }

[FirestoreData]
public class SerializableAchievement
{
    [FirestoreProperty] public string _type { get; set; }
    [FirestoreProperty] public string _object { get; set; }

    public SerializableAchievement() { }

    public SerializableAchievement(Achievement achievement)
    {
        _type = achievement.GetType().ToString();
        _object = JsonUtility.ToJson(achievement, true);
    }

    public Achievement DeserializedAchievement
    {
        get
        {
            switch(_type)
            {
                case "ResourceScanAchievement":
                    return JsonUtility.FromJson<ResourceScanAchievement>(_object);
                case "ResourceUseAchievement":
                    return JsonUtility.FromJson<ResourceUseAchievement>(_object);
                default:
                    return null;
            }
        }
    }
}

[FirestoreData]
public abstract class Achievement
{
    public Action<string> OnConditionProgressChanged { get; set; }
    public Action OnConditionCompleted { get; set; }

    public int _id;
    public bool _isUnlocked;
    public Reward _reward;

    public bool IsUnlocked => _isUnlocked;
    public abstract string ConditionProgress { get; }

    public Achievement(Reward reward = null)
    {
        _isUnlocked = false;
        _reward = reward;
        OnConditionCompleted += () => { GameManager.AudioManager.PlaySound(Sound.NewAchivement); };
    }

    public abstract void Init();

    protected void Unlock()
    {
        if (!_isUnlocked)
        {
            _isUnlocked = true;
            OnConditionCompleted?.Invoke();
        }
    }

    protected void CompleteEvent()
    {
        if(_reward is not null)
        {
            _reward.Collect();
        }
    }
}

[FirestoreData]
public class ResourceScanAchievement : Achievement
{
    public override string ConditionProgress
    {
        get
        {
            if (_isUnlocked) return "";
            else return $"({Mathf.Round(_progress * 10) / 10}/{Mathf.Round(_count)})";
        }
    }

    public ResourceType _resourceType;
    public float _count;
    public float _progress;

    public ResourceScanAchievement(ResourceType resourceType, float count, Reward reward = null) : base(reward)
    {
        _resourceType = resourceType;
        _count = count;
    }

    public override void Init()
    {
        GameManager.OnResourcesScanned += (ResourceType resourceType, float count) =>
        {
            if(!IsUnlocked && resourceType == _resourceType)
            {
                _progress += count;
                OnConditionProgressChanged?.Invoke(ConditionProgress);
                if (_progress >= _count)
                {
                    Unlock();
                    CompleteEvent();
                }
            }
        };
    }
}

[FirestoreData]
public class ResourceUseAchievement : Achievement
{
    public override string ConditionProgress
    {
        get
        {
            if (_isUnlocked) return "";
            else return $"({Mathf.Round(_progress * 10) / 10}/{Mathf.Round(_count)})";
        }
    }

    public ResourceType _resourceType;
    public float _count;
    public float _progress;

    public ResourceUseAchievement(ResourceType resourceType, float count, Reward reward = null) : base(reward)
    {
        _resourceType = resourceType;
        _count = count;
    }

    public override void Init()
    {
        GameManager.OnResourcesUsed += (ResourceType resourceType, float count) =>
        {
            if (!IsUnlocked && resourceType == _resourceType)
            {
                _progress += count;
                OnConditionProgressChanged?.Invoke(ConditionProgress);
                if (_progress >= _count)
                {
                    Unlock();
                    CompleteEvent();
                }
            }
        };
    }
}

public abstract class Reward
{
    public abstract void Collect();
}

public class SkinReward : Reward
{
    private Skin _skin;

    public SkinReward(Skin skin)
    {
        _skin = skin;
    }

    public override void Collect()
    {
        GameManager.UnlockSkin(_skin);
    }
}