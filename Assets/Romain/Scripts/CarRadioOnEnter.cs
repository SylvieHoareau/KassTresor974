using UnityEngine;
using UnityEngine.SceneManagement;

public class CarRadioOnEnter : MonoBehaviour
{
    // Singleton pour éviter plusieurs radios persistantes
    private static CarRadioOnEnter Instance;

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
        // Singleton + persistance entre scènes
        if (Instance != null && Instance != this)
        {
            // Si un autre existe déjà, celui-ci dégage
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

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

        ResolveSceneReferences();

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
        if (radioSource == null)
            return;

        // Si on a perdu des refs (changement de scène), on retente doucement
        if (playerTransform == null || cameraOrbit == null)
            ResolveSceneReferences();

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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // À chaque nouvelle scène: on retente de retrouver Player et CameraOrbit
        ResolveSceneReferences();

        // Recalcule l'état (sinon ça peut rester dans un mix incohérent)
        bool inside = IsInside();
        ApplyStateInstant(inside);
        wasInside = inside;
    }

    private void ResolveSceneReferences()
    {
        // Player
        if (playerTransform == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }

        // CameraOrbit (si tu en as une par scène)
        if (cameraOrbit == null)
        {
            cameraOrbit = FindFirstObjectByType<RomainCameraOrbit>();
        }

        // Car transform:
        // - si l'objet persiste, carTransform reste bon
        // - si tu changes de scène et que la voiture n'existe plus, carTransform reste l'ancien (invalide si détruit)
        //   -> on ne force pas un remplacement ici, car c'est contextuel.
        //   Si tu veux, tu peux exposer une méthode publique SetCar(Transform newCar).
        if (carTransform == null) carTransform = transform;
    }

    private bool IsInside()
    {
        // Si on ne retrouve pas la caméra ou la voiture, on considère "dehors"
        if (cameraOrbit == null || carTransform == null) return false;
        return cameraOrbit.target == carTransform;
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

        radioSource.volume = currentVol;
        if (lowPass != null)
            lowPass.cutoffFrequency = currentCutoff;
    }

    private float ComputeDistanceFactor()
    {
        if (playerTransform == null) return 1f;
        if (carTransform == null) return minDistanceFactor;

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

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Optionnel: si dans une nouvelle scène tu veux relier la radio à une nouvelle voiture
    public void SetCar(Transform newCarTransform)
    {
        carTransform = newCarTransform;
        bool inside = IsInside();
        ApplyStateInstant(inside);
        wasInside = inside;
    }
}
