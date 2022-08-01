using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    private static int MapWidth = 28;
    private static int Mapheight = 36;

    public int TotalPellets = 0;
    public static int Score = 0;
    public static int HighScore = 10000;
    public GameObject[,] Map = new GameObject[MapWidth, Mapheight];
    public static int Lives = 3;
    private static int Level = 1;

    private bool isDying;
    private bool isConsuming;

    public AudioClip StartBGM;
    public AudioClip BGM;
    public AudioClip FrightenedBGM;
    public AudioClip DeathBGM;
    public AudioClip ConsumedGhostBGM;

    public Text AREYOU;
    public Text Ready;
    public Text ConsumedScore;
    public Text HighScoreText;
    public Text ScoreText;
    public Image Life1;
    public Image Life2;

    // Start is called before the first frame update
    void Start()
    {
        if (Level == 1) 
        {
            Score = 0;
            ScoreText.text = 0.ToString();
            Lives = 3;
        }
        else //return to level 1 as we have not implemented changing levels
        {
            Level = 1;
        }

        Object[] Objects = GameObject.FindObjectsOfType(typeof(GameObject)); // makes an array of objects 
        foreach(GameObject Object in  Objects)
        {
            Vector2 position = Object.transform.position;

            if (Object.name != "Pacman" && Object.name != "Main Camera" && Object.name != "Nodes" && Object.tag != "Tile" && Object.tag != "Ghost" && Object.tag != "ScatterTarget" && Object.tag != "Canvas" && Object.name != "MAP" && Object.name != "Pellets" && Object.name != "PacmanReference" && Object.name !=  "GameMaster")
            {// Theoretically all these conditions could be replaced by tagging the elements We want to add in the array, and only allowing those, However that failed massively sooooo
                if(Object.GetComponent<Tile>() != null) 
                {
                    if(Object.GetComponent<Tile>().isPellet || Object.GetComponent<Tile>().isEnergizedPellet) 
                    {
                        TotalPellets++;
                    }
                }
                //Debug.Log(Object.name);
                Map[(int)position.x, (int)position.y] = Object;
            }
        }

        StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        CheckBGM();
        CheckUI();
        CheckWin();
        
    }


    public void StartGame() 
    {
        transform.GetComponent<AudioSource>().clip = StartBGM;
        transform.GetComponent<AudioSource>().Play();
        AREYOU.transform.GetComponent<Text>().enabled = true;
        Ready.transform.GetComponent<Text>().enabled = true;

        GameObject[] Ghosts = GameObject.FindGameObjectsWithTag("Ghost");
        
        foreach(GameObject Ghost in Ghosts) 
        {
            Ghost.transform.GetComponent<SpriteRenderer>().enabled = false;
            Ghost.transform.GetComponent<Ghost>().CanMove = false;
        }

        GameObject Pacman = GameObject.FindGameObjectWithTag("Pacman");
        Pacman.transform.GetComponent<SpriteRenderer>().enabled = false;
        Pacman.transform.GetComponent<Pac>().CanMove = false;

        StartCoroutine(ShowAfter(2.25f));
    }

    public void CheckBGM() 
    {
        if(transform.GetComponent<AudioSource>().clip == FrightenedBGM) 
        {
            int BGMReset = 0;
            int GhostCount = 0;

            GameObject[] Ghosts = GameObject.FindGameObjectsWithTag("Ghost");

            foreach (GameObject Ghost in Ghosts)
            {
                if (!Ghost.GetComponent<Ghost>().isFrightened)
                {
                    BGMReset++;
                }

                GhostCount++;
            }
            if (BGMReset == GhostCount)
            {
                transform.GetComponent<AudioSource>().clip = BGM;
                transform.GetComponent<AudioSource>().Play();
            }
        } 
    }

    public void CheckUI() 
    {
        if(Score > HighScore) 
        {
            HighScoreText.text = Score.ToString();
        }
        ScoreText.text = Score.ToString();
        
        if (Lives <= 2)
        {
            Life2.enabled = false;
        }
        if (Lives == 1)
        {
            Life1.enabled = false;
        }

    }

    public void CheckWin() 
    {
        GameObject Pacman = GameObject.Find("Pacman");
        if (Pacman.GetComponent<Pac>().PelletsConsumed == TotalPellets) 
        {
            transform.GetComponent<AudioSource>().Stop();
            Pacman.transform.GetComponent<Pac>().CanMove = false;
            Pacman.transform.GetComponent<Animator>().enabled = false;
            
            GameObject[] Ghosts = GameObject.FindGameObjectsWithTag("Ghost");

            foreach (GameObject Ghost in Ghosts)
            {

                Ghost.transform.GetComponent<Ghost>().CanMove = false;
                Ghost.transform.GetComponent<Animator>().enabled = false;
            }
            StartCoroutine(ProcessWin(2));
        }
    }

    public void Restart()
    {
        isDying = false;
        Lives -= 1;
        if(Lives <= 0) 
        {
            AREYOU.transform.GetComponent<Text>().text = "Game Over";
            AREYOU.transform.GetComponent<Text>().color = Color.red;
            AREYOU.transform.GetComponent<Text>().fontSize = 24;

            AREYOU.transform.GetComponent<Text>().enabled = true;

            Ready.transform.GetComponent<Text>().text = "Continue?";
            Ready.transform.GetComponent<Text>().fontSize = 24;

            Ready.transform.GetComponent<Text>().enabled = true;

            StartCoroutine(ProcessGameOver(2));
        }
        else 
        {
            if(Lives == 2) 
            {
                Life2.enabled = false;
            }
            else if(Lives == 1) 
            {
                Life1.enabled = false;
            }

            GameObject Pacman = GameObject.Find("Pacman");
            Pacman.transform.GetComponent<Pac>().Restart();
            GameObject[] arr = GameObject.FindGameObjectsWithTag("Ghost");
            foreach (GameObject Ghost in arr)
            {
                Ghost.transform.GetComponent<Ghost>().Restart();
            }

            StartGame();
        }
        

    }


    public void StartDeath()
    {
        if (!isDying)
        {
            isDying = true;
            GameObject[] ar = GameObject.FindGameObjectsWithTag("Ghost");
            foreach (GameObject Ghost in ar)
            {
                Ghost.transform.GetComponent<Ghost>().CanMove = false;
            }
            GameObject Pacman = GameObject.Find("Pacman");
            Pacman.transform.GetComponent<Pac>().CanMove = false;
            Pacman.transform.GetComponent<Animator>().enabled = false;
            transform.GetComponent<AudioSource>().Stop();
            StartCoroutine(ProcessDeathAfter(2));
        }
    }

    public void StartConsume(Ghost ConsumedGhost) 
    {
        if (!isConsuming) 
        {
            isConsuming = true;
            GameObject[] Ghosts = GameObject.FindGameObjectsWithTag("Ghost");
            foreach(GameObject Ghost in Ghosts) 
            {
                Ghost.transform.GetComponent<Ghost>().CanMove = false;
            }

            GameObject Pacman = GameObject.FindGameObjectWithTag("Pacman");
            Pacman.transform.GetComponent<Pac>().CanMove = false;
            Pacman.transform.GetComponent<SpriteRenderer>().enabled = false;

            ConsumedGhost.transform.GetComponent<SpriteRenderer>().enabled = false;

            transform.GetComponent<AudioSource>().Stop();

            Vector2 WorldView = Camera.main.WorldToViewportPoint(ConsumedGhost.transform.position); // UI Has Different Position System than the other Objects
            
            ConsumedScore.GetComponent<RectTransform>().anchorMin = WorldView;
            ConsumedScore.GetComponent<RectTransform>().anchorMax = WorldView;
            ConsumedScore.GetComponent<Text>().enabled = true;
            
            transform.GetComponent<AudioSource>().PlayOneShot(ConsumedGhostBGM);

            StartCoroutine(ProcessConsumeGhost(0.75f, ConsumedGhost));
        }
    }

    IEnumerator ShowAfter(float delay) 
    {
        yield return new WaitForSeconds(delay);
        
        GameObject[] Ghosts = GameObject.FindGameObjectsWithTag("Ghost");

        foreach (GameObject Ghost in Ghosts)
        {
            Ghost.transform.GetComponent<SpriteRenderer>().enabled = true;
        }

        GameObject Pacman = GameObject.FindGameObjectWithTag("Pacman");
        Pacman.transform.GetComponent<SpriteRenderer>().enabled = true;

        AREYOU.transform.GetComponent<Text>().enabled = false;

        StartCoroutine(StartAfter(2f));
    }

    IEnumerator StartAfter(float delay) 
    {
        yield return new WaitForSeconds(delay);

        GameObject[] Ghosts = GameObject.FindGameObjectsWithTag("Ghost");

        foreach (GameObject Ghost in Ghosts)
        {
            Ghost.transform.GetComponent<Ghost>().CanMove = true;
        }

        GameObject Pacman = GameObject.FindGameObjectWithTag("Pacman");
        
        Pacman.transform.GetComponent<Pac>().CanMove = true;

        Ready.transform.GetComponent<Text>().enabled = false;

        transform.GetComponent<AudioSource>().clip = BGM;
        transform.GetComponent<AudioSource>().Play();
    }

    IEnumerator ProcessDeathAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameObject[] arrr = GameObject.FindGameObjectsWithTag("Ghost");
        foreach (GameObject Ghost in arrr)
        {
            Ghost.transform.GetComponent<SpriteRenderer>().enabled = false;
        }

        StartCoroutine(ProcessDeathAnimation(1.9f));
    }

    IEnumerator ProcessDeathAnimation(float delay) 
    {
        GameObject Pacman = GameObject.FindGameObjectWithTag("Pacman");
        Pacman.transform.localScale= new Vector3(1, 1, 1);
        Pacman.transform.localRotation = Quaternion.Euler(0,0,0);
        Pacman.transform.GetComponent<Animator>().runtimeAnimatorController = Pacman.transform.GetComponent<Pac>().Death;
        Pacman.transform.GetComponent<Animator>().enabled = true;
        
        transform.GetComponent<AudioSource>().clip = DeathBGM;
        transform.GetComponent<AudioSource>().Play();
        
        yield return new WaitForSeconds(delay);
        StartCoroutine(ProcessRestart(2));
    }

    IEnumerator ProcessRestart(float delay) 
    {
        GameObject Pacman = GameObject.FindGameObjectWithTag("Pacman");
        Pacman.transform.GetComponent<SpriteRenderer>().enabled = false;
        
        transform.GetComponent<AudioSource>().Stop();

        yield return new WaitForSeconds(delay);

        Restart();
    }

    IEnumerator ProcessWin(float delay) 
    {
        yield return new WaitForSeconds(delay);

        Level++;

        SceneManager.LoadScene("Level");
    }

    IEnumerator ProcessGameOver(float delay)
    {
        yield return new WaitForSeconds(delay);

        SceneManager.LoadScene("Menu");

    }

    IEnumerator ProcessConsumeGhost(float delay, Ghost ConsumedGhost) 
    {
        yield return new WaitForSeconds(delay);
        
        ConsumedScore.GetComponent<Text>().enabled = false;

        GameObject Pacman = GameObject.FindGameObjectWithTag("Pacman");
        Pacman.transform.GetComponent<SpriteRenderer>().enabled = true;

        ConsumedGhost.transform.GetComponent<SpriteRenderer>().enabled = true;

        GameObject[] Ghosts = GameObject.FindGameObjectsWithTag("Ghost");
        foreach (GameObject Ghost in Ghosts)
        {
            Ghost.transform.GetComponent<Ghost>().CanMove = true;
        }

        transform.GetComponent<AudioSource>().Play();

        isConsuming = false;

        Pacman.transform.GetComponent<Pac>().CanMove = true;
    }

}
