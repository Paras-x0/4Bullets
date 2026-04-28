using System;

using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Referenes")]
    public Transform cameraTransform;
    public Transform meshRoot;
    public Animator animator;

    [Header("Movement")]
    public float walkSpeed;
    public float runSpeed;
    public float crouchSpeed;
    public float acceleration;
    public float gravity;
    public float jumpHeight;

    [Header("Charcter Controller")]
    public float StandingHeight;
    public float crouchingHeight;
    public Vector3 controllerCenterStanding = new Vector3(0, 0.9f, 0);
    public Vector3 controllerCenterCrouching = new Vector3(0, 0.5f, 0);

    [Header("Mouse look")]
    public float mouseSensitivity;
    public float clampAngle;
    float pitch = 0f;
    float yaw;
    [Header("HeadBOB")]
    public bool enableHeadBob = true;
    public float bobFrequency;
    public float bobHeight;
    Vector3 cameraInitialLocalPos;
    float bobTimer = 0f;

    [Header("Foot IK")]
    public bool enableFootIK = true;
    public float footIKRayDistance;
    public LayerMask groundMask = ~0;
    public float footIKHeight;
    public float footIKSmoothTime = 0.1f;
    public float footIKRot = 10f;
    public float footIKWeight = 1f;

    CharacterController controller;
    Vector3 velocity;
    float currentSpeed;
    bool isCrouching = false;
    bool isSwording;
    bool isShielding;
    bool grounded;

    public bool isSprinting;

    float leftFootWeight;
    float rightFootWeight;
    Vector3 leftFootPos;
    Vector3 rightFootPos;
    Quaternion leftFootRot;
    Quaternion rightFootRot;
    Vector3 leftFootVel, rightFootVel;

    public Transform headBone;
    public Transform spineBone;
    Quaternion baseHeadRotation;
    Quaternion baseSpineRotation;
    public float spineRotationWeight;

    int hashJumpTrigger;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        cameraInitialLocalPos = cameraTransform.localPosition;

        hashJumpTrigger = Animator.StringToHash("Jump");
    }

    void Start()
    {
        currentSpeed = walkSpeed;
        controller.height = StandingHeight;
        controller.center = controllerCenterStanding;

        if (headBone != null)
            baseHeadRotation = headBone.localRotation;

        if (spineBone != null)
            baseSpineRotation = spineBone.localRotation;
    }

    void Update()
    {
        
        HandleMouseLook();
        HandleMouseInput();
        HandleCrouchAndHeight();
        HandleHeadBob();
        UpdateAnimatorParameters();
        // handleAttackingandShielding();
    }

    void LateUpdate()
    {
        if (headBone == null || spineBone == null) return;

        Quaternion rot = Quaternion.Euler(pitch, yaw * 2f, 0f);
        headBone.localRotation = rot * baseHeadRotation;

        Quaternion spineRot = Quaternion.Euler(pitch * spineRotationWeight, yaw * spineRotationWeight, 0f);
        spineBone.localRotation = spineRot * baseSpineRotation;
    }

    void HandleMouseLook()
    {
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up, mx);
        //creating head to move left and right
        yaw = Mathf.Lerp(yaw , mx , 10f * Time.deltaTime);
        pitch -= my;
        pitch = Mathf.Clamp(pitch, -clampAngle, clampAngle);
        cameraTransform.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }

    // void handleAttackingandShielding()
    // {
    //     if (Input.GetMouseButtonDown(0))
    //     {
    //         animator.SetBool("isSwording", true);
    //     }
    //     if (Input.GetMouseButtonUp(0))
    //     {
    //         animator.SetBool("isSwording", false);
    //     }
    //     if (Input.GetMouseButton(1))
    //     {
    //         animator.SetBool("isShielding", true);
    //     }
    //     if (Input.GetMouseButtonUp(1))
    //     {
    //         animator.SetBool("isShielding" , false);
    //     }
    // }
    void HandleMouseInput()
    {
        Vector2 moveAxis = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        Vector3 forward = Quaternion.Euler(0, transform.eulerAngles.y, 0) * Vector3.forward;
        Vector3 right = Quaternion.Euler(0, transform.eulerAngles.y, 0) * Vector3.right;

        Vector3 inputDir = (forward * moveAxis.y + right * moveAxis.x);
        inputDir = Vector3.ClampMagnitude(inputDir, 1f);

        bool sprint = Input.GetKey(KeyCode.LeftShift) && !isCrouching;
        isSprinting = sprint;

        float desiredSpeed = sprint ? runSpeed : (isCrouching ? crouchSpeed : walkSpeed);
        currentSpeed = Mathf.MoveTowards(currentSpeed, desiredSpeed, acceleration * Time.deltaTime);

        Vector3 horizontalVel = inputDir * currentSpeed;

        if (controller.isGrounded)
        {
            if (!grounded)
                grounded = true;

            velocity.y = -2f;

            if (Input.GetButtonDown("Jump") && !isCrouching)
            {
                velocity.y = Mathf.Sqrt(2f * jumpHeight * -gravity);
                if (animator) animator.SetTrigger(hashJumpTrigger);
                grounded = false;
            }
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        Vector3 move = horizontalVel + Vector3.up * velocity.y;
        controller.Move(move * Time.deltaTime);
    }

    void HandleCrouchAndHeight()
    {
        // if (Input.GetKeyDown(KeyCode.C))
        //     isCrouching = !isCrouching;
        isCrouching = Input.GetKey(KeyCode.LeftControl);
        
        float targetHeight = isCrouching ? crouchingHeight : StandingHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight, 10f * Time.deltaTime);

        controller.center = Vector3.Lerp(
            controller.center,
            isCrouching ? controllerCenterCrouching : controllerCenterStanding,
            10f * Time.deltaTime
        );

        Vector3 camTarget = cameraInitialLocalPos + new Vector3(0, isCrouching ? -0.4f : 0f, 0);
        cameraTransform.localPosition = Vector3.Lerp(
            cameraTransform.localPosition,
            camTarget,
            8f * Time.deltaTime
        );
    }

    void HandleHeadBob()
    {
        if (!enableHeadBob) return;

        Vector3 horizontalVel = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
        float speed = horizontalVel.magnitude;

        if (controller.isGrounded && speed > 0.1f)
        {
            bobTimer += Time.deltaTime * bobFrequency * (speed / runSpeed + 0.5f);
            float bobOffset = Mathf.Sin(bobTimer) * bobHeight;
            cameraTransform.localPosition = cameraInitialLocalPos + new Vector3(0, bobOffset, 0);
        }
        else
        {
            bobTimer = 0f;
            cameraTransform.localPosition = Vector3.Lerp(
                cameraTransform.localPosition,
                cameraInitialLocalPos,
                8f * Time.deltaTime
            );
        }
    }

    void UpdateAnimatorParameters()
    {
        if (!animator) return;

        Vector3 vel = controller.velocity;
        vel.y = 0f;

        Vector3 localVel = transform.InverseTransformDirection(vel);

        float maxSpeed = isCrouching ? crouchSpeed : runSpeed;

        float moveX = Mathf.Clamp(localVel.x / maxSpeed, -1f, 1f);
        float moveY = Mathf.Clamp(localVel.z / maxSpeed, -1f, 1f);

        animator.SetFloat("MoveX", moveX, 0.1f, Time.deltaTime);
        animator.SetFloat("MoveY", moveY, 0.1f, Time.deltaTime);

        animator.SetBool("Sprint", isSprinting);
        animator.SetBool("IsGrounded", controller.isGrounded);
        animator.SetBool("IsCrouching", isCrouching);
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (!animator || !enableFootIK) return;

        UpdateFootIK(AvatarIKGoal.LeftFoot, ref leftFootWeight, ref leftFootPos, ref leftFootRot, ref leftFootVel);
        UpdateFootIK(AvatarIKGoal.RightFoot, ref rightFootWeight, ref rightFootPos, ref rightFootRot, ref rightFootVel);
    }

    void UpdateFootIK(
        AvatarIKGoal goal,
        ref float weight,
        ref Vector3 footPos,
        ref Quaternion footRot,
        ref Vector3 vel)
    {
        Vector3 origin = animator.GetIKPosition(goal) + Vector3.up * 0.5f;
        RaycastHit hit;

        if (Physics.Raycast(origin, Vector3.down, out hit, footIKRayDistance, groundMask))
        {
            Vector3 targetPos = hit.point + Vector3.up * 0.02f;
            Quaternion targetRot =
                Quaternion.FromToRotation(Vector3.up, hit.normal) * transform.rotation;

            footPos = Vector3.SmoothDamp(footPos, targetPos, ref vel, footIKSmoothTime);
            footRot = Quaternion.Slerp(footRot, targetRot, Mathf.Exp(-footIKRot * Time.deltaTime));

            weight = Mathf.MoveTowards(weight, footIKWeight, Time.deltaTime * 5f);
        }
        else
        {
            weight = Mathf.MoveTowards(weight, 0f, Time.deltaTime * 5f);
        }

        animator.SetIKPositionWeight(goal, weight);
        animator.SetIKRotationWeight(goal, weight);

        if (weight > 0.01f)
        {
            animator.SetIKPosition(goal, footPos);
            animator.SetIKRotation(goal, footRot);
        }
    }

    // void OnDrawGizmosSelected()
    // {
    //     // if (Application.isPlaying) return;

    //     Gizmos.color = Color.cyan;
    //     Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.1f, 0.05f);
    // }
}
