using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReacherGoalCustom : MonoBehaviour
{
    public GameObject agent;
    public GameObject hand;
    public GameObject goalOn;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == hand)
        {
            goalOn.transform.localScale = new Vector3(1.05f, 1.05f, 1.05f);
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == hand)
        {
            goalOn.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);
        }
    }
    void OnTriggerStay(Collider other)
    {
        if (other.gameObject == hand)
        {
            agent.GetComponent<ReacherRobot>().AddReward(0.01f);
        }
    }
}
