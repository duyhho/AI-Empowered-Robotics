using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.Robotics.UrdfImporter.Control;
using UnityEngine.Networking;

public class ReacherRobotLite6 : Agent
{
    Vector3 initialPosition;
    public GameObject pendulumA;
    public GameObject pendulumB;
    public GameObject pendulumC;
    public GameObject pendulumD;
    public GameObject pendulumE;
    public GameObject pendulumF;

    ArticulationBody m_RbA;
    ArticulationBody m_RbB;
    ArticulationBody m_RbC;
    ArticulationBody m_RbD;
    ArticulationBody m_RbE;
    ArticulationBody m_RbF;

    JointControl J1;
    JointControl J2;
    JointControl J3;
    JointControl J4;
    JointControl J5;
    JointControl J6;


    public GameObject hand;
    public GameObject goal;
    public GameObject entrance;
    public GameObject phantom;
    public GameObject pelvic1;
    public GameObject pelvic2;
    public GameObject bladder;
    public GameObject tip;


    Goal goalComponent;
    Entrance entranceComponent;
    bool resetScheduled = false;

    public bool randomizePhantom = true;


    float m_GoalRadius; //Radius of goal zone
    float m_GoalDegree; //How much goal rotates
    float m_GoalSpeed; //Speed of rotation
    float m_GoalDeviation; // Min and max range of up and down
    float m_GoalDeviationFreq; // Frequency of up and down

    public bool heuristicMode = false;
    private IEnumerator coroutine;
    FloatListConverter converter = new FloatListConverter();
    List<float> serverJointValues = new List<float>();
    List<float> heuristicJointValues = new List<float>();


    private float keyPressDuration = 0f;  // Timer to keep track of key press duration
    private int activeJointIndex = -1;    // To identify which joint is currently active
    float phantomRotationYAdjustment;

    struct JointRange
    {
        public float Min;
        public float Max;
        public JointRange(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
    JointRange[] jointRanges = new JointRange[]
{
    new JointRange(-175f, 175f),
    new JointRange(0f, 40f),
    new JointRange(-2f, 80f),
    new JointRange(-175f, 175f),
    new JointRange(-120f, -77f),
    new JointRange(-15f, 15f)
};
    public override void Initialize()
    {
        initialPosition = phantom.transform.position;
        m_RbA = pendulumA.GetComponent<ArticulationBody>();
        m_RbB = pendulumB.GetComponent<ArticulationBody>();
        m_RbC = pendulumC.GetComponent<ArticulationBody>();
        m_RbD = pendulumD.GetComponent<ArticulationBody>();
        m_RbE = pendulumE.GetComponent<ArticulationBody>();
        m_RbF = pendulumF.GetComponent<ArticulationBody>();

        goalComponent = goal.transform.GetComponent<Goal>();
        entranceComponent = entrance.transform.GetComponent<Entrance>();

        SetResetParameters();
        if (heuristicMode)
        {

            StartCoroutine("CallEveryXSeconds", 1f);

        }

    }

    public void SetResetParameters()
    {
        if (randomizePhantom)
        {
            float phantomPositionX = Random.Range(-0.1f, 0.1f);
            float phantomPositionZ = Random.Range(-0.05f, 0.05f);

            // Base Y rotation
            float baseYRotation = -90f;

            // Adjust the Y rotation based on the X position
            // float phantomRotationYAdjustment;
            if (phantomPositionX >= 0) // if X is positive or zero
            {
                phantomRotationYAdjustment = Random.Range(0f, 10f); // Choose a positive adjustment value
            }
            else // if X is negative
            {
                phantomRotationYAdjustment = Random.Range(-10f, 0f); // Choose a negative adjustment value
            }

            float finalRotationY = baseYRotation + phantomRotationYAdjustment;

            phantom.transform.position = initialPosition + new Vector3(phantomPositionX, 0f, phantomPositionZ);
            phantom.transform.rotation = Quaternion.Euler(0f, finalRotationY, 0f);
        }
        Controller controllerComponent = transform.GetComponent<Controller>();
        controllerComponent.RequestReset();
        if (heuristicMode)
        {
            RequestReset();
        }
        entrance.GetComponent<Entrance>().Reset();

        serverJointValues = new List<float> { 0f, 18.599986f,
                15.499998f,
                0.0f,
                -92.200025f,
                0.0f,
                };
        heuristicJointValues = new List<float> { 0f, 18.599986f,
                    15.499998f,
                    0.0f,
                    -92.200025f,
                    0.0f,
                   };

    }



    public override void OnEpisodeBegin()
    {
        J1 = m_RbA.GetComponent<JointControl>();
        J2 = m_RbB.GetComponent<JointControl>();
        J3 = m_RbC.GetComponent<JointControl>();
        J4 = m_RbD.GetComponent<JointControl>();
        J5 = m_RbE.GetComponent<JointControl>();
        J6 = m_RbF.GetComponent<JointControl>();

        // Debug.Log(J1);
        // J1.moveTarget(0f);
        // J2.moveTarget(9.5527f);
        // J3.moveTarget(14.2847f);
        // J4.moveTarget(0f);
        // J5.moveTarget(-84.36803f);
        // J6.moveTarget(0f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        float distanceZ = phantom.transform.position.z - transform.position.z;
        Vector2 distance = new Vector2(phantom.transform.localPosition.x - transform.localPosition.x, phantom.transform.localPosition.z - transform.localPosition.z);
        Vector3 directionToEntrance = (entrance.transform.position - tip.transform.position).normalized;
        Vector3 directionToGoal = (goal.transform.position - entrance.transform.position).normalized;

        Debug.Log("Direction to Entrance: " + directionToEntrance);
        Debug.Log("Direction to Goal: " + directionToGoal);
        Debug.Log(entranceComponent);
        bool canEnterEntrance = entranceComponent.CanEnter();
        sensor.AddObservation(canEnterEntrance ? 1 : 0); //1
        sensor.AddObservation(directionToEntrance); //3

        bool canEnterGoal = goalComponent.CanEnter();
        sensor.AddObservation(canEnterGoal ? 1 : 0); //1
        sensor.AddObservation(directionToGoal); //3

        // Define default joint values
        List<float> defaultJoints = new List<float>
        {
            0f, 18.599986f, 15.499998f, 0.0f, -92.200025f, 0.0f //6
        };

        ArticulationBody[] bodyList = { m_RbA, m_RbB, m_RbC, m_RbD, m_RbE, m_RbF };

        // Decide which joint values to use
        bool useDefaultJoints = !canEnterEntrance && !canEnterGoal;

        for (int i = 0; i < 6; i++)
        {
            float valueToUse = useDefaultJoints ? defaultJoints[i] : bodyList[i].xDrive.target;
            Debug.Log("Joint " + i + " value: " + valueToUse);
            sensor.AddObservation(NormalizeValueByJointIndex(valueToUse, i));
        }
    }


    public override void OnActionReceived(float[] vectorAction)
    {
        JointControl[] jointList = { J1, J2, J3, J4, J5, J6 };
        for (int i = 0; i < vectorAction.Length; i++)
        {
            // Print each element to the console
            if (vectorAction[i] != 0)
            {
                Debug.Log("Element at index " + i + ": " + vectorAction[i] + " De-normalized: " + DenormalizeValueByJointIndex(vectorAction[i], i));
                jointList[i].moveTarget(DenormalizeValueByJointIndex(vectorAction[i], i));
            }
        }
        // var jointValue = Mathf.Clamp(vectorAction[0], -1f, 1f) * 175f;
        // J1.moveTarget(jointValue);

        // jointValue = Mathf.Clamp(vectorAction[1], -1f, 1f);
        // jointValue = NormalizeValue(jointValue, 0f, 40f);
        // J2.moveTarget(jointValue);

        // // jointValue = Mathf.Clamp(vectorAction[2], -2f, 250f);
        // jointValue = Mathf.Clamp(vectorAction[2], -1f, 1f);
        // // jointValue = NormalizeValue(vectorAction[2], -2f, 80f);

        // jointValue = NormalizeValue(jointValue, -2f, 80f);
        // J3.moveTarget(jointValue);

        // jointValue = Mathf.Clamp(vectorAction[3], -1f, 1f) * 175f;
        // J4.moveTarget(jointValue);

        // // jointValue = Mathf.Clamp(vectorAction[4], -1f, 1f) * 120f;
        // jointValue = Mathf.Clamp(vectorAction[4], -1f, 1f);
        // jointValue = NormalizeValue(jointValue, -120f, -77f);
        // // Debug.Log("new J5: " + jointValue);
        // J5.moveTarget(jointValue);

        // jointValue = Mathf.Clamp(vectorAction[5], -1f, 1f) * 15f;
        // J6.moveTarget(jointValue);

        //Add negative rewards here
    }
    float NormalizeValue(float originalValue, float minRange, float maxRange, float minValue = -1f, float maxValue = 1f)
    {
        // Normalize the value within the given range
        float normalizedValue = (originalValue - minValue) / (maxValue - minValue);

        // Scale the normalized value to the desired range
        float scaledValue = normalizedValue * (maxRange - minRange) + minRange;

        return scaledValue;
    }

    IEnumerator ResetWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetResetParameters();
        resetScheduled = false;
    }
    void Update()
    {
        // Check which joint key is pressed
        if (Input.GetKeyDown(KeyCode.Alpha1)) activeJointIndex = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha2)) activeJointIndex = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha3)) activeJointIndex = 3;
        else if (Input.GetKeyDown(KeyCode.Alpha4)) activeJointIndex = 4;
        else if (Input.GetKeyDown(KeyCode.Alpha5)) activeJointIndex = 5;
        else if (Input.GetKeyDown(KeyCode.Alpha6)) activeJointIndex = 6;
        Debug.Log("activeJointIndex: " + activeJointIndex);
        if (goalComponent.collided && !resetScheduled)
        {
            resetScheduled = true;
            StartCoroutine(ResetWithDelay(3f));
        }

    }
    public override void Heuristic(float[] actionsOut)
    {
        List<float> differences = CalculateDifference(serverJointValues, heuristicJointValues);
        if (differences.Count > 0)
        {
            for (int index = 0; index <= 5; index++)
            {
                if (differences[index] != 0)
                {
                    Debug.Log("J" + (index + 1) + " diff: " + differences[index] + " Normalized: " + NormalizeValueByJointIndex(differences[index], index));
                }
                actionsOut[index] = NormalizeValueByJointIndex(serverJointValues[index], index);
                // This will print the difference for each corresponding pair of values.
            }
            heuristicJointValues = serverJointValues;

        }
        else
        {
            // Debug.Log("identical lists");
        }


        // for (int index = 0; index <= 5; index++)
        // {
        //     float jointValue = serverJointValues[index];
        //     actionsOut[index] = jointValue;
        //     Debug.Log("J" + (index + 1) + ": " + jointValue);
        // }
        // ArticulationBody[] bodyList = { m_RbA, m_RbB, m_RbC, m_RbD, m_RbE, m_RbF };
        // float[] valuesBefore = new float[6];
        // for (int i = 0; i < 6; i++)
        // {
        //     valuesBefore[i] = bodyList[i].xDrive.target;
        // }
        // Controller controllerComponent = transform.GetComponent<Controller>();
        // controllerComponent.Highlight(activeJointIndex);
        // controllerComponent.SetSelectedJointIndex(activeJointIndex); // to make sure it is in the valid range
        // controllerComponent.UpdateDirection(activeJointIndex);

        // // Assuming J1, J2, ... J6 are already defined somewhere in your script
        // // Create the joint list
        // // JointControl[] jointList = { J1, J2, J3, J4, J5, J6 };

        // // Set actions based on current target of each joint
        // for (int i = 0; i < 6; i++)
        // {
        //     Debug.Log("moving J: " + i + ": " + (bodyList[i].xDrive.target - valuesBefore[i]));

        //     actionsOut[i] = bodyList[i].xDrive.target - valuesBefore[i];
        // }

        // // Debug.Log("activeJointIndex: " + activeJointIndex);
    }

    private IEnumerator CallEveryXSeconds(int numSeconds)
    {
        while (true)
        {
            yield return new WaitForSeconds(numSeconds);

            string APIURL = string.Format("http://192.168.0.217:5000/get/jointangles");

            coroutine = GetRequest(APIURL);

            StartCoroutine(coroutine);
        }
    }

    IEnumerator GetRequest(string uri)
    {
        // yield return new WaitForSeconds(1f);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    string status = webRequest.downloadHandler.text;
                    if (status.ToLower() == "busy")
                    {
                        Debug.Log("Arm Busy: position unknown");
                    }
                    else
                    {
                        Debug.Log(status);
                        // Convert the server response to a list of floats
                        List<float> floatList = converter.ConvertTextToFloatList(status);
                        // Create a list to store the float values
                        serverJointValues = floatList.GetRange(0, 6); ;
                        // Distribute the floats into separate variables using deconstruction
                        float J1 = floatList[0];


                        /*Print Joint Values */

                        for (int index = 0; index <= 5; index++)
                        {
                            float jointValue = floatList[index];
                            Debug.Log("Server J" + (index + 1) + ": " + jointValue);
                            // JointControl currentJoint = articulationChain[index + 1].GetComponent<JointControl>();
                            // currentJoint.moveTarget(jointValue);
                        }
                    }
                    break;
            }
        }
    }
    public void RequestReset()
    {
        if (heuristicMode)
        {
            string APIURL = "http://192.168.0.217:5000/reset";
            StartCoroutine(GetRequest(APIURL));
        }

    }
    public List<float> CalculateDifference(List<float> list1, List<float> list2)
    {
        if (list1.Count != list2.Count)
        {
            Debug.Log("Size Different: " + list1.Count + " " + list2.Count);
            return new List<float>();
        }

        // Check if the lists are identical
        bool areListsIdentical = true;
        for (int i = 0; i < list1.Count; i++)
        {
            if (list1[i] != list2[i])
            {
                areListsIdentical = false;
                break;
            }
        }

        // If lists are identical, return null or an empty list (based on your preference)
        if (areListsIdentical)
        {
            Debug.Log("Identical Lists");

            return new List<float>();  // or return null; based on your requirement
        }

        // If the lists are different, calculate the differences
        List<float> differenceList = new List<float>();
        for (int i = 0; i < list1.Count; i++)
        {
            differenceList.Add(list1[i] - list2[i]);
        }

        return differenceList;
    }
    public float NormalizeValueByJointIndex(float value, int jointIndex)
    {
        var jointRange = jointRanges[jointIndex];
        return 2f * (value - jointRange.Min) / (jointRange.Max - jointRange.Min) - 1f;
    }

    public float DenormalizeValueByJointIndex(float normalizedValue, int jointIndex)
    {
        var jointRange = jointRanges[jointIndex];
        return ((normalizedValue + 1f) * (jointRange.Max - jointRange.Min) / 2f) + jointRange.Min;
    }
}