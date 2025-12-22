using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class MudSplatsCameraPS_Kinematic : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform carTransform;

    [Header("Speed → Emission")]
    [SerializeField] private float minSpeed = 4f;
    [SerializeField] private float maxSpeed = 18f;
    [SerializeField] private float maxRate = 40f;

    [Header("Mud ground")]
    [SerializeField] private bool requireMudGround = true;
    [SerializeField] private LayerMask mudLayers;
    [SerializeField] private float groundCheckDistance = 1.5f;

    [Header("Slip boost")]
    [SerializeField] private bool useSlipBoost = true;
    [SerializeField] private float slipBoostMultiplier = 1.8f;

    private ParticleSystem ps;
    private ParticleSystem.EmissionModule emission;

    private Vector3 lastPosition;
    private Vector3 velocity;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        emission = ps.emission;
        emission.rateOverTime = 0f;

        if (carTransform != null)
            lastPosition = carTransform.position;
    }

    void LateUpdate()
    {
        if (carTransform == null)
        {
            emission.rateOverTime = 0f;
            return;
        }

        // ===== Calcul vitesse réelle =====
        velocity = (carTransform.position - lastPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
        lastPosition = carTransform.position;

        float speed = velocity.magnitude;

        if (speed < minSpeed)
        {
            emission.rateOverTime = 0f;
            return;
        }

        if (requireMudGround && !IsOnMud())
        {
            emission.rateOverTime = 0f;
            return;
        }

        float t = Mathf.InverseLerp(minSpeed, maxSpeed, speed);
        float rate = Mathf.Lerp(0f, maxRate, t);

        if (useSlipBoost)
            rate *= GetSlipBoost();

        emission.rateOverTime = rate;
    }

    private bool IsOnMud()
    {
        Vector3 origin = carTransform.position + Vector3.up * 0.3f;
        return Physics.Raycast(origin, Vector3.down, groundCheckDistance, mudLayers, QueryTriggerInteraction.Ignore);
    }

    private float GetSlipBoost()
    {
        Vector3 flatVel = Vector3.ProjectOnPlane(velocity, Vector3.up);
        if (flatVel.sqrMagnitude < 0.01f)
            return 1f;

        float angle = Vector3.Angle(carTransform.forward, flatVel.normalized);
        float slip01 = Mathf.InverseLerp(0f, 40f, angle);

        return Mathf.Lerp(1f, slipBoostMultiplier, slip01);
    }
}
