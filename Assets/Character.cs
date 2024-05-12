using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class Character : MonoBehaviour
{
    private CharacterController Controller;

    [SerializeField] private float speed;
    [SerializeField] private float gravityScale;

    [Range(0f, 1f)]
    [SerializeField] protected float smoothTime;

    private bool leftGround = false;

    private Vector3 moveDir = Vector3.zero;
    private Vector3 smoothMoveDir = Vector3.zero;
    private Vector3 velocity = Vector3.zero;

    public float getSpeed() { return speed; }

    void Start()
    {
        Controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        float inputAxis = Input.GetAxis("Horizontal");
        moveDir.x = inputAxis * speed;

        switch (Controller.isGrounded)
        {
            case true:
                moveDir.y = -gravityScale;

                smoothMoveDir.y = moveDir.y;
                smoothMoveDir = Vector3.SmoothDamp(smoothMoveDir, moveDir, ref velocity, smoothTime);

                Controller.Move(smoothMoveDir * Time.deltaTime);

                leftGround = false;
                break;

            default:
                if (!leftGround)
                {
                    smoothMoveDir.y = 0f;
                    leftGround = true;
                }

                moveDir.y -= (gravityScale * Time.deltaTime);
                smoothMoveDir = Vector3.SmoothDamp(smoothMoveDir, moveDir * 0.5f, ref velocity, smoothTime * 2f);

                Controller.Move(smoothMoveDir * Time.deltaTime);
                break;
        }
    }
}
