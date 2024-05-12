using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class StepTargetPlacement : MonoBehaviour
{
    [SerializeField] private CharacterController Controller;
    [SerializeField] private Character Character;

    private TargetAnimator TargetAnimator;
    private SpineBehaviours SpineBehaviours;

    [SerializeField] private float raycastRangeModif;
    [SerializeField] [Range(0f, .2f)] private float slopeFocusFactor;
    [SerializeField] [Range(0f, 1f)] private float FootRotationWeight;

    [SerializeField] public Transform[] lerpTargets;

    public float stepSize;
    public float maxStepHeight;

    private float xVel;

    private float maxSpeed;

    private Ray ray;

    public RaycastHit performRaycast (Vector3 raycastOrigin, float angle)
    {
        ray = new Ray(raycastOrigin, Quaternion.AngleAxis(angle, -transform.forward) * transform.right);

        Physics.Raycast(ray, out RaycastHit hitInfo, maxStepHeight * raycastRangeModif);
        Debug.DrawLine(raycastOrigin, hitInfo.point, Color.yellow);

        return hitInfo;
    }

    void Start()
    {
        TargetAnimator = GetComponent<TargetAnimator>();
        SpineBehaviours = GetComponent<SpineBehaviours>();

        xVel = 0;
    }

    void Update()
    {
        xVel = Controller.velocity.x;
        maxSpeed = Character.getSpeed();

        float inverseSpeedRatio = (maxSpeed - Mathf.Abs(Controller.velocity.x)) / maxSpeed;

        for (int i = 0; i < lerpTargets.Length; i++)
        {
            float flipCorrection = SpineBehaviours.getFlipCorrection();
            lerpTargets[i].localPosition = new Vector3(flipCorrection * ((xVel / maxSpeed * stepSize) + inverseSpeedRatio * flipCorrection * (TargetAnimator.getDefaultPosition(i).x - (SpineBehaviours.getYOffset() * slopeFocusFactor * TargetAnimator.getDefaultPosition(i).x))), 0, 0);
            
            Vector3 raycastOrigin = new Vector3(lerpTargets[i].position.x, Controller.transform.position.y + maxStepHeight, 0);

            RaycastHit hitInfo = performRaycast(raycastOrigin, 90);

            if (hitInfo.point == new Vector3(0, 0, 0))
            {
                break;
            }

            else
            {
                lerpTargets[i].position = new Vector3(lerpTargets[i].position.x, hitInfo.point.y, 0);

                lerpTargets[i].localRotation = Quaternion.AngleAxis(-flipCorrection * Vector3.SignedAngle(hitInfo.normal, Vector3.up, Vector3.forward) * FootRotationWeight, Vector3.forward);
            }
        }
    }
}
