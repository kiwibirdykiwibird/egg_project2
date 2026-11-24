using UnityEngine;

public class rotation : MonoBehaviour
{
    // 인스펙터 창에서 수치를 바로 수정할 수 있도록 노출시킵니다.
    // 이전에 계산한 초당 164.96도의 회전 속도를 기본값으로 넣습니다.
    [SerializeField] private float rotationSpeed = 164.96f;

    void Update()
    {
        // 매 프레임마다 설정한 속도(rotationSpeed)에 맞춰 회전합니다.
        // 바퀴 모델이 만들어진 기준 축에 따라 Vector3.right(X축) 대신 
        // Vector3.forward(Z축)나 Vector3.up(Y축)을 사용해야 할 수 있습니다.
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}