using UnityEngine;

public class convery_parts : MonoBehaviour
{
    [Header("위치 설정 (Z축 기준)")]
    public float spawnPosition = 16.5f;
    public float centerPosition = 3.5f;
    public float despawnPosition = -9.5f;

    [Header("속도 설정")]
    public float timeToCenter = 1.0f; // 스폰 위치에서 중앙까지 걸리는 시간(초)

    private float speed;

    void Start()
    {
        // 스폰 위치(16.5)에서 중앙(3.5)까지의 이동 거리 계산 (절댓값 적용)
        float distance = Mathf.Abs(spawnPosition - centerPosition);
        
        // 이동 속도 계산 (속도 = 거리 / 시간) -> 13 / 1 = 13
        speed = distance / timeToCenter/2;
    }

    void Update()
    {
        // 컨베이어 벨트 전체를 이동/회전시켜도 작동하도록 절대 좌표가 아닌 부모 기준 상대 좌표(localPosition) 사용
        Vector3 pos = transform.localPosition;

        // 16.5에서 -9.5로 값이 작아지는 방향이므로 마이너스 이동
        pos.x -= speed * Time.deltaTime;

        // 사라지는 위치(-9.5)를 넘어갔는지 체크
        if (pos.x <= despawnPosition)
        {
            // 프레임 이동 단위가 딱 떨어지지 않아 초과 이동(Overshoot)한 만큼을 계산
            float overshoot = despawnPosition - pos.x;
            
            // 초과한 만큼을 스폰 위치에서 빼주어 정확한 간격을 유지하며 재배치
            pos.x = spawnPosition - overshoot; 
        }

        // 계산된 위치를 실제 파츠에 적용
        transform.localPosition = pos;
    }
}