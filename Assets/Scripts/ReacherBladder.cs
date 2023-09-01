using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReacherBladder : MonoBehaviour
{
    public GameObject agent;
    // public GameObject hand;
    public GameObject bladderOn;
    public GameObject bladderDefault;

    public float reward = -0.01f;
    void Start()
    {
        bladderOn.GetComponent<MeshRenderer>().enabled = false;
        GetComponent<MeshRenderer>().enabled = true;

    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "tip_collider" || other.gameObject.tag == "trocar")
        {
            bladderOn.GetComponent<MeshRenderer>().enabled = true;
            GetComponent<MeshRenderer>().enabled = false;

        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "tip_collider" || other.gameObject.tag == "trocar")
        {
            bladderOn.GetComponent<MeshRenderer>().enabled = false;
            GetComponent<MeshRenderer>().enabled = true;


        }
    }
    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.name == "tip_collider" || other.gameObject.tag == "trocar")
        {
            // Debug.Log("Stabbed Bladder!");
            bladderOn.GetComponent<MeshRenderer>().enabled = true;
            GetComponent<MeshRenderer>().enabled = false;


        }
    }
}
