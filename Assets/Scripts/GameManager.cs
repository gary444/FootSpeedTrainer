using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    enum GameState
    {
        INTRO,
        REPORT,
        READY,
        WAIT_FOR_HIT,
        POST_HIT
    }


    // GAME CONSTANTS
    private const float minTimeBetweenStateChangesSec = 0.5f;
    private const float maxTimeBetweenStateChangesSec = 1.5f;

    private const float introLength = 3f;
    private const float reportLength = 5f;
    private const float targetActivationTimeout = 3f;
    private const float postHitTime = 0.5f;

    private const int totalTargetsToHit = 5;

    // GAME VARIABLES
    private GameState state = GameState.INTRO;
    private int activeTarget = 0;

    private int numTargetsHit = 0;
    private float targetActivationTime;
    private float averageReactionTime = 0f;

    // GAME OBJECTS
    public AudioClip audioClipSuccess;
    public AudioClip audioClipStart;
    private AudioSource audioSource;
    
    private List<GameObject> targets;

    private APlausEMRInstructionsDisplay instructionDisplay;


    


    void Start()
    {
        FindTargets();
        UpdateTargets();

        audioSource = GetComponent<AudioSource>();

        // Set user position
        OVRManager.display.RecenterPose();


        // limit frame rate to 72
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 72;

        instructionDisplay = GameObject.Find("Instructions").GetComponent<APlausEMRInstructionsDisplay>();
        
        StartCoroutine(ShowIntro());

    }

    void Update()
    {

    }

    public void FindTargets()
    {
        targets = new List<GameObject>();
        GameObject targetRoot = gameObject.transform.GetChild(0).gameObject;
        for (int i = 0; i < targetRoot.transform.childCount; i++)
        {
            GameObject child = targetRoot.transform.GetChild(i).gameObject;
            if (child.name.Contains("Target"))
            {
                targets.Add(child);
            }
        }
        Debug.Log("Found " + targets.Count + " targets");
    }

    IEnumerator ShowIntro()
    {
        state = GameState.INTRO;

        instructionDisplay.ShowInstructions("Touch the targets as fast as you can, using either controller.");
        yield return new WaitForSeconds(introLength);
        instructionDisplay.HideInstructions();

        audioSource.PlayOneShot(audioClipStart);

        instructionDisplay.ShowInstructions("Game starting now...");
        yield return new WaitForSeconds(introLength);
        instructionDisplay.HideInstructions();

        state = GameState.READY;

        StartCoroutine(ActivateNewTarget());

    }

    IEnumerator ShowReport()
    {
        state = GameState.REPORT;

        instructionDisplay.ShowInstructions("Number of targets hit: \n" + numTargetsHit + "\nAverage reaction time:\n" + averageReactionTime + " seconds");
        yield return new WaitForSeconds(reportLength);
        instructionDisplay.HideInstructions();

        // quit game
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif

    }


    IEnumerator WaitForHit()
    {
        bool correctCollisionDetected = false;

        float timeSinceActivation = Time.realtimeSinceStartup - targetActivationTime;

        while (!correctCollisionDetected && timeSinceActivation < targetActivationTimeout)
        {

            correctCollisionDetected = targets[activeTarget].transform.Find("TargetArea").GetComponent<TargetCollisions>().wasCollisionDetected();
            timeSinceActivation = Time.realtimeSinceStartup - targetActivationTime;

            yield return null;
            
        }

        if (correctCollisionDetected)
        {
            audioSource.PlayOneShot(audioClipSuccess);
            ++numTargetsHit;

            averageReactionTime = (averageReactionTime * (numTargetsHit - 1) + timeSinceActivation) / numTargetsHit;

            if (numTargetsHit >= totalTargetsToHit)
            {
                state = GameState.REPORT;
                StartCoroutine(ShowReport());
            }
            else
            {
                state = GameState.POST_HIT;
                StartCoroutine(PostHit());
            }
        }
        else
        {
            state = GameState.READY;
        }
        UpdateTargets();

    }

    IEnumerator ActivateNewTarget()
    {
        float timeToWait = Random.Range(minTimeBetweenStateChangesSec, maxTimeBetweenStateChangesSec);
        yield return new WaitForSeconds(timeToWait);


        activeTarget = Random.Range(0, targets.Count);
        state = GameState.WAIT_FOR_HIT;
        UpdateTargets();

        targetActivationTime = Time.realtimeSinceStartup;

        StartCoroutine(WaitForHit());

    }

    IEnumerator PostHit()
    {
        yield return new WaitForSeconds(postHitTime);
        state = GameState.READY;
        UpdateTargets();

        StartCoroutine(ActivateNewTarget());

    }

    private void UpdateTargets()
    {
        if (state <= GameState.READY)
        {
            foreach (var target in targets)
            {
                target.transform.Find("TargetGeometry").GetComponent<Renderer>().material.SetColor("_Color", Color.gray);
            }
        }
        else if (state == GameState.WAIT_FOR_HIT)
        {
            targets[activeTarget].transform.Find("TargetGeometry").GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        }
        else if (state == GameState.POST_HIT)
        {
            targets[activeTarget].transform.Find("TargetGeometry").GetComponent<Renderer>().material.SetColor("_Color", Color.white);
        }
    }

}
