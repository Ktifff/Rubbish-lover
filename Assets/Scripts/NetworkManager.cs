using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using Firebase.Auth;
using UnityEngine;
using Google;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;
using System;
using Newtonsoft.Json;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Linq;

public class NetworkManager : MonoBehaviour
{
    public static Action<User> OnPlayerLoaded;
    public static Action<Sprite> OnPlayerAvatarLoaded;
    public static Action<DateTime> OnPlayerSaving;
    private static NetworkManager _instance;

    private FirebaseAuth _auth;
    private FirebaseUser _user;

#if UNITY_EDITOR
    public static void LoadFireBase()
    {
        OnPlayerLoaded?.Invoke(new User());
        OnPlayerAvatarLoaded?.Invoke(null);
    }

#else
    public static async void LoadFireBase()
    {
        await FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            _instance._auth = FirebaseAuth.DefaultInstance;
        });
        GoogleSignIn.Configuration = new GoogleSignInConfiguration
        {
            RequestIdToken = true,
            WebClientId = "719355802770-g45tfevjlgokb2f53eg3eipmskvod3ti.apps.googleusercontent.com"
        };
        Task<GoogleSignInUser> signIn = GoogleSignIn.DefaultInstance.SignIn();
        TaskCompletionSource<FirebaseUser> signInCompleted = new TaskCompletionSource<FirebaseUser>();
        await signIn.ContinueWith(async task => {
            if (task.IsCanceled)
            {
                signInCompleted.SetCanceled();
            }
            else if (task.IsFaulted)
            {
                signInCompleted.SetException(task.Exception);
            }
            else
            {

                Credential credential = GoogleAuthProvider.GetCredential(((Task<GoogleSignInUser>)task).Result.IdToken, null);
                await _instance._auth.SignInWithCredentialAsync(credential).ContinueWith(async authTask => {
                    if (authTask.IsCanceled)
                    {
                        signInCompleted.SetCanceled();
                    }
                    else if (authTask.IsFaulted)
                    {
                        signInCompleted.SetException(authTask.Exception);
                    }
                    else
                    {
                        signInCompleted.SetResult(((Task<FirebaseUser>)authTask).Result);
                        _instance._user = _instance._auth.CurrentUser;
                        User user = await _instance.GetUserSave();
                        OnPlayerLoaded?.Invoke(user);
                        _instance.StartCoroutine(LoadAvatarCoroutine(_instance._user.PhotoUrl.ToString()));
                        PlayerPrefs.SetInt("LoggedIn", 1);
                    }
                });
            }
        });
    }
#endif

#if UNITY_EDITOR
    //Server side in future
    public static void SetUserSave(User user) { }
#else
    //Server side in future
    public static async void SetUserSave(User user)
    {
        user.Save();
        OnPlayerSaving?.Invoke(DateTime.UtcNow);
        DocumentReference userDoc = FirebaseFirestore.DefaultInstance.Collection("users").Document(_instance._user.UserId);
        await userDoc.SetAsync(user);
    }
#endif

    //Server side in future
    public static async Task<Dictionary<ResourceType, float>> RedeemQrCode(string qrCode)
    {
        DocumentReference codeDoc = FirebaseFirestore.DefaultInstance.Collection("qrcodes").Document(qrCode);
        DocumentSnapshot snapshot = await codeDoc.GetSnapshotAsync();
        if (!snapshot.Exists)
        {
            return null;
        }
        if (snapshot.TryGetValue("usedBy", out string usedBy) && !string.IsNullOrEmpty(usedBy))
        {
            return null;
        }
        Dictionary<ResourceType, float> resources = new Dictionary<ResourceType, float>();
        for (int i = 0; i < Enum.GetValues(typeof(ResourceType)).Length; i++)
        {
            Dictionary<string, object> fields = snapshot.ToDictionary();
            if (fields.ContainsKey(((ResourceType)i).ToString()) && float.TryParse(fields[((ResourceType)i).ToString()].ToString(), out float result))
            {
                resources.Add((ResourceType)i, result);
            }
        }
        await codeDoc.UpdateAsync("usedBy", _instance._user.UserId);
        return resources;
    }

    //Server side in future
    public static async Task<string> GetCode(string service)
    {
        Query query = FirebaseFirestore.DefaultInstance.Collection($"codes_{service}").WhereEqualTo("isActivated", false).OrderBy(FieldPath.DocumentId).Limit(1);
        QuerySnapshot querySnapshot = await query.GetSnapshotAsync();
        if (querySnapshot.Count > 0)
        {
            DocumentReference codeDoc = FirebaseFirestore.DefaultInstance.Collection($"codes_{service}").Document(querySnapshot.Documents.First().Id);
            DocumentSnapshot snapshot = await codeDoc.GetSnapshotAsync();
            await codeDoc.UpdateAsync("isActivated", true);
            return querySnapshot.Documents.First().GetValue<string>("code");
        }
        return null;
    }

    private void Awake()
    {
        _instance = this;
    }

    private static IEnumerator LoadAvatarCoroutine(string url)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D avatarTexture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            Sprite avatarSprite = Sprite.Create(avatarTexture, new Rect(0, 0, avatarTexture.width, avatarTexture.height), new Vector2(0, 0));
            OnPlayerAvatarLoaded?.Invoke(avatarSprite);
        }
    }

    //Server side in future
    private async Task<User> GetUserSave()
    {
        DocumentReference userDoc = FirebaseFirestore.DefaultInstance.Collection("users").Document(_user.UserId);
        DocumentSnapshot snapshot = await userDoc.GetSnapshotAsync();
        if (snapshot.Exists)
        {
            User user = snapshot.ConvertTo<User>();
            double seconds = (DateTime.UtcNow - user.LastSaveDate.Value).TotalSeconds;
            user.Pet.WasteTime((float)seconds);
            return user;
        }
        else
        {
            return new User();
        }
    }
}
