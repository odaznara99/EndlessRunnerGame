using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public float score;
    public float highScore;
    public float scoreMilestone= 1000;
    public int currentLevel = 1; // the current difficulty
    public GameObject levelReachedDisplay;
    public int coin;
    public float obsSpeedMultiplier = 1;
    public float backgroundSpeed;
    private PlayerController playerControllerScript;
    private SpawnManager spawnManagerScript;

    public Transform startingPoint;
    public float lerpSpeed;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI highScoreText = null;

    public bool gameIsPaused;
    public bool setNewHighScore;
    public bool reachingMilestone = false;
    void Start()
    {
        playerControllerScript = GameObject.Find("Player").GetComponent<PlayerController>(); //reference PlayerController
        spawnManagerScript = GameObject.Find("SpawnManager").GetComponent<SpawnManager>(); // reference SpawnManager
        score = 0;
        coin = 0;
        //scoreMilestone = 1000; // meaning every 500 is the milestone
        spawnManagerScript.repeatRate = 3.0f;
        playerControllerScript.gameOver = true;
        StartCoroutine(PlayIntro());

        //Get the HighScore from PlayerPrefs
        highScore = PlayerPrefs.GetFloat("HighScore", 0);
        highScoreText.text = highScore.ToString();

    }

    // Update is called once per frame
    void Update()
    {
        if (!playerControllerScript.gameOver  && !gameIsPaused)

        {
            coinText.text = coin.ToString(); // display the coins to the UI text
            if (playerControllerScript.doubleSpeed)
            {
                score += 2;
            }
            else {
                //Add score per frame
                score++;
            }
            //Debug.Log("Score: " + score);
            scoreText.text = score.ToString(); //display the score to the UI text

            if (spawnManagerScript.repeatRate <= 1) {
                spawnManagerScript.repeatRate = 1f; // limit the minimum repeatRate
            }
            // if the current score REACHED the scoreMilestone, the DIFFULTY will increase
            else if (score >= scoreMilestone && spawnManagerScript.repeatRate !=1) {
                currentLevel++;
                spawnManagerScript.repeatRate -= 0.1f; // minus the repeatRate by 0.1 second
                obsSpeedMultiplier += 0.2f; // the Add Speed in Moving Left Objects
                scoreMilestone += (scoreMilestone*1.2f); // add the Milestone*2 to itself
                StartCoroutine(ReachMilestone());
                
            }

        }

        if (Input.GetKeyDown(KeyCode.R)) {
            RestartGame();
        }

        // Check if the current score beats the high score
        if (score > highScore)
        {          
            highScore = score;
            setNewHighScore = true;
            PlayerPrefs.SetFloat("HighScore", highScore);
            highScoreText.text = highScore.ToString();
        }

    }

    IEnumerator PlayIntro()
    {
        Vector3 startPos = playerControllerScript.transform.position;
        Vector3 endPos = startingPoint.position;
        float journeyLength = Vector3.Distance(startPos, endPos);
        float startTime = Time.time;
        float distanceCovered = (Time.time - startTime) * lerpSpeed;
        float fractionOfJourney = distanceCovered / journeyLength;
        playerControllerScript.GetComponent<Animator>().SetFloat("Speed_Multiplier",
        0.5f);
        while (fractionOfJourney < 1)
        {
            distanceCovered = (Time.time - startTime) * lerpSpeed;
            fractionOfJourney = distanceCovered / journeyLength;
            playerControllerScript.transform.position = Vector3.Lerp(startPos, endPos,
            fractionOfJourney);
            yield return null;
        }
        playerControllerScript.GetComponent<Animator>().SetFloat("Speed_Multiplier",
        1.0f);
        playerControllerScript.gameOver = false;
        playerControllerScript.topPanel.SetActive(true);
    }

    IEnumerator ReachMilestone()
    {
        //Stop spawning obstacles for 3 seconds
        Debug.Log("Reached Level: " + currentLevel);
        reachingMilestone = true;
        spawnManagerScript.StopSpawning(true);
        levelReachedDisplay.SetActive(true);
        playerControllerScript.topPanel.SetActive(false);
        playerControllerScript.playerInvincible = true;

        yield return new WaitForSeconds(5f);

        reachingMilestone = false;
        spawnManagerScript.StopSpawning(false);
        levelReachedDisplay.SetActive(false);
        playerControllerScript.topPanel.SetActive(true);
        playerControllerScript.playerInvincible = false;
        Debug.Log("Level Transition: Invincible time out.");
        Debug.Log("Level Transition: Spawning obstacles again... ");

    }

        public void RestartGame() {
        Physics.gravity = new Vector3(0, -9.8f, 0); //default gravity
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Home() {
        Physics.gravity = new Vector3(0, -9.8f, 0); //default gravity
        SceneManager.LoadScene("Main Menu");

    }

    public void QuitGame() {
        Application.Quit();

    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetFloat("HighScore", highScore);
        PlayerPrefs.Save();
    }

    public void ResetHighScore()
    {
        PlayerPrefs.DeleteKey("HighScore");
        highScore = 0;
    }

    public void PauseGame() {
        playerControllerScript.pausedPanel.SetActive(true); //display the pause panel
        playerControllerScript.topPanel.SetActive(false); //undisplay top panel
        gameIsPaused = true;
        Time.timeScale = 0; // pause the game
    }

    public void ResumeGame() {
        playerControllerScript.pausedPanel.SetActive(false); //remove the pause panel
        playerControllerScript.topPanel.SetActive(true); //display top panel
        gameIsPaused = false;
        Time.timeScale = 1; // resume the game

    }

    public float GetCurrentScore() {

        return score;
    
    }

    public float GetHighScore()
    {

        return highScore;

    }
}
