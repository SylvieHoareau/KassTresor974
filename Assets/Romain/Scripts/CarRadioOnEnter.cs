using UnityEngine;

public class CarRadioOnEnter : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private RomainCameraOrbit cameraOrbit;
    [SerializeField] private Transform carTransform;

    [Tooltip("Le joueur (Transform). Si vide, le script essaie de le trouver via tag Player.")]
    [SerializeField] private Transform playerTransform;

    [Header("Radio")]
    [SerializeField] private AudioSource radioSource;
    [SerializeField] private AudioClip radioClip;

    [Header("Mix Inside (dans la voiture)")]
    [Range(0f, 1f)] [SerializeField] private float insideVolume = 0.85f;
    [Tooltip("LowPass cutoff (Hz). 22000 = aucun étouffement.")]
    [SerializeField] private float insideLowPassCutoff = 22000f;

    [Header("Mix Outside (hors de la voiture)")]
    [Range(0f, 1f)] [SerializeField] private float outsideBaseVolume = 0.30f;
    [Tooltip("Plus bas = plus étouffé (ex: 800-2000)")]
    [SerializeField] private float outsideLowPassCutoff = 1200f;

    [Header("Distance (uniquement dehors)")]
    [Tooltip("Distance où le son est à 100% (facteur = 1)")]
    [SerializeField] private float distanceFullVolume = 2.5f;

    [Tooltip("Distance où le son tombe au minimum")]
    [SerializeField] private float distanceMinVolume = 18f;

    [Tooltip("Facteur minimal (ex: 0 = inaudible, 0.05 = très faible mais présent)")]
    [Range(0f, 1f)] [SerializeField] private float minDistanceFactor = 0f;

    [Tooltip("Courbe d’atténuation (X=0 proche, X=1 loin). Si vide: atténuation linéaire.")]
    [SerializeField] private AnimationCurve distanceCurve = null;

    [Header("Transition")]
    [Tooltip("Temps pour passer d’un état à l’autre (sec)")]
    [SerializeField] private float blendTime = 0.25f;

    [Header("Délai de démarrage radio")]
    [SerializeField] private float radioStartDelay = 2f;

    private AudioLowPassFilter lowPass;
    private bool wasInside = false;

    private float currentVol;
    private float targetVol;
    private float currentCutoff;
    private float targetCutoff;

    private bool radioStartScheduled = false;

    private void Reset()
    {
        carTransform = transform;
    }

    private void Awake()
    {
        if (radioSource != null)
        {
            radioSource.playOnAwake = false;

            lowPass = radioSource.GetComponent<AudioLowPassFilter>();
            if (lowPass == null)
                lowPass = radioSource.gameObject.AddComponent<AudioLowPassFilter>();
        }
    }

    private void Start()
    {
        if (carTransform == null) carTransform = transform;

        if (playerTransform == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }

        if (radioSource == null) return;

        if (radioClip != null)
            radioSource.clip = radioClip;

        // Planifie le démarrage avec délai si on a un clip
        if (radioSource.clip != null && !radioSource.isPlaying)
            ScheduleRadioStart();

        bool inside = IsInside();
        ApplyStateInstant(inside);
        wasInside = inside;
    }

    private void Update()
    {
        if (cameraOrbit == null || carTransform == null || radioSource == null)
            return;

        bool inside = IsInside();

        // Si la radio n'est pas en lecture, on la planifie (avec délai)
        if (!radioSource.isPlaying && radioSource.clip != null && !radioStartScheduled)
            ScheduleRadioStart();

        if (inside != wasInside)
        {
            SetTargets(inside);
            wasInside = inside;
        }

        // Si on est dehors, on applique l’atténuation par distance en temps réel
        float distanceFactor = 1f;
        if (!inside)
            distanceFactor = ComputeDistanceFactor();

        float finalTargetVol = targetVol * distanceFactor;

        // Blend doux
        float t = (blendTime <= 0.0001f) ? 1f : (Time.deltaTime / blendTime);
        currentVol = Mathf.Lerp(currentVol, finalTargetVol, t);
        currentCutoff = Mathf.Lerp(currentCutoff, targetCutoff, t);

        radioSource.volume = currentVol;
        if (lowPass != null) lowPass.cutoffFrequency = currentCutoff;
    }

    private bool IsInside()
    {
        return cameraOrbit != null && cameraOrbit.target == carTransform;
    }

    private void SetTargets(bool inside)
    {
        targetVol = inside ? insideVolume : outsideBaseVolume;
        targetCutoff = inside ? insideLowPassCutoff : outsideLowPassCutoff;
    }

    private void ApplyStateInstant(bool inside)
    {
        SetTargets(inside);

        float distanceFactor = inside ? 1f : ComputeDistanceFactor();

        currentVol = targetVol * distanceFactor;
        currentCutoff = targetCutoff;

        if (radioSource != null)
            radioSource.volume = currentVol;

        if (lowPass != null)
            lowPass.cutoffFrequency = currentCutoff;
    }

    private float ComputeDistanceFactor()
    {
        if (playerTransform == null) return 1f;

        float d = Vector3.Distance(playerTransform.position, carTransform.position);

        if (distanceMinVolume <= distanceFullVolume + 0.0001f)
            return 1f;

        // 0 = proche (plein), 1 = loin (min)
        float x = Mathf.InverseLerp(distanceFullVolume, distanceMinVolume, d);
        x = Mathf.Clamp01(x);

        float factor;
        if (distanceCurve != null && distanceCurve.keys != null && distanceCurve.length > 0)
            factor = Mathf.Clamp01(distanceCurve.Evaluate(x));
        else
            factor = 1f - x; // linéaire

        factor = Mathf.Max(minDistanceFactor, factor);
        return factor;
    }

    private void ScheduleRadioStart()
    {
        if (radioStartScheduled) return;
        radioStartScheduled = true;
        Invoke(nameof(StartRadioDelayed), Mathf.Max(0f, radioStartDelay));
    }

    private void StartRadioDelayed()
    {
        radioStartScheduled = false;

        if (radioSource != null && radioSource.clip != null && !radioSource.isPlaying)
            radioSource.Play();
    }

    private void OnDisable()
    {
        // Évite un Invoke qui traîne si l'objet est désactivé/détruit
        if (radioStartScheduled)
        {
            CancelInvoke(nameof(StartRadioDelayed));
            radioStartScheduled = false;
        }
    }
}
