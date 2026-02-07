using UnityEngine;

public class ChangeSprite : MonoBehaviour {
    public SpriteRenderer spriteRenderer; // ลาก Sprite Renderer ของตัวละครมาที่นี่
    public Sprite newSprite; // ลาก Sprite ที่ต้องการเปลี่ยนมาที่นี่

    void Start() {
        if (spriteRenderer != null && newSprite != null) {
            spriteRenderer.sprite = newSprite; // เปลี่ยน Sprite
        }
    }
}
