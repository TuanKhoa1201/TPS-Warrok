using UnityEngine;
using Photon.Pun;

public class PlayerHealth : MonoBehaviourPunCallbacks
{
    public int maxHits = 3;
    private int currentHits = 0;

    private PhotonView photonView;
    private Animator animator;
    private bool isDead = false;

    void Start()
    {
        photonView = GetComponent<PhotonView>();
        animator = GetComponentInChildren<Animator>();
        currentHits = 0;
        isDead = false;

        // ✅ Gọi GameManager để setup lại mạng (chỉ người chơi local)
        if (photonView.IsMine && GameManager.instance != null)
        {
            GameManager.instance.SetupLives(maxHits);
        }

        // ✅ Gán lại TagObject để nhận dạng player
        if (photonView.IsMine)
        {
            PhotonNetwork.LocalPlayer.TagObject = gameObject;
        }
    }

    // Enemy gọi ApplyHit() khi đánh trúng
    public void ApplyHit()
    {
        if (!photonView.IsMine || isDead) return;

        photonView.RPC(nameof(TakeHit), RpcTarget.All);
    }

    [PunRPC]
    public void TakeHit()
    {
        if (isDead) return;

        currentHits++;

        if (photonView.IsMine)
        {
            Debug.Log("🛑 Player bị bắn! Số lần trúng đạn: " + currentHits);

            if (GameManager.instance != null)
            {
                GameManager.instance.TakeDamage();
            }

            if (currentHits >= maxHits)
            {
                Die();
            }
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("💀 Player chết!");

        if (photonView.IsMine)
        {
            // ✅ Clear TagObject để GameManager biết player đã chết
            PhotonNetwork.LocalPlayer.TagObject = null;

            // ✅ Hủy player sau 1 chút để GameManager kịp xử lý
            PhotonNetwork.Destroy(gameObject);
        }
    }
}
