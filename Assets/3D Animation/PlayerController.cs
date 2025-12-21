using UnityEngine;


public class PlayerController : MonoBehaviour
{
    [Header("Animator Params")]
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string jumpTrigger = "Jump";

    [Header("Blend Tree Values (match your thresholds)")]
    [Range(0f, 1f)] public float walkSpeed = 0.5f;  // e.g. threshold for walk
    [Range(0f, 1f)] public float runSpeed  = 1.0f;  // e.g. threshold for run

    [Header("Acceleration")]
    public float accelTime = 0.25f;   // time to reach target
    public float decelTime = 0.20f;   // time to return to 0
    public float holdToRunTime = 0.35f; // hold W this long to reach run target

    [Header("Input")]
    public KeyCode forwardKey = KeyCode.W;
    public KeyCode jumpKey = KeyCode.Space;

    private Animator anim;
    private float currentSpeed01;
    private float wHoldTimer;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        UpdateSpeed();
        UpdateJump();
    }

    private void UpdateSpeed()
    {
        bool wHeld = Input.GetKey(forwardKey);

        float target = 0f;

        if (wHeld)
        {
            wHoldTimer += Time.deltaTime;
            target = (wHoldTimer >= holdToRunTime) ? runSpeed : walkSpeed;

            float accelRate = (accelTime <= 0.0001f) ? 9999f : (1f / accelTime);
            currentSpeed01 = Mathf.MoveTowards(currentSpeed01, target, accelRate * Time.deltaTime);
        }
        else
        {
            wHoldTimer = 0f;

            float decelRate = (decelTime <= 0.0001f) ? 9999f : (1f / decelTime);
            currentSpeed01 = Mathf.MoveTowards(currentSpeed01, 0f, decelRate * Time.deltaTime);
        }

        anim.SetFloat(speedParam, currentSpeed01);
    }

    private void UpdateJump()
    {
        if (Input.GetKeyDown(jumpKey))
        {
            anim.SetTrigger(jumpTrigger);
        }
    }
}