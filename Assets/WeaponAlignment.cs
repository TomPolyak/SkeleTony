using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAlignment : MonoBehaviour
{
    private Animator gunAnimator;

    [SerializeField] private Transform weaponWrapper;
    [SerializeField] private Transform armsObject;

    [SerializeField] private float mouseFollowXThreshold;
    [SerializeField] private float mouseFollowYThreshold;
    [SerializeField] private float lookHeightDisplacementFactor;
    [SerializeField] private float lookDepthDisplacementFactor;
    [SerializeField] private float armSway;

    private Vector3 defaultWeaponPosition;

    private Vector2 dir;
    public Vector2 getDir() { return dir; }

    private float mouseToViewpointAngle = 0f;
    public float getMouseToViewpointAngle() { return mouseToViewpointAngle; }

    private float dampingFactor;

    void Start()
    {
        gunAnimator = GetComponentInChildren<Animator>(false);
        defaultWeaponPosition = weaponWrapper.localPosition;

        dampingFactor = 0f;

        Vector3 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        dir = mousePoint - weaponWrapper.position;
    }

    void Update()
    {
        Vector3 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        dir = mousePoint - weaponWrapper.position;

        if (gunAnimator.GetCurrentAnimatorStateInfo(0).IsName("gun_reload"))
        {
            dampingFactor = Mathf.Sin(gunAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime * Mathf.PI);
        }

        float offsetX = lookDepthDisplacementFactor * Mathf.Clamp(Mathf.Abs(dir.x), 0, mouseFollowXThreshold - dir.y * lookDepthDisplacementFactor) + (armsObject.localPosition.x * armSway);
        float offsetY = lookHeightDisplacementFactor * Mathf.Clamp(Mathf.Abs(dir.y), 0, mouseFollowYThreshold);

        weaponWrapper.localPosition = new Vector3(defaultWeaponPosition.x + (offsetX * (1 - dampingFactor)) , defaultWeaponPosition.y + (offsetY * (1 - dampingFactor)), 0);
        
        mouseToViewpointAngle = Mathf.Atan2(dir.y, Mathf.Clamp(Mathf.Abs(dir.x), mouseFollowXThreshold, Mathf.Infinity)) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(mouseToViewpointAngle * (1 - dampingFactor), Vector3.forward);
        weaponWrapper.localRotation = rotation;
    }
}
