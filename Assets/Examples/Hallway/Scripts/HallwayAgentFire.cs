using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class HallwayAgentFire : Agent
{
    public GameObject ground;
    public GameObject area;
    public GameObject symbolOGoal;
    public ParticleSystem fireParticleSystem;
    public ParticleSystem waterParticleSystem;

    // public GameObject symbolO;
    // public GameObject symbolX;
    public GameObject door;
    public DoorGenerator doorGenerator;

    public Button3D doorSwitch;
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
        m_HallwaySettings = FindObjectOfType<HallwaySettings>();
        m_AgentRb = GetComponent<Rigidbody>();
        m_GroundRenderer = ground.GetComponent<Renderer>();
        m_GroundMaterial = m_GroundRenderer.material;

        m_GoalRenderer = symbolOGoal.GetComponent<Renderer>();
        m_GoalMaterial = m_GoalRenderer.material;

        DoorComponent = door.GetComponent<openandclosedoor>();

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
        m_GroundRenderer.material = mat;
        yield return new WaitForSeconds(time);
        m_GroundRenderer.material = m_GroundMaterial;
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
        AddReward(-1f / MaxStep);
        MoveAgent(vectorAction);
    }


    void OnCollisionEnter(Collision col)
    {

        if (col.gameObject.CompareTag("symbol_O_Goal"))
        {
            SetReward(2f);
            StartCoroutine(GoalScoredSwapGroundMaterial(m_HallwaySettings.goalScoredMaterial, 0.5f));
            StartCoroutine(SwapGoalMaterial(m_HallwaySettings.waterMaterial, 0.5f));

            PlayWaterAndStopFire();
            // EndEpisode();
            StartCoroutine(DelayedEndEpisode()); // Use the coroutine here

        }
        // else
        // {
        //     SetReward(-0.1f);
        //     StartCoroutine(GoalScoredSwapGroundMaterial(m_HallwaySettings.failMaterial, 0.5f));
        // }
        // DoorComponent.CloseDoor();
        // doorSwitch.isPressed = false;

    }
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
        var agentOffset = -18f;
        var blockOffset = -9f;
        m_Selection = Random.Range(0, 2);

        transform.position = new Vector3(0f + Random.Range(-3f, 3f),
            1f, agentOffset + Random.Range(-3f, 3f))
            + ground.transform.position;
        transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        m_AgentRb.velocity *= 0f;

        var goalPos = Random.Range(0, 2);
        symbolOGoal.transform.position = new Vector3(Random.Range(-7f, 7f), 0.5f, 22.29f) + area.transform.position;


        DoorComponent.CloseDoor();
        doorSwitch.isPressed = false;
        doorGenerator.GenerateDoor();

        fireParticleSystem.Clear();
        fireParticleSystem.Play();
    }
    public void PlayWaterAndStopFire()
    {
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
