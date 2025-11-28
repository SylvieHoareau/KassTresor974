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
        public string walkBackwardAnimation = "walk_backwards";  // pas utilisé ici mais on laisse
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

        private Vector3 startPosition;
        private Vector3 targetPosition;
        private bool isMoving = false;

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
                MoveTowardsTarget();
            }
        }

        /// <summary>
        /// Boucle principale de comportement de l'animal.
        /// </summary>
        private IEnumerator AIBehaviourLoop()
        {
            // Boucle infinie : l'animal vit sa meilleure vie
            while (true)
            {
                // 1) Choisir une action aléatoire
                float r = Random.value;

                if (r < 0.6f)
                {
                    // Se déplacer vers un point aléatoire
                    StartRandomMove();
                    // Attendre la fin du déplacement
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
                    // Idle / Sit
                    yield return StartCoroutine(IdleOrSit());
                }

                // Petit délai entre les actions pour éviter les enchaînements trop robotiques
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

            // Choisir une vitesse & anim aléatoire (walk / trot / run)
            float r = Random.value;
            float speed;

            if (r < 0.6f)
            {
                speed = walkSpeed;
                PlayIfNotEmpty(walkForwardAnimation);
            }
            else if (r < 0.9f)
            {
                speed = trotSpeed;
                PlayIfNotEmpty(trotAnimation);
            }
            else
            {
                speed = runSpeed;
                PlayIfNotEmpty(runForwardAnimation);
            }

            currentMoveSpeed = speed;
            isMoving = true;
        }

        private float currentMoveSpeed = 1.5f;

        /// <summary>
        /// Gère le déplacement frame par frame vers le targetPosition.
        /// </summary>
        private void MoveTowardsTarget()
        {
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0f;

            float distance = direction.magnitude;

            if (distance < 0.1f)
            {
                // Arrivé à destination
                isMoving = false;
                // Retour à l'idle
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

            // Si jamais l'animal dépasse trop le rayon, on le ramène doucement vers le centre
            float distFromCenter = Vector3.Distance(startPosition, transform.position);
            if (distFromCenter > wanderRadius * 1.2f)
            {
                targetPosition = startPosition;
            }
        }

        /// <summary>
        /// Tourne l'animal de 90° à gauche ou à droite en utilisant les anims de turn.
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

            // Retour à l'idle
            PlayIfNotEmpty(idleAnimation);

            // Petite pause après le turn
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
                // Attendre la durée approximative de l'anim
                yield return new WaitForSeconds(0.7f);

                // Assis (on peut laisser l'anim de fin en boucle si c'est un état)
                yield return new WaitForSeconds(sitTime);

                // Sit -> Stand
                PlayIfNotEmpty(sittostandAnimation);
                yield return new WaitForSeconds(0.7f);
            }
            else
            {
                // Idle debout classique
                PlayIfNotEmpty(idleAnimation);
                float idleTime = Random.Range(minIdleTime, maxIdleTime);
                yield return new WaitForSeconds(idleTime);
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
