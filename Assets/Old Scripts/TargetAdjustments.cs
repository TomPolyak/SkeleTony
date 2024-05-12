using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetAdjustments : MonoBehaviour
{
    [SerializeField] private Transform ArmsObject;
    [SerializeField] private Transform[] HandTargets;

    [SerializeField] private Transform LegsObject;
    [SerializeField] private Transform[] LegTargets;

    private Vector3 defaultArmsPosition;
    private Vector3 defaultLegsPosition;

    public void TargetAbsoluteAdjustment(Transform ParentObject, Transform[] Targets, Vector3 defaultParentPosition)
    {
        Vector3 targetAdjustmentVector = ParentObject.localPosition - defaultParentPosition;

        for (int i = 0; i < Targets.Length; i++)
        {
            Targets[i].localPosition -= targetAdjustmentVector;
        }
    }

    void Start()
    {
        defaultArmsPosition = ArmsObject.localPosition;
        defaultLegsPosition = LegsObject.localPosition;
    }
    void Update()
    {
        TargetAbsoluteAdjustment(ArmsObject, HandTargets, defaultArmsPosition);
        TargetAbsoluteAdjustment(LegsObject, LegTargets, defaultLegsPosition);
    }
}
