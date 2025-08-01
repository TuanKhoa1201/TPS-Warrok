using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class EnemyAI : MonoBehaviourPun
{
    [Header("Settings")]
    public float idleTime = 0.5f;
    public float chaseDistance = 20f;
    public float attackDistance = 2f;
    public float attackCooldown = 1.5f;
    public int maxHitsToDie = 3;

    private int hitCount = 0;
    private NavMeshAgent agent;
    private Animator animator;

    private float idleTimer;
    private float attackTimer;
    private bool isDead = false;
    private bool isAttacking = false;

    private Transform targetPlayer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
            animator.SetBool("Die", false);
        }

        if (agent != null)
        {
            agent.speed = 6f;
            agent.stoppingDistance = 1f;
        }

        hitCount = 0;
        idleTimer = idleTime;
        attackTimer = 0f;
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient || isDead || agent == null) return;

        idleTimer -= Time.deltaTime;

        UpdateClosestPlayer();

        if (targetPlayer == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);

        if (idleTimer > 0f)
        {
            StopMoving();
            return;
        }

        if (distanceToPlayer > attackDistance)
        {
            isAttacking = false;
            attackTimer = 0f;
            MoveToPlayer();
        }
        else
        {
            StopMoving();
            TryAttack();
        }
    }

    void UpdateClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        float closestDist = Mathf.Infinity;
        Transform closest = null;

        foreach (GameObject p in players)
        {
            float dist = Vector3.Distance(transform.position, p.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = p.transform;
            }
        }

        targetPlayer = closest;
    }

    void MoveToPlayer()
    {
        if (!agent.isOnNavMesh || targetPlayer == null) return;

        agent.isStopped = false;
        agent.SetDestination(targetPlayer.position);

        if (animator != null)
            animator.SetBool("Run", true);

        LookAtPlayerSmooth();
    }

    void StopMoving()
    {
        if (agent.isOnNavMesh)
            agent.isStopped = true;

        if (animator != null)
            animator.SetBool("Run", false);
    }

    void TryAttack()
    {
        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f && !isAttacking)
        {
            isAttacking = true;
            attackTimer = attackCooldown;

            LookAtPlayer();

            if (animator != null)
                animator.SetTrigger("Attack");

            if (targetPlayer != null)
            {
                PlayerHealth playerHealth = targetPlayer.GetComponent<PlayerHealth>();
                PhotonView targetView = targetPlayer.GetComponent<PhotonView>();

                if (playerHealth != null && targetView != null)
                {
                    targetView.RPC("TakeHit", targetView.Owner);
                }
            }

            Invoke(nameof(ResetAttackState), 0.2f);
        }
    }

    void LookAtPlayer()
    {
        if (targetPlayer == null) return;

        Vector3 dir = (targetPlayer.position - transform.position).normalized;
        dir.y = 0f;

        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    void LookAtPlayerSmooth()
    {
        if (targetPlayer == null) return;

        Vector3 dir = (targetPlayer.position - transform.position).normalized;
        dir.y = 0f;

        if (dir != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 5f);
        }
    }

    void ResetAttackState()
    {
        isAttacking = false;
    }

    [PunRPC]
    public void TakeHit(int killerActorNumber)
    {
        if (isDead) return;

        hitCount++;
        Debug.Log($"Enemy hit: {hitCount}/{maxHitsToDie}");

        if (hitCount >= maxHitsToDie && PhotonNetwork.IsMasterClient)
        {
            Die(killerActorNumber);
        }
    }

    void Die(int killerActorNumber)
    {
        isDead = true;
        StopMoving();

        photonView.RPC(nameof(PlayDeathAnimation), RpcTarget.All);

        // Gửi RPC cho client người bắn để tự cộng điểm
        photonView.RPC(nameof(NotifyKillerToAddScore), RpcTarget.All, killerActorNumber);

        Invoke(nameof(DestroyEnemy), 4f);
    }

    [PunRPC]
    void NotifyKillerToAddScore(int killerActorNumber)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == killerActorNumber && GameManager.instance != null)
        {
            GameManager.instance.AddScore(1);
        }
    }


    void DestroyEnemy()
    {
        PhotonNetwork.Destroy(gameObject);
    }

    [PunRPC]
    void PlayDeathAnimation()
    {
        if (animator != null)
            animator.SetBool("Die", true);
    }
}
