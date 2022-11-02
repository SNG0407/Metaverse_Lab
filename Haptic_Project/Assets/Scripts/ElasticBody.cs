using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class ElasticBody : MonoBehaviour
{
    private Camera cam;
    [SerializeField] private HandControllerSO controllerSO;
    private Mesh mesh;
    private MeshCollider meshCollider;

    private Vector3[] initVertices, vertices, velocities, normals;

    private const float maxElasticity = 20;
    // 탄성
    [Range(0,maxElasticity)] [SerializeField] float elasticity = 5f;

    // 누르는 압력
    [Min(0)] [SerializeField] float power = 5f;

    // 감쇠
    [Min(0)] [SerializeField] float damping = 5f;

    // 압력지점 거리에 따른 감쇠
    [Min(0)] [SerializeField] float attenuation = 15f;

    private bool isDeformed = false;

    private float initVertexSqrMag;

    private void Start()
    {
        cam = Camera.main;

        mesh = GetComponent<MeshFilter>().mesh;
        meshCollider = GetComponent<MeshCollider>();

        velocities = Enumerable.Repeat(Vector3.zero, mesh.vertices.Length).ToArray();
        vertices = (Vector3[])mesh.vertices.Clone();
        initVertices = (Vector3[])mesh.vertices.Clone();
        normals = mesh.normals;

        // 구체이므로 정점거리 한 곳만 측정
        initVertexSqrMag = initVertices[0].sqrMagnitude;
    }

    private void Update()
    {
#if UNITY_EDITOR
        ProcessInput();
#endif

        Restore();
        Damping();
        UpdateVertex();
        if (isDeformed)
        {
            UpdateMesh();
        }

    }

    void ProcessInput()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButton(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == this.gameObject)
                {
                    // 직접 닿은 표면에 압력을 주기 위해서
                    // 충돌점으로부터 법선벡터 쪽으로 약간 올라간 좌표를 입력. 
                    const float hitPointOffset = 0.1f;
                    Vector3 point = hit.point + hit.normal * hitPointOffset;
                    Press(0, point);
                }
            }
        }
    }

    public void Press(int fingerId, Vector3 pos)
    {
        // world -> local
        Vector3 contactLocalPos = transform.InverseTransformPoint(pos);
        // Debug.Log($" contactPos : {contactPos}{{");

        int pressingVertexID = 0;
        float minDistance = float.MaxValue;
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 diff = (vertices[i] - contactLocalPos);
            float distance = diff.sqrMagnitude;
            if (distance < minDistance)
            {
                minDistance = distance;
                pressingVertexID = i;
            }

            Vector3 direction = diff.normalized;
            float velocity = power / Mathf.Pow(1 + distance * attenuation, 2);
            velocities[i] += direction * velocity * Time.deltaTime;
        }

        float pressure = (vertices[pressingVertexID] - initVertices[pressingVertexID]).sqrMagnitude 
                         * elasticity / (initVertexSqrMag * maxElasticity );
        controllerSO.SetFingerPressure(fingerId, pressure);
    }

    void Restore()
    {
        for (int i = 0; i < velocities.Length; i++)
        {
            velocities[i] -= (vertices[i] - initVertices[i]) * elasticity * Time.deltaTime;
        }
    }

    void Damping()
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            velocities[i] *= damping * Time.deltaTime;
        }
    }

    void UpdateVertex()
    {
        float diff = 0;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] += velocities[i];
            diff += velocities[i].sqrMagnitude;
        }

        diff *= Mathf.Pow(10, 8);
        // Debug.Log($"diff : {diff}");
        isDeformed = diff > 0.1f;
    }

<<<<<<< HEAD
=======
    void UpdateFingerPressure()
    {
        for (int fingerID = 0; fingerID < controllerSO.pressureRight.Length; fingerID++)
        {
            if (controllerSO.pressureRight[fingerID].isPress)
            {
                int vertexID = controllerSO.pressureRight[fingerID].vertexID;
                float pressure = (vertices[vertexID] - initVertices[vertexID]).sqrMagnitude /
                                 initVertexSqrMag;
                controllerSO.SetFingerPressure(fingerID, pressure);
            }
        }
    }
>>>>>>> c41ece9 (압력 측정방식 수정, UI 표시)

    void UpdateMesh()
    {
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        meshCollider.sharedMesh = mesh;
    }
}