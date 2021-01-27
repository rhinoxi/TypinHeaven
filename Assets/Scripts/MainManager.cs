using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainManager : MonoBehaviour
{
    public GameObject textEnemyPrefab;
    public GameObject psPrefab;
    public TextAsset dictionaryTextFile;
    public Slider movingSlider;
    public Slider spawnSlider;
    public AudioSource hitSound;
    public AudioSource destroySound;
    public AudioSource crashSound;

    public AudioClip[] audioClips;

    public TextMeshProUGUI timeFromStartGUI;
    public TextMeshProUGUI apmGUI;
    public TextMeshProUGUI wordCountGUI;
    
    public float SpawnDeltaTime { get; set; }
    private float movingSpeed;
    public float MovingSpeed {
        get {
            return movingSpeed;
        }
        set {
            movingSpeed = value;
            foreach (var textEnemy in textEnemies) {
                textEnemy.speed = movingSpeed;
            }
        } 
    }

    private float dt;
    private List<GameObject> enemies = new List<GameObject>();
    private List<TextEnemy> textEnemies = new List<TextEnemy>();
    private float top;
    private float bottom;
    private float left;
    private float right;
    private System.Random rnd;

    private List<string> words = new List<string>();

    private float totalTime = 0f;
    private int charCount = 0;
    private int apm = 0;
    private int wordCount = 0;

    private float guiUpdateInterval = 1;
    private float nextUpdateTime;

    void Start()
    {
        SpawnDeltaTime = spawnSlider.maxValue;
        MovingSpeed = movingSlider.minValue;
        spawnSlider.value = SpawnDeltaTime;
        movingSlider.value = MovingSpeed;

        dt = SpawnDeltaTime;

        var topRight = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        var bottomLeft = Camera.main.ScreenToWorldPoint(Vector3.zero);
        right = topRight.x;
        top = topRight.y;
        left = bottomLeft.x;
        bottom = bottomLeft.y;
        rnd = new System.Random();

        string theWholeFileAsOneLongString = dictionaryTextFile.text;
        words.AddRange(theWholeFileAsOneLongString.Split("\n"[0]) );

        ResetGame();
    }

    private void NewEnemyInRandomPlace() {
        Vector3 birthPlace = RandomPlaceOnEdgeNotOverride();
        GameObject enemy = Instantiate(textEnemyPrefab, birthPlace * 0.9f, Quaternion.identity);
        var textEnemy = enemy.GetComponent<TextEnemy>();
        textEnemy.text = GetRandomWord();
        textEnemy.direction = Vector3.Normalize(Vector3.zero - birthPlace);
        textEnemy.speed = MovingSpeed;
        enemies.Add(enemy);
        textEnemies.Add(textEnemy);
    }

    private string GetRandomWord() {
        return words[rnd.Next(words.Count)];
    }

    private Vector3 RandomPlaceOnEdgeNotOverride() {
        while (true) {
            Vector3 tmp = RandomPlaceOnEdge();
            bool ov = false;
            foreach (var enemy in enemies) {
                if ((enemy.transform.position - tmp).sqrMagnitude < 6) {
                    ov = true;
                    break;
                }
            }
            if (!ov) {
                return tmp;
            }
        }
    }

    private Vector3 RandomPlaceOnEdge() {
        var tmp = (float)rnd.NextDouble();
        int direction = rnd.Next(4);
        switch (direction) {
            case 0:
                return new Vector3(left + (right - left) * tmp, top, 0);
            case 1:
                return new Vector3(right, bottom + (top - bottom) * tmp, 0);
            case 2:
                return new Vector3(left + (right - left) * tmp, bottom, 0);
            default:
                return new Vector3(left + (right - left) * 0.1f, bottom + (top - bottom) * tmp, 0); // 如果从左侧生成，则位置稍微往右移，否则前几个字母可能被挡住
        }
    }

    private void InvalidAttack() {
        foreach (var textEnemy in textEnemies) {
            textEnemy.Heal();
        }
    }

    private void EnemyDownByIndex(int i) {
        Destroy(enemies[i]);
        enemies.RemoveAt(i);
        textEnemies.RemoveAt(i);
    }

    private void UpdateTimeGUI() {
        timeFromStartGUI.text = ((int)totalTime).ToString();
    }

    private void UpdateApmGUI() {
        if (totalTime > 0) {
            apm = Convert.ToInt32((float)charCount / totalTime * 60);
        } else {
            apm = 0;
        }
        apmGUI.text = apm.ToString();
    }

    private void UpdateWordCountGUI() {
        wordCountGUI.text = wordCount.ToString();
    }

    private void UpdateWordCount(int n) {
        wordCount++;
        charCount += n;
        UpdateWordCountGUI();
    }

    // Update is called once per frame
    void Update()
    {
        dt += Time.deltaTime;
        if (enemies.Count == 0 || dt > SpawnDeltaTime) {
            NewEnemyInRandomPlace();
            dt = 0;
        }
        bool hitIt = false;
        bool destroyIt = false;
        if (Time.timeScale == 1) {
            foreach (char c in Input.inputString) {
                if (c >= 'a' && c <= 'z') {
                    for (int i = enemies.Count-1; i >= 0; i--) {
                        if (textEnemies[i].Attack(c)) {
                            hitIt = true;
                            if (textEnemies[i].Down()) {
                                NewExplosionParticleSystem(enemies[i].transform.position + Vector3.back);
                                UpdateWordCount(textEnemies[i].text.Length);
                                EnemyDownByIndex(i);
                                destroyIt = true;
                            }
                        }
                    }
                } else {
                    InvalidAttack();
                }
            }
            if (hitIt) {
                hitSound.Play();
            }
            if (destroyIt) {
                destroySound.clip = audioClips[rnd.Next(audioClips.Length)];
                destroySound.Play();
            }

            bool crashed = false;
            for (int i = enemies.Count-1; i >= 0; i--) {
                if ((enemies[i].transform.position - Vector3.zero).sqrMagnitude < 1) {
                    EnemyDownByIndex(i);
                    crashed = true;
                }
            }
            if (crashed) {
                crashSound.Play();
            }
        }
    }

    private void NewExplosionParticleSystem(Vector3 position) {
        Instantiate(psPrefab, position, Quaternion.identity);
    }

    public void ResetGame() {
        nextUpdateTime = guiUpdateInterval;
        totalTime = 0;
        wordCount = 0;
        charCount = 0;
        apm = 0;
        UpdateApmGUI();
        UpdateTimeGUI();
        UpdateWordCountGUI();
        for (int i = enemies.Count-1; i >= 0; i--) {
            EnemyDownByIndex(i);
        }
    }

    public void TogglePause() {
        if (Time.timeScale == 0) {
            Time.timeScale = 1;
        } else {
            Time.timeScale = 0;
        }
    }

    private void FixedUpdate() {
        totalTime += Time.fixedDeltaTime;
        if (totalTime > nextUpdateTime) {
            UpdateTimeGUI();
            UpdateApmGUI();
            nextUpdateTime += guiUpdateInterval;
        }
    }
}
