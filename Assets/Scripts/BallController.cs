using UnityEngine;
using System.Collections;

public class BallController : MonoBehaviour
{
    private float speed = 6.5f;
    private float startX = 16.5f;
    private Animator animator;
    private bool isSquashed = false;

    [Header("도장(PNG) 설정")]
    public SpriteRenderer stampRenderer;
    public Sprite[] stampSprites;
    public float stampDelay = 0.15f;
    public float targetStampSize = 1.5f;

    public bool IsSquashed => isSquashed;

    void Awake()
    {
        // 씬이 바뀌어도 생성된 계란(찌그러진 상태 + 도장 포함)을 그대로 유지
        DontDestroyOnLoad(gameObject);
    }

    public void SetStartX(float x)
    {
        startX = x;
        transform.position = new Vector3(startX, transform.position.y, transform.position.z);
    }

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        transform.position = new Vector3(startX, transform.position.y, transform.position.z);
        if (animator != null) animator.Play("tall_normal");

        if (stampRenderer != null)
        {
            stampRenderer.enabled = false;
        }
    }

    void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime, Space.World);
        // 왼쪽 끝(-9.5)에 도달하면 기존처럼 자동 삭제되므로 메모리가 쌓이지 않음
        if (transform.position.x <= -9.5f) Destroy(gameObject);
    }

    public void SquashEgg()
    {
        if (!isSquashed)
        {
            isSquashed = true;
            if (animator != null) animator.Play("tall_sqush");
            StartCoroutine(ShowStampAfterDelay());
        }
    }

    IEnumerator ShowStampAfterDelay()
    {
        if (stampDelay > 0f)
        {
            yield return new WaitForSeconds(stampDelay);
        }

        if (stampRenderer != null && stampSprites != null && stampSprites.Length > 0)
        {
            int randomIndex = Random.Range(0, stampSprites.Length);
            Sprite selected = stampSprites[randomIndex];
            stampRenderer.sprite = selected;

            Vector2 spriteSize = selected.bounds.size;
            float maxSide = Mathf.Max(spriteSize.x, spriteSize.y);
            if (maxSide > 0f)
            {
                float scaleFactor = targetStampSize / maxSide;
                stampRenderer.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
            }

            stampRenderer.enabled = true;
        }
    }
}