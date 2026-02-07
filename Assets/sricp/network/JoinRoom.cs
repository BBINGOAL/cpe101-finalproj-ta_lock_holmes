using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class JoinRoom : MonoBehaviourPunCallbacks
{
    [Header("UI Elements")]
    public InputField roomCodeInput; // ช่องกรอกรหัสห้อง
    public Text statusText;          // ข้อความแสดงสถานะ

    private bool isConnectedToMaster = false; // ตรวจสอบสถานะการเชื่อมต่อกับ Master Server

    private void Start()
    {
        // เปิดการซิงค์ Scene ระหว่างผู้เล่น
        PhotonNetwork.AutomaticallySyncScene = true;

        // เชื่อมต่อกับ Photon Server หากยังไม่ได้เชื่อมต่อ
        if (!PhotonNetwork.IsConnected)
        {
            statusText.text = "Connecting to Photon...";
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            isConnectedToMaster = true;
            statusText.text = "Connected to Master Server. Ready to join a room.";
        }
    }

    public override void OnConnectedToMaster()
    {
        // เรียกเมื่อเชื่อมต่อกับ Master Server สำเร็จ
        isConnectedToMaster = true;
        statusText.text = "Connected to Master Server. Ready to join a room.";
    }

    public void JoinRoomByCode()
    {
        if (!isConnectedToMaster)
        {
            statusText.text = "Connecting to Master Server. Please wait...";
            return;
        }

        // รับรหัสห้องจาก InputField
        string roomCode = roomCodeInput.text.Trim();

        if (!string.IsNullOrEmpty(roomCode))
        {
            PhotonNetwork.JoinRoom(roomCode);
            statusText.text = $"Joining Room: {roomCode}...";
        }
        else
        {
            statusText.text = "Please enter a valid room code.";
        }
    }

    public override void OnJoinedRoom()
    {
        // แสดงข้อความเมื่อเข้าห้องสำเร็จ
        statusText.text = $"Joined Room: {PhotonNetwork.CurrentRoom.Name}";
        Debug.Log($"Joined Room: {PhotonNetwork.CurrentRoom.Name}");

        // โหลด Scene ถัดไป (เช่น GameScene)
        if (PhotonNetwork.IsMasterClient)
        {
            statusText.text = "You are the Host. Starting the game...";
            PhotonNetwork.LoadLevel("GameScene");
        }
        else
        {
            statusText.text = "Joined the room. Waiting for the Host to start the game...";
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        // แสดงข้อความเมื่อการเข้าห้องล้มเหลว
        statusText.text = $"Failed to join room: {message}";
        Debug.LogError($"Failed to join room: {message}");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        // จัดการกรณีการตัดการเชื่อมต่อ
        isConnectedToMaster = false;
        statusText.text = $"Disconnected: {cause}";
        Debug.LogError($"Disconnected from Photon: {cause}");
    }
}
