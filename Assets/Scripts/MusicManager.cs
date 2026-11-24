using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("곡 종료 후 재시작 대기 시간 (초 단위)")]
    public float restartDelay = 5f;

    public AudioSource AudioSource { get; private set; }
    public int RoundId { get; private set; } = 0;
    public int NextNoteIndex { get; set; } = 0;
    public bool IsWaitingForRestart { get; private set; } = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        AudioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (AudioSource != null)
        {
            AudioSource.loop = false;
            if (!AudioSource.isPlaying)
            {
                AudioSource.time = 0f;
                AudioSource.Play();
            }
        }
    }

    void Update()
    {
        if (AudioSource == null || IsWaitingForRestart) return;

        if (!AudioSource.isPlaying)
        {
            StartCoroutine(RestartAfterDelay());
        }
    }

    IEnumerator RestartAfterDelay()
    {
        IsWaitingForRestart = true;
        yield return new WaitForSeconds(restartDelay);

        RoundId++;
        NextNoteIndex = 0;
        AudioSource.time = 0f;
        AudioSource.Play();
        IsWaitingForRestart = false;
    }
}