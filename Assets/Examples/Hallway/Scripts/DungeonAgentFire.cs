using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.Barracuda;
using Unity.MLAgents.Sensors;
using UnityEditor;
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
    public float sideAngle = 15f; // angle for the side rays
    public float rayDistance = 15.0f; // Change as needed
    public bool drawGizmos = false;
    // Depending on this value, the wall will have different room counts
    public int m_Configuration;

    [Header("Model Library")]
    public NNModel twoRoomBrain;

    public NNModel threeRoomBrain;

    public NNModel fourRoomBrain;
    public NNModel fiveRoomBrain;
    public NNModel dynamicRoomBrain;


    [Header("Evaluation Metrics")]
    public int maxAttempts = 5;
    public bool isEvaluation = false;
    private float episodeStartTime;
    private float cumulativeTimeToReachFire = 0f;
    private int attemptCount = 0;
    private float averageTime = 0f;
    private bool isSuccessful = false;
    private int successfulAttempts = 0;
    [SerializeField]
    int modelIndex = 0;
    public NNModel twoRoomBrain2;

    public NNModel threeRoomBrain2;

    public NNModel fourRoomBrain2;
    public NNModel fiveRoomBrain2;
    public NNModel dynamicRoomBrain2;
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
        if (sensor == null)
        {
            Debug.LogError("Sensor is null.");
            return;
        }

        RaycastUpdateGrid(); //this doesn't collect any observations ,just updating grid cell status;
        if (useVectorObs)
        {
            // // Agent's current position
            // Vector2Int agentGridPosition = gridManager.WorldToGrid(transform.position);

            // // Getting the grid in front of the agent
            // Vector2Int[] frontGrids = gridManager.GetFrontGrid(agentGridPosition, 5);
            // // bool hasGround = false;
            // bool hasWall = false;
            // bool hasDoor = false;
            // bool hasFire = false;

            // Vector2Int doorGridPosition = new Vector2Int(0, 0);
            // foreach (var gridPosition in frontGrids)
            // {
            //     // Get the properties of the cell at the current grid position
            //     var cell = gridManager.GetGridCell(gridPosition.x, gridPosition.y);
            //     // Update the variables if the current cell contains the respective properties
            //     if (cell != null)
            //     {
            //         // if (!hasGround) hasGround = cell.hasGround;
            //         if (!hasWall) hasWall = cell.hasWall;
            //         if (!hasDoor)
            //         {
            //             hasDoor = cell.hasDoor;
            //             doorGridPosition = gridPosition;
            //         }
            //         if (!hasFire) hasFire = cell.hasFire;
            //     }
            // }
            // // sensor.AddObservation(hasGround);
            // sensor.AddObservation(hasWall);
            // sensor.AddObservation(hasDoor);
            // if (hasDoor)
            // {
            //     // Compute direction to the nearest door
            //     Vector3 agentWorldPosition = transform.position;
            //     Vector3 doorWorldPosition = gridManager.GridToWorld(doorGridPosition);  // assuming GridToWorld converts a grid position to a world position
            //     Vector3 directionToDoor = doorWorldPosition - agentWorldPosition;
            //     Vector2 directionToDoor2D = new Vector2(directionToDoor.x, directionToDoor.z);
            //     sensor.AddObservation(directionToDoor2D.normalized);
            //     Debug.Log("hasDoor-true: direction:" + directionToDoor2D.normalized);
            // }
            // else
            // {
            //     // If no door is found, add a zero vector as the observation
            //     sensor.AddObservation(Vector2.zero);
            // }

            // sensor.AddObservation(hasFire);
            sensor.AddObservation(StepCount / (float)MaxStep);
            // Logging the observations to Unity console
            // Debug.Log($"Observations: hasWall - {hasWall}, hasDoor - {hasDoor}, hasFire - {hasFire}");
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
                    // Debug.Log("Raycast hit door!");
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
        if (!drawGizmos)
            return;
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

        // AddReward(CalculateRewardForDoor());

    }

    // A stub function for calculating a reward based on the direction to a door
    private float CalculateRewardForDoor()
    {
        Vector2Int agentGridPosition = gridManager.WorldToGrid(transform.position);
        Vector2Int[] frontGrids = gridManager.GetFrontGrid(agentGridPosition, 5);

        bool hasWall = false;
        bool hasDoor = false;
        bool hasFire = false;

        Vector2Int doorGridPosition = new Vector2Int(0, 0);
        foreach (var gridPosition in frontGrids)
        {
            var cell = gridManager.GetGridCell(gridPosition.x, gridPosition.y);
            if (cell != null)
            {
                if (!hasWall) hasWall = cell.hasWall;
                if (!hasDoor)
                {
                    hasDoor = cell.hasDoor;
                    doorGridPosition = gridPosition;
                }
                if (!hasFire) hasFire = cell.hasFire;
            }
        }

        if (hasDoor)
        {

            Debug.Log("Detected door: Facing towards the door, positive reward granted.");
            return 0.1f;

        }
        if (hasFire)
        {

            Debug.Log("Detected door: Facing towards the door, positive reward granted.");
            return 0.2f;

        }
        return 0f;
    }


    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("wall"))
        {
            // Debug.Log("Hit wall!!");
            // AddReward(-0.1f);
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
                /*Reward is set in Grid Manager.cs script for exploring new areas */
                gridManager.SetVisited(transform.position);
                lastGridPosition = currentGridPosition;
            }
        }
        if (col.gameObject.CompareTag("wall"))
        {
            // Debug.Log("Hit wall!!");
            // AddReward(-0.1f);
            gridManager.SetWall(transform.position);
            gridManager.SetVisited(transform.position);


        }
    }
    private IEnumerator DelayedEndEpisode()
    {
        yield return new WaitForSeconds(0.01f); // Wait for 1 second
        if (!isEvaluation)
        {
            ResetEnvironment();
        }
        EndEpisode();

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("door_switch"))
        {
            /*Reward is set in Door Button.cs script */
            // AddReward(1f);
            gridManager.SetDoor(transform.position);
            gridManager.SetVisited(transform.position);

        }

        // Debug.Log(other.gameObject.tag);
        if (other.gameObject.CompareTag("symbol_O_Goal") || other.gameObject.CompareTag("fire"))
        {
            float timeToReachFire = Time.time - episodeStartTime;
            cumulativeTimeToReachFire += timeToReachFire;

            successfulAttempts += 1; // Increment successful attempts
            isSuccessful = true;
            averageTime = cumulativeTimeToReachFire / successfulAttempts;
            Debug.Log("Time to reach the fire: " + timeToReachFire + " seconds");
            Debug.Log("Average time to reach the fire successfully: " + averageTime + " seconds");
            Debug.Log("Success rate: " + ((float)successfulAttempts / attemptCount) * 100 + "% (" + successfulAttempts + "-" + attemptCount + ")");



            SetReward(2f);
            StartCoroutine(GoalScoredSwapGroundMaterial(m_HallwaySettings.goalScoredMaterial, 0.5f));
            StartCoroutine(SwapGoalMaterial(m_HallwaySettings.waterMaterial, 0.5f));
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
    private void OnLayoutChange()
    {
        cumulativeTimeToReachFire = 0f;
        attemptCount = 0;
        averageTime = 0f;
        successfulAttempts = 0;
    }
    public override void OnEpisodeBegin()
    {
        attemptCount += 1;

        Debug.Log("ON EPISODE BEGIN" + "- Attempt Count: " + attemptCount + "Success: " + successfulAttempts);

        // Debug.Log("Attempt Count: " + attemptCount);

        isSuccessful = false; // Reset the success flag for the new episode
        episodeStartTime = Time.time;
        m_Configuration = modernRoomGenerator.maximumRoomCount;
        gridManager.ResetGrid();

        // Reset all doors in the scene
        DoorController[] allDoors = area.GetComponentsInChildren<DoorController>();
        foreach (DoorController door in allDoors)
        {
            door.Reset();
        }

        if (symbolOGoal)
        {
            // Vector3 randomFirePosition = roomManager.GetRandomGoalPosition();
            // symbolOGoal.transform.position = randomFirePosition;
            m_GoalRenderer = symbolOGoal.GetComponent<Renderer>();
            m_GoalMaterial = m_GoalRenderer.material;

            Transform fireTransform = symbolOGoal.transform.Find("CFX4 Fire");
            if (fireTransform != null)
            {
                fireParticleSystem = fireTransform.GetComponent<ParticleSystem>();

                if (fireParticleSystem != null)
                {
                    fireParticleSystem.Clear();
                    fireParticleSystem.Play();
                }
                else
                {
                    Debug.LogWarning("CFX4 Fire component does not have a ParticleSystem attached.");
                }
            }
            else
            {
                Debug.LogWarning("CFX4 Fire object could not be found.");
            }
        }
        else
        {
            Debug.LogWarning("symbolOGoal is not set.");
        }

        transform.position = roomManager.GetStartPoint() + new Vector3(0f, 0.5f, 0f);
        transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        m_AgentRb.velocity *= 0f;
    }
    void OnEpisodeEnd()
    {
        if (isEvaluation)
        {
            // Switch model for the next episode
            modelIndex += 1;

            // If all models have been tested in the current layout
            if (modelIndex == 2)
            {
                modelIndex = 0;  // Reset the index
                ResetEnvironment();// Generate a new environment layout here
            }

        }
        if (isEvaluation && attemptCount >= maxAttempts)
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }


    }
    public void ResetEnvironment()
    {
        modernRoomGenerator.ClearOldDungeon();
        modernRoomGenerator.Generate();
        gridManager.ResetGrid();
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
    void FixedUpdate()
    {
        if (m_Configuration != -1)
        {
            ConfigureAgent(m_Configuration);
            m_Configuration = -1;
        }
    }

    /// <summary>
    /// Configures the agent's neural network model based on the specified room configuration.
    /// </summary>
    /// <param name="config">The configuration identifier. Accepts values 2, 3, and others for default behavior.</param>
    void ConfigureAgent(int config)
    {
        if (!isEvaluation)
        {
            Debug.Log("Config: " + config);
            if (twoRoomBrain == null || threeRoomBrain == null || dynamicRoomBrain == null)
            {
                Debug.LogError("CUSTOM ERROR: One or more brain models are null. Please assign brain models in the inspector.");
                return;
            }
            if (config == 2)
            {
                SetModel("TwoRoom", twoRoomBrain);
            }
            else if (config == 3)
            {

                SetModel("ThreeRoom", threeRoomBrain);
            }
            else if (config == 4)
            {
                SetModel("FourRoom", fourRoomBrain);
            }
            else if (config == 5)
            {
                SetModel("FiveRoom", fiveRoomBrain);
            }
            else
            {
                SetModel("FiveRoom", dynamicRoomBrain);
            }
        }
        else
        {
            Debug.Log("Config: " + config);
            if (twoRoomBrain == null || threeRoomBrain == null || dynamicRoomBrain == null)
            {
                Debug.LogError("CUSTOM ERROR: One or more brain models are null. Please assign brain models in the inspector.");
                return;
            }
            if (config == 2)
            {
                if (modelIndex == 0)
                {
                    SetModel("TwoRoom", twoRoomBrain);
                }
                else if (modelIndex == 1)
                {
                    SetModel("TwoRoom", twoRoomBrain2);
                }
            }
            else if (config == 3)
            {
                if (modelIndex == 0)
                {
                    SetModel("ThreeRoom", threeRoomBrain);
                }
                else if (modelIndex == 1)
                {
                    SetModel("ThreeRoom", threeRoomBrain2);
                }

            }
            else if (config == 4)
            {
                if (modelIndex == 0)
                {
                    SetModel("FourRoom", fourRoomBrain);
                }
                else if (modelIndex == 1)
                {
                    SetModel("FourRoom", fourRoomBrain2);
                }

            }
            else if (config == 5)
            {
                if (modelIndex == 0)
                {
                    SetModel("FiveRoom", fiveRoomBrain);
                }
                else if (modelIndex == 1)
                {
                    SetModel("FiveRoom", fiveRoomBrain2);
                }
            }
            else
            {
                if (modelIndex == 0)
                {
                    SetModel("FiveRoom", fiveRoomBrain);
                }
                else if (modelIndex == 1)
                {
                    SetModel("FiveRoom", fiveRoomBrain2);
                }
            }
        }
    }
}
