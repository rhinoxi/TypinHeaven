using UnityEngine;
using UnityEngine.UI;

public class PlayPause : MonoBehaviour
{
    public Sprite onSprite;
    public Sprite offSprite;
    private Button btn;

    private void Start() {
        btn = GetComponent<Button>();
    }

    public void TogglePlayPause() {
        if (Time.timeScale == 0) {
            Time.timeScale = 1;
            btn.image.sprite = offSprite;
        } else {
            Time.timeScale = 0;
            btn.image.sprite = onSprite;
        }
    }
}
