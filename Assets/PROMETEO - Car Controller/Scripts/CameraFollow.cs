using UnityEngine;

public class CameraFollow1 : MonoBehaviour
{
    public Transform carTransform;

    [Header("Camera Position")]
    public float distanceBehind = 8f;   // How far behind the car
    public float height = 3f;            // Camera height

    [Header("Camera Smoothness")]
    [Range(1f, 15f)]
    public float followSpeed = 6f;

    [Range(1f, 15f)]
    public float lookSpeed = 8f;

    void LateUpdate()
    {
        if (!carTransform) return;

        // Desired camera position (behind the car)
        Vector3 targetPosition =
            carTransform.position
            - carTransform.forward * distanceBehind
            + Vector3.up * height;

        // Smooth movement
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );

        // Smooth look at car
        Quaternion targetRotation = Quaternion.LookRotation(
            carTransform.position - transform.position,
            Vector3.up
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            lookSpeed * Time.deltaTime
        );
    }
}
