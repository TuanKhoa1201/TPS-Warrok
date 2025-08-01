// using UnityEngine;
// using System.Collections;
// using Photon.Pun;

// [RequireComponent(typeof(CharacterController))]
// [RequireComponent(typeof(PhotonView))]
// public class PlayerControllerr : MonoBehaviourPun
// {
//     [Header("Movement Settings")]
//     public float moveSpeed = 5f;
//     public float runMultiplier = 1.8f;
//     public float jumpHeight = 2.5f;
//     public float gravity = -9.81f;
//     public float rotationSmoothTime = 0.25f;

//     [Header("References")]
//     public Animator animator;
//     public LineRenderer lineRenderer;
//     public Transform shootPoint;

//     private CharacterController controller;
//     private Transform cameraTransform;
//     private Vector3 velocity;
//     private bool isGrounded;
//     private float rotationVelocity;

//     void Start()
//     {
//         controller = GetComponent<CharacterController>();
//         if (animator == null) animator = GetComponent<Animator>();

//         if (!photonView.IsMine)
//         {
//             enabled = false;
//             return;
//         }

//         cameraTransform = Camera.main?.transform;

//         // Chuột hiển thị
//         Cursor.lockState = CursorLockMode.None;
//         Cursor.visible = true;

//         if (lineRenderer != null)
//             lineRenderer.enabled = false;

//         GunRaycast gun = GetComponentInChildren<GunRaycast>();
//         if (gun != null)
//             gun.playerPhotonView = photonView;

//         if (GameManager.instance != null)
//             GameManager.instance.SetupLives(3);
//     }

//     void Update()
//     {
//         if (!photonView.IsMine) return;

//         MovePlayer();
//         HandleJump();
//         HandleShoot();
//     }

//     void MovePlayer()
//     {
//         isGrounded = controller.isGrounded;

//         if (isGrounded && velocity.y < 0)
//             velocity.y = -2f;

//         float horizontal = Input.GetAxis("Horizontal");
//         float vertical = Input.GetAxis("Vertical");
//         bool isRunning = Input.GetKey(KeyCode.LeftShift);

//         Vector3 inputDir = new Vector3(horizontal, 0f, vertical);
//         float inputMagnitude = Mathf.Clamp01(inputDir.magnitude);
//         float currentSpeed = isRunning ? moveSpeed * runMultiplier : moveSpeed;

//         if (cameraTransform == null) return;

//         // Tính toán hướng di chuyển dựa theo camera
//         Vector3 camForward = cameraTransform.forward;
//         Vector3 camRight = cameraTransform.right;
//         camForward.y = 0f;
//         camRight.y = 0f;
//         camForward.Normalize();
//         camRight.Normalize();

//         Vector3 moveDir = camForward * vertical + camRight * horizontal;
//         moveDir.Normalize();

//         // Quay mặt theo hướng di chuyển
//         if (moveDir.magnitude > 0.1f)
//         {
//             float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
//             float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationVelocity, rotationSmoothTime);
//             transform.rotation = Quaternion.Euler(0f, angle, 0f);
//         }

//         controller.Move(moveDir * currentSpeed * Time.deltaTime);
//         velocity.y += gravity * Time.deltaTime;
//         controller.Move(velocity * Time.deltaTime);

//         // 🎯 Gửi thông số cho Blend Tree
//         animator.SetFloat("Horizontal", horizontal);
//         animator.SetFloat("Vertical", vertical);
//         animator.SetBool("IsRunning", isRunning);
//     }

//     void HandleJump()
//     {
//         if (Input.GetButtonDown("Jump") && isGrounded)
//         {
//             velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
//             animator.SetTrigger("Jump");
//         }
//     }

//     void HandleShoot()
//     {
//         if (Input.GetMouseButtonDown(0))
//         {
//             animator.SetTrigger("Shoot");

//             if (cameraTransform == null) return;

//             Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
//             RaycastHit hit;
//             Vector3 hitPoint = ray.origin + ray.direction * 100f;

//             if (Physics.Raycast(ray, out hit, 100f))
//             {
//                 hitPoint = hit.point;

//                 if (hit.collider.CompareTag("Enemy"))
//                 {
//                     EnemyAI enemy = hit.collider.GetComponentInParent<EnemyAI>();
//                     if (enemy != null)
//                     {
//                         PhotonView enemyView = enemy.GetComponent<PhotonView>();
//                         if (enemyView != null)
//                         {
//                             int killerID = photonView.OwnerActorNr;
//                             enemyView.RPC("TakeHit", RpcTarget.MasterClient, killerID);
//                         }
//                     }
//                 }
//             }

//             if (lineRenderer != null && shootPoint != null)
//                 StartCoroutine(ShowRay(shootPoint.position, hitPoint));
//         }
//     }

//     IEnumerator ShowRay(Vector3 start, Vector3 end)
//     {
//         lineRenderer.SetPosition(0, start);
//         lineRenderer.SetPosition(1, end);
//         lineRenderer.enabled = true;

//         yield return new WaitForSeconds(0.2f);

//         lineRenderer.enabled = false;
//     }

//     private void OnApplicationFocus(bool focus)
//     {
//         if (!photonView.IsMine) return;

//         Cursor.lockState = CursorLockMode.None;
//         Cursor.visible = true;
//     }
// }






using UnityEngine;
using System.Collections;
using Photon.Pun;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PhotonView))]
public class PlayerControllerr : MonoBehaviourPun
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float runMultiplier = 2.5f;
    public float jumpHeight = 2.5f;
    public float gravity = -20f;
    public float rotationSmoothTime = 0.3f;

    [Header("References")]
    public Animator animator;
    public Transform cameraHolder;
    public LineRenderer lineRenderer;

    [Header("Shoot Effect Settings")]
    public float lineDuration = 0.2f;

    [Header("Shooting Settings")]
    public Transform shootPoint;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float rotationVelocity;
    private Camera playerCam;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponent<Animator>();

        if (photonView.IsMine)
        {
            Camera oldCam = Camera.main;
            if (oldCam != null) Destroy(oldCam.gameObject);

            GameObject camObj = new GameObject("PlayerCamera");
            camObj.tag = "MainCamera";
            camObj.transform.SetParent(cameraHolder != null ? cameraHolder : transform);
            camObj.transform.localPosition = new Vector3(0, 8f, -8f);
            camObj.transform.localRotation = Quaternion.Euler(30f, 0f, 0f);

            playerCam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }

        if (lineRenderer != null)
            lineRenderer.enabled = false;

        GunRaycast gun = GetComponentInChildren<GunRaycast>();
        if (gun != null)
            gun.playerPhotonView = photonView;

        if (photonView.IsMine && GameManager.instance != null)
        {
            GameManager.instance.SetupLives(3);
        }
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        MovePlayer();
        HandleJump();
        HandleShoot();
    }

    void MovePlayer()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? moveSpeed * runMultiplier : moveSpeed;

        if (playerCam == null) return;

        Vector3 camForward = playerCam.transform.forward;
        Vector3 camRight = playerCam.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camForward * verticalInput + camRight * horizontalInput;
        Vector3 moveDirNormalized = moveDir.normalized;

        if (moveDirNormalized.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }

        controller.Move(moveDirNormalized * currentSpeed * Time.deltaTime);
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // 🎯 Gửi thông số cho blend tree
        Vector3 localVelocity = transform.InverseTransformDirection(moveDirNormalized);
        animator.SetFloat("Horizontal", localVelocity.x, 0.1f, Time.deltaTime);
        animator.SetFloat("Vertical", localVelocity.z, 0.1f, Time.deltaTime);
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("Jump");
        }
    }

    void HandleShoot()
    {
        if (Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger("Shoot");

            if (playerCam == null) return;

            Ray ray = playerCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Vector3 hitPoint = ray.origin + ray.direction * 100f;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                hitPoint = hit.point;

                if (hit.collider.CompareTag("Enemy"))
                {
                    EnemyAI enemy = hit.collider.GetComponentInParent<EnemyAI>();
                    if (enemy != null)
                    {
                        PhotonView enemyView = enemy.GetComponent<PhotonView>();
                        if (enemyView != null)
                        {
                            int killerID = photonView.OwnerActorNr;
                            enemyView.RPC("TakeHit", RpcTarget.MasterClient, killerID);
                        }
                    }
                }
            }

            if (lineRenderer != null && shootPoint != null)
                StartCoroutine(ShowRay(shootPoint.position, hitPoint));
        }
    }

    IEnumerator ShowRay(Vector3 start, Vector3 end)
    {
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        lineRenderer.enabled = true;

        yield return new WaitForSeconds(lineDuration);
        lineRenderer.enabled = false;
    }
}
