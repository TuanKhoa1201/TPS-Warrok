using UnityEngine;
using Photon.Pun;
using System.Collections;

public class GunRaycast : MonoBehaviour
{
    [Header("References")]
    public Transform shootPoint;
    public LineRenderer lineRenderer;
    public LayerMask hitLayers;
    public PhotonView playerPhotonView;

    [Header("Settings")]
    public float range = 100f;
    public float shootRate = 0.3f;
    public float lineDuration = 0.5f;

    [Header("Optional Effects")]
    public AudioSource gunAudio;
    public Light muzzleFlash;

    private float shootTimer;
    private Camera mainCam;

    void Start()
    {
        if (playerPhotonView != null && playerPhotonView.IsMine)
        {
            mainCam = Camera.main;
        }
    }

    void Update()
    {
        if (playerPhotonView == null || !playerPhotonView.IsMine) return;

        shootTimer += Time.deltaTime;

        if (Input.GetMouseButtonDown(0) && shootTimer >= shootRate)
        {
            Shoot();
            shootTimer = 0f;
        }
    }

    public void Shoot()
    {
        if (shootPoint == null || mainCam == null || !playerPhotonView.IsMine) return;

        // 👉 Bắn theo hướng chuột
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Vector3 endPoint = ray.origin + ray.direction * range;

        if (Physics.Raycast(ray, out hit, range, hitLayers))
        {
            endPoint = hit.point;

            if (hit.collider.CompareTag("Enemy"))
            {
                EnemyAI enemy = hit.collider.GetComponentInParent<EnemyAI>();
                if (enemy != null)
                {
                    PhotonView enemyView = enemy.GetComponent<PhotonView>();
                    if (enemyView != null)
                    {
                        int killerActorNumber = playerPhotonView.OwnerActorNr;
                        enemyView.RPC("TakeHit", RpcTarget.MasterClient, killerActorNumber);
                    }
                }
            }
        }

        // Vẽ ray từ nòng súng đến điểm hit (theo hướng chuột)
        if (lineRenderer != null)
            StartCoroutine(ShowLine(shootPoint.position, endPoint));

        PlayEffects();

        // Quay hướng súng về mục tiêu
        if (shootPoint != null)
            shootPoint.LookAt(endPoint);
    }

    IEnumerator ShowLine(Vector3 start, Vector3 end)
    {
        if (lineRenderer == null) yield break;

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        lineRenderer.enabled = true;

        yield return new WaitForSeconds(lineDuration);

        lineRenderer.enabled = false;
    }

    void PlayEffects()
    {
        if (muzzleFlash != null)
            StartCoroutine(FlashMuzzle());

        if (gunAudio != null)
            gunAudio.Play();
    }

    IEnumerator FlashMuzzle()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash.enabled = true;
            yield return new WaitForSeconds(0.05f);
            muzzleFlash.enabled = false;
        }
    }
}
