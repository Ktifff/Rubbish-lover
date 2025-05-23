using UnityEngine;
using UnityEngine.UI;

public class CopyCodeUI : MonoBehaviour
{
    [SerializeField] private Text _codeText;

    private string _code;

    public void Init(string code)
    {
        _code = code;
        _codeText.text = code;
        gameObject.SetActive(true);
    }

    public void CopyCode()
    {
        GUIUtility.systemCopyBuffer = _code;
        gameObject.SetActive(false);
    }
}
