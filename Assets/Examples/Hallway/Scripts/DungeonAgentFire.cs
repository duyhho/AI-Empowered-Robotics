using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;
public class DungeonAgentFire : Agent
{
    // public GameObject ground;
    private List<Renderer> m_GroundRenderers = new List<Renderer>();
    public GameObject area;
    public GameObject symbolOGoal;
    ParticleSystem fireParticleSystem;
    ParticleSystem waterParticleSystem;
    RoomManager roomManager;

    // public GameObject symbolO;
    // public GameObject symbolX;
    // public GameObject door;
    // public DoorGenerator doorGenerator;

    // public Button3D doorSwitch;
    public ModernRoomGenerator modernRoomGenerator;
    public bool useVectorObs;

    Rigidbody m_AgentRb;
    Material m_GroundMaterial;
    Renderer m_GroundRenderer;
    HallwaySettings m_HallwaySettings;
    int m_Selection;
    openandclosedoor DoorComponent;

    Material m_GoalMaterial;
    Renderer m_GoalRenderer;
    public override void Initialize()
    {
        // Debug.Log("Init");
        // symbolOGoal = modernRoomGenerator.ReturnExitGameObject();
        roomManager = modernRoomGenerator.GetComponent<RoomManager>();
        m_HallwaySettings = FindObjectOfType<HallwaySettings>();
        m_AgentRb = GetComponent<Rigidbody>();
        // m_GroundRenderer = ground.GetComponent<Renderer>();
        GameObject[] groundObjects = GameObject.FindGameObjectsWithTag("ground");

        // Get the Renderer component from each GameObject and add it to our list
        foreach (GameObject groundObject in groundObjects)
        {
            Renderer rend = groundObject.GetComponent<Renderer>();
            if (rend != null)
            {
                m_GroundRenderers.Add(rend);
            }
        }
        // m_GroundMaterial = m_GroundRenderer.material;

        m_GoalRenderer = symbolOGoal.GetComponent<Renderer>();
        m_GoalMaterial = m_GoalRenderer.material;

        // DoorComponent = door.GetComponent<openandclosedoor>();

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Debug.Log("Collecting Observations");

        if (useVectorObs)
        {
            sensor.AddObservation(StepCount / (float)MaxStep);
        }
    }

    IEnumerator GoalScoredSwapGroundMaterial(Material mat, float time)
    {
        if (m_GroundRenderers.Count == 0)
        {
            GameObject[] groundObjects = GameObject.FindGameObjectsWithTag("ground");

            // Get the Renderer component from each GameObject and add it to our list
            foreach (GameObject groundObject in groundObjects)
            {
                Renderer rend = groundObject.GetComponent<Renderer>();
                if (rend != null)
                {
                    m_GroundRenderers.Add(rend);
                }
            }
            // m_GroundMaterial = m_GroundRenderer.material; // Note: You might need a mechanism to set this.
        }

        List<Renderer> validRenderers = new List<Renderer>();

        foreach (Renderer rend in m_GroundRenderers)
        {
            if (rend) // Check if the Renderer is still valid
            {
                validRenderers.Add(rend);
                rend.material = mat;
            }
        }

        yield return new WaitForSeconds(time);

        foreach (Renderer rend in validRenderers)
        {
            if (rend) // Check if the Renderer is still valid
            {
                rend.material = m_GroundMaterial;
            }
        }
    }

    IEnumerator SwapGoalMaterial(Material mat, float time)
    {
        m_GoalRenderer.material = mat;
        yield return new WaitForSeconds(time);
        m_GoalRenderer.material = m_GoalMaterial;
    }
    public void MoveAgent(float[] act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = Mathf.FloorToInt(act[0]);
        switch (action)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
            case 3:
                rotateDir = transform.up * 1f;
                break;
            case 4:
                rotateDir = transform.up * -1f;
                break;
        }
        transform.Rotate(rotateDir, Time.deltaTime * 150f);
        m_AgentRb.AddForce(dirToGo * m_HallwaySettings.agentRunSpeed, ForceMode.VelocityChange);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        if (MaxStep != 0)
        {
            AddReward(-1f / MaxStep);
        }

        MoveAgent(vectorAction);
    }



    // void OnCollisionEnter(Collision col)
    // {
    //     // Debug.Log(col.gameObject.tag);
    //     if (col.gameObject.CompareTag("symbol_O_Goal"))
    //     {
    //         SetReward(2f);
    //         StartCoroutine(GoalScoredSwapGroundMaterial(m_HallwaySettings.goalScoredMaterial, 0.5f));
    //         StartCoroutine(SwapGoalMaterial(m_HallwaySettings.waterMaterial, 0.5f));

    //         PlayWaterAndStopFire();
    //         // EndEpisode();
    //         StartCoroutine(DelayedEndEpisode()); // Use the coroutine here

    //     }
    //     // else
    //     // {
    //     //     SetReward(-0.1f);
    //     //     StartCoroutine(GoalScoredSwapGroundMaterial(m_HallwaySettings.failMaterial, 0.5f));
    //     // }
    //     // DoorComponent.CloseDoor();
    //     // doorSwitch.isPressed = false;

    // }
    private IEnumerator DelayedEndEpisode()
    {
        yield return new WaitForSeconds(0.5f); // Wait for 1 second
        EndEpisode();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("door_switch"))
        {
            Debug.Log("Hit door! Reward!");
            SetReward(1f);
            // EndEpisode();

        }
        Debug.Log(other.gameObject.tag);
        if (other.gameObject.CompareTag("symbol_O_Goal"))
        {
            SetReward(2f);
            StartCoroutine(GoalScoredSwapGroundMaterial(m_HallwaySettings.goalScoredMaterial, 0.5f));
            StartCoroutine(SwapGoalMaterial(m_HallwaySettings.waterMaterial, 0.5f));

            PlayWaterAndStopFire();
            // EndEpisode();
            StartCoroutine(DelayedEndEpisode()); // Use the coroutine here

        }
    }



    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = 0;
        if (Input.GetKey(KeyCode.D))
        {
            actionsOut[0] = 3;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            actionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            actionsOut[0] = 4;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            actionsOut[0] = 2;
        }
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("ON EPISODE BEGIN");
        // Reset all doors in the scene
        DoorController[] allDoors = FindObjectsOfType<DoorController>();
        foreach (DoorController door in allDoors)
        {
            door.Reset();
        }
        if (symbolOGoal)
        {
            Vector3 randomFirePosition = roomManager.GetRandomGoalPosition();
            // Debug.Log("RoomManager.GetRandomGoalPosition()" + randomFirePosition);
            symbolOGoal.transform.position = randomFirePosition;
            m_GoalRenderer = symbolOGoal.GetComponent<Renderer>();
            m_GoalMaterial = m_GoalRenderer.material;

            fireParticleSystem = symbolOGoal.transform.Find("CFX4 Fire").GetComponent<ParticleSystem>();
            // waterParticleSystem = symbolOGoal.transform.Find("CFX2_Big_Splash (No Collision)").GetComponent<ParticleSystem>();
            fireParticleSystem.Clear();
            fireParticleSystem.Play();
        }
        transform.position = roomManager.GetStartPoint() + new Vector3(0f, 0.5f, 0f);
        transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        m_AgentRb.velocity *= 0f;

    }
    public void PlayWaterAndStopFire()
    {
        Debug.Log("Play Water");
        if (symbolOGoal)
        {
            fireParticleSystem = symbolOGoal.transform.Find("CFX4 Fire").GetComponent<ParticleSystem>();
            waterParticleSystem = symbolOGoal.transform.Find("CFX2_Big_Splash (No Collision)").GetComponent<ParticleSystem>();
            if (!waterParticleSystem.gameObject.activeInHierarchy)
            {
                waterParticleSystem.gameObject.SetActive(true);
            }

            if (!waterParticleSystem.isPlaying)
            {
                waterParticleSystem.Play();
            }

            if (fireParticleSystem.isPlaying)
            {
                fireParticleSystem.Stop();
            }
        }

    }

}
