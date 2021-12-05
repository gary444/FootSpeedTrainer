using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update

    private List<GameObject> targets;

    private const float minTimeBetweenStateChangesSec = 0.5f;
    private const float maxTimeBetweenStateChangesSec = 1.5f;

    private int state = -1;
    private float timeUntilNextStateChange;


    public AudioClip audioClipSuccess;
    private AudioSource audioSource;

    void Start()
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

        timeUntilNextStateChange = Random.Range(minTimeBetweenStateChangesSec, maxTimeBetweenStateChangesSec);

        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        // update targets
        // target state: 
        // -1: nothing is on
        // >= 0: indexed target is on

        // time until next state change - set when state changes and subject on each frame
        // when time until next state change < 0, change state

        // also check for collisions here

        timeUntilNextStateChange -= Time.deltaTime;


        bool correctCollisionDetected = false;

        if (state > -1)
        {
            correctCollisionDetected = targets[state].transform.Find("TargetArea").GetComponent<TargetCollisions>().wasCollisionDetected();

            if (correctCollisionDetected)
            {
                audioSource.PlayOneShot(audioClipSuccess);
                Debug.Log("Correct Collision!");
            }
        }

        if (correctCollisionDetected || timeUntilNextStateChange < 0f)
        {
            // if nothing was on, determine which target should be turned on 
            if (state == -1)
            {
                state = Random.Range(0, targets.Count);
            }
            else
            {
                state = -1;
            }

            timeUntilNextStateChange = Random.Range(minTimeBetweenStateChangesSec, maxTimeBetweenStateChangesSec);

            UpdateTargets();
        }
    }
    private void UpdateTargets()
    {
        if (state == -1)
        {
            foreach (var target in targets)
            {
                target.transform.Find("TargetGeometry").GetComponent<Renderer>().material.SetColor("_Color", Color.gray);
            }
        }
        else
        {
            targets[state].transform.Find("TargetGeometry").GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        }
    }

}
