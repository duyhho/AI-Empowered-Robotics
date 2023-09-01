using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public GameObject agent;
    // public GameObject hand;
    public GameObject goalOn;
    public float reward = 1f;
    public bool collided = false;
    bool canEnter = false;
    private MeshRenderer meshRenderer;
    private Collider goalCollider;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        goalCollider = GetComponent<Collider>();
        // StartCoroutine(SetCollidedAfterDelay(6f));
    }
    void Update()
    {
        goalOn.SetActive(canEnter);

        if (meshRenderer)
        {
            meshRenderer.enabled = canEnter;
        }
        if (goalCollider)
        {
            goalCollider.enabled = canEnter;
        }
        if (collided)
        {
            goalOn.transform.localScale = new Vector3(1.05f, 1.05f, 1.05f);
        }
    }
    IEnumerator SetCollidedAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        collided = true;
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "tip_collider" || other.gameObject.tag == "trocar")
        {
            goalOn.transform.localScale = new Vector3(1.05f, 1.05f, 1.05f);
            agent.GetComponent<ReacherRobotLite6>().AddReward(reward);
            collided = true;
            // StartCoroutine(SetCollidedAfterDelay(3f));
        }
    }
    // void OnTriggerExit(Collider other)
    // {
    //     if (other.gameObject.name == "tip_collider" || other.gameObject.tag == "trocar")
    //     {
    //         goalOn.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);
    //     }
    // }
    // void OnTriggerStay(Collider other)
    // {
    //     if (other.gameObject.name == "tip_collider" || other.gameObject.tag == "trocar")
    //     {
    //         // Debug.Log("Reward!");
    //         // agent.GetComponent<ReacherRobotLite6>().AddReward(reward);
    //         agent.GetComponent<ReacherRobotLite6>().SetResetParameters();

    //     }
    // }
    public void SetCanEnter(bool val)
    {
        canEnter = val;
    }
    public bool CanEnter()
    {
        return canEnter;
    }
    public void Reset()
    {
        goalOn.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);
        collided = false;
        canEnter = false;
    }
}
