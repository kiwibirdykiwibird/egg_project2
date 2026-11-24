using UnityEngine;
using UnityEngine.InputSystem;

public class PressController : MonoBehaviour
{
    private float initialY = 8.67f;
    private float bottomY = 1.02f;

    private float returnSpeed = 30f;
    private float squashDuration = (20f / 24f) / 6f; // 약 0.14초 (6배속 기준)

    [Header("Sound")]
    [SerializeField] private AudioClip boingSound; // 인스펙터에서 boing.wav 연결
    [Range(0f, 1f)]
    [SerializeField] private float boingVolume = 1f;

    private bool isDropping = false;
    private bool isSquashing = false;
    private bool isReturning = false;

    private float dropTimer = 0f;
    private float squashTimer = 0f;
    private Transform eggTrigger;
    private float yOffset;
    private Collider pressCol;
    private AudioSource audioSource;

    void Start()
    {
        transform.position = new Vector3(transform.position.x, initialY, transform.position.z);
        pressCol = GetComponentInChildren<Collider>();

        // AudioSource가 없으면 자동 추가하고 2D 사운드로 설정
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame
            && !isDropping && !isSquashing && !isReturning)
        {
            isDropping = true;
            dropTimer = 0f;
        }

        // 허공 하강
        if (isDropping)
        {
            dropTimer += Time.deltaTime;
            float progress = dropTimer / squashDuration;

            float currentY = Mathf.Lerp(initialY, bottomY, progress);
            transform.position = new Vector3(transform.position.x, currentY, transform.position.z);

            if (progress >= 1f || transform.position.y <= bottomY)
            {
                transform.position = new Vector3(transform.position.x, bottomY, transform.position.z);
                isDropping = false;
                isReturning = true;
            }
        }

        // 공에 닿았을 때의 찌그러짐 추적
        if (isSquashing)
        {
            if (eggTrigger != null)
            {
                float targetY = eggTrigger.position.y + yOffset;

                if (targetY < bottomY)
                {
                    targetY = bottomY;
                }

                transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
            }

            squashTimer += Time.deltaTime;
            if (squashTimer >= squashDuration)
            {
                isSquashing = false;
                isReturning = true;
                squashTimer = 0f;
            }
        }

        // 프레스 원위치 복귀
        if (isReturning)
        {
            transform.Translate(Vector3.up * returnSpeed * Time.deltaTime, Space.World);

            if (transform.position.y >= initialY)
            {
                transform.position = new Vector3(transform.position.x, initialY, transform.position.z);
                isReturning = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isDropping || !other.CompareTag("eggtag")) return;

        Collider targetCol = FindFrontmostEgg(other);
        if (targetCol != null)
        {
            StartSquash(targetCol);
        }
    }

    // 현재 프레스와 겹친 공들 중 아직 안 찌그러졌고 가장 앞(-X 방향)에 있는 공을 반환
    private Collider FindFrontmostEgg(Collider fallback)
    {
        Collider bestCol = null;
        float minX = float.MaxValue;

        if (pressCol != null)
        {
            Collider[] hits = Physics.OverlapBox(
                pressCol.bounds.center,
                pressCol.bounds.extents,
                Quaternion.identity,
                Physics.AllLayers,
                QueryTriggerInteraction.Collide
            );

            foreach (Collider col in hits)
            {
                if (!col.CompareTag("eggtag")) continue;

                BallController ball = col.GetComponentInParent<BallController>();
                if (ball == null || ball.IsSquashed) continue;

                if (ball.transform.position.x < minX)
                {
                    minX = ball.transform.position.x;
                    bestCol = col;
                }
            }
        }

        if (bestCol == null)
        {
            BallController fallbackBall = fallback.GetComponentInParent<BallController>();
            if (fallbackBall != null && !fallbackBall.IsSquashed)
            {
                bestCol = fallback;
            }
        }

        return bestCol;
    }

    private void StartSquash(Collider target)
    {
        isDropping = false;
        isSquashing = true;
        squashTimer = 0f;
        eggTrigger = target.transform;

        yOffset = transform.position.y - eggTrigger.position.y;

        // 프레스와 공이 만났을 때 효과음 재생
        if (boingSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(boingSound, boingVolume);
        }

        BallController ball = target.GetComponentInParent<BallController>();
        if (ball != null)
        {
            ball.SquashEgg();
        }
    }
}