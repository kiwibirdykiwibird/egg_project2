using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneSwitcher : MonoBehaviour
{
    [Header("씬 전환 주기 (초 단위)")]
    public float switchInterval = 60f; // 60초 = 1분

    void Start()
    {
        StartCoroutine(SwitchSceneRoutine());
    }

    IEnumerator SwitchSceneRoutine()
    {
        yield return new WaitForSeconds(switchInterval);

        int totalScenes = SceneManager.sceneCountInBuildSettings;
        if (totalScenes < 2)
        {
            Debug.LogWarning("Build Profiles의 Scene List에 씬이 2개 이상 등록되어 있어야 합니다.");
            yield break;
        }

        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = (currentIndex + 1) % totalScenes;
        SceneManager.LoadScene(nextIndex);
    }
}