using UnityEngine;

public class CameraRotate : MonoBehaviour
{
    public float mouseSensitivity = 2f;
    public Transform cameraTarget;

    float rotationX = 0f;
    float rotationY = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        rotationY += mouseX;
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f); // giới hạn góc ngẩng đầu

        transform.rotation = Quaternion.Euler(0, rotationY, 0);
        cameraTarget.localRotation = Quaternion.Euler(rotationX, 0, 0); // Xoay theo trục dọc
    }
}
