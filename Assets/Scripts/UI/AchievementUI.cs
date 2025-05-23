using UnityEngine;
using UnityEngine.UI;

public class AchievementUI : MonoBehaviour
{
    [SerializeField] private Image _picture;
    [SerializeField] private Image _pictureShadow;
    [SerializeField] private Text _nameText;
    [SerializeField] private Text _descriptionText;
    [SerializeField] private GameObject _completedMark;

    private string _name;
    private string _prefix;

    public void Initialize(Sprite image, string name, string prefix, string description, bool isCompleted)
    {
        _picture.sprite = image;
        _pictureShadow.sprite = image;
        _name = name;
        _prefix = prefix;
        _nameText.text = $"{_name}{_prefix}";
        _descriptionText.text = description;
        _completedMark.SetActive(isCompleted);
        gameObject.SetActive(true);
    }

    public void UpdateVisual(string prefix)
    {
        _prefix = prefix;
        _nameText.text = $"{_name}{_prefix}";
    }

    public void UpdateVisual(string prefix, bool isCompleted)
    {
        UpdateVisual(prefix);
        _completedMark.SetActive(isCompleted);
    }
}
