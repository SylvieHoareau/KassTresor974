using System;
using UnityEditor;
using UnityEngine;

namespace ithappy.Animals_FREE
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    [DisallowMultipleComponent]
    public class CreatureMover : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField]
        private float m_WalkSpeed = 1f;
        [SerializeField]
        private float m_RunSpeed = 4f;
        [SerializeField, Range(0f, 360f)]
        private float m_RotateSpeed = 90f;
        [SerializeField]
        private Space m_Space = Space.Self;
        [SerializeField]
        private float m_JumpHeight = 5f;

        [Header("Animator")]
        [SerializeField]
        private string m_VerticalID = "Vert";
        [SerializeField]
        private string m_StateID = "State";
        [SerializeField]
        private LookWeight m_LookWeight = new(1f, 0.3f, 0.7f, 1f);

        [Header("IA - Wander Settings")]
        [SerializeField, Tooltip("Activer l'IA autonome de déplacement")]
        private bool m_EnableAI = true;

        [SerializeField, Tooltip("Rayon de la zone dans laquelle la créature se déplace")]
        private float m_WanderRadius = 5f;

        [SerializeField, Tooltip("Temps minimum d'attente à l'arrêt")]
        private float m_MinIdleTime = 1f;

        [SerializeField, Tooltip("Temps maximum d'attente à l'arrêt")]
        private float m_MaxIdleTime = 3f;

        [SerializeField, Tooltip("Durée max d'un déplacement avant de forcer un changement")]
        private float m_MaxMoveDuration = 5f;

        [SerializeField, Range(0f, 1f), Tooltip("Probabilité de courir plutôt que marcher")]
        private float m_RunProbability = 0.3f;

        [Header("IA - Obstacles")]
        [SerializeField, Tooltip("Layers des obstacles (murs, barrières, etc.)")]
        private LayerMask m_ObstacleLayers;

        [SerializeField, Tooltip("Distance de détection d'obstacle devant la créature")]
        private float m_ObstacleCheckDistance = 1f;

        private Transform m_Transform;
        private CharacterController m_Controller;
        private Animator m_Animator;

        private MovementHandler m_Movement;
        private AnimationHandler m_Animation;

        private Vector2 m_Axis;
        private Vector3 m_Target;
        private bool m_IsRun;

        private bool m_IsMoving;

        // IA
        private Vector3 m_Origin;
        private Vector3 m_CurrentDestination;
        private float m_MoveStartTime;
        private Coroutine m_AICoroutine;

        public Vector2 Axis => m_Axis;
        public Vector3 Target => m_Target;
        public bool IsRun => m_IsRun;

        private void OnValidate()
        {
            m_WalkSpeed = Mathf.Max(m_WalkSpeed, 0f);
            m_RunSpeed = Mathf.Max(m_RunSpeed, m_WalkSpeed);

            m_Movement?.SetStats(m_WalkSpeed / 3.6f, m_RunSpeed / 3.6f, m_RotateSpeed, m_JumpHeight, m_Space);
        }

        private void Awake()
        {
            m_Transform = transform;
            m_Controller = GetComponent<CharacterController>();
            m_Animator = GetComponent<Animator>();

            m_Movement = new MovementHandler(m_Controller, m_Transform, m_WalkSpeed, m_RunSpeed, m_RotateSpeed, m_JumpHeight, m_Space);
            m_Animation = new AnimationHandler(m_Animator, m_VerticalID, m_StateID);
        }

        private void Start()
        {
            m_Origin = m_Transform.position;

            if (m_EnableAI)
            {
                m_AICoroutine = StartCoroutine(AIBehaviourLoop());
            }
        }

        private void Update()
        {
            // Détection d'obstacle devant pour l'IA
            if (m_EnableAI && m_IsMoving && IsObstacleAhead())
            {
                ChooseNewDestination();
            }

            m_Movement.Move(Time.deltaTime, in m_Axis, in m_Target, m_IsRun, m_IsMoving, out var animAxis, out var isAir);
            m_Animation.Animate(in animAxis, m_IsRun ? 1f : 0f, Time.deltaTime);
        }

        private void OnAnimatorIK()
        {
            m_Animation.AnimateIK(in m_Target, m_LookWeight);
        }

        /// <summary>
        /// Ancienne méthode d'input. Tu n'as plus besoin de l'appeler pour l'IA.
        /// </summary>
        public void SetInput(in Vector2 axis, in Vector3 target, in bool isRun, in bool isJump)
        {
            // Si tu veux forcer le contrôle manuel, tu peux désactiver l'IA dans l'inspector (m_EnableAI = false)
            if (m_EnableAI)
                return;

            m_Axis = axis;
            m_Target = target;
            m_IsRun = isRun;

            if (m_Axis.sqrMagnitude < Mathf.Epsilon)
            {
                m_Axis = Vector2.zero;
                m_IsMoving = false;
            }
            else
            {
                m_Axis = Vector3.ClampMagnitude(m_Axis, 1f);
                m_IsMoving = true;
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.normal.y > m_Controller.stepOffset)
            {
                m_Movement.SetSurface(hit.normal);
            }

            // Si on touche un obstacle (mur, barrière) : nouvelle destination
            if (m_EnableAI && m_ObstacleLayers != 0)
            {
                if (((1 << hit.gameObject.layer) & m_ObstacleLayers) != 0)
                {
                    ChooseNewDestination();
                }
            }
        }

        /// <summary>
        /// IA principale : alterne entre idle et déplacements vers des points aléatoires.
        /// </summary>
        private System.Collections.IEnumerator AIBehaviourLoop()
        {
            while (true)
            {
                // Phase d'attente / idle
                float idleTime = UnityEngine.Random.Range(m_MinIdleTime, m_MaxIdleTime);
                m_Axis = Vector2.zero;
                m_IsMoving = false;
                yield return new WaitForSeconds(idleTime);

                // Choix d'un point de destination dans le rayon
                ChooseNewDestination(startMove: true);

                // Attente jusqu'à ce que le déplacement soit terminé ou timeout
                while (m_IsMoving)
                {
                    float dist = HorizontalDistance(m_Transform.position, m_CurrentDestination);

                    if (dist < 0.5f)
                    {
                        // Arrivé à destination
                        m_IsMoving = false;
                        m_Axis = Vector2.zero;
                        break;
                    }

                    if (Time.time - m_MoveStartTime > m_MaxMoveDuration)
                    {
                        // Anti-boucle : trop longtemps sur le même move
                        m_IsMoving = false;
                        m_Axis = Vector2.zero;
                        break;
                    }

                    yield return null;
                }
            }
        }

        /// <summary>
        /// Choisit une nouvelle destination dans le rayon et met à jour l'input IA.
        /// </summary>
        private void ChooseNewDestination(bool startMove = false)
        {
            Vector2 circle = UnityEngine.Random.insideUnitCircle * m_WanderRadius;
            m_CurrentDestination = m_Origin + new Vector3(circle.x, 0f, circle.y);

            // On dit à la créature de "regarder" vers cette destination
            m_Target = m_CurrentDestination;

            // On se déplace vers l'avant relatif à ce target (voir MovementHandler)
            m_Axis = Vector2.up;
            m_IsRun = UnityEngine.Random.value < m_RunProbability;
            m_IsMoving = true;
            m_MoveStartTime = Time.time;

            if (!startMove)
            {
                // Pas besoin de plus : la boucle IA vérifie déjà la distance / timeout
            }
        }

        /// <summary>
        /// Vérifie s'il y a un obstacle devant via un Raycast.
        /// </summary>
        private bool IsObstacleAhead()
        {
            if (m_ObstacleLayers == 0)
                return false;

            Vector3 origin = m_Transform.position + Vector3.up * (m_Controller.height * 0.5f);
            Vector3 dir = m_Transform.forward;

            return Physics.Raycast(origin, dir, m_ObstacleCheckDistance, m_ObstacleLayers);
        }

        private float HorizontalDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }

        [Serializable]
        private struct LookWeight
        {
            public float weight;
            public float body;
            public float head;
            public float eyes;

            public LookWeight(float weight, float body, float head, float eyes)
            {
                this.weight = weight;
                this.body = body;
                this.head = head;
                this.eyes = eyes;
            }
        }

        #region Handlers
        private class MovementHandler
        {
            private readonly CharacterController m_Controller;
            private readonly Transform m_Transform;

            private float m_WalkSpeed;
            private float m_RunSpeed;
            private float m_RotateSpeed;

            private Space m_Space;

            private readonly float m_Luft = 75f;

            private float m_TargetAngle;
            private bool m_IsRotating = false;

            private Vector3 m_Normal;
            private Vector3 m_GravityAcelleration = Physics.gravity;

            private float m_jumpTimer;
            private Vector3 m_LastForward;

            public MovementHandler(CharacterController controller, Transform transform, float walkSpeed, float runSpeed, float rotateSpeed, float jumpHeight, Space space)
            {
                m_Controller = controller;
                m_Transform = transform;

                m_WalkSpeed = walkSpeed;
                m_RunSpeed = runSpeed;
                m_RotateSpeed = rotateSpeed;

                m_Space = space;
            }

            public void SetStats(float walkSpeed, float runSpeed, float rotateSpeed, float jumpHeight, Space space)
            {
                m_WalkSpeed = walkSpeed;
                m_RunSpeed = runSpeed;
                m_RotateSpeed = rotateSpeed;

                m_Space = space;
            }

            public void SetSurface(in Vector3 normal)
            {
                m_Normal = normal;
            }

            public void Move(float deltaTime, in Vector2 axis, in Vector3 target, bool isRun, bool isMoving, out Vector2 animAxis, out bool isAir)
            {
                var cameraLook = Vector3.Normalize(target - m_Transform.position);
                var targetForward = m_LastForward;

                ConvertMovement(in axis, in cameraLook, out var movement);
                if (movement.sqrMagnitude > 0.5f)
                {
                    m_LastForward = Vector3.Normalize(movement);
                }

                CaculateGravity(deltaTime, out isAir);
                Displace(deltaTime, in movement, isRun);
                Turn(in targetForward, isMoving);
                UpdateRotation(deltaTime);

                GenAnimationAxis(in movement, out animAxis);
            }

            private void ConvertMovement(in Vector2 axis, in Vector3 targetForward, out Vector3 movement)
            {
                Vector3 forward;
                Vector3 right;

                if (m_Space == Space.Self)
                {
                    forward = new Vector3(targetForward.x, 0f, targetForward.z).normalized;
                    right = Vector3.Cross(Vector3.up, forward).normalized;
                }
                else
                {
                    forward = Vector3.forward;
                    right = Vector3.right;
                }

                movement = axis.x * right + axis.y * forward;
                movement = Vector3.ProjectOnPlane(movement, m_Normal);
            }

            private void Displace(float deltaTime, in Vector3 movement, bool isRun)
            {
                Vector3 displacement = (isRun ? m_RunSpeed : m_WalkSpeed) * movement;
                displacement += m_GravityAcelleration;
                displacement *= deltaTime;

                m_Controller.Move(displacement);
            }

            private void CaculateGravity(float deltaTime, out bool isAir)
            {
                m_jumpTimer = Mathf.Max(m_jumpTimer - deltaTime, 0f);

                if (m_Controller.isGrounded)
                {
                    m_GravityAcelleration = Physics.gravity;
                    isAir = false;

                    return;
                }

                isAir = true;

                m_GravityAcelleration += Physics.gravity * deltaTime;
                return;
            }

            private void GenAnimationAxis(in Vector3 movement, out Vector2 animAxis)
            {
                if (m_Space == Space.Self)
                {
                    animAxis = new Vector2(Vector3.Dot(movement, m_Transform.right), Vector3.Dot(movement, m_Transform.forward));
                }
                else
                {
                    animAxis = new Vector2(Vector3.Dot(movement, Vector3.right), Vector3.Dot(movement, Vector3.forward));
                }
            }

            private void Turn(in Vector3 targetForward, bool isMoving)
            {
                var angle = Vector3.SignedAngle(m_Transform.forward, Vector3.ProjectOnPlane(targetForward, Vector3.up), Vector3.up);

                if (!m_IsRotating)
                {
                    if (!isMoving && Mathf.Abs(angle) < m_Luft)
                    {
                        m_IsRotating = false;
                        return;
                    }

                    m_IsRotating = true;
                }

                m_TargetAngle = angle;
            }

            private void UpdateRotation(float deltaTime)
            {
                if (!m_IsRotating)
                {
                    return;
                }

                var rotDelta = m_RotateSpeed * deltaTime;
                if (rotDelta + Mathf.PI * 2f + Mathf.Epsilon >= Mathf.Abs(m_TargetAngle))
                {
                    rotDelta = m_TargetAngle;
                    m_IsRotating = false;
                }
                else
                {
                    rotDelta *= Mathf.Sign(m_TargetAngle);
                }

                m_Transform.Rotate(Vector3.up, rotDelta);
            }
        }

        private class AnimationHandler
        {
            private readonly Animator m_Animator;
            private readonly string m_VerticalID;
            private readonly string m_StateID;

            private readonly float k_InputFlow = 4.5f;

            private float m_FlowState;
            private Vector2 m_FlowAxis;

            public AnimationHandler(Animator animator, string verticalID, string stateID)
            {
                m_Animator = animator;
                m_VerticalID = verticalID;
                m_StateID = stateID;
            }

            public void Animate(in Vector2 axis, float state, float deltaTime)
            {
                m_Animator.SetFloat(m_VerticalID, m_FlowAxis.magnitude);
                m_Animator.SetFloat(m_StateID, Mathf.Clamp01(m_FlowState));

                m_FlowAxis = Vector2.ClampMagnitude(m_FlowAxis + k_InputFlow * deltaTime * (axis - m_FlowAxis).normalized, 1f);
                m_FlowState = Mathf.Clamp01(m_FlowState + k_InputFlow * deltaTime * Mathf.Sign(state - m_FlowState));
            }

            public void AnimateIK(in Vector3 target, in LookWeight lookWeight)
            {
                m_Animator.SetLookAtPosition(target);
                m_Animator.SetLookAtWeight(lookWeight.weight, lookWeight.body, lookWeight.head, lookWeight.eyes);
            }
        }
        #endregion
    }
}
