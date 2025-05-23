using UnityEngine;
using UnityEngine.UI;

public class CodeUI : MonoBehaviour
{
    [SerializeField] private Image _picture;
    [SerializeField] private Text _priceText;
    [SerializeField] private Button _button;

    public void Initialize(Sprite image, int cost)
    {
        _picture.sprite = image;
        _priceText.text = $"{cost}";
        gameObject.SetActive(true);
    }

    public Button GetButton() => _button;
}
