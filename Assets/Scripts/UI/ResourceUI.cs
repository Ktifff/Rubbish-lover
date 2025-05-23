using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ResourceUI : MonoBehaviour
{
    [SerializeField] private string _name;
    [SerializeField] private Texture _image;
    [SerializeField] private Texture _rotateImage;
    [Space]
    [SerializeField] private RawImage _picture;
    [SerializeField] private RawImage _pictureShadow;
    [SerializeField] private Text _nameText;
    [SerializeField] private Text _countText;
    [SerializeField] private GameObject _plusEffectPrefab;

    private float _count = 0;
    private float _visualCount = 0;

    public Texture Image => _rotateImage;
    public Vector2 IconPosition => _picture.transform.position;

    public float Count
    {
        get => Count;
        set
        {
            float temp = _count;
            _count = value;
            VisualCount += value - temp;
        }
    }

    public float VisualCount
    {
        get => _visualCount;
        set
        {
            _visualCount = Mathf.Min(value, _count);
            _countText.text = $"{Mathf.Round(_visualCount * 100) / 100} kg";
        }
    }

    public void Initialize(string name, Texture image)
    {
        _picture.texture = image;
        _pictureShadow.texture = image;
        _nameText.text = name;
        Count = 0;
    }

    public void MakePlusAnimation(float count)
    {
        _count = count;
        for (int i = 0; i < (int)Mathf.Ceil(count / 0.1f); i++)
        {
            GameObject obj = Instantiate(_plusEffectPrefab, UIManager.MainCanvas.GetComponent<Transform>());
            obj.GetComponent<ResourcePlusEffect>().Initialize(_rotateImage, _picture.transform.position, this, i * 0.1f);
        }
    }

    private void Start()
    {
        Initialize(_name, _image);
    }
}
