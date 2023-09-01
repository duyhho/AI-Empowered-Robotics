using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReacherBone : MonoBehaviour
{
    public GameObject agent;
    // public GameObject hand;
    public GameObject boneOn;
    public GameObject boneDefault;

    public float reward = -0.01f;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "tip_collider")
        {
            boneOn.GetComponent<MeshRenderer>().enabled = true;
            boneDefault.GetComponent<MeshRenderer>().enabled = false;

        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "tip_collider")
        {
            boneOn.GetComponent<MeshRenderer>().enabled = false;
            boneDefault.GetComponent<MeshRenderer>().enabled = true;


        }
    }
    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.name == "tip_collider")
        {
            // Debug.Log("Stabbed Bone!");
            boneOn.GetComponent<MeshRenderer>().enabled = true;
            boneDefault.GetComponent<MeshRenderer>().enabled = false;


        }
    }
}
