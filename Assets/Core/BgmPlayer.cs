using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BgmPlayer : MonoBehaviour
{
    public static BgmPlayer Instance { get; private set; }

    public List<AudioClip> musics;
    public AudioSource musicPlayer;
    
    public bool allowRepeatSameAudio = false;

    private int _lastPlayedIndex = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicPlayer = GetComponent<AudioSource>();
        LoadResources();
    }

    public void playRandomMusic()
    {
        if (musics == null || musics.Count == 0)
        {
            Debug.LogWarning("AudioClip列表为空，无法播放音频！");
            return;
        }

        int randomIndex = 0;
        if (allowRepeatSameAudio)
        {
            randomIndex = Random.Range(0, musics.Count);
        }
        else
        {
            if (musics.Count == 1)
            {
                randomIndex = 0;
            }
            else
            {
                do
                {
                    randomIndex = Random.Range(0, musics.Count);
                } while (randomIndex == _lastPlayedIndex);
            }
        }

        _lastPlayedIndex = randomIndex;
        AudioClip targetAudio = musics[randomIndex];

        musicPlayer.clip = targetAudio;
        musicPlayer.volume = 0.05f;
        musicPlayer.Play();

        Debug.Log($"正在播放：{targetAudio.name}");

        StartCoroutine(WaitForAudioFinishThenPlayNext());
    }

    private void LoadResources()
    {
        musics.Clear();
        try
        {
            Object[] audioObjects = Resources.LoadAll("Sound/Music", typeof(AudioClip));

            if (audioObjects == null)
            {
                Debug.Log("audioObjects null");
            }

            Debug.Log(audioObjects.Count());

            musics = audioObjects.Cast<AudioClip>().ToList();
            Debug.Log($"成功从Resources/{"Sound/Music"}目录加载到 {musics.Count} 个AudioClip");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"加载AudioClip失败：{e.Message}");
        }
    }

    IEnumerator WaitForAudioFinishThenPlayNext()
    {
        yield return new WaitForSeconds(musicPlayer.clip.length);
        playRandomMusic();
    }
}
