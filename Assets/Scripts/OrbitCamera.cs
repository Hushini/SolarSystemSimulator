using UnityEngine;

/*
    Kamera orbitująca wokół punktu docelowego.
    Prawy przycisk myszy = obrót, kółko = zoom,
    WASD/strzałki = przesuwanie punktu, R = reset widoku.
*/
[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{
    [Header("Punkt docelowy")]
    public Vector3 focusPoint = Vector3.zero;

    [Header("Odległość / zoom")]
    public float distance = 100f;
    public float minDistance = 2f;
    public float maxDistance = 1000f;
    public float zoomSpeed = 80f;

    [Header("Obrót")]
    public float rotationSpeed = 200f;
    public float minPitch = -85f;
    public float maxPitch = 85f;
    public float startYaw = 0f;
    public float startPitch = 35f;

    [Header("Przesuwanie")]
    public float panSpeed = 30f;

    private float yaw;
    private float pitch;
    private Vector3 defaultFocus;
    private float defaultDistance;

    void Start()
    {
        yaw = startYaw;
        pitch = startPitch;
        defaultFocus = focusPoint;
        defaultDistance = distance;

        Camera cam = GetComponent<Camera>();
        cam.farClipPlane = 5000f;
        cam.nearClipPlane = 0.1f;

        ApplyTransform();
    }

    void Update()
    {
        HandleRotation();
        HandleZoom();
        HandlePan();
        HandleReset();
        ApplyTransform();
    }

    private void HandleRotation()
    {
        if (Input.GetMouseButton(1)) // prawy przycisk myszy
        {
            yaw   += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            pitch -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            // zoom proporcjonalny do odległości
            distance -= scroll * zoomSpeed * (distance / 100f);
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    private void HandlePan()
    {
        float h = Input.GetAxis("Horizontal"); // A/D, strzałki
        float v = Input.GetAxis("Vertical");   // W/S, strzałki
        if (Mathf.Abs(h) > 0.0001f || Mathf.Abs(v) > 0.0001f)
        {
            float speed = panSpeed * (distance / 100f);
            Vector3 move = transform.right * h + transform.up * v;
            focusPoint += move * speed * Time.deltaTime;
        }
    }

    private void HandleReset()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            focusPoint = defaultFocus;
            distance = defaultDistance;
            yaw = startYaw;
            pitch = startPitch;
        }
    }

    private void ApplyTransform()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 position = focusPoint - rotation * Vector3.forward * distance;
        transform.SetPositionAndRotation(position, rotation);
    }

    // Ustawia nowy punkt, wokół którego orbituje kamera (pomocniczy).
    public void SetFocus(Vector3 point)
    {
        focusPoint = point;
    }
}