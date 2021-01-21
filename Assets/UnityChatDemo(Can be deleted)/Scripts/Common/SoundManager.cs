using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Play sound manager
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager _instance;
    AudioSource m_bgSound;
    AudioSource m_effectSound;
    /// <summary>
    /// All sound effects are stored in the folder under the Resources folder
    /// </summary>
    private string ResourceDir="Sounds";

    void Awake()
    {
        _instance = this;
        m_bgSound = gameObject.AddComponent<AudioSource>();
        m_bgSound.loop = true;
        m_effectSound = gameObject.AddComponent<AudioSource>();
        m_effectSound.loop = false;
    }
    private void Start()
    {
     
    }
    /// <summary>
    /// Play background music
    /// </summary>
    public void PlayBg(string audioName, float volume)
    {

        string oldName;
        if (m_bgSound.clip == null)
            oldName = "";
        else
            oldName = m_bgSound.clip.name;

        if (oldName != audioName)
        {
            string path;
            if (string.IsNullOrEmpty(ResourceDir))
                path = audioName;
            else
                path = ResourceDir + "/" + audioName;
            AudioClip clip = Resources.Load<AudioClip>(path);
            if (clip != null)
            {
                m_bgSound.clip = clip;
                m_bgSound.Play();
                m_bgSound.loop = true;
                m_bgSound.volume = volume;
            }
        }
    }

    /// <summary>
    /// Stop playing background music
    /// </summary>
    public void StopBg()
    {
        m_bgSound.Stop();
        m_bgSound.clip = null;
    }

    /// <summary>
    /// Play sound effect OneShot
    /// </summary>
    public void PlayEffect(string audioName)
    {
        string path;
        if (string.IsNullOrEmpty(ResourceDir))
            path = audioName;
        else
            path = ResourceDir + "/" + audioName;

        AudioClip clip = Resources.Load<AudioClip>(path);
        m_effectSound.PlayOneShot(clip);
    }
    /// <summary>
    /// Stop playing sound effect
    /// </summary>
    public void StopEffect()
    {
        m_effectSound.Stop();
        m_effectSound.clip = null;
    }
   
}
