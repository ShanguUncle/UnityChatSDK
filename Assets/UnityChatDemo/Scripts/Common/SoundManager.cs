using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 播放音效管理
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager _instance;
    AudioSource m_bgSound;
    AudioSource m_effectSound;
    /// <summary>
    /// 所有音效存放在Resources文件夹下的Sounds文件夹下
    /// </summary>
    private string ResourceDir="Sounds";
    private bool isPlayBg = true;
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
    /// 播放背景音乐
    /// </summary>
    /// <param name="audioName">音乐名字</param>
    /// <param name="volume">音量</param>
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
            //播放
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
    /// 停止播放背景音乐
    /// </summary>
    public void StopBg()
    {
        m_bgSound.Stop();
        m_bgSound.clip = null;
    }

    /// <summary>
    /// 播放音效（一次）
    /// </summary>
    /// <param name="audioName">音效名字</param>
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
    /// 停止播放音效
    /// </summary>
    public void StopEffect()
    {
        m_effectSound.Stop();
        m_effectSound.clip = null;
    }
   
}
