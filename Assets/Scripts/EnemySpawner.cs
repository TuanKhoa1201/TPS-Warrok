using UnityEngine;
using Photon.Pun;

public class EnemySpawner : MonoBehaviourPun
{
    public GameObject enemyPrefab;
    public float spawnInterval = 5f;
    public int maxEnemies = 20;

    public Vector3 spawnAreaMin;
    public Vector3 spawnAreaMax;

    private int enemyCount = 0;

    void Start()
    {
        InvokeRepeating(nameof(SpawnEnemy), 1f, spawnInterval);

        // 👇 Gửi target cho GameManager
        if (GameManager.instance != null)
        {
            GameManager.instance.winScoreTarget = maxEnemies;
        }
    }

    void SpawnEnemy()
    {
        if (!PhotonNetwork.IsMasterClient) return; // chỉ máy chủ spawn

        if (enemyCount >= maxEnemies) return;

        Vector3 randomPos = new Vector3(
            Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            spawnAreaMax.y, // Raycast from above
            Random.Range(spawnAreaMin.z, spawnAreaMax.z)
        );

        if (Physics.Raycast(randomPos, Vector3.down, out RaycastHit hit, 100f))
        {
            if (hit.collider.CompareTag("Ground"))
            {
                PhotonNetwork.Instantiate(enemyPrefab.name, hit.point, Quaternion.identity);
                enemyCount++;
            }
        }
    }
}