using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Text _expText;
    [SerializeField] private Text _levelText;
    [SerializeField] private Text _levelName;
    [SerializeField] private Image _levelBar;
    [SerializeField] private GameBalance _gameBalance;
    [SerializeField] private List<Text> _rubbishButtonsText;
    [SerializeField] private Text _satietyText;
    [SerializeField] private Animator _petAnimator;
    [SerializeField] private GameObject _mainScene;
    [SerializeField] private GameObject _chooseScene;
    [SerializeField] private Text _chooseSceneName;
    [SerializeField] private List<GameObject> _choosePanels;
    [SerializeField] private Transform _achiList;
    [SerializeField] private GameObject _achiScene;
    [SerializeField] private List<RuntimeAnimatorController> _animators;

    private User _user;
    private float _userExpFloat;
    private int _currentAnimator = -1;

    private static GameManager instance;
    public static GameBalance GameBalance => instance._gameBalance;
    public static GameBalance.LevelData GetLevelData(int level)
    {
        if(level < GameBalance.Levels.Count - 1)
        {
            return GameBalance.Levels[level];
        }
        else return GameBalance.Levels[GameBalance.Levels.Count - 1];
    }

    private void Awake()
    {
        instance = this;
        _user = Load("User.json");
        _user.OnExpChanged += OnUserExpChanged;
        OnUserExpChanged(_user.Level, (float)_user.Exp / GameBalance.Levels[_user.Level].ExpCount);
        for (int i = 0; i < 3; i++)
        {
            _user.Rubbish[i].OnCountChanged += OnUserRubbishChanged;
            OnUserRubbishChanged(i, _user.Rubbish[i].Count);
        }
        _user.Pet.OnSatietyChanged += OnSatietyChanged;
        for (int i = 0; i < GameBalance.AchievementsData.Count; i++)
        {
            _user.Achievements[i].OnAchievementUnlocked += UpdateAchievements;
        }
        UpdateAchievements();
        ChangeAnimator();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            //_user.Pet.Satiety -= 10;
        }
        _levelBar.fillAmount = Mathf.Lerp(_levelBar.fillAmount, _userExpFloat, 5 * Time.deltaTime);
        _user.Pet.UpdateSatiety();
        _petAnimator.SetBool("Hungry", _user.Pet.Satiety < 5);
    }

    private void OnUserExpChanged(int level, float exp)
    {
        _levelName.text = GetLevelData(_user.Level - 1).Name;
        _levelText.text = "Level\n" + level;
        _userExpFloat = exp;
        _expText.text = Mathf.Round(exp * GetLevelData(_user.Level).ExpCount) + "/" + GetLevelData(_user.Level).ExpCount;
    }

    private void OnUserRubbishChanged(int id, float count)
    {
        _rubbishButtonsText[id].text = (Mathf.Round(count * 100) / 100) + " kg";
    }

    private void OnSatietyChanged(float count)
    {
        _satietyText.text = "Satiety\n" + (Mathf.Round(count * 10) / 10) + "/100";
    }

    public void ChooseRubbish(int id)
    {
        _mainScene.SetActive(false);
        _chooseScene.SetActive(true);
        _chooseSceneName.text = GameBalance.Resources[id];
        for (int i = 0; i < _choosePanels.Count; i++) _choosePanels[i].gameObject.SetActive(false);
        _choosePanels[id].gameObject.SetActive(true);
    }

    public void AddRubbish()
    {
        _mainScene.SetActive(true);
        _chooseScene.SetActive(false);
    }

    public void AddRubbishPaper(float count)
    {
        AddRubbish();
        _user.Rubbish[0].Add(count);
    }

    public void AddRubbishPlastic(float count)
    {
        AddRubbish();
        _user.Rubbish[1].Add(count);
    }

    public void AddRubbishGlass(float count)
    {
        AddRubbish();
        _user.Rubbish[2].Add(count);
    }

    public void BackToMain()
    {
        _mainScene.SetActive(true);
        _chooseScene.SetActive(false);
        _achiScene.SetActive(false);
    }

    public void Feed()
    {
        if (_user.Pet.Feed(_user.Rubbish))
        {
            _petAnimator.SetTrigger("Feed");
            _user.AddExp(200);
            _user.Achievements[0].Unlock();
            if (_user.Rubbish[(int)RabbishType.Paper].Disposed > 5) _user.Achievements[1].Unlock();
            if (_user.Rubbish[(int)RabbishType.Plastic].Disposed > 10) _user.Achievements[2].Unlock();
        }
    }

    public void UpdateAchievements()
    {
        for (int i = 0; i < _achiList.childCount; i++)
        {
            if (i < GameBalance.AchievementsData.Count)
            {
                _achiList.GetChild(i).GetChild(0).GetChild(0).GetComponent<Text>().text = GameBalance.AchievementsData[i].Description;
                _achiList.GetChild(i).GetChild(1).GetComponent<Text>().text = GameBalance.AchievementsData[i].Name;
                if (_user.Achievements[i].Unlocked) _achiList.GetChild(i).GetChild(2).gameObject.SetActive(true);
                _achiList.GetChild(i).gameObject.SetActive(true);
            }
            else _achiList.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void OpenAchi()
    {
        _mainScene.SetActive(false);
        _achiScene.SetActive(true);
    }

    public void ChangeAnimator()
    {
        _currentAnimator++;
        if (_currentAnimator >= _animators.Count) _currentAnimator = 0;
        _petAnimator.runtimeAnimatorController = _animators[_currentAnimator];
    }

    private static string GetPath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, fileName);
        //return Path.Combine(Application.dataPath, fileName);
    }

    public static void Save(User data, string fileName)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetPath(fileName), json);
    }

    public static User Load(string fileName)
    {
        string path = GetPath(fileName);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<User>(json);
        }
        else return new User();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        Save(_user, "User.json");
    }

    void OnApplicationQuit()
    {
        Save(_user, "User.json");
    }
}
