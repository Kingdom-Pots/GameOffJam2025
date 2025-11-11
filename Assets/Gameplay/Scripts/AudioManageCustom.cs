using UnityEngine;
using System;

/// <summary>
/// Simple audio data structure
/// </summary>
[Serializable]
public class AudioClipData
{
    public string Key;
    public AudioClip Clip;
    [Range(0f, 1f)]
    public float Volume = 1f;
}

/// <summary>
/// Simple Audio Manager for playing sounds by string
/// </summary>
public class AudioManageCustom : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource AudioSource;
    
    [Header("Audio Clips")]
    public AudioClipData[] AudioClips;
    
    // Singleton instance
    private static AudioManageCustom _instance;
    public static AudioManageCustom Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindAnyObjectByType<AudioManageCustom>();
            return _instance;
        }
    }
    
    private void Awake()
    {   
        // Create audio source if not assigned
        if (AudioSource == null)
        {
            AudioSource = gameObject.AddComponent<AudioSource>();
            AudioSource.playOnAwake = false;
        }
    }
    
    /// <summary>
    /// Play a sound by its string key
    /// </summary>
    public void PlaySound(string soundKey)
    {
        AudioClipData clipData = GetClip(soundKey);
        if (clipData != null && clipData.Clip != null)
        {
            if (AudioSource.isPlaying) AudioSource.Stop();
            AudioSource.PlayOneShot(clipData.Clip, clipData.Volume);
        }
        else
        {
            Debug.LogWarning($"Sound '{soundKey}' not found!");
        }
    }

    public void Stop()
    {
        if (AudioSource.isPlaying) AudioSource.Stop();
    }
    
    /// <summary>
    /// Find audio clip by key
    /// </summary>
    private AudioClipData GetClip(string key)
    {
        foreach (AudioClipData clip in AudioClips)
        {
            if (clip.Key == key)
                return clip;
        }
        return null;
    }
}
