using UnityEngine;
using UnityEngine.UI;

public class FeedPlusEffect : MonoBehaviour
{
    [SerializeField] private RawImage _image;

    private Vector2 _target;
    private PetUI _petUI;
    private ResourceType _resourceType;

    public void Initialize(Texture image, Vector2 target, PetUI petUI, ResourceType resourceType)
    {
        _image.texture = image;
        _target = target;
        _petUI = petUI;
        _resourceType = resourceType;
    }

    private void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, _target, Time.deltaTime * 7);
        if (Vector2.Distance(transform.position, _target) < 0.1f)
        {
            _petUI.FeedAnimation();
            switch(_resourceType)
            {
                case ResourceType.Paper: GameManager.AudioManager.PlaySound(Sound.FeedPetPaper); break;
                case ResourceType.Plastic: GameManager.AudioManager.PlaySound(Sound.FeedPetPlastic);  break;
                case ResourceType.Glass: GameManager.AudioManager.PlaySound(Sound.FeedPetGlass); break;
            }
            Destroy(gameObject);
        }
    }
}
