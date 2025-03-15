using System.Collections.Generic;
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

    private User _user;
    private float _userExpFloat; 

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
        _user = new User();
        _user.OnExpChanged += OnUserExpChanged;
        OnUserExpChanged(_user.Level, (float)_user.Exp / GameBalance.Levels[_user.Level].ExpCount);
        for (int i = 0; i < 3; i++)
        {
            _user.Rubbish[i].OnCountChanged += OnUserRubbishChanged;
            OnUserRubbishChanged(i, 0);
        }
        _user.Pet.OnSatietyChanged += OnSatietyChanged;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            _user.AddExp(300);
        }
        _levelBar.fillAmount = Mathf.Lerp(_levelBar.fillAmount, _userExpFloat, 5 * Time.deltaTime);
        _user.Pet.UpdateSatiety();
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
    }

    public void Feed()
    {
        if (_user.Pet.Feed(_user.Rubbish))
        {
            _petAnimator.SetTrigger("Feed");
            _user.AddExp(200);
        }
    }
}
