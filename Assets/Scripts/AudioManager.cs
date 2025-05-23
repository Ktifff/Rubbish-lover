using UnityEngine;
using System.Collections.Generic;
using System;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [Space]
    [SerializeField] private List<SoundData> _sounds;

    public void PlaySound(Sound sound)
    {
        foreach(SoundData soundData in _sounds)
        {
            if (soundData.IsEqualKey(sound))
            {
                soundData.Play(_audioSource);
                break;
            }
        }
    }

    public void PlayButton()
    {
        PlaySound(Sound.PressButton);
    }

    [Serializable]
    public class SoundData
    {
        [SerializeField] private Sound Key;
        [SerializeField] private List<AudioClip> clips;

        public bool IsEqualKey(Sound key)
        {
            return Key == key;
        }

        public void Play(AudioSource audioSource)
        {
            audioSource.PlayOneShot(clips[UnityEngine.Random.Range(0, clips.Count)]);
        }
    }
}

public enum Sound { PressButton, FeedPetPaper, FeedPetPlastic, FeedPetGlass, GetResource, LevelUp, NewAchivement }