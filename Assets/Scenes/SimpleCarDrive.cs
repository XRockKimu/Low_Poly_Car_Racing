using UnityEngine;

public class SimpleCarDrive : MonoBehaviour
{
    public float moveSpeed = 12f;
    public float turnSpeed = 60f;

    [Header("Front Wheels (for steering visual only)")]
    public Transform wheelFrontLeft;
    public Transform wheelFrontRight;

    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float move = Input.GetAxis("Vertical");     // W / S
        float turn = Input.GetAxis("Horizontal");   // A / D

        // ✅ MOVE FORWARD ONLY
        Vector3 movement = -transform.forward * move * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);

        // ✅ TURN ONLY WHEN MOVING (realistic)
        if (move != 0)
        {
            Quaternion turnOffset = Quaternion.Euler(0f, turn * turnSpeed * Time.fixedDeltaTime, 0f);
            rb.MoveRotation(rb.rotation * turnOffset);
        }

        // ✅ FRONT WHEEL STEERING VISUAL
        float steerAngle = turn * 30f;

        if (wheelFrontLeft && wheelFrontRight)
        {
            wheelFrontLeft.localRotation = Quaternion.Euler(0, steerAngle, 0);
            wheelFrontRight.localRotation = Quaternion.Euler(0, steerAngle, 0);
        }
    }
}
