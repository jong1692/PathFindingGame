using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private int score = 0;
    [SerializeField]
    private Text scoreText;

    private int life = 3;
    [SerializeField]
    private Text lifeText;
    [SerializeField]
    private Image titleImage;
    [SerializeField]
    private Image defeatImage;
    [SerializeField]
    private Image restartImage;
    [SerializeField]
    private Image victoryImage;
    [SerializeField]
    private UnitController[] units;
    [SerializeField]
    private AudioSource introAudio;
    

    private int shpereCnt;
    private int combo;
    private bool pause = true;

    private float mainTimer = 0;
    private readonly float mainDelay = 4.0f;

    public int Combo
    {
        set { combo = value; }
    }

    public int SphereCnt
    {
        set { shpereCnt = value; }
    }

    public bool Pause
    {
        get { return pause; }
    }

    private void Awake()
    {
        initialize();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && pause == true)
        {
            StartCoroutine(resetStage());

            if (titleImage.gameObject.activeInHierarchy)
                titleImage.gameObject.SetActive(false);
        }

        if (shpereCnt == 0) victory();
    }

    private void victory()
    {
        victoryImage.gameObject.SetActive(true);
        victoryImage.GetComponentInChildren<Text>().text = "CLEAR!\nYOUR SCORE IS " + score.ToString();

        Time.timeScale = 0;
    }

    private void initialize()
    {
        lifeText.text = "X " + life.ToString();

        combo = 0;
        pause = true;
        Time.timeScale = 0;
    }

    private IEnumerator resetStage()
    {
        combo = 0;
        mainTimer = 0;

        pause = false;

        if (introAudio != null)
            introAudio.Play();

        restartImage.gameObject.SetActive(false);

        for (int i = 0; i < units.Length; i++)
        {
            units[i].reset();
        }

        while (mainTimer < mainDelay)
        {
            mainTimer += Time.fixedDeltaTime;
            yield return null;
        }

        Time.timeScale = 1;
    }

    public void increaseScore(int amount = 10, bool useCombo = false)
    {
        if (!useCombo)
        {
            score += amount;
            shpereCnt--;
        }
        else score += amount * ++combo;

        scoreText.text = "SCORE : " + score.ToString();
    }

    public void decreaseLife()
    {
        pause = true;
        Time.timeScale = 0;

        life--;
        lifeText.text = "X " + life.ToString();

        if (life > 0) restartImage.gameObject.SetActive(true);
        else if (life == 0) defeat();
    }

    private void defeat()
    {
        defeatImage.gameObject.SetActive(true);
        defeatImage.GetComponentInChildren<Text>().text = "DEFEAT!\nYOUR SCORE IS " + score.ToString();
    }
}
