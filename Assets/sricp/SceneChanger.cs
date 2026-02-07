using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void GoToCreateRoom()
    {
        SceneManager.LoadScene("CreateRoomScene"); // ชื่อ Scene ที่ใช้สำหรับสร้างห้อง
    }

    public void GoToJoinRoom()
    {
        SceneManager.LoadScene("JoinRoomScene"); // ชื่อ Scene ที่ใช้สำหรับเข้าห้อง
    }
    public void backbutton()
    {
        SceneManager.LoadScene("lobby");
    }
}

