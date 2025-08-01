using UnityEngine;
using Photon.Pun;
using Unity.Cinemachine;

public class CinemachineTargetAssigner : MonoBehaviourPun
{
    [Header("Camera follow target")]
    public Transform followTarget;

    void Start()
    {
        if (!photonView.IsMine) return;

        var cineCam = FindObjectOfType<CinemachineCamera>();
        if (cineCam != null && followTarget != null)
        {
            // Gán trực tiếp vào virtual camera (không phải component con)
            cineCam.Follow = followTarget;
            cineCam.LookAt = followTarget;

            // Debug kiểm tra
            Debug.Log("Đã gán Follow và LookAt cho CinemachineCamera!");
        }
        else
        {
            Debug.LogWarning("Không tìm thấy CinemachineCamera hoặc FollowTarget.");
        }
    }
}
