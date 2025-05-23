using UnityEngine;
using System.Collections.Generic;

public class PetUI : MonoBehaviour
{
    [SerializeField] private Animator _petAnimator;
    [SerializeField] private GameObject _feedEffectPrefab;
    [SerializeField] private List<Animator> _pets;

    private Skin _currentPet;

    public void MakeFeedAnimation(ResourceType resourceType)
    {
        GameObject obj = Instantiate(_feedEffectPrefab, UIManager.MainCanvas.GetComponent<Transform>());
        obj.transform.position = UIManager.GetResourceIconPosition(resourceType);
        obj.GetComponent<FeedPlusEffect>().Initialize(UIManager.GetResourceImage(resourceType), transform.position, this, resourceType);
    }

    public void FeedAnimation()
    {
        _pets[(int)_currentPet].Play("Feed");
    }

    public void SetPet(Skin skin)
    {
        foreach(Animator animator in _pets)
        {
            animator.gameObject.SetActive(false);
        }
        _currentPet = skin;
        _pets[(int)_currentPet].gameObject.SetActive(true);
    }
}
