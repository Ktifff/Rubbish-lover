using UnityEngine;
using UnityEngine.UI;

public class SkinUI : MonoBehaviour
{
    [SerializeField] private Image _picture;
    [SerializeField] private Image _pictureShadow;
    [SerializeField] private Text _nameText;
    [SerializeField] private Text _descriptionText;
    [SerializeField] private GameObject _completedMark;
    [SerializeField] private GameObject _selectedMark;
    [SerializeField] private Button _button;

    public void Initialize(Sprite image, string name, string description, bool isCompleted, bool isSelected)
    {
        _picture.sprite = image;
        _pictureShadow.sprite = image;
        _nameText.text = $"{name}";
        _descriptionText.text = description;
        _completedMark.SetActive(isCompleted && !isSelected);
        _selectedMark.SetActive(isSelected);
        gameObject.SetActive(true);
    }

    public void UpdateVisual(bool isCompleted, bool isSelected)
    {
        _completedMark.SetActive(isCompleted && !isSelected);
        _selectedMark.SetActive(isSelected);
    }

    public Button GetButton() => _button;
}
