using UnityEngine;
using System.Collections.Generic;

public class OsuSpawner : MonoBehaviour
{
    [Header("Settings")]
    public TextAsset osuFile;
    public GameObject ballPrefab;
    public AudioSource musicSource;

    [Header("리듬 싱크 조절 (초 단위)")]
    public float syncOffset = 0.4f;

    [Header("계란 위치 기준")]
    public float startX = 16.5f;
    public float ballSpeed = 6.5f;

    [Header("속도 조절")]
    [Range(0.2f, 3f)]
    public float playbackSpeed = 1f;
    public bool autoVarySpeed = false;
    public float minSpeed = 0.5f;
    public float maxSpeed = 2.0f;
    public float speedChangeCycle = 10f;

    public static float CurrentSpeed { get; private set; } = 1f;

    private List<float> spawnTimes = new List<float>();
    private int currentNoteIndex = 0;
    private float travelTime = 2.0f;
    private int lastRoundId = -1;

    void Start()
    {
        ParseOsuFile();

        if (MusicManager.Instance != null)
        {
            musicSource = MusicManager.Instance.AudioSource;
            lastRoundId = MusicManager.Instance.RoundId;
            currentNoteIndex = MusicManager.Instance.NextNoteIndex;
        }
    }

    void ParseOsuFile()
    {
        spawnTimes.Clear();
        if (osuFile == null) return;

        string[] lines = osuFile.text.Split('\n');
        bool isHitObject = false;

        foreach (string line in lines)
        {
            if (line.Trim() == "[HitObjects]")
            {
                isHitObject = true;
                continue;
            }

            if (isHitObject && !string.IsNullOrWhiteSpace(line))
            {
                string[] data = line.Split(',');
                if (data.Length >= 3 && float.TryParse(data[2], out float hitTimeMs))
                {
                    float hitTimeSec = hitTimeMs / 1000f;
                    float spawnTime = hitTimeSec - travelTime + syncOffset;
                    if (spawnTime >= 0f)
                    {
                        spawnTimes.Add(spawnTime);
                    }
                }
            }
        }
    }

    void SpawnBall(int noteIndex, float spawnX)
    {
        GameObject spawnedBall = Instantiate(ballPrefab);
        float zOffset = (noteIndex % 2 == 1) ? 0.3f : 0f;
        spawnedBall.transform.position += new Vector3(0f, 0f, zOffset);

        BallController ball = spawnedBall.GetComponent<BallController>();
        if (ball != null)
        {
            ball.SetStartX(spawnX);
        }
    }

    void UpdateSpeed()
    {
        if (autoVarySpeed)
        {
            float cycle = Mathf.Max(0.1f, speedChangeCycle);
            float t = (Mathf.Sin(Time.time * (2f * Mathf.PI / cycle)) + 1f) * 0.5f;
            CurrentSpeed = Mathf.Lerp(minSpeed, maxSpeed, t);
        }
        else
        {
            CurrentSpeed = playbackSpeed;
        }

        CurrentSpeed = Mathf.Max(0.05f, CurrentSpeed);
    }

    void Update()
    {
        if (MusicManager.Instance != null)
        {
            musicSource = MusicManager.Instance.AudioSource;

            if (lastRoundId == -1)
            {
                lastRoundId = MusicManager.Instance.RoundId;
                currentNoteIndex = MusicManager.Instance.NextNoteIndex;
            }

            if (MusicManager.Instance.IsWaitingForRestart) return;

            if (MusicManager.Instance.RoundId != lastRoundId)
            {
                lastRoundId = MusicManager.Instance.RoundId;
                currentNoteIndex = MusicManager.Instance.NextNoteIndex;
            }
        }

        if (musicSource == null || !musicSource.isPlaying) return;

        UpdateSpeed();
        musicSource.pitch = CurrentSpeed;

        float currentTime = musicSource.time;

        while (currentNoteIndex < spawnTimes.Count && currentTime >= spawnTimes[currentNoteIndex])
        {
            float elapsed = Mathf.Max(0f, currentTime - spawnTimes[currentNoteIndex]);
            float spawnX = startX - (elapsed * ballSpeed);

            if (spawnX > -9.5f)
            {
                SpawnBall(currentNoteIndex, spawnX);
            }

            currentNoteIndex++;

            if (MusicManager.Instance != null)
            {
                MusicManager.Instance.NextNoteIndex = currentNoteIndex;
            }
        }
    }
}