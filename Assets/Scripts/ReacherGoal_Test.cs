using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReacherGoal_Test : MonoBehaviour
{
    public float m_GoalHeight = 3f;
    public GameObject goal;
    public GameObject robot;
    float m_GoalRadius; //Radius of goal zone
    float m_GoalDegree; //How much goal rotates
    float m_GoalSpeed; //Speed of rotation
    float m_GoalDeviation; // Min and max range of up and down
    float m_GoalDeviationFreq; // Frequency of up and down
    void Start()
    {
        SetResetParameters();
    }

    // Update is called once per frame
    void Update()
    {
        m_GoalDegree += m_GoalSpeed;
        UpdateGoalPosition();
    }
    public void SetResetParameters()
    {
        m_GoalRadius = Random.Range(1.1f, 1.4f);
        m_GoalDegree = Random.Range(0f, 360f);
        m_GoalSpeed = Random.Range(-2f, 2f);
        m_GoalDeviation = Random.Range(-1f, 1f);
        m_GoalDeviationFreq = Random.Range(0f, 3.14f);
    }
    void UpdateGoalPosition()
    {
        var m_GoalDegree_rad = m_GoalDegree * Mathf.PI / 180f;
        var goalX = m_GoalRadius * Mathf.Cos(m_GoalDegree_rad) + 1.1f;
        var goalY = m_GoalHeight + m_GoalDeviation * Mathf.Cos(m_GoalDeviationFreq * m_GoalDegree_rad);
        var goalZ = m_GoalRadius * Mathf.Sin(m_GoalDegree_rad) + 0.5f;

        goal.transform.position = new Vector3(goalX, goalY, goalZ);
    }
}
