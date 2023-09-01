using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entrance : MonoBehaviour
{
    public GameObject agent;
    // public GameObject hand;
    public GameObject goalOn;
    public float reward = 1f;
    public bool collided = false;
    public GameObject goal;
    Goal goalComponent;
    private MeshRenderer meshRenderer;
    private Collider collider;
    private void Awake()
    {
        goalComponent = goal.transform.GetComponent<Goal>();
        meshRenderer = GetComponent<MeshRenderer>();
        collider = GetComponent<Collider>();
        // StartCoroutine(SetCollidedAfterDelay(3f));
    }
    void Update()
    {
        goalComponent.SetCanEnter(collided);
        if (meshRenderer)
        {
            meshRenderer.enabled = !collided;
        }
        if (collider)
        {
            collider.enabled = !collided;
        }

    }
    // IEnumerator SetCollidedAfterDelay(float delay)
    // {
    //     yield return new WaitForSeconds(delay);
    //     collided = true;
    // }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "tip_collider" || other.gameObject.tag == "trocar")
        {
            agent.GetComponent<ReacherRobotLite6>().AddReward(reward);
            goalOn.transform.localScale = new Vector3(1.05f, 1.05f, 1.05f);
            collided = true;

            // Disable the GameObject the script is attached to

        }
    }

    // void OnTriggerExit(Collider other)
    // {
    //     if (other.gameObject.name == "tip_collider" || other.gameObject.tag == "trocar")
    //     {
    //         goalOn.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);
    //     }
    // }
    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.name == "tip_collider" || other.gameObject.tag == "trocar")
        {

        }
    }

    public bool CanEnter()
    {
        return collided == false;
    }

    public void Reset()
    {
        Debug.Log("Reset in Entrance called");
        goalOn.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);
        collided = false;
        if (goalComponent)
            goalComponent.Reset();
    }
}
