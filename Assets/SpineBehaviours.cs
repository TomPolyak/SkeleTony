using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class SpineBehaviours : MonoBehaviour
{
    [SerializeField] private CharacterController Controller;
    [SerializeField] private Character Character;

    private const float turnCooldown = .4f;

    private TargetAnimator TargetAnimator;
    private StepTargetPlacement StepTargetPlacement;

    [SerializeField] private WeaponAlignment WeaponAlignment;

    [SerializeField] private Transform[] affectedTransforms;

    [SerializeField] private float lookStrength;
    [SerializeField] private float dampingFactor;
    [SerializeField] private float stationarySlopeOffset;
    [SerializeField] private float movementSway;

    [SerializeField] private Transform lookPoint;
    [SerializeField] private Transform modelWrapper;
    [SerializeField] private Transform pelvis;

    private float[] defaultRotations;
    private float flipCorrection;

    private bool facingRight = true;
    private float turnCooldownTimer;
    private float defaultTargetDistance;

    private Vector3[] positions;
    private Vector3[] lerpPositions;

    private Vector3 defaultPelvisPosition;

    private float YOffsetValue;

    public float getYOffset() { return YOffsetValue; }
    public float getFlipCorrection() { return flipCorrection; }

    void Start()
    {
        TargetAnimator = GetComponent<TargetAnimator>();
        StepTargetPlacement = GetComponent<StepTargetPlacement>();

        positions = new Vector3[TargetAnimator.legTargets.Length];
        lerpPositions = new Vector3[StepTargetPlacement.lerpTargets.Length];

        defaultRotations = new float[affectedTransforms.Length];

        defaultPelvisPosition = pelvis.localPosition;

        for (int i = 0; i < defaultRotations.Length; i++)
        {
            defaultRotations[i] = affectedTransforms[i].localEulerAngles.z;
        }

        for (int i = 0; i < TargetAnimator.legTargets.Length; i++)
        {
            positions[i] = TargetAnimator.getDefaultPosition(i);
            lerpPositions[i] = StepTargetPlacement.lerpTargets[i].localPosition;

            switch (i % 2)
            {
                case 1:
                    defaultTargetDistance = Vector3.Distance(TargetAnimator.getDefaultPosition(i), TargetAnimator.getDefaultPosition(i - 1));
                    break;
                default:
                    break;
            }
        }
    }

    void Update()
    {
        if (turnCooldownTimer >= turnCooldown)
        {
            if (WeaponAlignment.getDir().x <= 0 && facingRight)
            {
                turnCooldownTimer = 0;
                facingRight = false;
                modelWrapper.localRotation = Quaternion.AngleAxis(180, Vector3.up);
            }
            else if (WeaponAlignment.getDir().x >= 0 && !facingRight)
            {
                turnCooldownTimer = 0;
                facingRight = true;
                modelWrapper.localRotation = Quaternion.AngleAxis(0, Vector3.up);
            }
        }
        else
        {
            turnCooldownTimer += Time.deltaTime;
        }

        float speedRatio = Controller.velocity.x / Character.getSpeed();

        for (int i = 0; i < affectedTransforms.Length; i++)
        {
            float cooldownEasing = Mathf.Sin(turnCooldownTimer / 2 * Mathf.PI);

            flipCorrection = facingRight ? 1 : -1;

            float movementRotationFactor = flipCorrection * (i * affectedTransforms.Length - affectedTransforms.Length) * (movementSway / affectedTransforms.Length) * speedRatio;

            Quaternion rotation = Quaternion.AngleAxis(movementRotationFactor + (WeaponAlignment.getMouseToViewpointAngle() * (lookStrength * cooldownEasing)) / (i + 1) + defaultRotations[i], Vector3.forward);
            affectedTransforms[i].localRotation = rotation;
        }

        for (int i = 0; i < positions.Length; i++) {
            positions[i] = TargetAnimator.legTargets[i].localPosition;
            lerpPositions[i] = StepTargetPlacement.lerpTargets[i].localPosition;

            switch (i % 2)
            {
                case 1:
                    float targetDistance = Vector3.Distance(positions[i], positions[i - 1]);
                    float lerpYDifferenceFactor = (lerpPositions[i].y - lerpPositions[i - 1].y) / (dampingFactor / (1 + Mathf.Abs(speedRatio)));
                    YOffsetValue = stationarySlopeOffset * Mathf.Abs(lerpYDifferenceFactor) - ((targetDistance - defaultTargetDistance) / dampingFactor);
                    float newPelvisY = defaultPelvisPosition.y - YOffsetValue;

                    pelvis.localPosition = new Vector3(defaultPelvisPosition.x, newPelvisY, 0);
                    break;

                default:
                    break;
            }
        }
    }
}
