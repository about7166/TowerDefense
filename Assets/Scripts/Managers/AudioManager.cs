using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("BGM設定")]
    [SerializeField] private bool playBGM;
    [SerializeField] private AudioSource[] bgm;
    private int currentBGMIndex;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

        InvokeRepeating(nameof(PlayMusicIfNeed), 0, 2);//這裡的2是指間隔2秒後播放下一首音樂
    }

    private void PlayMusicIfNeed()
    {
        if (bgm.Length <= 0)
        {
            Debug.Log("你試著播放音樂，但你沒有分配任何曲目!");
            return;
        }

        if (playBGM == false)
            return;

        if (bgm[currentBGMIndex].isPlaying == false)
            PlayRandomBGM();
    }

    [ContextMenu("隨機播放音樂")]
    public void PlayRandomBGM()
    {
        currentBGMIndex = Random.Range(0, bgm.Length);
        PlayBGM(currentBGMIndex);
    }

    public void PlayBGM(int bgmToPlay)
    {
        if (bgm.Length <= 0)
        {
            Debug.Log("你試著播放音樂，但你沒有分配任何曲目!");
            return;
        }

        StopAllBGM();

        currentBGMIndex = bgmToPlay;
        bgm[bgmToPlay].Play();
    }

    [ContextMenu("停止撥放音樂")]
    public void StopAllBGM()
    {
        for (int i = 0; i < bgm.Length; i++)
        {
            bgm[i].Stop();
        }
    }

    public void PlaySfX()
    {
        Debug.Log("播音樂了!");
    }
}
