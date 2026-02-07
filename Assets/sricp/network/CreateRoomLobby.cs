using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement; // สำหรับเปลี่ยน Scene
using System.Collections.Generic;
using System.Linq;

public class CreateRoomLobby : MonoBehaviourPunCallbacks
{
    public Text roomCodeText; // Text สำหรับแสดงรหัสห้อง
    public Text playerCountText; // Text สำหรับแสดงจำนวนผู้เล่นในห้อง
    public Button startGameButton; // ปุ่มสำหรับเริ่มเกม (เฉพาะ Host)
    public Button backButton; // ปุ่มสำหรับกลับไปยัง Scene ก่อนหน้า
    public int maxPlayers = 6; // จำนวนผู้เล่นสูงสุดในห้อง

    private Dictionary<int, string> playerRoles = new Dictionary<int, string>(); // เก็บ Role ของผู้เล่น
    private bool rolesAssigned = false; // เช็คว่า Role ถูกแจกแล้วหรือยัง

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            ConnectToServer();
        }
        else if (PhotonNetwork.InRoom)
        {
            OnJoinedRoom();
        }

        // ให้ปุ่ม Back แสดงสำหรับทุกคน
        if (backButton != null)
        {
            backButton.onClick.AddListener(LeaveRoomAndReturn);
            backButton.gameObject.SetActive(true);
        }

        // ให้ปุ่ม Start ใช้งานได้เฉพาะ MasterClient
        if (startGameButton != null)
        {
            startGameButton.interactable = PhotonNetwork.IsMasterClient;
            startGameButton.onClick.AddListener(AssignRolesAndStartGame);
        }
    }

    private void ConnectToServer()
    {
        PhotonNetwork.ConnectUsingSettings(); // เชื่อมต่อกับ Photon Server
        PhotonNetwork.AutomaticallySyncScene = true; // ให้ทุกผู้เล่น Sync Scene เดียวกัน
        Debug.Log("Connecting to Photon Server...");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server.");
        PhotonNetwork.JoinLobby(); // เข้าร่วม Lobby ก่อนสร้างหรือเข้าห้อง
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby.");
        JoinOrCreateRoom(); // สร้างหรือเข้าห้องเมื่อพร้อม
    }

    private void JoinOrCreateRoom()
    {
        string roomName = Random.Range(1000, 9999).ToString(); // สร้างรหัสห้องสุ่ม
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = (byte)maxPlayers // ตั้งค่าจำนวนผู้เล่นสูงสุด
        };
        PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
        Debug.Log("Trying to create or join room: " + roomName);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log($"Room created: {PhotonNetwork.CurrentRoom.Name}");
        UpdateRoomUI();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Player joined room: {PhotonNetwork.NickName}, Room: {PhotonNetwork.CurrentRoom.Name}");
        UpdateRoomUI();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"{newPlayer.NickName} joined the room. Total players: {PhotonNetwork.CurrentRoom.PlayerCount}");
        UpdateRoomUI();
    }

    private void UpdateRoomUI()
    {
        if (roomCodeText != null)
        {
            roomCodeText.text = $"Room Code: {PhotonNetwork.CurrentRoom.Name}";
        }

        if (playerCountText != null)
        {
            playerCountText.text = $"Players: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
        }

        if (startGameButton != null)
        {
            startGameButton.interactable = PhotonNetwork.IsMasterClient && !rolesAssigned;
        }
    }

    private void AssignRolesAndStartGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Debug.Log("[Lobby] Assigning roles...");
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        int murderCount = playerCount >= 5 ? 2 : 1;

        // แจก Role
        List<int> playerIndices = PhotonNetwork.PlayerList.Select(p => p.ActorNumber).ToList();
        List<int> murderIndices = new List<int>();

        while (murderIndices.Count < murderCount)
        {
            int randomIndex = Random.Range(0, playerIndices.Count);
            murderIndices.Add(playerIndices[randomIndex]);
            playerIndices.RemoveAt(randomIndex);
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            string role = murderIndices.Contains(player.ActorNumber) ? "Murder" : "Player";
            ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable
            {
                { "Role", role }
            };
            player.SetCustomProperties(properties);
            Debug.Log($"Assigned Role {role} to {player.NickName} (ActorNumber: {player.ActorNumber})");
        }

        rolesAssigned = true;
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "RolesAssigned", true } });

        Debug.Log("[Lobby] Starting game...");
        PhotonNetwork.LoadLevel("GameScene");
    }

    public void LeaveRoomAndReturn()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            ReturnToLobbyScene();
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"{otherPlayer.NickName} left the room. Total players: {PhotonNetwork.CurrentRoom.PlayerCount}");
        UpdateRoomUI();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Left the room.");
        UpdateRoomUI();
        ReturnToLobbyScene();
    }

    private void ReturnToLobbyScene()
    {
        Debug.Log("Returning to Lobby Scene...");
        SceneManager.LoadScene("lobby");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError($"Disconnected from server: {cause}");
        ReturnToLobbyScene();
    }
}