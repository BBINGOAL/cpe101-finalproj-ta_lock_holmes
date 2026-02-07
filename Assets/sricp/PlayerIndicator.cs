using UnityEngine;
using TMPro;
using Photon.Pun;

public class PlayerIndicator : MonoBehaviourPunCallbacks
{
    [Header("UI Elements")]
    public TMP_Text playerText; // เชื่อมโยง TMP_Text ที่อยู่ใน Prefab

    private void Start()
    {
        AssignPlayerNumber();
    }

    private void AssignPlayerNumber()
    {
        if (photonView.IsMine) // ตรวจสอบว่าเป็น Player ของตัวเองหรือไม่
        {
            int playerNumber = PhotonNetwork.LocalPlayer.ActorNumber;
            playerText.text = $"Player {playerNumber}";
        }
        else
        {
            // ซ่อนข้อความสำหรับผู้เล่นอื่น
            playerText.gameObject.SetActive(false);
        }
    }
}
