using System.Collections;
using UnityEngine;

namespace Ursaanimation.CubicFarmAnimals
{
    public class AnimationController : MonoBehaviour
    {
        [Header("Animator & Animations")]
        public Animator animator;
        public string idleAnimation = "idle";           // Met le nom de ton anim d'idle si tu en as une
        public string walkForwardAnimation = "walk_forward";
        public string walkBackwardAnimation = "walk_backwards";  // pas utilisé ici mais on garde
        public string runForwardAnimation = "run_forward";
        public string turn90LAnimation = "turn_90_L";
        public string turn90RAnimation = "turn_90_R";
        public string trotAnimation = "trot_forward";
        public string sittostandAnimation = "sit_to_stand";
        public string standtositAnimation = "stand_to_sit";

        [Header("IA - Déplacements")]
        [Tooltip("Vitesse de marche de l'animal")]
        public float walkSpeed = 1.5f;

        [Tooltip("Vitesse de trot de l'animal")]
        public float trotSpeed = 2.5f;

        [Tooltip("Vitesse de course de l'animal")]
        public float runSpeed = 3.5f;

        [Tooltip("Vitesse de rotation (deg/sec)")]
        public float turnSpeed = 120f;

        [Tooltip("Rayon de la zone dans laquelle l'animal se déplace")]
        public float wanderRadius = 5f;

        [Header("IA - Timings")]
        [Tooltip("Temps minimum d'attente à l'arrêt")]
        public float minIdleTime = 1f;

        [Tooltip("Temps maximum d'attente à l'arrêt")]
        public float maxIdleTime = 3f;

        [Tooltip("Durée d'un quart de tour (turn_90)")]
        public float turnDuration = 0.5f;

        [Tooltip("Probabilité de s'asseoir au lieu de juste attendre (0 à 1)")]
        [Range(0f, 1f)]
        public float sitProbability = 0.2f;

        [Tooltip("Durée passée assis avant de se relever")]
        public float sitTime = 2f;

        [Header("Anti-boucle / Obstacles")]
        [Tooltip("Durée max d'un déplacement avant de forcer un changement")]
        public float maxMoveDuration = 5f;

        [Tooltip("Layers des obstacles (murs, barrières, etc.)")]
        public LayerMask obstacleLayers;

        [Tooltip("Distance de détection d'obstacle devant l'animal")]
        public float obstacleCheckDistance = 0.75f;

        private Vector3 startPosition;
        private Vector3 targetPosition;
        private bool isMoving = false;
        private float currentMoveSpeed = 1.5f;
        private float moveStartTime;

        void Start()
        {
            if (animator == null)
                animator = GetComponent<Animator>();

            startPosition = transform.position;

            // On lance la boucle IA
            StartCoroutine(AIBehaviourLoop());
        }

        void Update()
        {
            if (isMoving)
            {
                // Détection d'obstacle devant
                if (IsObstacleAhead())
                {
                    HandleObstacleHit();
                }

                MoveTowardsTarget();
            }
        }

        /// <summary>
        /// Boucle principale de comportement de l'animal.
        /// </summary>
        private IEnumerator AIBehaviourLoop()
        {
            while (true)
            {
                float r = Random.value;

                if (r < 0.6f)
                {
                    // Se déplacer vers un point aléatoire
                    StartRandomMove();
                    while (isMoving)
                        yield return null;
                }
                else if (r < 0.8f)
                {
                    // Tourner sur place
                    yield return StartCoroutine(TurnRandom90());
                }
                else
                {
                    // Idle ou sit
                    yield return StartCoroutine(IdleOrSit());
                }

                yield return null;
            }
        }

        /// <summary>
        /// Lance un déplacement vers un point aléatoire dans le rayon.
        /// </summary>
        private void StartRandomMove()
        {
            // Point aléatoire dans un disque autour de la position de départ
            Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
            targetPosition = startPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);

            // Choisir vitesse & anim
            float r = Random.value;

            if (r < 0.6f)
            {
                currentMoveSpeed = walkSpeed;
                PlayIfNotEmpty(walkForwardAnimation);
            }
            else if (r < 0.9f)
            {
                currentMoveSpeed = trotSpeed;
                PlayIfNotEmpty(trotAnimation);
            }
            else
            {
                currentMoveSpeed = runSpeed;
                PlayIfNotEmpty(runForwardAnimation);
            }

            isMoving = true;
            moveStartTime = Time.time;
        }

        /// <summary>
        /// Gère le déplacement frame par frame vers le targetPosition.
        /// </summary>
        private void MoveTowardsTarget()
        {
            // Si ça fait trop longtemps qu'il bouge : on stoppe pour éviter les boucles
            if (Time.time - moveStartTime > maxMoveDuration)
            {
                isMoving = false;
                PlayIfNotEmpty(idleAnimation);
                return;
            }

            Vector3 direction = targetPosition - transform.position;
            direction.y = 0f;

            float distance = direction.magnitude;

            if (distance < 0.1f)
            {
                isMoving = false;
                PlayIfNotEmpty(idleAnimation);
                return;
            }

            // Rotation vers la cible
            if (direction.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRot,
                    turnSpeed * Time.deltaTime
                );
            }

            // Déplacement vers l'avant
            transform.position += transform.forward * currentMoveSpeed * Time.deltaTime;

            // Clamp dans la zone : si jamais il sort trop, on remet un target vers le centre
            float distFromCenter = Vector3.Distance(startPosition, transform.position);
            if (distFromCenter > wanderRadius * 1.5f)
            {
                targetPosition = startPosition;
                moveStartTime = Time.time;
            }
        }

        /// <summary>
        /// Tourne l'animal de 90° à gauche ou à droite.
        /// </summary>
        private IEnumerator TurnRandom90()
        {
            bool toLeft = Random.value < 0.5f;
            string animName = toLeft ? turn90LAnimation : turn90RAnimation;

            PlayIfNotEmpty(animName);

            float elapsed = 0f;
            float angle = toLeft ? -90f : 90f;

            Quaternion startRot = transform.rotation;
            Quaternion endRot = startRot * Quaternion.Euler(0f, angle, 0f);

            while (elapsed < turnDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / turnDuration);
                transform.rotation = Quaternion.Slerp(startRot, endRot, t);
                yield return null;
            }

            PlayIfNotEmpty(idleAnimation);

            float waitTime = Random.Range(minIdleTime * 0.5f, minIdleTime);
            yield return new WaitForSeconds(waitTime);
        }

        /// <summary>
        /// Idle simple ou séquence s'asseoir / rester / se relever.
        /// </summary>
        private IEnumerator IdleOrSit()
        {
            bool willSit = Random.value < sitProbability;

            if (willSit && !string.IsNullOrEmpty(standtositAnimation) && !string.IsNullOrEmpty(sittostandAnimation))
            {
                // Stand -> Sit
                PlayIfNotEmpty(standtositAnimation);
                yield return new WaitForSeconds(0.7f);

                // Assis
                yield return new WaitForSeconds(sitTime);

                // Sit -> Stand
                PlayIfNotEmpty(sittostandAnimation);
                yield return new WaitForSeconds(0.7f);
            }
            else
            {
                // Idle debout
                PlayIfNotEmpty(idleAnimation);
                float idleTime = Random.Range(minIdleTime, maxIdleTime);
                yield return new WaitForSeconds(idleTime);
            }
        }

        /// <summary>
        /// Vérifie s'il y a un obstacle devant avec un raycast.
        /// </summary>
        private bool IsObstacleAhead()
        {
            if (obstacleLayers == 0) return false;

            Vector3 origin = transform.position + Vector3.up * 0.2f;
            Vector3 dir = transform.forward;

            return Physics.Raycast(origin, dir, obstacleCheckDistance, obstacleLayers);
        }

        /// <summary>
        /// Appelé quand on tape dans une barrière/obstacle.
        /// </summary>
        private void HandleObstacleHit()
        {
            // On force un changement de direction plus ou moins opposée
            float angle = Random.Range(100f, 180f) * (Random.value < 0.5f ? 1f : -1f);
            transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y + angle, 0f);

            // Nouveau point cible dans cette nouvelle direction
            Vector3 dir = transform.forward;
            dir.y = 0f;
            dir.Normalize();

            float distance = Random.Range(1f, wanderRadius);
            targetPosition = transform.position + dir * distance;

            moveStartTime = Time.time;
        }

        /// <summary>
        /// Collision physique avec un obstacle.
        /// </summary>
        private void OnCollisionEnter(Collision collision)
        {
            // Si l'objet touché est dans les layers d'obstacles, on réagit
            if (((1 << collision.gameObject.layer) & obstacleLayers) != 0)
            {
                HandleObstacleHit();
            }
        }

        /// <summary>
        /// Joue une anim seulement si le nom n'est pas vide.
        /// </summary>
        private void PlayIfNotEmpty(string animationName)
        {
            if (animator == null)
                return;

            if (!string.IsNullOrEmpty(animationName))
            {
                animator.Play(animationName);
            }
        }
    }
}
