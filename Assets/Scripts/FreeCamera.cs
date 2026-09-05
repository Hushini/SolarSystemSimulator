using UnityEngine;

/*
Prawy przycisk + ruch = rozglądanie.
Wciśnięte kółko + ruch = swobodne przesuwanie.
Kółko = przybliżanie/oddalanie, R = powrót do pozycji startowej.
*/
public class FreeCamera : MonoBehaviour
{
    [Header("Rozglądanie (prawy przycisk)")]
    public float lookSpeed = 3f;
    public float minPitch = -89f;
    public float maxPitch = 89f;

    [Header("Przesuwanie (wciśnięte kółko)")]
    public float panSpeed = 0.05f;

    [Header("Przybliżanie / oddalanie (kółko)")]
    public float dollySpeed = 4f;

    [Header("Pozycja startowa")]
    public Vector3 startPosition = new Vector3(0f, 40f, -90f);

    private float yaw;
    private float pitch;
    private Quaternion startRotation;

    void Start()
    {
        Camera cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("Ten skrypt musi byc na Main Camera.");
            enabled = false;
            return;
        }

        cam.farClipPlane = 6000f;
        cam.nearClipPlane = 0.1f;

        transform.position = startPosition;
        transform.LookAt(Vector3.zero);   // na starcie patrzymy w stronę Słońca
        startRotation = transform.rotation;
        ReadAnglesFromTransform();
    }

    void Update()
    {
        HandleLook();
        HandlePan();
        HandleDolly();
        if (Input.GetKeyDown(KeyCode.R)) ResetView();
    }

    private void HandleLook()
    {
        if (Input.GetMouseButton(1)) // prawy przycisk myszy
        {
            yaw   += Input.GetAxis("Mouse X") * lookSpeed;
            pitch -= Input.GetAxis("Mouse Y") * lookSpeed;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }

    private void HandlePan()
    {
        if (Input.GetMouseButton(2)) // wciśnięte kółko (środkowy przycisk)
        {
            float dx = Input.GetAxis("Mouse X");
            float dy = Input.GetAxis("Mouse Y");
            Vector3 move = (-transform.right * dx - transform.up * dy)
                           * panSpeed * MoveScale();
            transform.position += move;
        }
    }

    private void HandleDolly()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
            transform.position += transform.forward * scroll * dollySpeed * MoveScale();
    }

    /*
        Skala ruchu zależna od odległości od Słońca — blisko planet kamera 
        porusza się wolniej i precyzyjniej, daleko w przestrzeni szybciej.
    */
    private float MoveScale()
    {
        float distanceFromSun = transform.position.magnitude;
        return Mathf.Clamp(distanceFromSun, 8f, 500f);
    }

    private void ResetView()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;
        ReadAnglesFromTransform();
    }

    private void ReadAnglesFromTransform()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
        if (pitch > 180f) pitch -= 360f;
    }
}