using UnityEngine;
using UnityEngine.UI;

public class ResourcePlusEffect : MonoBehaviour
{
    [SerializeField] private RawImage _image; 

    private Vector2 _target;
    private float _delay;
    private ResourceUI _resourceUI;

    public void Initialize(Texture image, Vector2 target, ResourceUI resourceUI, float delay)
    {
        _image.texture = image;
        _target = target;
        _resourceUI = resourceUI;
        _delay = delay;
    }

    private void Update()
    {
        if (_delay <= 0)
        {
            transform.position = Vector2.MoveTowards(transform.position, _target, Time.deltaTime * 7);
            if (Vector2.Distance(transform.position, _target) < 0.1f)
            {
                _resourceUI.VisualCount += 0.1f;
                Destroy(gameObject);
            }
        }
        else _delay -= Time.deltaTime;
    }
}
