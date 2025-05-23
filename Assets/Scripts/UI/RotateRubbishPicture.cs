using UnityEngine;

public class RotateRubbishPicture : MonoBehaviour
{
    private const float RotateSpeed = 3f;

    [SerializeField] private GameObject _axis;

    private void Update()
    {
        _axis.transform.Rotate(Vector3.up * RotateSpeed);
    }
}
