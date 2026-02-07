using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;  // สำหรับ TextMeshPro
using UnityEngine.SceneManagement; // สำหรับโหลด Scene

public class BombManager : MonoBehaviourPunCallbacks
{
    public float countdownTime = 10f;          // เวลานับถอยหลัง 10 วินาที
    public TMP_Text timerText;                 // TextMeshPro สำหรับแสดงเวลา
    public GameObject bombPrefab;              // Prefab ของระเบิด
    public GameObject explosionEffectPrefab;   // Prefab ของเอฟเฟกต์ระเบิด
    public TMP_Text notificationText;          // TextMeshPro สำหรับแสดงข้อความแจ้งเตือน

    private GameObject playerWithBomb;
    private GameObject currentBomb;
    private bool isCountingDown = false;
    private bool hasGameStarted = false;       // เช็คว่าเกมเริ่มแล้วหรือยัง

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient && !hasGameStarted)
        {
            hasGameStarted = true;
            AssignBombToRandomPlayer();
        }
    }

    [PunRPC]
    public void AssignBombToRandomPlayer()
    {
        // ตรวจสอบว่ามีผู้เล่นเหลือมากกว่า 1 คนหรือไม่
        if (PhotonNetwork.PlayerList.Length <= 1)
        {
            DeclareWinner();
            return;
        }

        Player[] players = PhotonNetwork.PlayerList;
        int randomIndex = Random.Range(0, players.Length);
        photonView.RPC("GiveBomb", RpcTarget.AllBuffered, players[randomIndex].ActorNumber);
    }

    [PunRPC]
    private void GiveBomb(int actorNumber)
    {
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            PhotonView pv = player.GetComponent<PhotonView>();
            if (pv != null && pv.Owner.ActorNumber == actorNumber)
            {
                playerWithBomb = player;
                AttachBombToPlayer(player);
                StartBombCountdown();
                ShowNotification($"{pv.Owner.NickName} has the bomb!");
                return;
            }
        }
    }

    private void AttachBombToPlayer(GameObject player)
    {
        if (currentBomb != null)
        {
            Destroy(currentBomb);
        }

        currentBomb = Instantiate(bombPrefab, player.transform);
        currentBomb.transform.localPosition = new Vector3(0, 1.5f, 0);
    }

    private void StartBombCountdown()
    {
        if (!isCountingDown)
        {
            isCountingDown = true;
            StartCoroutine(BombCountdown());
        }
    }

    private IEnumerator BombCountdown()
    {
        float timeLeft = countdownTime;
        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            UpdateTimerUI(timeLeft);
            yield return null;
        }
        Explode();
    }

    private void UpdateTimerUI(float timeLeft)
    {
        if (timerText != null)
        {
            timerText.text = $"Time Left: {timeLeft:F2}";
        }
    }

    private void Explode()
    {
        if (playerWithBomb != null)
        {
            Debug.Log($"{playerWithBomb.name} has exploded!");

            // แสดงเอฟเฟกต์ระเบิด
            if (explosionEffectPrefab != null)
            {
                Instantiate(explosionEffectPrefab, playerWithBomb.transform.position, Quaternion.identity);
            }

            PhotonNetwork.Destroy(playerWithBomb);
            RemoveBombFromPlayer();

            // ตรวจสอบว่ามีผู้เล่นเหลือเพียงคนเดียวหรือไม่
            if (PhotonNetwork.PlayerList.Length <= 1)
            {
                DeclareWinner();
            }
            else if (PhotonNetwork.IsMasterClient)
            {
                Invoke("AssignBombToRandomPlayer", 3f);
            }
        }
    }

    private void RemoveBombFromPlayer()
    {
        if (currentBomb != null)
        {
            Destroy(currentBomb);
        }
        playerWithBomb = null;
        isCountingDown = false;
    }

    private void ShowNotification(string message)
    {
        if (notificationText != null)
        {
            notificationText.text = message;
            StartCoroutine(ClearNotification());
        }
    }

    private IEnumerator ClearNotification()
    {
        yield return new WaitForSeconds(3f);
        if (notificationText != null)
        {
            notificationText.text = "";
        }
    }

    public void DeclareWinner()
    {
        // ประกาศผู้ชนะและโหลด Endgame Scene
        Player winner = PhotonNetwork.PlayerList[0];
        ShowNotification($"{winner.NickName} is the winner!");
        Invoke("LoadEndgameScene", 3f);
    }

    private void LoadEndgameScene()
    {
        SceneManager.LoadScene("EndgameScene");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // เมื่อมีผู้เล่นใหม่เข้าห้อง ถ้ายังไม่ได้เริ่มเกม ให้เริ่มเกม
        if (PhotonNetwork.IsMasterClient && !hasGameStarted)
        {
            hasGameStarted = true;
            AssignBombToRandomPlayer();
        }
    }
}
