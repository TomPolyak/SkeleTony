using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TargetAnimator : MonoBehaviour
{
    [SerializeField] private CharacterController Controller;
    [SerializeField] private Character Character;

    private StepTargetPlacement StepTargetPlacement;

    public float stepTiming;
    [Range (0f, .5f)] public float velocityTimeFactor;
    public float maxTargetY;
    public EasingList easingMethod = new EasingList();

    private float currentTime;
    private float maxSpeed;
    private bool turnBool;

    private float[] defaultEulerRotations;

    [SerializeField] public Transform[] legTargets;
    [SerializeField] private Transform[] ankleJoints;

    private Vector3[] defaultAnklePositions;

    private Vector3[] lerpPointSnapshot;
    private Vector3[] legTargetSnapshot;

    private bool[] frozen;

    private Vector3[] prevPos;

    public Vector3 getDefaultPosition(int i) { return defaultAnklePositions[i]; }

    void animStep(int id)
    {
        maxSpeed = Character.getSpeed();
        float speedRatio = Controller.velocity.x / maxSpeed;
        float inverseSpeedRatio = (maxSpeed - Mathf.Abs(Controller.velocity.x)) / maxSpeed;
        float t = currentTime / (stepTiming - (stepTiming * Mathf.Abs(inverseSpeedRatio) * velocityTimeFactor));

        float newX = Mathf.Lerp(legTargetSnapshot[id].x, lerpPointSnapshot[id].x, sineEasing(t));

        float yOffset = (Mathf.Sin(t * Mathf.PI) / maxTargetY) * Mathf.Abs(speedRatio);
        float newY = Mathf.Lerp(legTargetSnapshot[id].y, lerpPointSnapshot[id].y, sineEasing(t)) + yOffset;

        legTargets[id].localPosition = new Vector3(newX, newY, 0);
        legTargets[id].localRotation = Quaternion.AngleAxis(defaultEulerRotations[id] + StepTargetPlacement.lerpTargets[id].localEulerAngles.z, Vector3.forward);
    }

    float sineEasing(float x)
    {
        switch (easingMethod) {
            case EasingList.In:
                return -(Mathf.Cos(x * Mathf.PI) - 1) / 2;
            case EasingList.Out:
                return Mathf.Sin((x/2) * Mathf.PI);
            case EasingList.InOut:
                return -Mathf.Cos((x/2) * Mathf.PI) + 1;
            default:
                return x;
        }
    }

    void Start()
    {
        StepTargetPlacement = GetComponent<StepTargetPlacement>();

        turnBool = false;

        frozen = new bool[legTargets.Length];

        lerpPointSnapshot = new Vector3[legTargets.Length];
        legTargetSnapshot = new Vector3[legTargets.Length];

        prevPos = new Vector3[legTargets.Length];

        defaultAnklePositions = new Vector3[legTargets.Length];

        defaultEulerRotations = new float[legTargets.Length];

        for (int i = 0; i < legTargets.Length; i++)
        {
            frozen[i] = true;

            legTargets[i].transform.position = ankleJoints[i].transform.position;
            defaultAnklePositions[i] = legTargets[i].localPosition;

            lerpPointSnapshot[i] = StepTargetPlacement.lerpTargets[i].localPosition;
            legTargetSnapshot[i] = legTargets[i].localPosition;

            defaultEulerRotations[i] = legTargets[i].localEulerAngles.z;

            prevPos[i] = legTargets[i].position;
        }
    }

    void Update()
    {
        for (int i = 0; i < legTargets.Length; i++)
        {
            legTargets[i].position = frozen[i] ? prevPos[i] : legTargets[i].position;
            prevPos[i] = legTargets[i].position;

            if (i % 2 == 0 && !turnBool)
            {
                frozen[i] = false;
                lerpPointSnapshot[i] = StepTargetPlacement.lerpTargets[i].localPosition;
                animStep(i);
                prevPos[i] = legTargets[i].position;

                frozen[i] = Controller.isGrounded;
            }

            else if (i % 2 != 0 && turnBool)
            {
                frozen[i] = false;
                lerpPointSnapshot[i] = StepTargetPlacement.lerpTargets[i].localPosition;
                animStep(i);
                prevPos[i] = legTargets[i].position;

                frozen[i] = Controller.isGrounded;
            }
        }

        maxSpeed = Character.getSpeed();
        float inverseSpeedRatio = (maxSpeed - Mathf.Abs(Controller.velocity.x)) / maxSpeed;
        if (currentTime < stepTiming - (stepTiming * Mathf.Abs(inverseSpeedRatio) * velocityTimeFactor))
        {
            currentTime += Time.deltaTime;
        }
        else
        {
            currentTime = 0;
            turnBool = !turnBool;

            for (int i = 0; i < legTargets.Length; i++)
            {
                legTargetSnapshot[i] = legTargets[i].localPosition;
            }
        }
    }
}

public enum EasingList
{
    Linear,
    In,
    Out,
    InOut
}