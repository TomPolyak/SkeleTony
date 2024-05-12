using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OldAnimator : MonoBehaviour
{
    private CharacterController Controller;
    private Character Character;

    private float maxSpeed;

    public float animTime;
    public float correctionFactor;
    public float maxDist;

    private float minDist;
    private float stepSize;

    public Transform[] targets;
    public Transform rootPoint;

    private bool[] frozen;
    private float[] currentTimeStep;

    private Vector3[] prevPos;
    private Vector3[] defPos;

    private float prevVel;

    void xAnimStep (int id, float xVel, float dist)
    {
        float currentX = targets[id].localPosition.x;
        float dir = xVel.CompareTo(0);

        float timeStep = animTime / Time.deltaTime;
        float distanceCorrection = 1;

        float currentTargDist;

        switch (id % 2)
        {
            case 0:
                currentTargDist = Vector3.Distance(targets[id].localPosition, targets[id + 1].localPosition);
                distanceCorrection -= (dir / dist) * ((minDist * correctionFactor) / timeStep);
                break;

            default:
                break;
        } 

        float movRatio = dir * (stepSize / timeStep) * distanceCorrection;
        float newX = currentX + movRatio;

        Vector3 newPos = new(newX, 0, 0);

        targets[id].localPosition = newPos;
    }

    void Start()
    {
        Controller = GetComponent<CharacterController>();
        Character = GetComponent<Character>();

        maxSpeed = Character.getSpeed();

        minDist = maxDist;
        stepSize = (maxSpeed / ((maxDist * 10)/3) * (minDist / 2));

        frozen = new bool[targets.Length];
        currentTimeStep = new float[targets.Length];

        prevPos = new Vector3[targets.Length];
        defPos = new Vector3[targets.Length];

        for (int i = 0; i < targets.Length; i++)
        {
            frozen[i] = true;
            currentTimeStep[i] = 0;

            prevPos[i] = targets[i].position;
            defPos[i] = targets[i].position;
        }
    }

    void Update()
    {
        float xVel = Mathf.Clamp(Controller.velocity.x, -maxSpeed, maxSpeed);    //velx
        
        for (int i = 0; i < targets.Length; i++)
        {
            float distFromRoot = Vector3.Distance(targets[i].position, rootPoint.position);

            targets[i].position = frozen[i] ? prevPos[i] : targets[i].position;
            prevPos[i] = targets[i].position;

            if (prevVel == 0 && xVel != 0)
            {
                frozen[i] = true;
                currentTimeStep[i] = 0;
            }

            if (frozen[i] && maxDist.CompareTo(distFromRoot) <= 0)
            {
                frozen[i] = false;
            }

            if (!frozen[i] && (currentTimeStep[i].CompareTo(animTime) < 0))
            {
                currentTimeStep[i] += Time.deltaTime;
                xAnimStep(i, xVel, distFromRoot);
            }

            if (currentTimeStep[i].CompareTo(animTime) >= 0) {
                frozen[i] = true;
                currentTimeStep[i] = 0;
            }
        }

        prevVel = xVel;
    }
}
