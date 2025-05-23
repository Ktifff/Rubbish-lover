using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static Action<ResourceType, float> OnResourcesScanned;
    public static Action<ResourceType, float> OnResourcesUsed;
    public static GameBalance GameBalance => _instance._gameBalance;
    public static AudioManager AudioManager => _instance._audioManager;
    private static GameManager _instance;

    [SerializeField] private GameBalance _gameBalance;
    [SerializeField] private AudioManager _audioManager;
    [Space]
    [SerializeField] private GameObject _gamePage;
    [SerializeField] private GameObject _signInPage;
    [SerializeField] private GameObject _mainPage;
    [SerializeField] private GameObject _achievementPage;
    [SerializeField] private GameObject _skinPage;
    [SerializeField] private GameObject _codesPage;

    private User _user;
    private bool _isAuthorizationStarted = false;
    private bool _isFindingCode = false;

    public static void UnlockSkin(Skin skin)
    {
        _instance._user.UnlockSkin(skin);
    }

    public void StartAuthorization()
    {
        if (!_isAuthorizationStarted)
        {
            _isAuthorizationStarted = true;
            NetworkManager.OnPlayerLoaded += (User user) =>
            {
                _user = user;
                _user.Load();
                UIManager.SetPlayerExp(_user.Exp, User.GetLevelData(_user.Level).ExpCount, _user.Level);
                _user.OnExpChanged += (int exp, int level) =>
                {
                    UIManager.SetPlayerExp(exp, User.GetLevelData(level).ExpCount, level);
                    NetworkManager.SetUserSave(_user);
                };
                for (int i = 0; i < Enum.GetValues(typeof(ResourceType)).Length; i++)
                {
                    UIManager.SetResource(i, _user.GetResource((ResourceType)i).Count);
                    int temp = i;
                    _user.GetResource((ResourceType)temp).OnCountChanged += (float count) =>
                    {
                        UIManager.SetResource(temp, count, true);
                        NetworkManager.SetUserSave(_user);
                    };
                    _user.GetResource((ResourceType)temp).OnUsed += (float count) =>
                    {
                        UIManager.SetResource(temp, count);
                        NetworkManager.SetUserSave(_user);
                    };
                }
                UIManager.PetSatiety = _user.Pet.Satiety;
                _user.Pet.OnSatietyChanged += (float satiety) =>
                {
                    UIManager.PetSatiety = satiety;
                    NetworkManager.SetUserSave(_user);
                };
                for (int i = 0; i < _user.Achievements.Count; i++)
                {
                    if (GameBalance.AchievementsData.Count <= i) break;
                    GameBalance.AchievementData achievementData = GameBalance.AchievementsData[i];
                    Achievement achievement = _user.GetAchievement(i);
                    achievement.Init();
                    UIManager.GetAchievementUI(i).Initialize(achievementData.Image, achievementData.Name, achievement.ConditionProgress, achievementData.Description, achievement.IsUnlocked);
                    int temp = i;
                    achievement.OnConditionProgressChanged += (string progress) =>
                    {
                        UIManager.GetAchievementUI(temp).UpdateVisual(progress);
                        NetworkManager.SetUserSave(_user);
                    };
                    string achievementName = achievementData.Name;
                    achievement.OnConditionCompleted += () =>
                    {
                        UIManager.GetAchievementUI(temp).UpdateVisual(achievement.ConditionProgress, true);
                        UIManager.PetSay($"New achievement! <{achievementName}>");
                        NetworkManager.SetUserSave(_user);
                    };
                }
                for (int i = 0; i < Enum.GetValues(typeof(Skin)).Length; i++)
                {
                    GameBalance.SkinData skinData = GameBalance.SkinsData[i];
                    SkinUI skinUI = UIManager.GetSkinUI((Skin)i);
                    skinUI.Initialize(skinData.Image, skinData.Name, skinData.Description, _user.IsSkinUnlocked((Skin)i), (int)_user.CurrentSkin == i);
                    Skin skin = (Skin)i;
                    skinUI.GetButton().onClick.AddListener(() => { _user.SetSkin(skin); });
                }
                _user.OnSkinUnlocked += (Skin skin) =>
                {
                    UIManager.GetSkinUI(skin).UpdateVisual(true, _user.CurrentSkin == skin);
                    NetworkManager.SetUserSave(_user);
                };
                _user.OnSkinChanged += (Skin previousSkin, Skin skin) =>
                {
                    UIManager.GetSkinUI(previousSkin).UpdateVisual(true, false);
                    UIManager.GetSkinUI(skin).UpdateVisual(true, _user.CurrentSkin == skin);
                    UIManager.PetVisualizer.SetPet(skin);
                    NetworkManager.SetUserSave(_user);
                };
                _user.SetSkin(_user.CurrentSkin);
                for (int i = 0; i < GameBalance.CodesData.Count; i++)
                {
                    GameBalance.CodeData data = GameBalance.CodesData[i];
                    CodeUI codeUI = UIManager.GetCodeUI(i);
                    codeUI.Initialize(data.Image, data.Cost);
                    int temp = i;
                    codeUI.GetButton().onClick.AddListener(() => { UseCode(temp); });
                }
                UIManager.SetPointsCount(_user.Points);
                _user.OnPointsChanged += (int points) =>
                {
                    UIManager.SetPointsCount(points);
                    NetworkManager.SetUserSave(_user);
                };
            };
            NetworkManager.OnPlayerAvatarLoaded += (Sprite avatar) => 
            { 
                UIManager.PlayerAvatar = avatar;
                _signInPage.gameObject.SetActive(false);
            };
            NetworkManager.OnPlayerSaving += (DateTime date) =>
            {
                _user.LastSaveDate.Value = date;
            };
            NetworkManager.LoadFireBase();
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            _user.GetResource(ResourceType.Plastic).Add(1);
            OnResourcesScanned.Invoke(ResourceType.Plastic, 1);
            AudioManager.PlaySound(Sound.GetResource);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _user.GetResource(ResourceType.Glass).Add(1);
            OnResourcesScanned.Invoke(ResourceType.Glass, 1);
            AudioManager.PlaySound(Sound.GetResource);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            _user.GetResource(ResourceType.Paper).Add(1);
            OnResourcesScanned.Invoke(ResourceType.Paper, 1);
            AudioManager.PlaySound(Sound.GetResource);
        }
    }
#endif

    public async void ScanQRCode(bool mode)
    {
        if (mode)
        {
            if (await QRCodeScanner.SetScannerState(true))
            {
                _gamePage.gameObject.SetActive(false);
            }
        }
        else
        {
            await QRCodeScanner.SetScannerState(false);
            _gamePage.gameObject.SetActive(true);
        }
    }

    public void OpenAchievements(bool mode)
    {
        _mainPage.gameObject.SetActive(!mode);
        _achievementPage.gameObject.SetActive(mode);
    }

    public void OpenSkins(bool mode)
    {
        _mainPage.gameObject.SetActive(!mode);
        _skinPage.gameObject.SetActive(mode);
    }

    public void OpenCodes(bool mode)
    {
        _mainPage.gameObject.SetActive(!mode);
        _codesPage.gameObject.SetActive(mode);
    }

    public void FeedPet()
    {
        if (_user is null) return;
        List<ResourceType> resourceTypes = new List<ResourceType>();
        Dictionary<ResourceType, Resource> resources = new Dictionary<ResourceType, Resource>(); 
        for (int i = 0; i < Enum.GetValues(typeof(ResourceType)).Length; i++)
        {
            Resource resource = _user.GetResource((ResourceType)i);
            if (resource.Count > 0)
            {
                resourceTypes.Add((ResourceType)i);
                resources.Add((ResourceType)i, resource);
            }
        }
        if (resourceTypes.Count > 0)
        {
            ResourceType randomResource = resourceTypes[UnityEngine.Random.Range(0, resourceTypes.Count)];
            float preFeedResult = _user.Pet.Satiety;
            float feedResult = _user.Pet.Feed(resources[randomResource]);
            if (feedResult > 0)
            {
                float feedCoefficient = 1f;
                if (randomResource == ResourceType.Glass && _user.CurrentSkin == Skin.Oven || randomResource == ResourceType.Paper && _user.CurrentSkin == Skin.Shredder)
                {
                    feedCoefficient *= 1.3f;
                }
                OnResourcesUsed?.Invoke(randomResource, feedResult);
                _user.AddExp((int)(feedResult / GameBalance.FoodPerFeed * GameBalance.ExpPerFeed));
                _user.AddPoints((int)(feedResult / GameBalance.FoodPerFeed * GameBalance.PointsPerFeed * feedCoefficient * GameBalance.FeedCurve.Evaluate(preFeedResult / 100)));
                UIManager.PetFeedAnimation(randomResource);
            }
        }
    }

    private void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(gameObject);
        Application.targetFrameRate = 120;
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey("LoggedIn") && PlayerPrefs.GetInt("LoggedIn") == 1)
        {
            StartAuthorization();
        }
        QRCodeScanner.OnQRCodeDetected += async (string code) =>
        {
            ScanQRCode(false);
            Dictionary<ResourceType, float> resources = await NetworkManager.RedeemQrCode(code);
            if (resources is not null && resources.Count > 0)
            {
                UIManager.PetSay("Good job! ;D");
                foreach (KeyValuePair<ResourceType, float> resource in resources)
                {
                    _user.GetResource(resource.Key).Add(resource.Value);
                    OnResourcesScanned?.Invoke(resource.Key, resource.Value);
                }
                AudioManager.PlaySound(Sound.GetResource);
            }
            else
            {
                UIManager.PetSay("Sorry, this code has already been activated :(");
            }
        };
    }

    private async void UseCode(int id)
    {
        if (_isFindingCode) return;
        _isFindingCode = true;
        if (_user.WastePoints(GameBalance.CodesData[id].Cost))
        {
            string code = await NetworkManager.GetCode(GameBalance.CodesData[id].Name);
            if (String.IsNullOrEmpty(code))
            {
                _user.AddPoints(GameBalance.CodesData[id].Cost);
            }
            else
            {
                UIManager.CopyCodeUI.Init(code);
                AudioManager.PlaySound(Sound.PressButton);
            }
        }
        _isFindingCode = false;
    }
}
