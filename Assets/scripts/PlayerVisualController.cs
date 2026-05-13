using UnityEngine;
using UnityEngine.InputSystem;

[ExecuteAlways]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerVisualController : MonoBehaviour
{
    [Header("Core Sprites")]
    [SerializeField] private Sprite headSprite;
    [SerializeField] private Sprite neckSprite;
    [SerializeField] private Sprite bodySprite;
    [SerializeField] private Sprite upperLeftArmSprite;
    [SerializeField] private Sprite lowerLeftArmSprite;
    [SerializeField] private Sprite upperRightArmSprite;
    [SerializeField] private Sprite lowerRightArmSprite;
    [SerializeField] private Sprite leftHandSprite;
    [SerializeField] private Sprite rightHandSprite;
    [SerializeField] private Sprite upperLeftLegSprite;
    [SerializeField] private Sprite lowerLeftLegSprite;
    [SerializeField] private Sprite upperRightLegSprite;
    [SerializeField] private Sprite lowerRightLegSprite;

    [Header("Face Sprites")]
    [SerializeField] private Sprite idleLeftEyeSprite;
    [SerializeField] private Sprite idleRightEyeSprite;
    [SerializeField] private Sprite angryLeftEyeSprite;
    [SerializeField] private Sprite angryRightEyeSprite;
    [SerializeField] private Sprite sadLeftEyeSprite;
    [SerializeField] private Sprite sadRightEyeSprite;
    [SerializeField] private Sprite idleMouthSprite;
    [SerializeField] private Sprite angryMouthSprite;
    [SerializeField] private Sprite sadMouthSprite;

    [Header("Layout")]
    [SerializeField] private Vector3 visualOffset = Vector3.zero;
    [SerializeField] private float headScale = 0.24f;
    [SerializeField] private float bodyScale = 0.24f;
    [SerializeField] private float limbScale = 0.34f;
    [SerializeField] private float handScale = 0.24f;
    [SerializeField] private float faceScale = 0.24f;

    [Header("Animation")]
    [SerializeField] private float walkCycleSpeed = 8f;
    [SerializeField] private float walkSwingAngle = 18f;
    [SerializeField] private float jumpLegTuckAngle = 16f;
    [SerializeField] private float jumpArmLiftAngle = 12f;
    [SerializeField] private float attackPoseDuration = 0.18f;
    [SerializeField] private float hitPoseDuration = 0.35f;

    private PlayerMovement movement;
    private Rigidbody2D rb;
    private PlayerInput playerInput;
    private InputAction attackAction;

    private Transform visualRoot;
    private Transform torsoTransform;
    private Transform neckTransform;
    private Transform headTransform;
    private Transform faceRoot;
    private Transform leftUpperArmTransform;
    private Transform leftLowerArmTransform;
    private Transform leftHandTransform;
    private Transform rightUpperArmTransform;
    private Transform rightLowerArmTransform;
    private Transform rightHandTransform;
    private Transform leftUpperLegTransform;
    private Transform leftLowerLegTransform;
    private Transform rightUpperLegTransform;
    private Transform rightLowerLegTransform;

    private SpriteRenderer placeholderRenderer;
    private SpriteRenderer torsoRenderer;
    private SpriteRenderer neckRenderer;
    private SpriteRenderer headRenderer;
    private SpriteRenderer leftEyeRenderer;
    private SpriteRenderer rightEyeRenderer;
    private SpriteRenderer mouthRenderer;
    private SpriteRenderer leftUpperArmRenderer;
    private SpriteRenderer leftLowerArmRenderer;
    private SpriteRenderer leftHandRenderer;
    private SpriteRenderer rightUpperArmRenderer;
    private SpriteRenderer rightLowerArmRenderer;
    private SpriteRenderer rightHandRenderer;
    private SpriteRenderer leftUpperLegRenderer;
    private SpriteRenderer leftLowerLegRenderer;
    private SpriteRenderer rightUpperLegRenderer;
    private SpriteRenderer rightLowerLegRenderer;

    private float walkTime;
    private float facing = 1f;
    private float attackTimer;
    private float hitTimer;

    private static readonly Vector2 TorsoPosition = new Vector2(0f, 0.95f);
    private static readonly Vector2 NeckPosition = new Vector2(0f, 1.58f);
    private static readonly Vector2 HeadPosition = new Vector2(0f, 2.46f);
    private static readonly Vector2 LeftShoulder = new Vector2(-0.28f, 1.30f);
    private static readonly Vector2 RightShoulder = new Vector2(0.28f, 1.30f);
    private static readonly Vector2 LeftHip = new Vector2(-0.10f, 0.42f);
    private static readonly Vector2 RightHip = new Vector2(0.10f, 0.42f);
    private static readonly Vector2 LeftEyeLocalPosition = new Vector2(-0.36f, 0.30f);
    private static readonly Vector2 RightEyeLocalPosition = new Vector2(0.36f, 0.30f);
    private static readonly Vector2 MouthLocalPosition = new Vector2(0f, -0.30f);

    private const float UpperArmLength = 0.58f;
    private const float LowerArmLength = 0.54f;
    private const float UpperLegLength = 0.86f;
    private const float LowerLegLength = 1.00f;

    private const float LeftUpperArmArtAngle = 27f;
    private const float LeftLowerArmArtAngle = 43f;
    private const float RightUpperArmArtAngle = -27f;
    private const float RightLowerArmArtAngle = -45f;
    private const float LeftUpperLegArtAngle = 42f;
    private const float LeftLowerLegArtAngle = 65f;
    private const float RightUpperLegArtAngle = -42f;
    private const float RightLowerLegArtAngle = -65f;
    private const float LeftHandArtAngle = 0f;
    private const float RightHandArtAngle = 0f;

    private void Awake()
    {
        CacheComponents();
        EnsureRig();
        ApplySprites();
    }

    private void OnEnable()
    {
        CacheComponents();
        CacheActions();
        EnsureRig();
        ApplySprites();
        SubscribeToActions();
    }

    private void OnDisable()
    {
        UnsubscribeFromActions();
    }

    private void OnValidate()
    {
        CacheComponents();
        EnsureRig();
        ApplySprites();
        ApplyPose(0f);
    }

    private void LateUpdate()
    {
        CacheComponents();
        EnsureRig();
        ApplySprites();

        if (Application.isPlaying)
        {
            UpdateTimers(Time.deltaTime);
            UpdateFacing();
            UpdateWalkCycle(Time.deltaTime);
        }

        ApplyPose(Application.isPlaying ? Time.time : 0f);
    }

    public void TriggerHitReaction()
    {
        hitTimer = hitPoseDuration;
    }

    public void TriggerAttackPose()
    {
        attackTimer = attackPoseDuration;
    }

    private void CacheComponents()
    {
        movement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        placeholderRenderer = GetComponent<SpriteRenderer>();
    }

    private void CacheActions()
    {
        if (playerInput == null || playerInput.actions == null)
        {
            attackAction = null;
            return;
        }

        attackAction = playerInput.actions["Attack"];
    }

    private void SubscribeToActions()
    {
        if (attackAction != null)
        {
            attackAction.performed -= OnAttackPerformed;
            attackAction.performed += OnAttackPerformed;
        }
    }

    private void UnsubscribeFromActions()
    {
        if (attackAction != null)
        {
            attackAction.performed -= OnAttackPerformed;
        }
    }

    private void EnsureRig()
    {
        visualRoot = EnsureChild(transform, "VisualRoot");
        visualRoot.localPosition = visualOffset;
        visualRoot.localRotation = Quaternion.identity;
        visualRoot.localScale = Vector3.one;

        torsoTransform = EnsureSpritePart(visualRoot, "Torso", ref torsoRenderer, bodySprite, 0);
        neckTransform = EnsureSpritePart(visualRoot, "Neck", ref neckRenderer, neckSprite, 1);
        headTransform = EnsureSpritePart(visualRoot, "Head", ref headRenderer, headSprite, 5);
        faceRoot = EnsureChild(headTransform, "FaceRoot");

        leftUpperArmTransform = EnsureSpritePart(visualRoot, "LeftUpperArm", ref leftUpperArmRenderer, upperLeftArmSprite, 1);
        leftLowerArmTransform = EnsureSpritePart(visualRoot, "LeftLowerArm", ref leftLowerArmRenderer, lowerLeftArmSprite, 1);
        leftHandTransform = EnsureSpritePart(visualRoot, "LeftHand", ref leftHandRenderer, leftHandSprite, 2);
        rightUpperArmTransform = EnsureSpritePart(visualRoot, "RightUpperArm", ref rightUpperArmRenderer, upperRightArmSprite, 1);
        rightLowerArmTransform = EnsureSpritePart(visualRoot, "RightLowerArm", ref rightLowerArmRenderer, lowerRightArmSprite, 1);
        rightHandTransform = EnsureSpritePart(visualRoot, "RightHand", ref rightHandRenderer, rightHandSprite, 2);

        leftUpperLegTransform = EnsureSpritePart(visualRoot, "LeftUpperLeg", ref leftUpperLegRenderer, upperLeftLegSprite, -1);
        leftLowerLegTransform = EnsureSpritePart(visualRoot, "LeftLowerLeg", ref leftLowerLegRenderer, lowerLeftLegSprite, -1);
        rightUpperLegTransform = EnsureSpritePart(visualRoot, "RightUpperLeg", ref rightUpperLegRenderer, upperRightLegSprite, -1);
        rightLowerLegTransform = EnsureSpritePart(visualRoot, "RightLowerLeg", ref rightLowerLegRenderer, lowerRightLegSprite, -1);

        EnsureFacePart(faceRoot, "LeftEye", ref leftEyeRenderer, idleLeftEyeSprite, 7);
        EnsureFacePart(faceRoot, "RightEye", ref rightEyeRenderer, idleRightEyeSprite, 7);
        EnsureFacePart(faceRoot, "Mouth", ref mouthRenderer, idleMouthSprite, 7);

        if (placeholderRenderer != null)
        {
            placeholderRenderer.enabled = false;
        }
    }

    private void ApplySprites()
    {
        AssignSprite(torsoRenderer, bodySprite);
        AssignSprite(neckRenderer, neckSprite);
        AssignSprite(headRenderer, headSprite);
        AssignSprite(leftUpperArmRenderer, upperLeftArmSprite);
        AssignSprite(leftLowerArmRenderer, lowerLeftArmSprite);
        AssignSprite(leftHandRenderer, leftHandSprite);
        AssignSprite(rightUpperArmRenderer, upperRightArmSprite);
        AssignSprite(rightLowerArmRenderer, lowerRightArmSprite);
        AssignSprite(rightHandRenderer, rightHandSprite);
        AssignSprite(leftUpperLegRenderer, upperLeftLegSprite);
        AssignSprite(leftLowerLegRenderer, lowerLeftLegSprite);
        AssignSprite(rightUpperLegRenderer, upperRightLegSprite);
        AssignSprite(rightLowerLegRenderer, lowerRightLegSprite);
        AssignSprite(leftEyeRenderer, ResolveLeftEyeSprite());
        AssignSprite(rightEyeRenderer, ResolveRightEyeSprite());
        AssignSprite(mouthRenderer, ResolveMouthSprite());
    }

    private void UpdateTimers(float deltaTime)
    {
        attackTimer = Mathf.Max(0f, attackTimer - deltaTime);
        hitTimer = Mathf.Max(0f, hitTimer - deltaTime);
    }

    private void UpdateFacing()
    {
        if (movement != null && Mathf.Abs(movement.MoveInput.x) > 0.05f)
        {
            facing = Mathf.Sign(movement.MoveInput.x);
        }

        visualRoot.localScale = new Vector3(facing, 1f, 1f);
    }

    private void UpdateWalkCycle(float deltaTime)
    {
        if (movement == null || rb == null)
        {
            return;
        }

        if (movement.IsGrounded && Mathf.Abs(rb.linearVelocity.x) > 0.05f)
        {
            walkTime += deltaTime * walkCycleSpeed * Mathf.Clamp01(Mathf.Abs(rb.linearVelocity.x) / 3f + 0.25f);
        }
    }

    private void ApplyPose(float timeValue)
    {
        bool grounded = movement != null && movement.IsGrounded;
        Vector2 velocity = rb != null ? rb.linearVelocity : Vector2.zero;
        float horizontalSpeed = Mathf.Abs(velocity.x);
        float walkBlend = grounded ? Mathf.Clamp01(horizontalSpeed / 4f) : 0f;
        float swing = Mathf.Sin(walkTime) * walkSwingAngle * walkBlend;
        float bounce = grounded ? Mathf.Abs(Mathf.Sin(walkTime * 2f)) * 0.04f * walkBlend : 0f;
        float idleTilt = Application.isPlaying ? Mathf.Sin(timeValue * 2.2f) * 1.5f : 0f;
        float airborneTilt = grounded ? 0f : Mathf.Clamp(velocity.y * 1.2f, -8f, 8f);
        float attackBlend = attackPoseDuration > 0f ? attackTimer / attackPoseDuration : 0f;
        float hitBlend = hitPoseDuration > 0f ? hitTimer / hitPoseDuration : 0f;

        PoseStaticPart(torsoTransform, TorsoPosition + new Vector2(0f, bounce - hitBlend * 0.04f), bodyScale, idleTilt * 0.25f - airborneTilt * 0.08f);
        PoseStaticPart(neckTransform, NeckPosition + new Vector2(0f, bounce * 0.75f), bodyScale, -idleTilt * 0.15f);
        PoseStaticPart(headTransform, HeadPosition + new Vector2(0f, bounce + (grounded ? 0f : 0.04f) - hitBlend * 0.03f), headScale, -idleTilt * 0.45f + airborneTilt * 0.12f - hitBlend * 5f);

        faceRoot.localPosition = new Vector3(0f, hitBlend * 0.03f, 0f);
        faceRoot.localRotation = Quaternion.identity;
        faceRoot.localScale = Vector3.one;
        PoseFacePart(leftEyeRenderer.transform, LeftEyeLocalPosition + new Vector2(0f, attackBlend * 0.01f), faceScale, 0f);
        PoseFacePart(rightEyeRenderer.transform, RightEyeLocalPosition + new Vector2(0f, attackBlend * 0.01f), faceScale, 0f);
        PoseFacePart(mouthRenderer.transform, MouthLocalPosition + new Vector2(0f, hitBlend * 0.04f - attackBlend * 0.01f), faceScale, 0f);

        float leftUpperArmAngle;
        float leftLowerArmAngle;
        float leftHandAngle;
        float rightUpperArmAngle;
        float rightLowerArmAngle;
        float rightHandAngle;
        float leftUpperLegAngle;
        float leftLowerLegAngle;
        float rightUpperLegAngle;
        float rightLowerLegAngle;

        if (grounded)
        {
            leftUpperArmAngle = -148f - swing * 0.35f + attackBlend * 14f - hitBlend * 6f;
            leftLowerArmAngle = -136f - swing * 0.18f + attackBlend * 8f - hitBlend * 4f;
            leftHandAngle = -18f + attackBlend * 18f;

            rightUpperArmAngle = -32f + swing * 0.35f - attackBlend * 46f + hitBlend * 8f;
            rightLowerArmAngle = -44f + swing * 0.18f - attackBlend * 18f + hitBlend * 5f;
            rightHandAngle = 18f - attackBlend * 24f;

            leftUpperLegAngle = -132f - swing * 0.55f - attackBlend * 2f;
            leftLowerLegAngle = -102f - swing * 0.18f;
            rightUpperLegAngle = -48f + swing * 0.55f + attackBlend * 2f;
            rightLowerLegAngle = -78f + swing * 0.18f;
        }
        else
        {
            leftUpperArmAngle = -156f - jumpArmLiftAngle;
            leftLowerArmAngle = -144f - jumpArmLiftAngle * 0.5f;
            leftHandAngle = -8f;

            rightUpperArmAngle = -24f + jumpArmLiftAngle;
            rightLowerArmAngle = -34f + jumpArmLiftAngle * 0.5f;
            rightHandAngle = 8f;

            leftUpperLegAngle = -136f - jumpLegTuckAngle * 0.35f;
            leftLowerLegAngle = -110f - jumpLegTuckAngle * 0.35f;
            rightUpperLegAngle = -44f + jumpLegTuckAngle * 0.35f;
            rightLowerLegAngle = -70f + jumpLegTuckAngle * 0.35f;
        }

        Vector2 leftElbow = PoseSegment(leftUpperArmTransform, LeftShoulder, UpperArmLength, leftUpperArmAngle, LeftUpperArmArtAngle, limbScale);
        Vector2 leftHandJoint = PoseSegment(leftLowerArmTransform, leftElbow, LowerArmLength, leftLowerArmAngle, LeftLowerArmArtAngle, limbScale);
        PoseHand(leftHandTransform, leftHandJoint + AngleToVector(leftLowerArmAngle) * 0.03f, leftHandAngle, LeftHandArtAngle);

        Vector2 rightElbow = PoseSegment(rightUpperArmTransform, RightShoulder, UpperArmLength, rightUpperArmAngle, RightUpperArmArtAngle, limbScale);
        Vector2 rightHandJoint = PoseSegment(rightLowerArmTransform, rightElbow, LowerArmLength, rightLowerArmAngle, RightLowerArmArtAngle, limbScale);
        PoseHand(rightHandTransform, rightHandJoint + AngleToVector(rightLowerArmAngle) * 0.03f, rightHandAngle, RightHandArtAngle);

        Vector2 leftKnee = PoseSegment(leftUpperLegTransform, LeftHip, UpperLegLength, leftUpperLegAngle, LeftUpperLegArtAngle, limbScale);
        PoseSegment(leftLowerLegTransform, leftKnee, LowerLegLength, leftLowerLegAngle, LeftLowerLegArtAngle, limbScale);

        Vector2 rightKnee = PoseSegment(rightUpperLegTransform, RightHip, UpperLegLength, rightUpperLegAngle, RightUpperLegArtAngle, limbScale);
        PoseSegment(rightLowerLegTransform, rightKnee, LowerLegLength, rightLowerLegAngle, RightLowerLegArtAngle, limbScale);
    }

    private void PoseStaticPart(Transform part, Vector2 localPosition, float scale, float angle)
    {
        part.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);
        part.localRotation = Quaternion.Euler(0f, 0f, angle);
        part.localScale = Vector3.one * scale;
    }

    private void PoseFacePart(Transform part, Vector2 localPosition, float scale, float angle)
    {
        part.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);
        part.localRotation = Quaternion.Euler(0f, 0f, angle);
        part.localScale = Vector3.one * scale;
    }

    private Vector2 PoseSegment(Transform part, Vector2 start, float length, float desiredAngle, float artAngle, float scale)
    {
        Vector2 direction = AngleToVector(desiredAngle);
        Vector2 midpoint = start + direction * (length * 0.5f);

        part.localPosition = new Vector3(midpoint.x, midpoint.y, 0f);
        part.localRotation = Quaternion.Euler(0f, 0f, desiredAngle - artAngle);
        part.localScale = Vector3.one * scale;

        return start + direction * length;
    }

    private void PoseHand(Transform part, Vector2 localPosition, float desiredAngle, float artAngle)
    {
        part.localPosition = new Vector3(localPosition.x, localPosition.y, 0f);
        part.localRotation = Quaternion.Euler(0f, 0f, desiredAngle - artAngle);
        part.localScale = Vector3.one * handScale;
    }

    private Sprite ResolveLeftEyeSprite()
    {
        if (hitTimer > 0f)
        {
            return sadLeftEyeSprite;
        }

        if (attackTimer > 0f)
        {
            return angryLeftEyeSprite;
        }

        return idleLeftEyeSprite;
    }

    private Sprite ResolveRightEyeSprite()
    {
        if (hitTimer > 0f)
        {
            return sadRightEyeSprite;
        }

        if (attackTimer > 0f)
        {
            return angryRightEyeSprite;
        }

        return idleRightEyeSprite;
    }

    private Sprite ResolveMouthSprite()
    {
        if (hitTimer > 0f)
        {
            return sadMouthSprite;
        }

        if (attackTimer > 0f)
        {
            return angryMouthSprite;
        }

        return idleMouthSprite;
    }

    private Transform EnsureSpritePart(Transform parent, string name, ref SpriteRenderer renderer, Sprite sprite, int sortingOrder)
    {
        Transform part = EnsureChild(parent, name);
        DisableLegacyRendererChild(part);
        renderer = part.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = part.gameObject.AddComponent<SpriteRenderer>();
        }

        renderer.sortingOrder = sortingOrder;
        renderer.sprite = sprite;
        return part;
    }

    private void EnsureFacePart(Transform parent, string name, ref SpriteRenderer renderer, Sprite sprite, int sortingOrder)
    {
        Transform part = EnsureChild(parent, name);
        DisableLegacyRendererChild(part);
        renderer = part.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = part.gameObject.AddComponent<SpriteRenderer>();
        }

        renderer.sortingOrder = sortingOrder;
        renderer.sprite = sprite;
    }

    private Transform EnsureChild(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        if (child != null)
        {
            return child;
        }

        GameObject childObject = new GameObject(childName);
        child = childObject.transform;
        child.SetParent(parent, false);
        return child;
    }

    private void AssignSprite(SpriteRenderer renderer, Sprite sprite)
    {
        if (renderer != null)
        {
            renderer.sprite = sprite;
        }
    }

    private void DisableLegacyRendererChild(Transform part)
    {
        Transform legacyRenderer = part.Find("Renderer");
        if (legacyRenderer == null)
        {
            return;
        }

        legacyRenderer.gameObject.SetActive(false);
    }

    private Vector2 AngleToVector(float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        TriggerAttackPose();
    }
}
