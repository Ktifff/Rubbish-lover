using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private const float PlayerExpBarValueChangeSpeed = 5;

    public static Canvas MainCanvas => _instance._mainCanvas;
    private static UIManager _instance;

    [SerializeField] private Canvas _mainCanvas;

    [Header("Avatar")]
    [SerializeField] private Image _playerAvatar;
    [SerializeField] private Image _playerExpBar;

    [Header("Level")]
    [SerializeField] private Text _playerLevelName;
    [SerializeField] private Text _playerLevelCount;

    [Header("Pet")]
    [SerializeField] private Text _petSatiety;

    [Header("Resources")]
    [SerializeField] private List<ResourceUI> _resourcesUI;

    [Header("Achievements")]
    [SerializeField] private List<AchievementUI> _achievementsUI;

    [Header("Skins")]
    [SerializeField] private List<SkinUI> _skinsUI;

    [Header("Codes")]
    [SerializeField] private Text _pointsCount;
    [SerializeField] private List<CodeUI> _codesUI;
    [SerializeField] private CopyCodeUI _copyCodeUI;

    [Header("Pet")]
    [SerializeField] private Text _petSatietyCounter;
    [SerializeField] private PetPhrase _petPhrase;
    [SerializeField] private PetUI _petUI;

    private float _targetPlayerExpBarValue;

    public static PetUI PetVisualizer => _instance._petUI;

    public static CopyCodeUI CopyCodeUI => _instance._copyCodeUI;

    public static Sprite PlayerAvatar
    {
        set
        {
            if (_instance is null) return;
            _instance._playerAvatar.sprite = value;
        }
    }

    public static float PetSatiety
    {
        set
        {
            if (_instance is null) return;
            _instance._petSatietyCounter.text = $"Satiety: {Mathf.Round(value * 10) / 10}/100";
        }
    }

    public static void SetPointsCount(int count)
    {
        _instance._pointsCount.text = $"{count}";
    }

    public static void SetPlayerExp(int current, int target, int level)
    {
        if (_instance is null) return;
        _instance._playerLevelName.text = $"Level {level}. {User.GetLevelData(level - 1).Name}";
        _instance._playerLevelCount.text = $"{current}/{target}";
        _instance._targetPlayerExpBarValue = (float)current / target;
    }

    public static void SetResource(int id, float count, bool animation = false)
    {
        if (_instance is null) return;
        if (animation)
        {
            _instance._resourcesUI[id].MakePlusAnimation(count);
        }
        else
        {
            _instance._resourcesUI[id].Count = count;
        }
    }

    public static AchievementUI GetAchievementUI(int id)
    {
        if (_instance is null) return null;
        return _instance._achievementsUI[id];
    }

    public static SkinUI GetSkinUI(Skin skin)
    {
        if (_instance is null) return null;
        return _instance._skinsUI[(int)skin];
    }

    public static CodeUI GetCodeUI(int id)
    {
        if (_instance is null) return null;
        return _instance._codesUI[id];
    }

    public static Texture GetResourceImage(ResourceType resourceType)
    {
        if (_instance is null) return null;
        return _instance._resourcesUI[(int)resourceType].Image;
    }

    public static Vector2 GetResourceIconPosition(ResourceType resourceType)
    {
        if (_instance is null) return Vector2.zero;
        return _instance._resourcesUI[(int)resourceType].IconPosition;
    }

    public static void PetSay(string text)
    {
        if (_instance is null) return;
        _instance._petPhrase.Say(text);
    }

    public static void PetFeedAnimation(ResourceType resourceType)
    {
        if (_instance is null) return;
        _instance._petUI.MakeFeedAnimation(resourceType);
    }

    private void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        _instance._playerExpBar.fillAmount = Mathf.Lerp(_instance._playerExpBar.fillAmount, _targetPlayerExpBarValue, Time.deltaTime * PlayerExpBarValueChangeSpeed);
    }
}
