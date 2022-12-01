using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [Header("Music Settings")]
    [SerializeField] List<AudioClip> musicClips;
    [SerializeField] [Range(0, 1)] float musicVolume;
    public float interMusicDelay = 1f;

    //Music wobble settings
    [Header("Special Effects")]
    public bool musicWobbleOn;
    public float musicWobbleLength = 2f;
    public float musicWobbleSpeed = 0.01f;

    private float timeToPitchUp;
    private bool pitchIsUp;

    public bool alwaysWowWow;
    private Coroutine currentSongCoroutine;


    [SerializeField] public AudioSource myAudioSource;

    private int songIndex;
    // Start is called before the first frame update
    void Awake()
    {
        SetUpSingleton();
        //Debug.Log("Has set up singleton");
    }
    private void SetUpSingleton()
    {
        if (FindObjectsOfType(GetType()).Length > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }
    void Start()
    {
        PlayRandomSong();
    }
    void Update()
    {
        if (pitchIsUp == false && timeToPitchUp < Time.time)
        {
            StartCoroutine(PitchUp());

        }
    }
    public void MusicWobbleOnHit()
    {
        if (musicWobbleOn)
        {
            timeToPitchUp = Time.time + musicWobbleLength;
            pitchIsUp = false;
            StartCoroutine(PitchDown());
        }

    }

    // Update is called once per frame
    public IEnumerator PitchDown()
    {
        while (myAudioSource.pitch >= 0.8f)
        {
            myAudioSource.pitch -= 0.01f;
            yield return new WaitForSeconds(musicWobbleSpeed);
        }
    }
    public IEnumerator PitchUp()
    {
        while (myAudioSource.pitch < 1f)
        {
            myAudioSource.pitch += 0.01f;
            yield return new WaitForSeconds(musicWobbleSpeed);
        }
        pitchIsUp = true;
    }
    void PlayRandomSong()
    {
        if (alwaysWowWow)
        {
            songIndex = 3;
        }
        else
        {
            int newIndex = Random.Range(0, musicClips.Count);

            while (songIndex == newIndex)
            {
                newIndex = Random.Range(0, musicClips.Count);
            }
            songIndex = newIndex;
        }

        currentSongCoroutine = StartCoroutine(WaitForTheClipToEnd(songIndex));
    }
    public void PlayThisSong(int songToPlay)
    {
        StopCoroutine(currentSongCoroutine);
        if (songToPlay <= (musicClips.Count - 1) && songToPlay >= 0)
        {
            currentSongCoroutine = StartCoroutine(WaitForTheClipToEnd(songToPlay));
        }
    }
    private IEnumerator WaitForTheClipToEnd(int currentClip)
    {
        float currentSongLength = musicClips[currentClip].length;
        myAudioSource.PlayOneShot(musicClips[currentClip], musicVolume);

        yield return new WaitForSeconds(currentSongLength + interMusicDelay);
        PlayRandomSong();
    }
    public void SkipSongToNext()
    {
        StopCoroutine(currentSongCoroutine);
        myAudioSource.Stop();
        songIndex++;
        if (songIndex > musicClips.Count - 1)
        {
            songIndex = 0;
        }
        currentSongCoroutine = StartCoroutine(WaitForTheClipToEnd(songIndex));
    }
    public void SkipSongToPrevious()
    {
        StopCoroutine(currentSongCoroutine);
        myAudioSource.Stop();
        songIndex--;
        if (songIndex < 0)
        {
            songIndex = musicClips.Count - 1;
        }
        currentSongCoroutine = StartCoroutine(WaitForTheClipToEnd(songIndex));
    }
}