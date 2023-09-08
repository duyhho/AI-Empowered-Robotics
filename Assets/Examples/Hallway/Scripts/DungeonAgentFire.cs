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
    public GridManager gridManager;
    Vector2Int lastGridPosition;
    public float upAngle = 15f;
    public float downAngle = 15f;
    public float sideAngle = 25f; // angle for the side rays
    public float rayDistance = 15.0f; // Change as needed

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
        RaycastUpdateGrid();
        if (useVectorObs)
        {
            sensor.AddObservation(StepCount / (float)MaxStep);
        }
    }
    void RaycastUpdateGrid()
    {

        Vector3 rayOrigin = transform.position;

        // Get the current rotation of the agent
        Quaternion agentRotation = transform.rotation;

        // Initialize the forward direction in world space
        Vector3 rayDirection = agentRotation * Vector3.forward;

        // To create a downward ray, we first create it in world space, then apply the agent's rotation
        Vector3 rayDirectionDown = agentRotation * (Quaternion.Euler(downAngle, 0, 0) * Vector3.forward);

        // Define directions for diverging rays
        Vector3 rayDirectionDownLeft = Quaternion.Euler(0, -sideAngle, 0) * rayDirectionDown;
        Vector3 rayDirectionDownRight = Quaternion.Euler(0, sideAngle, 0) * rayDirectionDown;
        Vector3 rayDirectionLeft = Quaternion.Euler(0, -sideAngle, 0) * rayDirection;
        Vector3 rayDirectionRight = Quaternion.Euler(0, sideAngle, 0) * rayDirection;

        Vector3[] rayDirections = {
        rayDirection, rayDirectionDown, rayDirectionDownLeft,
        rayDirectionDownRight, rayDirectionLeft, rayDirectionRight
    };
        RaycastHit hitInfo;

        foreach (var dir in rayDirections)
        {
            if (Physics.Raycast(rayOrigin, dir, out hitInfo, rayDistance))
            {
                // Debug.Log("hitInfo.collider.gameObject: " + hitInfo.collider.gameObject.tag);
                if (hitInfo.collider.gameObject.CompareTag("wall"))
                {
                    gridManager.SetWall(hitInfo.point);
                }
                else if (hitInfo.collider.gameObject.CompareTag("ground"))
                {
                    gridManager.SetGround(hitInfo.point);
                }
                else if (hitInfo.collider.gameObject.CompareTag("door_switch"))
                {
                    Debug.Log("Raycast hit door!");
                    gridManager.SetDoor(hitInfo.point);
                }
                else if (hitInfo.collider.gameObject.CompareTag("symbol_O_Goal") || hitInfo.collider.gameObject.CompareTag("fire"))
                {
                    gridManager.SetFire(hitInfo.point);
                }
            }
        }
    }
    void OnDrawGizmos()
    {

        // float downAngle = 10f;
        // float sideAngle = 25f; // angle for the side rays
        Vector3 rayOrigin = transform.position;

        // Get the current rotation of the agent
        Quaternion agentRotation = transform.rotation;

        // Initialize the forward direction in world space
        Vector3 rayDirection = agentRotation * Vector3.forward;

        // To create a downward ray, we first create it in world space, then apply the agent's rotation
        Vector3 rayDirectionDown = agentRotation * (Quaternion.Euler(downAngle, 0, 0) * Vector3.forward);

        // Define directions for diverging rays
        Vector3 rayDirectionDownLeft = Quaternion.Euler(0, -sideAngle, 0) * rayDirectionDown;
        Vector3 rayDirectionDownRight = Quaternion.Euler(0, sideAngle, 0) * rayDirectionDown;
        Vector3 rayDirectionLeft = Quaternion.Euler(0, -sideAngle, 0) * rayDirection;
        Vector3 rayDirectionRight = Quaternion.Euler(0, sideAngle, 0) * rayDirection;

        Vector3[] rayDirections = {
        rayDirection, rayDirectionDown, rayDirectionDownLeft,
        rayDirectionDownRight, rayDirectionLeft, rayDirectionRight
    };

        Color[] rayColors = {
        Color.red, Color.blue, Color.yellow,
        Color.green, Color.magenta, Color.cyan
    };

        for (int i = 0; i < rayDirections.Length; i++)
        {
            Gizmos.color = rayColors[i];
            Gizmos.DrawLine(rayOrigin, rayOrigin + rayDirections[i] * rayDistance);
        }

        // ... (rest of your existing OnDrawGizmos code)
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
        if (m_GoalRenderer)
        {
            m_GoalRenderer.material = mat;
            yield return new WaitForSeconds(time);
            m_GoalRenderer.material = m_GoalMaterial;
        }
        else
        {

        }
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



    void OnCollisionEnter(Collision col)
    {
        // Debug.Log(col.gameObject.tag);
        // if (col.gameObject.CompareTag("ground"))
        // {
        //     Debug.Log("Set ground!!");

        //     gridManager.SetGround(transform.position);
        // }
        if (col.gameObject.CompareTag("wall"))
        {
            Debug.Log("Hit wall!!");

            gridManager.SetWall(transform.position);
            gridManager.SetVisited(transform.position);


        }
    }
    void OnCollisionStay(Collision col)
    {
        if (col.gameObject.CompareTag("ground"))
        {
            Vector2Int currentGridPosition = gridManager.WorldToGrid(transform.position);

            if (currentGridPosition != lastGridPosition)
            {
                // Debug.Log("Set ground!!");

                gridManager.SetGround(transform.position);
                gridManager.SetVisited(transform.position);
                lastGridPosition = currentGridPosition;
            }
        }
    }
    private IEnumerator DelayedEndEpisode()
    {
        yield return new WaitForSeconds(0.6f); // Wait for 1 second
        gridManager.ResetGrid();
        EndEpisode();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("door_switch"))
        {
            Debug.Log("Hit door! Reward!");
            SetReward(1f);
            // EndEpisode();
            gridManager.SetDoor(transform.position);
            gridManager.SetVisited(transform.position);

        }

        Debug.Log(other.gameObject.tag);
        if (other.gameObject.CompareTag("symbol_O_Goal") || other.gameObject.CompareTag("fire"))
        {
            SetReward(2f);
            StartCoroutine(GoalScoredSwapGroundMaterial(m_HallwaySettings.goalScoredMaterial, 0.5f));
            StartCoroutine(SwapGoalMaterial(m_HallwaySettings.waterMaterial, 0.5f));

            PlayWaterAndStopFire();
            // EndEpisode();
            StartCoroutine(DelayedEndEpisode()); // Use the coroutine here

            gridManager.SetFire(transform.position);
            gridManager.SetVisited(transform.position);

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
        gridManager.ResetGrid();
        // Reset all doors in the scene
        DoorController[] allDoors = area.GetComponentsInChildren<DoorController>();
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
