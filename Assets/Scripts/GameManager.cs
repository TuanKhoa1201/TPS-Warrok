using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;

    [Header("Game Stats")]
    public int score = 0;
    public int maxLives = 3;
    public int currentLives;

    [Header("UI Elements")]
    public Text scoreText;
    public Text livesText;
    public GameObject gameOverPanel;
    public Text finalScoreText;
    public GameObject winPanel;

    [Header("Win Condition")]
    public int winScoreTarget = 10;

    private bool isGameOver = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        currentLives = maxLives;
        UpdateUI();

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);

        isGameOver = false;
    }

    public void SetupLives(int lives)
    {
        currentLives = lives;
        UpdateUI();
        Debug.Log($"✅ SetupLives: {lives}");
    }

    public void AddScore(int amount)
    {
        if (isGameOver) return;

        score += amount;
        UpdateUI();

        if (score >= winScoreTarget)
        {
            WinGame();
        }
    }

    public void TakeDamage()
    {
        if (isGameOver) return;

        currentLives--;
        UpdateUI();

        if (currentLives <= 0)
        {
            GameOver();
        }
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Kill: {score}";

        if (livesText != null)
            livesText.text = $"Lives: {currentLives}";
    }

    void GameOver()
    {
        isGameOver = true;

        if (finalScoreText != null)
            finalScoreText.text = $"Final Kill: {score}";

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    void WinGame()
    {
        isGameOver = true;

        if (winPanel != null)
            winPanel.SetActive(true);

        if (finalScoreText != null)
            finalScoreText.text = $"Final Score: {score}";
    }

    public void Retry()
    {
        if (PhotonNetwork.LocalPlayer.TagObject == null)
        {
            StartCoroutine(RespawnLocalPlayer());
        }
        else
        {
            score = 0;
            currentLives = maxLives;
            UpdateUI();

            gameOverPanel?.SetActive(false);
            winPanel?.SetActive(false);
            isGameOver = false;
        }
    }

    IEnumerator RespawnLocalPlayer()
    {
        yield return new WaitForSeconds(0.2f);

        // ✅ Đảm bảo không spawn trùng chỗ
        Vector3 spawnPos = new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));

        // ✅ Spawn prefab mới
        GameObject player = PhotonNetwork.Instantiate("PlayerPrefab", spawnPos, Quaternion.identity);

        // ✅ Gán lại cho TagObject để các enemy có thể tìm ra player mới
        PhotonNetwork.LocalPlayer.TagObject = player;

        // Reset stats
        score = 0;
        currentLives = maxLives;
        isGameOver = false;

        UpdateUI();
        gameOverPanel?.SetActive(false);
        winPanel?.SetActive(false);
    }
}
