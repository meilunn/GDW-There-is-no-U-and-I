using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(PatrolController))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(VoiceLineSystem))]
public class TeammateController : MonoBehaviour
{
    #region Structs & Enums
    public enum TeammateState
    {
        AtWorkplace,
        Sleeping,
        Shitting,
        Patrolling,
        GoingToDestination,
        Yapping

        // TODO: add AtToilet
    }

    /// <summary>
    /// Mainly used to set curDestination. Helps to determine behaviour upon arriving at place
    /// </summary>
    public enum Place
    {
        None,
        Workplace,
        Toilet,
        Exit
    }

    /// <summary>
    /// Properties:
    /// <para><b>decrease</b>: Base constant decrease</para>
    /// <para><b>increase</b>: Base constant increase</para>
    /// <para><b>actThreshold</b>: Acting on low stat only possible below threshold</para>
    /// <para><b>tryActInterval</b>: Check/Roll for acting every x seconds</para>
    /// <para><b>probCurveExponent</b>: Probability for acting grows exponentially towards 0 stat</para>
    /// </summary>
    [Serializable]
    public struct StatConfig
    {
        [Tooltip("Base constant decrease")]
        public float decrease;
        [Tooltip("Base constant increase")]
        public float increase;
        [Tooltip("Acting on low stat only possible below threshold")]
        [Range(1f, 100f)] public float actThreshold;
        [Tooltip("Check/Roll for acting every x seconds")]
        public float tryActInterval;
        [Tooltip("Probability for acting grows exponentially towards 0 stat")]
        [Range(1f, 5f)] public float probCurveExponent;  // 1 = linear
    }
    
    [Serializable]
    public struct YapConfig
    {
        [Header("Detection")]
        [Tooltip("How close teammates must be to trigger a yap check")]
        public float detectionRadius;
        [Tooltip("How often (seconds) a patrolling teammate scans for nearby teammates")]
        public float checkInterval;

        [Header("Probability")]
        [Tooltip("Chance to start yapping when two patrollers meet")]
        [Range(0f, 1f)] public float probabilityPatrollerToPatroller;
        [Tooltip("Chance a patroller starts yapping at a working teammate")]
        [Range(0f, 1f)] public float probabilityPatrollerToWorker;

        [Header("Duration")]
        [Tooltip("Base duration of a yap conversation (seconds)")]
        public float duration;
        [Tooltip("Random ± offset so conversations vary in length")]
        public float durationVariance;

        [Header("Stat Effects")]
        [Tooltip("Fun gained per second while yapping")]
        public float funIncrease;

        [Header("Disturbance (for the working teammate)")]
        [Tooltip("Work efficiency multiplier while being yapped at (e.g. 0.5 = half)")]
        [Range(0f, 1f)] public float disturbedEfficiencyScale;
        [Tooltip("How long the productivity penalty lingers after the yap ends (seconds)")]
        public float disturbedDuration;

        [Header("Cooldown")]
        [Tooltip("After a yap ends, this teammate can't yap again for this many seconds")]
        public float globalCooldown;
        
    }

    #endregion

    #region Fields & Params

    [Header("State")]
    public TeammateState initialTeammateState = TeammateState.AtWorkplace;
    public TeammateState curTeammateState;

    [Header("Stats")]
    public StatConfig energyStatConfig = new();
    [Range(1f, 10f)] public float energyDecreaseScaleWhenWorking;
    [Range(-10f, 10f)]public float energyYappingScale;
    
    private float energy;
    private float energyActCooldown;

    public StatConfig bladderStatConfig = new();
    private float bladder;
    private float bladderActCooldown;

    public StatConfig hungerStatConfig = new();
    private float hunger;
    private float hungerActCooldown;
    
    public StatConfig funStatConfig = new();
    private float fun;
    private float funActCooldown;

    [Header("Places")] 
    public Place curDestination = Place.None;
    [Space(10)] 
    public GameObject workplace;
    public GameObject toilet;
    public GameObject exit;
    
    [Header("Walking params")]
    public float baseWalkSpeed;
    public float walkSpeedEnergyScale;
    public float walkSpeedBladderScale;
    public float walkSpeedHungerScale;

    [Header("Field of View params")] 
    public float angle = 90f;
    public float radius = 10f;

    [Header("Detection params")] 
    [Tooltip("Calls Gamemanager.IncreaseSus() when local sus points over threshold")]
    public int detectionThreshold = 100;
    [Tooltip("Local sus points gained per sus item seen")]
    [SerializeField] private int pointsPerCheck;
    private int currentPoints = 0;
    
    [SerializeField] private LayerMask traceAgainst;
    public Transform rayCastOrigin;

    [Header("Working params")]
    [Tooltip("Teammate should make progress every x minutes in game")]
    public int makeProgressInterval;
    private double lastProgressMadeTime;
    public float baseWorkEfficiency;
    private float curWorkEfficiency;
    
    [Header("Patrol params")]
    [Tooltip("Rolls for going to patrol every x minutes in game")]
    public int patrolCheckInterval;
    private double lastPatrolCheckTime;
    [Tooltip("Probability of going on patrol when rolling")]
    [Range(0f, 1f)] public float patrolProbability;

    [Header("Yapping params")]
    public YapConfig yapConfig = new();
    private float yapCheckCooldown;
    private float yapTimer;
    private float yapGlobalCooldownTimer;
    private float yapDisturbedTimer;
    private bool isDisturbed;
    private TeammateController yapPartner;

    [Header("Food n Drink")]
    private EdibleData.EdibleType _foodAwaited;
    private EdibleData.EdibleType _energyAwaited;

    public List<EdiblePreference> foodPreferences;
    public List<EdiblePreference> energyPreferences;
/*
    [Header("UI")]
    public TMP_Text teammateStateText;
    public TMP_Text energyText;
    public TMP_Text bladderText;
    public TMP_Text hungerText;
    public TMP_Text curDestText;
*/ 
    private GameManager gameManager;
    private NavMeshAgent agent;
    private PatrolController patrolController;
    private VoiceLineSystem voiceLineSystem;
    #endregion

    #region Methods

    private void Start()
    {
        gameManager = GameManager.instance;
        agent = GetComponent<NavMeshAgent>();
        patrolController = GetComponent<PatrolController>();
        voiceLineSystem = GetComponent<VoiceLineSystem>();

        DayReset();

        //GoToDestination(Place.Workplace);
    }

    private void Update()
    {
        UpdateStats();
        UpdateYapping();
        UpdateDisturbance();
        //UpdateUI();

        switch (curTeammateState)
        {
            case TeammateState.AtWorkplace:
                if (energy <= 0)
                {
                    Debug.Log($"{gameObject.name} fell asleep");

                    curTeammateState = TeammateState.Sleeping;

                    break;
                }

                DetectHand();

                // make progress
                int makeProgIntervallInSec = makeProgressInterval * 60;
                if (gameManager.dayTime >= lastProgressMadeTime + makeProgIntervallInSec)
                {
                    Debug.Log($"{gameObject.name} ProjectProgress.Work");

                    curWorkEfficiency = baseWorkEfficiency; // TODO: calculate based on stats

                    // Apply disturbance penalty
                    if (isDisturbed)
                        curWorkEfficiency *= yapConfig.disturbedEfficiencyScale;

                    gameManager.projectProgress.Work(curWorkEfficiency);

                    lastProgressMadeTime = gameManager.dayTime;
                }

                // go patrol?
                int patrolCheckIntervallInSec = patrolCheckInterval * 60;
                if (gameManager.dayTime >= lastPatrolCheckTime + patrolCheckIntervallInSec)
                {
                    Debug.Log($"{gameObject.name} patrol check");

                    if (Random.value < patrolProbability)
                        patrolController.StartPatrol();

                    lastPatrolCheckTime = gameManager.dayTime;
                }

                break;

            case TeammateState.GoingToDestination:
                // if agent hasn't arrived at dest
                DetectHand();
                if (agent.remainingDistance > 0.1f) break;

                // else
                Debug.Log($"{gameObject.name} arrived at {curDestination}");

                switch (curDestination)
                {
                    case Place.Workplace:
                        curTeammateState = TeammateState.AtWorkplace;
                        lastPatrolCheckTime = gameManager.dayTime;

                        break;
                    case Place.Toilet:
                        // TODO: do something after arrived at toilet

                        curTeammateState = TeammateState.Shitting;

                        break;
                    case Place.Exit:
                        // TODO: do something after arrived at exit

                        break;
                }
                
                curDestination = Place.None;

                break;

            case TeammateState.Sleeping:
                if (energy >= 100) // wake up
                {
                    Debug.Log($"{gameObject.name} wakes up");

                    curTeammateState = TeammateState.AtWorkplace;
                    break;
                }

                break;

            case TeammateState.Yapping:
                break;

            case TeammateState.Patrolling:
                DetectHand();
                TryStartYapping();
                break;

            case TeammateState.Shitting: 
                if (bladder >= 100)
                {
                    Debug.Log($"{gameObject.name} finished shitting");

                    GoToDestination(Place.Workplace);
                }
                break;

            default:
                DetectHand();
                break;

            // TODO: other states?
        }
    }

    private void DetectHand()
    {
        Vector3 handPosition = Player.Instance.PlayerHand.position;
        Vector3 distanceToHand = handPosition - transform.position;

        if (distanceToHand.magnitude <= radius)
        {
            if (Vector3.Dot(distanceToHand.normalized, transform.forward) > Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad))
            {
                Vector3 direction = handPosition - rayCastOrigin.position;

                if (Physics.Raycast(rayCastOrigin.position, direction, out RaycastHit hit, traceAgainst))
                {
                    Debug.Log(hit.collider.gameObject.name);
                    
                    if (hit.collider.TryGetComponent(out MovableInteractable detectedItem) &&
                        detectedItem == Player.Instance.ItemInHand && detectedItem.isSuspicious)
                    {
                        Debug.DrawRay(rayCastOrigin.position, direction, Color.red);

                        currentPoints += (int)Math.Ceiling(pointsPerCheck * Time.deltaTime);

                        //TODO: calculate current/needed detect-points depending on sleepiness

                        if (currentPoints >= detectionThreshold)
                        {
                            gameManager.IncreaseSus();
                            currentPoints = 0;
                        }
                    }
                    else
                    {
                        Debug.DrawRay(rayCastOrigin.position, direction, Color.green);

                        if (currentPoints > 0) currentPoints -= (int)Math.Ceiling(1 * Time.deltaTime);
                    }
                }
            }
            else
            {
                if (currentPoints > 0) currentPoints -= (int)Math.Ceiling(1 * Time.deltaTime);
            }
        }
    }


    /// <summary>
    /// Constant stat updates. Try making teammate act upon low stat
    /// </summary>
    private void UpdateStats()
    {
        
        switch (curTeammateState)
        {
            case TeammateState.AtWorkplace:
                energy -= energyStatConfig.decrease * energyDecreaseScaleWhenWorking * Time.deltaTime;
                break;
            case TeammateState.Sleeping:
                energy += energyStatConfig.increase * Time.deltaTime;
                break;
            case TeammateState.Yapping:
                energy += energyStatConfig.increase * energyYappingScale * Time.deltaTime;
                break;
            default:
                energy -= energyStatConfig.decrease * Time.deltaTime;
                break;
        }

        if (curTeammateState == TeammateState.Shitting)
            bladder += bladderStatConfig.increase * Time.deltaTime;
        else 
            bladder -= bladderStatConfig.decrease * Time.deltaTime;
        
        hunger -= hungerStatConfig.decrease * Time.deltaTime;
        fun -= funStatConfig.decrease * Time.deltaTime;

        energy = Mathf.Clamp(energy, 0f, 100f);
        bladder = Mathf.Clamp(bladder, 0f, 100f);
        hunger = Mathf.Clamp(hunger, 0f, 100f);
        fun = Mathf.Clamp(fun, 0f, 100f);

        if (curTeammateState == TeammateState.Sleeping) return;
        // try acting on low stat if cooldown reached & not sleeping
        energyActCooldown -= Time.deltaTime;
        if (energyActCooldown <= 0f)
        {
            energyActCooldown = energyStatConfig.tryActInterval;
            if (TryActOnStat(energy, energyStatConfig))
                OnLowEnergy();
        }

        bladderActCooldown -= Time.deltaTime;
        if (bladderActCooldown <= 0f)
        {
            bladderActCooldown = bladderStatConfig.tryActInterval;
            if (TryActOnStat(bladder, bladderStatConfig))
                OnLowBladder();
        }

        hungerActCooldown -= Time.deltaTime;
        if (hungerActCooldown <= 0f)
        {
            hungerActCooldown = hungerStatConfig.tryActInterval;
            if (TryActOnStat(hunger, hungerStatConfig))
                OnLowHunger();
        }

        funActCooldown -= Time.deltaTime;
        if (funActCooldown <= 0f)
        {
            funActCooldown = funStatConfig.tryActInterval;
            if (TryActOnStat(fun, funStatConfig))
            {
                OnLowFun();
            }
        }
    }

    /// <summary>
    /// Determines if teammate acts on low stat based on its config/probability
    /// </summary>
    /// <returns>True if should act, false if shouldn't act</returns>
    private bool TryActOnStat(float statValue, StatConfig statConfig)
    {
        if (statValue > statConfig.actThreshold) return false;

        // Remap stat from [threshold -> 0] to [0 -> 1]
        float t = 1f - (statValue / statConfig.actThreshold);
        float probability = Mathf.Pow(t, statConfig.probCurveExponent);

        return Random.value < probability;
    }

    private void OnLowEnergy()
    {
        // TODO: 
        voiceLineSystem.PlayBark(VoiceLineSystem.BarkType.Tired);
        if (curTeammateState == TeammateState.Patrolling || curTeammateState == TeammateState.Yapping)
            patrolController.EndPatrol();
        
        EdibleData.EdibleType pref = ChooseEdible(energyPreferences);

        switch (pref)
        {
            case EdibleData.EdibleType.Coffee: 
                gameManager.questManager.CreateAndStartQuest(QuestId.BringCoffee, this);
                break;
            case EdibleData.EdibleType.EnergyDrink: 
                gameManager.questManager.CreateAndStartQuest(QuestId.BringEnergy, this);
                break;
        }
        Debug.Log($"{gameObject.name} wants {pref}");
    }

    private void OnLowBladder()
    {
        Debug.Log($"{gameObject.name} acting on low bladder");
        voiceLineSystem.PlayBark(VoiceLineSystem.BarkType.Shitting);
        // TODO: 

        GoToDestination(Place.Toilet);
    }

    private void OnLowHunger()
    {
        // TODO: 
        if (curTeammateState == TeammateState.AtWorkplace)
        {
            voiceLineSystem.PlayBark(VoiceLineSystem.BarkType.Hungry);

            gameManager.questManager.CreateAndStartQuest(QuestId.BringFood, this);

            Debug.Log($"{gameObject.name} wants food");
        }
    }

    private void OnLowFun()
    {
        Debug.Log($"{gameObject.name} acting on low fun");

        if (curTeammateState != TeammateState.AtWorkplace)
            return;
        
        voiceLineSystem.PlayBark(VoiceLineSystem.BarkType.Bored);

        // Reset the timed patrol cooldown so we don't get a double patrol shortly after
        lastPatrolCheckTime = gameManager.dayTime;

        patrolController.StartPatrol();
    }
    
    #region Yapping

    /// <summary>
    /// Called every frame from Patrolling state. Scans for nearby teammates to yap with.
    /// Only the patrolling teammate initiates — this avoids double-triggering.
    /// </summary>
    private void TryStartYapping()
    {
        // Global cooldown still active
        if (yapGlobalCooldownTimer > 0f) return;

        // Periodic check
        yapCheckCooldown -= Time.deltaTime;
        if (yapCheckCooldown > 0f) return;
        yapCheckCooldown = yapConfig.checkInterval;

        foreach (var other in GameManager.instance.teammates)
        {
            if (other == this) continue;
            if (other == null) continue;

            // Don't yap with someone who's already yapping or on global cooldown
            if (other.curTeammateState == TeammateState.Yapping) continue;
            if (other.yapGlobalCooldownTimer > 0f) continue;

            // Only yap with patrollers or workers
            if (other.curTeammateState != TeammateState.Patrolling &&
                other.curTeammateState != TeammateState.AtWorkplace)
                continue;

            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist > yapConfig.detectionRadius) continue;

            // Yap probability
            float prob = other.curTeammateState == TeammateState.Patrolling
                ? (yapConfig.probabilityPatrollerToPatroller + other.yapConfig.probabilityPatrollerToPatroller)/2
                : (yapConfig.probabilityPatrollerToWorker + other.yapConfig.probabilityPatrollerToWorker)/2;
            

            if (Random.value > prob) continue;

            // Success — start yapping!
            BeginYapping(other);
            return; // Only one conversation at a time
        }
    }

    /// <summary>
    /// Initiates a yap conversation between this teammate and the given partner.
    /// </summary>
    private void BeginYapping(TeammateController other)
    {
        float duration = (yapConfig.duration + other.yapConfig.duration)/2 + Random.Range(-yapConfig.durationVariance, yapConfig.durationVariance);
        duration = Mathf.Max(duration, 0.5f); // At least half a second

        bool otherWasWorking = other.curTeammateState == TeammateState.AtWorkplace;

        Debug.Log($"{gameObject.name} starts yapping with {other.gameObject.name} " +
                  $"(duration: {duration:F1}s, other was working: {otherWasWorking})");
        
        EnterYappingState(other, duration);
        other.EnterYappingState(this, duration);

        if (otherWasWorking)
            other.isDisturbed = true;
    }

    /// <summary>
    /// Puts this teammate into the Yapping state with the given partner and timer.
    /// </summary>
    private void EnterYappingState(TeammateController partner, float duration)
    {
        // Remember previous state so PatrolController can be paused
        yapPartner = partner;
        yapTimer = duration;

        // Stop NavMesh movement
        agent.isStopped = true;

        curTeammateState = TeammateState.Yapping;
    }

    /// <summary>
    /// Ticked every frame. Counts down yap timer, applies fun increase,
    /// and ends the conversation when time is up.
    /// </summary>
    private void UpdateYapping()
    {
        // Tick global cooldown regardless of state
        if (yapGlobalCooldownTimer > 0f)
            yapGlobalCooldownTimer -= Time.deltaTime;

        if (curTeammateState != TeammateState.Yapping) return;

        // Increase fun while yapping
        fun += yapConfig.funIncrease * Time.deltaTime;
        fun = Mathf.Clamp(fun, 0f, 100f);

        yapTimer -= Time.deltaTime;
        if (yapTimer <= 0f)
            EndYapping();
    }

    /// <summary>
    /// Ends this teammate's yapping session and returns them to their previous activity.
    /// </summary>
    private void EndYapping()
    {
        Debug.Log($"{gameObject.name} stops yapping");

        // Start global cooldown
        yapGlobalCooldownTimer = yapConfig.globalCooldown;

        // Resume movement
        agent.isStopped = false;

        // If this teammate was disturbed (i.e. was working), start the linger timer
        if (isDisturbed)
        {
            yapDisturbedTimer = yapConfig.disturbedDuration;
            // isDisturbed stays true until the linger timer expires
        }

        yapPartner = null;

        // Return to appropriate state:
        // If patrol was in progress, resume patrol; otherwise go back to workplace
        if (patrolController.IsPatrolInProgress())
            curTeammateState = TeammateState.Patrolling;
        else
            GoToDestination(Place.Workplace);
    }

    /// <summary>
    /// Ticks the post-yap disturbance timer. While active, work efficiency is reduced.
    /// </summary>
    private void UpdateDisturbance()
    {
        if (!isDisturbed) return;

        yapDisturbedTimer -= Time.deltaTime;
        if (yapDisturbedTimer <= 0f)
        {
            isDisturbed = false;
            Debug.Log($"{gameObject.name} is no longer disturbed");
        }
    }

    #endregion

    private void CheckTableFor()
    {

    }

    /// <summary>
    /// Dynamically calculate currentWalkSpeed, which is based on teammate stats
    /// </summary>
    /// <returns>Calculated walkSpeed</returns>
    public float GetWalkSpeed()
    {
        // At 50% stat = 1.0 (no change), scales up/down from there
        float energyMultiplier = energy / 100f * walkSpeedEnergyScale + (1f - walkSpeedEnergyScale * 0.5f);
        float bladderMultiplier = (1f - bladder / 100f) * walkSpeedBladderScale + (1f - walkSpeedBladderScale * 0.5f);
        float hungerMultiplier = hunger / 100f * walkSpeedHungerScale + (1f - walkSpeedHungerScale * 0.5f);

        // Not 0
        energyMultiplier = Mathf.Max(energyMultiplier, 0.1f);
        bladderMultiplier = Mathf.Max(bladderMultiplier, 0.1f);
        hungerMultiplier = Mathf.Max(hungerMultiplier, 0.1f);

        float walkSpeed = baseWalkSpeed * energyMultiplier * hungerMultiplier * bladderMultiplier * 2f;

        Debug.Log($"{gameObject.name} walkSpeed set to: {walkSpeed}");

        return walkSpeed;
    }

    public void GoToDestination(Place place)
    {
        curTeammateState = TeammateState.GoingToDestination;

        agent.speed = GetWalkSpeed();

        curDestination = place;

        switch (place)
        {
            case Place.Workplace:
                agent.SetDestination(workplace.transform.position);
                break;

            case Place.Toilet:
                agent.SetDestination(toilet.transform.position);
                break;

            case Place.Exit:
                agent.SetDestination(exit.transform.position);

                break;
        }

        Debug.Log($"{gameObject.name} going to destination: {curDestination}");
    }

    public void DayReset()
    {
        lastProgressMadeTime = lastPatrolCheckTime = gameManager.dayStartTime;
        curTeammateState = initialTeammateState;

        energy = 100f;
        bladder = 100f;
        hunger = 100f;
        fun = 100f;

        energyActCooldown = 0f;
        bladderActCooldown = 0f;
        hungerActCooldown = 0f;
        funActCooldown = 0f;
        
        yapCheckCooldown = 0f;
        yapTimer = 0f;
        yapGlobalCooldownTimer = 0f;
        yapDisturbedTimer = 0f;
        isDisturbed = false;
        yapPartner = null;
    }

    /*
    private void UpdateUI()
    {
        teammateStateText.text = $"Teammate state: {curTeammateState}";

        energyText.text = $"Energy: {energy}";
        bladderText.text = $"Bladder: {bladder}";
        hungerText.text = $"Hunger: {hunger}";

        curDestText.text = $"Cur Dest: {curDestination}";
    }
    */
    
    // Draw places
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;

        Gizmos.DrawCube(workplace.transform.position, new Vector3(0.4f, 0.4f, 0.4f));

        //FoV
        Color c = new(0, 0, 0.6f, 0.4f);
        UnityEditor.Handles.color = c;
        Vector3 rotatedForward = Quaternion.Euler(0, -angle * 0.5f, 0) * transform.forward;
        UnityEditor.Handles.DrawSolidArc(rayCastOrigin.position, Vector3.up, rotatedForward, angle, radius);
    }

    private EdibleData.EdibleType ChooseEdible(List<EdiblePreference> preferences)
    {
        //TODO
        
        float totalWeight = preferences.Sum(preference => preference.weight);

        float random = Random.Range(0, totalWeight);

        foreach (var p in preferences)
        {
            random -= p.weight;

            if (random <= 0)
                return p.type;
        }

        return EdibleData.EdibleType.Coffee;
    }
}

#endregion

[Serializable]
public class EdiblePreference
{
    public EdibleData.EdibleType type;
    public float weight;
}
