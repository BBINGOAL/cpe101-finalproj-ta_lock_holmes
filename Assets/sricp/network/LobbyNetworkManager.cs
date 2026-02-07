using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class LobbyRoomManager : MonoBehaviourPunCallbacks
{
    [Header("UI Elements")]
    public Text statusText;  // ข้อความแสดงสถานะใน UI

    private string roomName = "MainRoom";

    private void Start()
    {
        statusText.text = "Connecting to Photon...";
        ConnectToServer();
    }

    private void ConnectToServer()
    {
        // ตั้งชื่อผู้เล่นแบบสุ่มโดยไม่ต้องกรอกชื่อ
        PhotonNetwork.NickName = "Player" + Random.Range(0, 5000);

        // เชื่อมต่อกับ Photon Server
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnConnectedToMaster()
    {
        statusText.text = "Connected to Master. Joining Room...";

        // เข้าร่วมห้องหรือสร้างห้องใหม่ชื่อ "MainRoom"
        PhotonNetwork.JoinOrCreateRoom(
            roomName,
            new RoomOptions { MaxPlayers = 4 },
            TypedLobby.Default
        );
    }

    public override void OnJoinedRoom()
    {
        statusText.text = $"Joined Room: {PhotonNetwork.CurrentRoom.Name}";
        Debug.Log($"Joined Room: {PhotonNetwork.CurrentRoom.Name}, Players: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        statusText.text = $"Failed to join room: {message}";
        Debug.LogError($"Failed to join room: {message}");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        statusText.text = $"Disconnected: {cause}";
        Debug.LogError($"Disconnected from Photon with reason: {cause}");
    }
}
