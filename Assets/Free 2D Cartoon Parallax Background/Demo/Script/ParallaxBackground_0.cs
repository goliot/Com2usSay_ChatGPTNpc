using UnityEngine;

public class ParallaxBackground_0 : MonoBehaviour
{
    [Header("Layer Setting")]
    public float[] Layer_Speed = new float[7]; // 0 ~ 1 (멀수록 느리게)
    public GameObject[] Layer_Objects = new GameObject[7]; // 각 레이어의 그룹 (2개 이상의 타일 필요)

    private Transform _camera;
    private Vector3 _previousCameraPos;
    private float[] spriteWidth; // 각 레이어의 타일 너비

    void Start()
    {
        _camera = Camera.main.transform;
        _previousCameraPos = _camera.position;

        spriteWidth = new float[Layer_Objects.Length];

        for (int i = 0; i < Layer_Objects.Length; i++)
        {
            if (Layer_Objects[i] == null) continue;

            SpriteRenderer sr = Layer_Objects[i].GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
                spriteWidth[i] = sr.bounds.size.x;
        }
    }

    void LateUpdate()
    {
        Vector3 deltaMovement = _camera.position - _previousCameraPos;

        for (int i = 0; i < Layer_Objects.Length; i++)
        {
            if (Layer_Objects[i] == null) continue;

            // 이동
            Vector3 layerMove = new Vector3(deltaMovement.x * Layer_Speed[i], 0f, 0f);
            Layer_Objects[i].transform.position += layerMove;

            // 무한 스크롤 구현
            Transform[] children = Layer_Objects[i].GetComponentsInChildren<Transform>();

            foreach (Transform child in children)
            {
                if (child == Layer_Objects[i]) continue;

                float camX = _camera.position.x;
                float childX = child.position.x;

                if (camX - childX >= spriteWidth[i])
                {
                    child.position += Vector3.right * spriteWidth[i] * 2f;
                }
                else if (childX - camX >= spriteWidth[i])
                {
                    child.position -= Vector3.right * spriteWidth[i] * 2f;
                }
            }
        }

        _previousCameraPos = _camera.position;
    }
}
