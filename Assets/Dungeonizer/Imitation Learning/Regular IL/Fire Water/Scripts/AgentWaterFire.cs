using System.Collections;
using UnityEngine;
using Unity.MLAgents;
using Unity.Barracuda;
using Unity.MLAgents.Sensors;
using UnityEditor;
using System.Collections.Generic;

public class AgentWaterFire : DungeonAgentFire
{
    [Header("Agent State Visualization")]
    public Material normalMaterial;
    public Material waterMaterial;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);
    }
    protected override void RaycastUpdateGrid()
    {
        base.RaycastUpdateGrid();
    }
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
    }


    public override void OnActionReceived(float[] vectorAction)
    {
        if (MaxStep != 0)
        {
            AddReward(-1f / MaxStep);
        }

        MoveAgent(vectorAction);

        // AddReward(CalculateRewardForDoor());

    }

    protected override void OnCollisionEnter(Collision col)
    {
        base.OnCollisionEnter(col);
    }
    protected override void OnCollisionStay(Collision col)
    {
        base.OnCollisionStay(col);
    }
    protected override IEnumerator DelayedEndEpisode()
    {
        yield return StartCoroutine(base.DelayedEndEpisode());
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
    }
    protected override void UpdateModelStats()
    {
        base.UpdateModelStats();
    }
    public override void Heuristic(float[] actionsOut)
    {
        base.Heuristic(actionsOut);
    }
    protected override void OnLayoutChange()
    {
        base.OnLayoutChange();
    }

    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
    }
    protected override void CheckCurrentEvaluationModels()
    {
        base.CheckCurrentEvaluationModels();
    }
    public override void ResetEnvironment()
    {
        base.ResetEnvironment();
    }
    public override void PlayWaterAndStopFire()
    {
        base.PlayWaterAndStopFire();
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    // /// <summary>
    // /// Configures the agent's neural network model based on the specified room configuration.
    // /// </summary>
    // /// <param name="config">The configuration identifier. Accepts values 2, 3, and others for default behavior.</param>
    // protected override void ConfigureAgent(int config)
    // {
    //     base.ConfigureAgent(config);
    // }
}
