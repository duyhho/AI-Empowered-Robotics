using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.Barracuda;
using Unity.MLAgents.Sensors;
using UnityEditor;
using System.Collections.Generic;

public class CollabFireAgent : DungeonAgentFire
{
    public FireLifeScript fireLifeScript;
    float parentOffsetHeight;
    public override void Initialize()
    {
        base.Initialize();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Debug.Log("Collecting Observations");
        if (sensor == null)
        {
            Debug.LogError("Sensor is null.");
            return;
        }

        RaycastUpdateGrid(); //this doesn't collect any observations ,just updating grid cell status;
        if (useVectorObs)
        {
            sensor.AddObservation(StepCount / (float)MaxStep);
        }
    }
    protected override void RaycastUpdateGrid()
    {
        base.RaycastUpdateGrid();
    }
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
    }


    public override void OnActionReceived(float[] vectorAction)
    {
        if (MaxStep != 0)
        {
            AddReward(-1f / MaxStep);
        }

        MoveAgent(vectorAction);

        // AddReward(CalculateRewardForDoor());
        if (fireLifeScript != null)
        {
            if (fireLifeScript.fireLife <= 0)
            {
                SetReward(2f);
                // StartCoroutine(GoalScoredSwapGroundMaterial(m_HallwaySettings.goalScoredMaterial, 0.5f));
                StartCoroutine(DelayedEndEpisode());
            }
            if (isEvaluation)
            {
                UpdateModelStats();
            }
        }

    }

    protected override void OnCollisionEnter(Collision col)
    {
        base.OnCollisionEnter(col);
    }
    protected override void OnCollisionStay(Collision col)
    {
        base.OnCollisionStay(col);
    }
    protected override IEnumerator DelayedEndEpisode()
    {
        yield return new WaitForSeconds(0.001f); // Wait for 1 second
        if (!isEvaluation) //in training mode
        {
            ResetEnvironment();
        }

        EndEpisode();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        // base.OnTriggerEnter(other);
        if (other.gameObject.CompareTag("door_switch"))
        {
            /*Reward is set in Door Button.cs script */
            // AddReward(1f);
            gridManager.SetDoor(transform.position);
            gridManager.SetVisited(transform.position);

        }
        // Debug.Log(other.gameObject.tag);
        if (other.gameObject.CompareTag("fire"))
        {
            // SetReward(2f);

            // if (isEvaluation)
            // {
            //     UpdateModelStats();
            // }
            // StartCoroutine(GoalScoredSwapGroundMaterial(m_HallwaySettings.goalScoredMaterial, 0.5f));
            // // StartCoroutine(SwapGoalMaterial(m_HallwaySettings.waterMaterial, 0.5f));
            // StartCoroutine(DelayedEndEpisode()); // Use the coroutine here

            gridManager.SetFire(transform.position);
            gridManager.SetVisited(transform.position);

        }


    }

    protected override void UpdateModelStats()
    {
        // base.UpdateModelStats();
        if (modelIndex >= 0)
        {
            ModelStats currentStats = modelStatsList[modelIndex];
            float timeToReachFire = Time.time - episodeStartTime;
            currentStats.cumulativeTimeToReachFire += timeToReachFire;

            currentStats.successfulAttempts += 1; // Increment successful attempts
            float averageTime = currentStats.cumulativeTimeToReachFire / currentStats.successfulAttempts;
            currentStats.averageTime = averageTime;

            // Include the model set in your log messages
            // Debug.Log("Model Set: " + modelIndex);
            Debug.Log("Time to reach the fire: " + timeToReachFire + " seconds");
            Debug.Log("Average time to reach the fire successfully (Model Set " + modelIndex + "): " + averageTime + " seconds");
            Debug.Log("Success rate (Model Set " + modelIndex + "): " + ((float)currentStats.successfulAttempts / currentStats.attemptCount) * 100 + "% (" + currentStats.successfulAttempts + "-" + currentStats.attemptCount + ")");
        }
        else
        {
            Debug.Log("Reached Fire without water! Failed!");
        }

    }
    public override void Heuristic(float[] actionsOut)
    {
        base.Heuristic(actionsOut);
    }
    protected override void OnLayoutChange()
    {
        base.OnLayoutChange();
    }

    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
        parentOffsetHeight = modernRoomGenerator.parentOffsetHeight;
        if (parentOffsetHeight <= -10000f)
        {
            parentOffsetHeight = area.transform.position.y;
        }
        Vector3 newStartPosition = roomManager.startPoint + new Vector3(0f, parentOffsetHeight, 0f);
        Vector3 randomFirePosition = roomManager.GetRandomGoalPosition() + new Vector3(0f, parentOffsetHeight, 0f); ;
        if (symbolOGoal)
        {
            fireLifeScript = symbolOGoal.GetComponent<FireLifeScript>();
            // fireLifeScript.Reset();
            symbolOGoal.transform.position = randomFirePosition;
        }
        transform.position = newStartPosition;
        // Reset all doors in the scene

    }
    protected override void CheckCurrentEvaluationModels()
    {
        base.CheckCurrentEvaluationModels();
    }
    public override void ResetEnvironment()
    {
        // base.ResetEnvironment();
        // modernRoomGenerator.ClearOldDungeon();
        // modernRoomGenerator.Generate();
        gridManager.ResetGrid();
        if (symbolOGoal)
        {
            fireLifeScript = symbolOGoal.GetComponent<FireLifeScript>();
            fireLifeScript.Reset();
            // symbolOGoal.transform.position = randomFirePosition;
        }
        CollaborativeAgentScript[] allCollabAgents = area.GetComponentsInChildren<CollaborativeAgentScript>();
        foreach (CollaborativeAgentScript cAgent in allCollabAgents)
        {
            Vector3 randomPosition = roomManager.GetRandomObjectPosition() + new Vector3(0f, parentOffsetHeight, 0f); ;
            // Debug.Log("randomPosition: " + randomPosition);
            cAgent.gameObject.transform.position = randomPosition;
            cAgent.Reset();
        }
    }
    public override void PlayWaterAndStopFire()
    {
        base.PlayWaterAndStopFire();
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    // /// <summary>
    // /// Configures the agent's neural network model based on the specified room configuration.
    // /// </summary>
    // /// <param name="config">The configuration identifier. Accepts values 2, 3, and others for default behavior.</param>
    // protected override void ConfigureAgent(int config)
    // {
    //     base.ConfigureAgent(config);
    // }
}
