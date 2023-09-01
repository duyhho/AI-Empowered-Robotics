using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReacherGoalLite6 : MonoBehaviour
{
    public GameObject agent;
    // public GameObject hand;
    public GameObject goalOn;
    public float reward = 1f;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "tip_collider" || other.gameObject.tag == "trocar")
        {
            goalOn.transform.localScale = new Vector3(1.05f, 1.05f, 1.05f);
            agent.GetComponent<ReacherRobotLite6>().AddReward(reward);
            agent.GetComponent<ReacherRobotLite6>().SetResetParameters();
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "tip_collider" || other.gameObject.tag == "trocar")
        {
            goalOn.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);
        }
    }
    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.name == "tip_collider" || other.gameObject.tag == "trocar")
        {
            // Debug.Log("Reward!");
            // agent.GetComponent<ReacherRobotLite6>().AddReward(reward);
            agent.GetComponent<ReacherRobotLite6>().SetResetParameters();

        }
    }
}
