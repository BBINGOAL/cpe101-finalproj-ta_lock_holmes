using ExitGames.Client.Photon; // สำหรับ RaiseEvent
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Timeline;

public class Network : MonoBehaviourPunCallbacks
{
    [Header("UI Elements")]
    public Text statusText;              // UI Text สำหรับแสดงสถานะ
    public CameraFollow playerCamera;    // กล้องที่ใช้สำหรับติดตามผู้เล่น

    [Header("Spawn Settings")]
    public string botPrefabName = "Bot"; // ชื่อ Prefab ของ Bot
    [SerializeField, Range(1, 5)] public int botMultiplier = 2;        // ตัวคูณจำนวน Bot ต่อจำนวน Player

    [Header("UI Elements for Display")]
    public TMP_Text rolesText;    // แสดง Roles ของผู้เล่น
    public TMP_Text botCountText; // แสดงจำนวน Bots
    public TMP_Text playerCountText; // UI สำหรับแสดงจำนวนผู้เล่น
    public TMP_Text murderCountText; // แสดงจำนวน Murders (ใหม่)

    private int currentBotCount = 0; // จำนวน botCount ปัจจุบัน
    private int currentPlayerCount = 0; // จำนวน PlayerCount ปัจจุบัน
    private int currentMurderCount = 0;// จำนวน MurderCount ปัจจุบัน

    private int totalPlayerCount = 0; // กำหนดให้รวมผู้เล่นทั้งหมด
    private bool gameEnded = false;
    private bool botsSpawned = false;
    private void Start()
    {
        // ให้เกมทำงานต่อเนื่องแม้ไม่ได้ Focus หน้าจอ
        Application.runInBackground = true;

        PhotonNetwork.AutomaticallySyncScene = true;

        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            Debug.Log($"[Network] Connected to Room: {PhotonNetwork.CurrentRoom.Name}");
            StartCoroutine(WaitAndSpawnPlayer()); // Spawn ตัวละครเมื่อ Role พร้อม
        }
        else
        {
            statusText.text = "Not connected to a room. Returning to Lobby...";
            Debug.LogError("[Network] Not connected to a room. Returning to Lobby...");
            PhotonNetwork.LoadLevel("LobbyScene");
        }
    }

    private IEnumerator WaitAndSpawnPlayer()
    {
        yield return new WaitForSeconds(1f); // รอให้ Custom Properties ถูกซิงค์

        Debug.Log($"[Network] Checking Custom Properties for Role...");
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Role", out object roleObject))
        {
            string role = roleObject.ToString();
            Debug.Log($"[Network] Local Player Role: {role}");

            // Spawn ตัวละคร
            GameObject player = PhotonNetwork.Instantiate(
                role,
                new Vector3(Random.Range(-10, 10), 1, Random.Range(-10, 10)),
                Quaternion.identity
            );
            if (rolesText != null)
            {
                rolesText.text = $"Your Role: {role}";
            }

            if (player != null)
            {
                SetupCamera(player);
                Debug.Log($"[Network] Spawned Player: {role}, Position: {player.transform.position}");
            }
            else
            {
                Debug.LogError($"[Network] Failed to spawn player with Role: {role}");
            }

            // ให้ Master Client นับจำนวนผู้เล่นที่มี Role เป็น "Player" หลังจาก Spawn เสร็จ
            if (PhotonNetwork.IsMasterClient)
            {
                totalPlayerCount = PhotonNetwork.PlayerList.Length; // ใช้จำนวนผู้เล่นปัจจุบันในห้อง
                Debug.Log($"[Network] Total Player Count: {totalPlayerCount}");
                CountPlayersWithRole("Player");
                CountMurdersWithRole("Murder"); // นับจำนวน Murder
                SendPlayerCountToAll();
                SendMurderCountToAll(); // ส่งจำนวน Murder ไปยังทุกเครื่อง
            }


            if (PhotonNetwork.IsMasterClient && !botsSpawned)
            {
                SpawnBots();
                botsSpawned = true;
            }
        }
        else
        {
            Debug.LogError("[Network] Role not found in Custom Properties!");
        }
    }

    private void SetupCamera(GameObject player)
    {
        if (playerCamera != null)
        {
            playerCamera.target = player.transform;
            Debug.Log($"[Network] Camera set to follow player: {player.name}");
        }
        else
        {
            Debug.LogError("[Network] CameraFollow component is missing or not assigned!");
        }
    }

    private void SpawnBots()
    {
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        int newBotCount = playerCount * botMultiplier; // คำนวณจำนวน Bot ที่จะ Spawn

        Debug.Log($"[Network] Spawning {currentBotCount} Bots for {playerCount} Players.");

        for (int i = 0; i < newBotCount; i++)
        {
            Vector3 randomPosition = new Vector3(Random.Range(-10, 10), 1, Random.Range(-10, 10));
            PhotonNetwork.Instantiate(
                botPrefabName,
                randomPosition,
                Quaternion.identity
            );
        }

        // ซิงค์จำนวน botCount ไปยังทุกคน
        UpdateBotCount(newBotCount);
    }

    public void UpdateBotCount(int change)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            currentBotCount += change; // กำหนดจำนวน Bot ใหม่
            currentBotCount = Mathf.Max(currentBotCount, 0); // ป้องกันไม่ให้ติดลบ
            Debug.Log($"[Network] Updated Bot Count: {currentBotCount}");
            //ส่งอัปเดต botCount ไปยังทุกคน
            object[] content = new object[] { currentBotCount }; // ข้อมูลที่จะส่ง
            RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            SendOptions sendOptions = new SendOptions { Reliability = true };

            PhotonNetwork.RaiseEvent(1, content, options, sendOptions); // EventCode = 1
            Debug.Log($"[Network] Updated Bot Count: {currentBotCount}");

            // ตรวจสอบเงื่อนไขการจบเกม
            CheckGameEnd();
        }
    }

    // ฟังก์ชันสำหรับนับจำนวนผู้เล่นที่มี Role เป็น "Player"
    private void CountPlayersWithRole(string roleToCount)
    {
        currentPlayerCount = 0;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.TryGetValue("Role", out object role))
            {
                if (role.ToString() == roleToCount)
                {
                    currentPlayerCount++;
                }
            }
        }
    }

    private void CountMurdersWithRole(string roleToCount)
    {
        currentMurderCount = 0;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.TryGetValue("Role", out object role))
            {
                if (role.ToString() == roleToCount)
                {
                    currentMurderCount++;
                }
            }
        }

    }

    // ส่งจำนวนผู้เล่นที่ถูกนับไปยังทุกเครื่อง
    private void SendPlayerCountToAll()
    {
        object[] content = new object[] { currentPlayerCount };
        RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        PhotonNetwork.RaiseEvent(2, content, options, sendOptions);
        Debug.Log($"[Network] Sent Player Count: {currentPlayerCount}");
    }
    public void UpdatePlayerCount(int change)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            currentPlayerCount  += change;

            // ส่งอัปเดตจำนวนผู้เล่นไปยังทุกคน
            object[] content = new object[] { currentPlayerCount };
            RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            SendOptions sendOptions = new SendOptions { Reliability = true };

            PhotonNetwork.RaiseEvent(2, content, options, sendOptions); // EventCode = 2
            Debug.Log($"[Network] Updated Player Count: {currentPlayerCount}");

            // ตรวจสอบเงื่อนไขการจบเกม
            CheckGameEnd();
        }
    }

    private void SendMurderCountToAll()
    {
        object[] content = new object[] { currentMurderCount };
        RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        PhotonNetwork.RaiseEvent(3, content, options, sendOptions);
        Debug.Log($"[Network] Sent Murder Count: {currentMurderCount}");
    }

    public void UpdateMurderCount(int change)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // อัปเดตจำนวน Murder โดยป้องกันไม่ให้ติดลบ
            currentMurderCount += change;

            // ส่งจำนวน Murder ที่อัปเดตไปยังทุกเครื่องในห้อง
            object[] content = new object[] { currentMurderCount };
            RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            SendOptions sendOptions = new SendOptions { Reliability = true };

            PhotonNetwork.RaiseEvent(3, content, options, sendOptions); // EventCode = 3 สำหรับ Murder Count
            Debug.Log($"[Network] Updated Murder Count: {currentMurderCount}");

            // ตรวจสอบเงื่อนไขการจบเกม
            CheckGameEnd();
        }
    }


    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    private void UpdatePlayerCountUI()
    {
        if (playerCountText != null)
        {
            playerCountText.text = $"Players Remaining: {currentPlayerCount}";
        }
    }

    private void UpdateMurderCountUI()
    {
        if (murderCountText != null)
        {
            murderCountText.text = $"Murders Remaining: {currentMurderCount}";
        }
    }


    private void UpdateBotCountUI()
    {
        if (botCountText != null)
        {
            botCountText.text = $"Bots Remaining: {currentBotCount}";
        }
    }
    private void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == 1) // EventCode = 1 สำหรับอัปเดต botCount
        {
            object[] data = (object[])photonEvent.CustomData;
            currentBotCount = (int)data[0];
            UpdateBotCountUI();

            Debug.Log($"[Network] Received Updated Bot Count: {currentBotCount}");
        }
        else if (photonEvent.Code == 2) // อัปเดต playerCount
        {
            object[] data = (object[])photonEvent.CustomData;
            currentPlayerCount = (int)data[0];

            UpdatePlayerCountUI(); // เรียกฟังก์ชันเพื่ออัปเดต UI

            Debug.Log($"[Network] Received Updated Player Count: {currentPlayerCount}");
        }
        else if (photonEvent.Code == 3) // EventCode = 3 สำหรับอัปเดต Murder Count
        {
            object[] data = (object[])photonEvent.CustomData;
            currentMurderCount = (int)data[0];

            UpdateMurderCountUI(); // อัปเดต UI แสดงจำนวน Murder

            Debug.Log($"[Network] Received Updated Murder Count: {currentMurderCount}");
        }
        else if (photonEvent.Code == 4) // EventCode = 4 สำหรับการจบเกม
        {
            object[] data = (object[])photonEvent.CustomData;
            string sceneName = (string)data[0];

            Debug.Log($"[Network] Received Game End Event: {sceneName}");
            // โหลดซีนตามที่ได้รับ
            PhotonNetwork.LoadLevel(sceneName);
        }
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        statusText.text = $"Disconnected: {cause}";
        Debug.LogError($"[Network] Disconnected from Photon with reason: {cause}");
        PhotonNetwork.LoadLevel("LobbyScene");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        statusText.text = $"Failed to join room: {message}";
        Debug.LogError($"[Network] Failed to join room: {message}");
        PhotonNetwork.LoadLevel("LobbyScene");
    }

    //private void LoadWinScene(string sceneName)
    //{
    //    Debug.Log($"Loading Scene: {sceneName}");
    //    PhotonNetwork.LoadLevel(sceneName);
    //}

    private void CheckGameEnd()
    {
        Debug.Log($"[Network] Checking Game End: currentBotCount = {currentBotCount}, totalPlayerCount = {totalPlayerCount}, currentMurderCount = {currentMurderCount}");

        if (currentMurderCount <= 0)
        {
            Debug.Log("Game Over! Players Win!");
            StartCoroutine(WaitForBotDeathAnimation("TA WIN"));
        }
        else if (((currentBotCount <= totalPlayerCount) || currentPlayerCount<=0) && currentMurderCount > 0)
        {
            Debug.Log("Game Over! Murder Wins!");
            StartCoroutine(WaitForBotDeathAnimation("KILLER WIN"));
        }

    }

    private void RaiseGameEndEvent(string sceneName)
    {
        if (gameEnded) return; // ป้องกันการส่ง Event ซ้ำ
        gameEnded = true;

        object[] content = new object[] { sceneName };
        RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        PhotonNetwork.RaiseEvent(4, content, options, sendOptions); // EventCode = 4 สำหรับการจบเกม
        Debug.Log($"[Network] Raised Game End Event: {sceneName}");
    }
    private IEnumerator WaitForBotDeathAnimation(string sceneName)
    {
        // ระยะเวลาที่รอให้แอนิเมชันการตายเล่นเสร็จ (เช่น 1.5 วินาที)
        float deathAnimationDuration = 1.5f;

        // รอให้แอนิเมชันการตายเล่นจนจบ
        yield return new WaitForSeconds(deathAnimationDuration);

        // ส่ง Event ให้ทุกเครื่องเปลี่ยน Scene
        RaiseGameEndEvent(sceneName);
    }
}

