using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PetPhrase : MonoBehaviour
{
    [SerializeField] private Text _text;
    [SerializeField] private Animator _animator;

    private CancellationTokenSource _sayCancellationTokenSource;

    public void Say(string text)
    {
        if (_sayCancellationTokenSource is not null) _sayCancellationTokenSource.Cancel();
        _sayCancellationTokenSource = new CancellationTokenSource();
        Print(text, _sayCancellationTokenSource.Token);
    }

    private async void Print(string text, CancellationToken token)
    {
        _text.text = "";
        for (int i = 0; i < text.Length; i++)
        {
            if (token.IsCancellationRequested) return;
            _text.text += text[i];
            await Task.Delay(60);
        }
        await Task.Delay(4000);
        _animator.Play("Disappear");
        await Task.Delay(750);
        _text.text = "";
    }
}
