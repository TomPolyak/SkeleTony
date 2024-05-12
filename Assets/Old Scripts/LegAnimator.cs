using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class LegAnimator : MonoBehaviour
{
    private CharacterController Controller;

    //temporarily disjointed max speed setter
    private float maxSpeed = 4f;

    private bool[] isFrozen;

    public Transform[] legTargets;

    public Transform targetPoint;
    public Transform originPoint;

    private Vector3[] lastLegPosition;
    private Vector3[] defaultLegPosition;
    private Vector3[] targetSnapshots;

    public float maxDistance;
    public float transitionTime;        //tmax
    private float[] currentTime;        //time for each individual limb

    public float stepSize;              //kx
    private float prevVel = 0;

    void moveTarget(int limbId, Vector3 target)
    {
        lastLegPosition[limbId] = legTargets[limbId].position;
        float currentX = legTargets[limbId].transform.position.x;
        float step = (target.x - currentX) / (transitionTime * 100);
        float newX = currentX + step;     // (currentTime[limbId]/transitionTime) will be useful for transitions

        Vector3 newPos = new(newX, 0, 0);
        legTargets[limbId].position = newPos;
    }

    bool ReachedMaxDist(Vector3 targetPos, float maxDist)
    {
        //bit hacky but it'll do for now
        float dist = Vector3.Distance(targetPos, originPoint.position);

        if ((int)(dist*100) >= (int)(maxDist*100))
        {
            return true;
        }

        return false;
    }

    void Start()
    {
        Controller = GetComponent<CharacterController>();

        currentTime = new float[legTargets.Length];
        isFrozen = new bool[legTargets.Length];

        lastLegPosition = new Vector3[legTargets.Length];
        defaultLegPosition = new Vector3[legTargets.Length];
        targetSnapshots = new Vector3[legTargets.Length];

        for (int i = 0; i < legTargets.Length; ++i)
        {
            currentTime[i] = 0;
            isFrozen[i] = true;

            lastLegPosition[i] = legTargets[i].position;
            defaultLegPosition[i] = legTargets[i].localPosition;
            targetSnapshots[i] = targetPoint.position; //could be changed to localPosition if you can't wrangle it
        }
    }

    void Update()
    {
        float xVelocity = Mathf.Clamp(Controller.velocity.x, -maxSpeed, maxSpeed);    //velx

        //changing direction
        if (xVelocity == 0 && xVelocity != prevVel) { 
            for (int i = 0; i < legTargets.Length; i++) {
                targetSnapshots[i] = targetPoint.position;
                moveTarget(i, originPoint.position + defaultLegPosition[i]);
                isFrozen[i] = true;
            }

            return;
        }

        //center of gravity update
        targetPoint.localPosition = new Vector3(stepSize * (xVelocity / maxSpeed), 0);

        for (int i = 0; i < legTargets.Length; i++)
        {
            //checking if d has reached dmax and unfreezing if true
            //plus snapshotting target point
            if (isFrozen[i] && ReachedMaxDist(legTargets[i].position, maxDistance)) {
                isFrozen[i] = false;
                targetSnapshots[i] = targetPoint.position;
            }

            //setting frozen limbs to their previous (locked) position in the world
            legTargets[i].position = isFrozen[i] ? lastLegPosition[i] : legTargets[i].position;

            //moving unfrozen limbs frame by frame + counting time
            if (!isFrozen[i] && (int)(currentTime[i]*10000) < (int)(transitionTime*10000)) {
                currentTime[i] += Time.deltaTime;
                targetSnapshots[i] = targetPoint.position;

                moveTarget(i, targetSnapshots[i]);
            }

            if ((int)(currentTime[i] * 10000) >= (int)(transitionTime * 10000))
            {
                Debug.Log("Freezing");
                isFrozen[i] = true;
                currentTime[i] = 0;
            }
        }

        prevVel = xVelocity;
    }
}
