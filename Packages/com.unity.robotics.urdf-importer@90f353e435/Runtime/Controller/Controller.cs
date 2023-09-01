﻿using System;
using Unity.Robotics;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
namespace Unity.Robotics.UrdfImporter.Control
{
    public enum RotationDirection { None = 0, Positive = 1, Negative = -1 };
    public enum ControlType { PositionControl };

    public class Controller : MonoBehaviour
    {
        private ArticulationBody[] articulationChain;
        // Stores original colors of the part being highlighted
        private Color[] prevColor;
        private int previousIndex;

        [InspectorReadOnly(hideInEditMode: true)]
        public string selectedJoint;
        [HideInInspector]
        public int selectedIndex;

        public ControlType control = ControlType.PositionControl;
        public float stiffness;
        public float damping;
        public float forceLimit;
        public float speed = 5f; // Units: degree/s
        public float torque = 100f; // Units: Nm or N
        public float acceleration = 5f;// Units: m/s^2 / degree/s^2

        [Tooltip("Color to highlight the currently selected join")]
        public Color highLightColor = new Color(1.0f, 0, 0, 1.0f);
        private IEnumerator coroutine;
        // Create an instance of FloatListConverter
        FloatListConverter converter = new FloatListConverter();

        public bool callAPI = false;
        void Start()
        {
            previousIndex = selectedIndex = 1;
            this.gameObject.AddComponent<FKRobot>();
            articulationChain = this.GetComponentsInChildren<ArticulationBody>();
            int defDyanmicVal = 10;
            foreach (ArticulationBody joint in articulationChain)
            {
                joint.gameObject.AddComponent<JointControl>();
                joint.jointFriction = defDyanmicVal;
                joint.angularDamping = defDyanmicVal;
                ArticulationDrive currentDrive = joint.xDrive;
                currentDrive.forceLimit = forceLimit;
                joint.xDrive = currentDrive;
            }
            DisplaySelectedJoint(selectedIndex);
            StoreJointColors(selectedIndex);
            if (callAPI)
            {
                StartCoroutine("CallEveryXSeconds", 1f);

            }
        }

        public void SetSelectedJointIndex(int index)
        {
            if (articulationChain.Length > 0)
            {
                selectedIndex = (index + articulationChain.Length) % articulationChain.Length;
            }
        }

        void Update()
        {
            // bool SelectionInput1 = Input.GetKeyDown("right");
            // bool SelectionInput2 = Input.GetKeyDown("left");

            // SetSelectedJointIndex(selectedIndex); // to make sure it is in the valid range
            // UpdateDirection(selectedIndex);

            // if (SelectionInput2)
            // {
            //     SetSelectedJointIndex(selectedIndex - 1);
            //     Highlight(selectedIndex);
            // }
            // else if (SelectionInput1)
            // {
            //     SetSelectedJointIndex(selectedIndex + 1);
            //     Highlight(selectedIndex);
            // }

            // UpdateDirection(selectedIndex);
        }

        /// <summary>
        /// Highlights the color of the robot by changing the color of the part to a color set by the user in the inspector window
        /// </summary>
        /// <param name="selectedIndex">Index of the link selected in the Articulation Chain</param>
        public void Highlight(int selectedIndex)
        {
            if (selectedIndex == previousIndex || selectedIndex < 0 || selectedIndex >= articulationChain.Length)
            {
                return;
            }

            // reset colors for the previously selected joint
            ResetJointColors(previousIndex);

            // store colors for the current selected joint
            StoreJointColors(selectedIndex);

            DisplaySelectedJoint(selectedIndex);
            Renderer[] rendererList = articulationChain[selectedIndex].transform.GetChild(1).GetComponentsInChildren<Renderer>();

            // set the color of the selected join meshes to the highlight color
            foreach (var mesh in rendererList)
            {
                MaterialExtensions.SetMaterialColor(mesh.material, highLightColor);
            }
        }

        void DisplaySelectedJoint(int selectedIndex)
        {
            if (selectedIndex < 0 || selectedIndex >= articulationChain.Length)
            {
                return;
            }
            selectedJoint = articulationChain[selectedIndex].name + " (" + selectedIndex + ")";
        }

        /// <summary>
        /// Sets the direction of movement of the joint on every update
        /// </summary>
        /// <param name="jointIndex">Index of the link selected in the Articulation Chain</param>
        public void UpdateDirection(int jointIndex)
        {
            if (jointIndex < 0 || jointIndex >= articulationChain.Length)
            {
                return;
            }

            float moveDirection = Input.GetAxis("Vertical");
            JointControl current = articulationChain[jointIndex].GetComponent<JointControl>();
            if (previousIndex != jointIndex)
            {
                JointControl previous = articulationChain[previousIndex].GetComponent<JointControl>();
                previous.direction = RotationDirection.None;
                previousIndex = jointIndex;
            }

            if (current.controltype != control)
            {
                UpdateControlType(current);
            }

            if (moveDirection > 0)
            {
                current.direction = RotationDirection.Positive;
            }
            else if (moveDirection < 0)
            {
                current.direction = RotationDirection.Negative;
            }
            else
            {
                current.direction = RotationDirection.None;
            }
        }

        /// <summary>
        /// Stores original color of the part being highlighted
        /// </summary>
        /// <param name="index">Index of the part in the Articulation chain</param>
        private void StoreJointColors(int index)
        {
            Renderer[] materialLists = articulationChain[index].transform.GetChild(1).GetComponentsInChildren<Renderer>();
            // Debug.Log(articulationChain[index].transform.GetChild());

            prevColor = new Color[materialLists.Length];
            for (int counter = 0; counter < materialLists.Length; counter++)
            {
                prevColor[counter] = MaterialExtensions.GetMaterialColor(materialLists[counter]);
            }
        }

        /// <summary>
        /// Resets original color of the part being highlighted
        /// </summary>
        /// <param name="index">Index of the part in the Articulation chain</param>
        private void ResetJointColors(int index)
        {
            Renderer[] previousRendererList = articulationChain[index].transform.GetChild(1).GetComponentsInChildren<Renderer>();

            for (int counter = 0; counter < previousRendererList.Length; counter++)
            {
                MaterialExtensions.SetMaterialColor(previousRendererList[counter].material, prevColor[counter]);
            }
        }

        public void UpdateControlType(JointControl joint)
        {
            joint.controltype = control;
            if (control == ControlType.PositionControl)
            {
                ArticulationDrive drive = joint.joint.xDrive;
                drive.stiffness = stiffness;
                drive.damping = damping;
                joint.joint.xDrive = drive;
            }
        }

        public void OnGUI()
        {
            GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
            centeredStyle.alignment = TextAnchor.UpperCenter;
            GUI.Label(new Rect(Screen.width / 2 - 200, 10, 400, 20), "Press left/right arrow keys to select a robot joint.", centeredStyle);
            GUI.Label(new Rect(Screen.width / 2 - 200, 30, 400, 20), "Press up/down arrow keys to move " + selectedJoint + ".", centeredStyle);
        }
        public void RequestReset()
        {
            if (callAPI)
            {
                string APIURL = "http://192.168.0.217:5000/reset";
                StartCoroutine(GetRequest(APIURL));
            }

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

                            // Distribute the floats into separate variables using deconstruction
                            float J1 = floatList[0];


                            /*Print Joint Values */

                            for (int index = 0; index <= 5; index++)
                            {
                                float jointValue = floatList[index];
                                // Debug.Log("J" + (index + 1) + ": " + jointValue);
                                JointControl currentJoint = articulationChain[index + 1].GetComponent<JointControl>();
                                currentJoint.moveTarget(jointValue);
                            }
                        }
                        break;
                }
            }
        }
    }
    public class FloatListConverter
    {
        public List<float> ConvertTextToFloatList(string text)
        {
            // Remove white spaces and line breaks
            text = text.Replace(" ", "").Replace("\n", "").Replace("\r", "");

            // Remove square brackets
            text = text.Trim('[', ']');

            // Split text into individual float values
            string[] floatStrings = text.Split(',');

            List<float> floatList = new List<float>();
            // Parse each float string and add it to the list
            foreach (string floatString in floatStrings)
            {
                float floatValue;

                if (float.TryParse(floatString, out floatValue))
                {
                    floatList.Add(floatValue);
                }
            }

            return floatList;
        }
    }


}
