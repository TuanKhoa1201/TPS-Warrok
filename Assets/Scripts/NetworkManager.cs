using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public string gameVersion = "1.0";
    public GameObject playerPrefab;

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
        else if (PhotonNetwork.InRoom)
        {
            SpawnPlayer(); // Nếu đã kết nối và ở trong phòng
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom("Room1", new RoomOptions { MaxPlayers = 4 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        Vector3 spawnPos = new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5));
        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, Quaternion.identity);

        // Nếu prefab không có camera thì gắn thêm
        // if (player.GetComponentInChildren<Camera>() == null)
        // {
        //     GameObject cam = new GameObject("PlayerCamera");
        //     cam.tag = "MainCamera";
        //     cam.AddComponent<Camera>();
        //     cam.transform.SetParent(player.transform);
        //     cam.transform.localPosition = new Vector3(0, 5, -7);
        //     cam.transform.LookAt(player.transform);
        // }
    }
}
