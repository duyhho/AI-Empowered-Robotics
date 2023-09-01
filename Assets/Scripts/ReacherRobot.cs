using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
public class ReacherRobot : Agent
{
    public GameObject pendulumA;
    public GameObject pendulumB;
    public GameObject pendulumC;
    public GameObject pendulumD;
    public GameObject pendulumE;
    public GameObject pendulumF;

    Rigidbody m_RbA;
    Rigidbody m_RbB;
    Rigidbody m_RbC;
    Rigidbody m_RbD;
    Rigidbody m_RbE;
    Rigidbody m_RbF;

    public GameObject hand;
    public GameObject goal;
    public float m_GoalHeight = 3.0f;

    float m_GoalRadius; //Radius of goal zone
    float m_GoalDegree; //How much goal rotates
    float m_GoalSpeed; //Speed of rotation
    float m_GoalDeviation; // Min and max range of up and down
    float m_GoalDeviationFreq; // Frequency of up and down

    public bool blHeuristics = false;

    public override void Initialize()
    {
        m_RbA = pendulumA.GetComponent<Rigidbody>();
        m_RbB = pendulumB.GetComponent<Rigidbody>();
        m_RbC = pendulumC.GetComponent<Rigidbody>();
        m_RbD = pendulumD.GetComponent<Rigidbody>();
        m_RbE = pendulumE.GetComponent<Rigidbody>();
        m_RbF = pendulumF.GetComponent<Rigidbody>();
        SetResetParameters();

    }

    public void SetResetParameters()
    {
        m_GoalRadius = Random.Range(1.1f, 1.4f);
        m_GoalDegree = Random.Range(0f, 360f);
        m_GoalSpeed = Random.Range(-2f, 2f);
        m_GoalDeviation = Random.Range(-1f, 1f);
        m_GoalDeviationFreq = Random.Range(0f, 3.14f);
    }

    public override void OnEpisodeBegin()
    {
        pendulumA.transform.position = new Vector3(1.173784f, 1.161073f, -0.399564f) + transform.position;
        pendulumA.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        m_RbA.velocity = Vector3.zero;
        m_RbA.angularVelocity = Vector3.zero;

        pendulumB.transform.position = new Vector3(1.0237838f, 1.16107273f, -0.399564803f) + transform.position;
        pendulumB.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        m_RbB.velocity = Vector3.zero;
        m_RbB.angularVelocity = Vector3.zero;

        pendulumC.transform.position = new Vector3(1.0237838f, 1.9860729f, -0.399564862f) + transform.position;
        pendulumC.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        m_RbC.velocity = Vector3.zero;
        m_RbC.angularVelocity = Vector3.zero;

        pendulumD.transform.position = new Vector3(1.0237838f, 1.9860729f, -0.399564862f) + transform.position;
        pendulumD.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        m_RbD.velocity = Vector3.zero;
        m_RbD.angularVelocity = Vector3.zero;

        pendulumE.transform.position = new Vector3(1.0237838f, 2.61107302f, -0.399564981f) + transform.position;
        pendulumE.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        m_RbE.velocity = Vector3.zero;
        m_RbE.angularVelocity = Vector3.zero;

        pendulumF.transform.position = new Vector3(1.0237838f, 2.72107291f, -0.399564981f) + transform.position;
        pendulumF.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        m_RbF.velocity = Vector3.zero;
        m_RbF.angularVelocity = Vector3.zero;

        SetResetParameters();
        // m_GoalDegree += m_GoalSpeed;
        // UpdateGoalPosition();

    }

    void UpdateGoalPosition()
    {
        if (!blHeuristics)
        {
            // var m_GoalDegree_rad = m_GoalDegree * Mathf.PI / 180f;
            // var goalX = m_GoalRadius * Mathf.Cos(m_GoalDegree_rad) + 1.1f;
            // var goalY = m_GoalHeight + m_GoalDeviation * Mathf.Cos(m_GoalDeviationFreq * m_GoalDegree_rad);
            // var goalZ = m_GoalRadius * Mathf.Sin(m_GoalDegree_rad);

            // goal.transform.position = new Vector3(goalX, goalY, goalZ) + transform.position;
        }
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(pendulumA.transform.localPosition);
        sensor.AddObservation(pendulumA.transform.rotation);
        sensor.AddObservation(m_RbA.velocity);
        sensor.AddObservation(m_RbA.angularVelocity);

        sensor.AddObservation(pendulumB.transform.localPosition);
        sensor.AddObservation(pendulumB.transform.rotation);
        sensor.AddObservation(m_RbB.velocity);
        sensor.AddObservation(m_RbB.angularVelocity);

        sensor.AddObservation(pendulumC.transform.localPosition);
        sensor.AddObservation(pendulumC.transform.rotation);
        sensor.AddObservation(m_RbC.velocity);
        sensor.AddObservation(m_RbC.angularVelocity);

        sensor.AddObservation(pendulumD.transform.localPosition);
        sensor.AddObservation(pendulumD.transform.rotation);
        sensor.AddObservation(m_RbD.velocity);
        sensor.AddObservation(m_RbD.angularVelocity);

        sensor.AddObservation(pendulumE.transform.localPosition);
        sensor.AddObservation(pendulumE.transform.rotation);
        sensor.AddObservation(m_RbE.velocity);
        sensor.AddObservation(m_RbE.angularVelocity);

        sensor.AddObservation(pendulumF.transform.localPosition);
        sensor.AddObservation(pendulumF.transform.rotation);
        sensor.AddObservation(m_RbF.velocity);
        sensor.AddObservation(m_RbF.angularVelocity);

        sensor.AddObservation(hand.transform.localPosition);
        sensor.AddObservation(goal.transform.localPosition);

        sensor.AddObservation(m_GoalSpeed);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        var torque = Mathf.Clamp(vectorAction[0], -1f, 1f) * 150f;
        m_RbA.AddTorque(new Vector3(0f, torque, 0f));

        torque = Mathf.Clamp(vectorAction[1], -1f, 1f) * 150f;
        m_RbB.AddTorque(new Vector3(0f, 0f, torque));

        torque = Mathf.Clamp(vectorAction[2], -1f, 1f) * 150f;
        m_RbC.AddTorque(new Vector3(0f, 0f, torque));

        torque = Mathf.Clamp(vectorAction[3], -1f, 1f) * 150f;
        m_RbD.AddTorque(new Vector3(0f, torque, 0f));

        torque = Mathf.Clamp(vectorAction[4], -1f, 1f) * 150f;
        m_RbE.AddTorque(new Vector3(0f, 0f, torque));

        torque = Mathf.Clamp(vectorAction[5], -1f, 1f) * 150f;
        m_RbF.AddTorque(new Vector3(0f, torque, 0f));

        // m_GoalDegree += m_GoalSpeed;
        // UpdateGoalPosition();

    }
}