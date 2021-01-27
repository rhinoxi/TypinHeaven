using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextEnemy : MonoBehaviour
{
    public Color hlColor;
    public float speed;
    [HideInInspector]
    public Vector3 direction;
    private string raw;
    public string text {
        get {
            return raw;
        }
        set {
            raw = value;
            UpdateRichText();
        }
    }
    private int index = 0;
    private TextMeshProUGUI textObj;

    private void Awake() {
        textObj = this.GetComponentInChildren<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
        if ((transform.position - Vector3.zero).sqrMagnitude < 0.001) {
            Destroy(gameObject);
        }
    }

    public void Heal() {
        index = 0;
        UpdateRichText();
    }

    public bool Down() {
        return index == raw.Length;
    }

    public bool Attack(char c) {
        if (raw[index] == c) {
            index++;
            UpdateRichText();
            return true;
        } else {
            Heal();
            return false;
        }
    }

    private void UpdateRichText() {
        textObj.text = $"<color=#{ColorUtility.ToHtmlStringRGB(hlColor)}>{raw.Substring(0, index)}</color>{raw.Substring(index)}";
    }
}
