using UnityEngine;
using ZXing;
using UnityEngine.UI;
using System;
using System.Threading;
using System.Threading.Tasks;

public class QRCodeScanner : MonoBehaviour
{
    public static Action<string> OnQRCodeDetected { get; set; }
    private static QRCodeScanner _instance;

    [SerializeField] private GameObject _scanPage;
    [SerializeField] private RawImage _rawImageBackground;
    [SerializeField] private AspectRatioFitter _aspectRatioFitter;
    [SerializeField] private RectTransform _scanZone;

    private bool _isCamAvaible;
    private WebCamTexture _cameraTexture;
    private bool _isScannerActive = false;
    private CancellationTokenSource _scanCancellationTokenSource;
    private CancellationTokenSource _devicesWaitingCancellationTokenSource;

    public static async Task<bool> SetScannerState(bool value)
    {
        if (_instance is null) return false;
        if (_instance._devicesWaitingCancellationTokenSource is not null) _instance._devicesWaitingCancellationTokenSource.Cancel();
        if (!_instance._isCamAvaible)
        {
            _instance._devicesWaitingCancellationTokenSource = new CancellationTokenSource();
            _instance._isCamAvaible = await _instance.SetUpCamera(_instance._devicesWaitingCancellationTokenSource.Token);
        }
        if (_instance._isCamAvaible)
        {
            _instance._isScannerActive = value;
            if (_instance._isScannerActive)
            {
                _instance._scanCancellationTokenSource = new CancellationTokenSource();
                _instance.TryScan(_instance._scanCancellationTokenSource.Token);
                _instance._scanPage.gameObject.SetActive(true);
            }
            else
            {
                _instance._scanCancellationTokenSource.Cancel();
                _instance._scanPage.gameObject.SetActive(false);
            }
            return true;
        }
        return false;
    }

    private void Awake()
    {
        _instance = this;
    }

    private void Update()
    {
        if (_isScannerActive)
        {
            UpdateCameraRender();
        }
    }

    private async Task<bool> SetUpCamera(CancellationToken token)
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            int attempts = 10;
            while(attempts > 0)
            {
                if (token.IsCancellationRequested)
                {
                    return false;
                }
                devices = WebCamTexture.devices;
                if (devices.Length > 0)
                {
                    break;
                }
                attempts--;
                await Task.Delay(1000);
            }
            if (attempts == 0) return false;
        }
        for (int i = 0; i < devices.Length; i++)
        {
            if (devices[i].isFrontFacing == false)
            {
                _cameraTexture = new WebCamTexture(devices[i].name, (int)_scanZone.rect.width, (int)_scanZone.rect.height);
                _cameraTexture.Play();
                _rawImageBackground.texture = _cameraTexture;
                return true;
            }
        }
        return false;
    }

    private void UpdateCameraRender()
    {
        if (!_isCamAvaible) return;
        float ratio = (float)_cameraTexture.width / (float)_cameraTexture.height;
        _aspectRatioFitter.aspectRatio = ratio;
        int orientation = _cameraTexture.videoRotationAngle;
        orientation = orientation * 3;
        _rawImageBackground.rectTransform.localEulerAngles = new Vector3(0, 0, orientation);
    }

    private async void TryScan(CancellationToken token)
    {
        while(!token.IsCancellationRequested)
        {
            IBarcodeReader barcodeReader = new BarcodeReader();
            Result result = barcodeReader.Decode(_cameraTexture.GetPixels32(), _cameraTexture.width, _cameraTexture.height);
            if (result != null)
            {
                OnQRCodeDetected?.Invoke(result.Text);
            }
            await Task.Delay(100);
        }
    }
}