using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetController : MonoBehaviour
{
    
    public CameraController mainCamera;
    public Transform jetMesh;
    public GameObject[] landingGears;

    public float engineThrust = 10000f;
    public float pitchSpeed = 30f;
    public float rollSpeed = 45f;
    public float yawSpeed = 25f;
    public float autoTurnAngle = 30f;

    public bool startInAir;
    public bool autoTakeOff;
    public bool autoLevel;

    private Camera cam;
    private Rigidbody rb;

    private float thrust;
    private float pitch;
    private float roll;
    private float yaw;
    private bool enableMouseControls;

    private bool landingGearsRetracted;

    internal float speed;
    internal float height;
    internal float throttle { get { return thrust; } }
    internal bool showCrosshairs;
    internal Vector3 crosshairPosition;

    private const float mToKm = 3.6f;
    private const float kmToKnots = 0.539f;
    private const float aerodynamicEffect = 0.1f;

    private void Awake()
    {
        cam = Camera.main;

        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        if (rb.mass == 1)
        {
            rb.mass = 20000;
            rb.drag = 0.075f;
            rb.angularDrag = 0.05f;
        }
    }

    private void Start()
    {
        if (startInAir)
        {
            transform.position = new Vector3(0, 20000, 0);
            thrust = 100f;
            rb.AddForce(transform.forward * 500f, ForceMode.VelocityChange);
        }

        if (autoTakeOff)
            thrust = 100f;
    }

    void Update()
    {
        //Clear out old values
        pitch = 0f;
        roll = 0f;
        yaw = 0f;

        //Update control surfaces
        if (Input.GetKey(KeyCode.Q)) yaw = -1f;
        if (Input.GetKey(KeyCode.E)) yaw = 1f;

        if (Input.GetKey(KeyCode.A)) roll = 1f;
        if (Input.GetKey(KeyCode.D)) roll = -1f;

        if (Input.GetKey(KeyCode.W)) pitch = 1f;
        if (Input.GetKey(KeyCode.S)) pitch = -1f;

        CheckAutoTakeoff();
        UpdateThrottle();
        UpdateCamera();
        if (enableMouseControls) CheckMouseControls();

        //todo: update our height using a raycast
        height = transform.position.y - 1f;

        if (height > 5 && !landingGearsRetracted)
            RetractLandingGears();

        if (landingGearsRetracted && !enableMouseControls)
            SetupMouseControls();
    }

    void RetractLandingGears()
    {
        landingGearsRetracted = true;
        for (int i = 0; i < landingGears.Length; i++)
        {
            landingGears[i].SetActive(false);
        }
    }

    void CheckAutoTakeoff()
    {
        if (!autoTakeOff || landingGearsRetracted) return;
        if (rb.velocity.magnitude > 100f)
            pitch = -1f;
    }

    void UpdateThrottle()
    {
        if (Input.GetKeyDown(KeyCode.LeftBracket)) thrust = 30f;
        if (Input.GetKeyDown(KeyCode.RightBracket)) thrust = 60f;
        if (Input.GetKeyDown(KeyCode.Backspace)) thrust = 100f;
        if (Input.GetKeyDown(KeyCode.Backslash)) thrust = 0f;

        if (Input.GetKey(KeyCode.KeypadPlus)) thrust += 10f;
        if (Input.GetKey(KeyCode.KeypadMinus)) thrust -= -10f;

        thrust = Mathf.Clamp(thrust, 0f, 100f);
    }

    void UpdateCamera()
    {
        mainCamera.updatePosition(Input.GetAxisRaw("Mouse X"), -Input.GetAxisRaw("Mouse Y"));
    }

    void CheckMouseControls()
    {
        var localTarget = transform.InverseTransformDirection(cam.transform.forward).normalized * 5f;
        var targetRollAngle = Mathf.Lerp(0f, autoTurnAngle, Mathf.Abs(localTarget.x));
        if (localTarget.x > 0f) targetRollAngle *= -1f;

        var rollAngle = FindAngle(jetMesh.transform.localEulerAngles.z);
        var newAngle = targetRollAngle - rollAngle;

        pitch = -Mathf.Clamp(localTarget.y, -1f, 1f);
        roll = Mathf.Clamp(newAngle, -1f, 1f);
        yaw = Mathf.Clamp(localTarget.x, -1f, 1f);
    }

    float FindAngle(float v)
    {
        if (v > 180f) v -= 360f;
        return v;
    }

    void SetupMouseControls()
    {
        showCrosshairs = true;
        enableMouseControls = true;
    }

    private void FixedUpdate()
    {
        transform.RotateAround(transform.position, transform.up, yaw * Time.fixedDeltaTime * yawSpeed);     //Yaw

        if (height > 2f)
            jetMesh.transform.RotateAround(jetMesh.transform.position, jetMesh.transform.forward, roll * Time.fixedDeltaTime * rollSpeed);     //Roll

        //if (rb.velocity.magnitude > 100f)
            transform.RotateAround(transform.position, transform.right, pitch * Time.fixedDeltaTime * pitchSpeed);     //Pitch


        //Auto level the plane
        if (autoLevel && landingGearsRetracted)
        {
            var rotateSpeed = Mathf.Clamp(transform.right.y, -1f, 1f) * -1f;

            if (Mathf.Abs(pitch) > 0.1f)
                transform.RotateAround(transform.position, transform.forward, rotateSpeed);
        }

        var localVelocity = transform.InverseTransformDirection(rb.velocity);
        var localSpeed = Mathf.Max(0, localVelocity.z);
        speed = (localSpeed * mToKm) * kmToKnots;

        //Borrowed from Unity Standard Assets
        var aerofactor = Vector3.Dot(transform.forward, rb.velocity.normalized);
        aerofactor *= aerofactor;
        rb.velocity = Vector3.Lerp(rb.velocity, transform.forward * localSpeed, aerofactor * localSpeed * aerodynamicEffect * Time.fixedDeltaTime);

        rb.AddForce((thrust * engineThrust) * transform.forward);
    }

    private void LateUpdate()
    {
        if (!enableMouseControls) return;
        crosshairPosition = cam.WorldToScreenPoint(transform.position + (transform.forward * 500f));
    }
}
