using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(PatrolController))]
[RequireComponent(typeof(NavMeshAgent))]
public class TeammateController : MonoBehaviour
{
    public enum TeammateState
    {
        AtWorkplace,
        Sleeping,
        Shitting,
        Patrolling,
        GoingToDestination

        // TODO: add AtToilet
    }

    /// <summary>
    /// Properties:
    /// <para><b>decrease</b>: Constant decrease</para>
    /// <para><b>increase</b>: Constant increase</para>
    /// <para><b>actThreshold</b>: Acting on low stat only possible below threshold</para>
    /// <para><b>tryActInterval</b>: Check/Roll for acting every x seconds</para>
    /// <para><b>probCurveExponent</b>: Probability for acting grows exponentially towards 0 stat</para>
    /// </summary>
    [Serializable]
    public struct StatConfig
    {
        public float decrease;
        public float increase;
        [Range(0f, 100f)] public float actThreshold;
        public float tryActInterval;
        [Range(1f, 5f)] public float probCurveExponent;  // 1 = linear
    }

    [Header("State")]
    public TeammateState initialTeammateState = TeammateState.AtWorkplace;
    public TeammateState curTeammateState;

    [Header("Stats")]
    public StatConfig energyStatConfig = new();
    [Range(1f, 10f)] public float energyDecreaseScaleWhenWorking;
    private float energy = 100f;
    private float energyActCooldown = 0f;

    public StatConfig bladderStatConfig = new();
    private float bladder = 100f;
    private float bladderActCooldown = 0f;

    public StatConfig hungerStatConfig = new();
    private float hunger = 100f;
    private float hungerActCooldown = 0f;


    [Header("Walking")]
    public float baseWalkSpeed;
    public float walkSpeedEnergyScale;
    public float walkSpeedBladderScale;
    public float walkSpeedHungerScale;

    [Header("Places")] public Place curDestination = Place.None;
    [Space(10)] public GameObject workplace;
    public GameObject toilet;
    public GameObject exit;

    [Header("Field of View params")] public float angle = 90f;
    public float radius = 10f;

    [Header("Detection params")] public int detectionThreshold = 100;
    public static int currentPoints;
    [SerializeField] private int pointsPerCheck;
    [SerializeField] private LayerMask traceAgainst;
    private bool susDetected;
    public Transform rayCastOrigin;

    //Food/Drink
    private EdibleData.EdibleType _foodAwaited;
    private EdibleData.EdibleType _energyAwaited;

    public List<EdiblePreference> foodPreferences;
    public List<EdiblePreference> energyPreferences;

    public enum Place
    {
        None,
        Workplace,
        Toilet,
        Exit
    }

    [Header("UI")]
    public TMP_Text teammateStateText;

    private float time = 0f;

    private NavMeshAgent agent;
    private PatrolController patrolController;


    public GameObject player;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        patrolController = GetComponent<PatrolController>();

        curTeammateState = initialTeammateState;

        GoToDestination(Place.Workplace);
    }

    private void Update()
    {
        UpdateStats();
        UpdateUI();

        switch (curTeammateState)
        {
            case TeammateState.AtWorkplace:
                if (energy <= 100)
                {
                    curTeammateState = TeammateState.Sleeping;

                    break;
                }

                DetectHand();
                // for testing: start patrol after 5 secs of working
                time += Time.deltaTime; // TODO: randomise going to patrol

                if (time >= 5f)
                {
                    time = 0f;
                    patrolController.StartPatrol();
                }

                break;

            case TeammateState.GoingToDestination:
                // if agent hasn't arrived at dest
                DetectHand();
                if (agent.remainingDistance > 0.1f) break;

                // else
                Debug.Log($"Arrived at {curDestination}");

                switch (curDestination)
                {
                    case Place.Workplace:
                        curTeammateState = TeammateState.AtWorkplace;
                        curDestination = Place.None;

                        break;
                    case Place.Toilet: 
                        // TODO: do something after arrived at toilet

                        break;
                    case Place.Exit: 
                        // TODO: do something after arrived at exit

                        break;
                }

                break;

            case TeammateState.Sleeping:
                if (energy >= 100) // wake up
                {
                    curTeammateState = TeammateState.AtWorkplace;
                    break;
                }

                energy -= energyStatConfig.decrease * Time.deltaTime;

                break;

            default:
                DetectHand();
                break;

            // TODO: other states? 
        }
    }

    private void DetectHand()
    {
        //TODO: optionally, change amount of current/needed detect-points depending on sleepiness
        susDetected = false;
        Vector3 handPosition = Player.Instance.PlayerHand.position;
        Vector3 distanceToHand = handPosition - transform.position;
        if (distanceToHand.magnitude <= radius)
        {
            if (Vector3.Dot(distanceToHand.normalized, transform.forward) > Mathf.Cos(angle * 0.5f * Mathf.Deg2Rad))
            {
                RaycastHit hit;

                Vector3 direction = handPosition - rayCastOrigin.position;

                if (Physics.Raycast(rayCastOrigin.position, direction, out hit, traceAgainst))
                {
                    Debug.Log(hit.collider.gameObject.name);
                    if (hit.collider.TryGetComponent<MovableInteractable>(out MovableInteractable detectedItem) &&
                        detectedItem == Player.Instance.ItemInHand && detectedItem.isSuspicious)
                    {
                        Debug.DrawRay(rayCastOrigin.position, direction, Color.green);
                        susDetected = true;
                        currentPoints += (int)Math.Ceiling(pointsPerCheck * Time.deltaTime * 4);
                        if (currentPoints >= detectionThreshold)
                        {
                            //TODO: Loose condition
                            Debug.Log("Player would loose here because sus item was detected");
                            currentPoints = 0;
                        }
                    }
                    else
                    {
                        susDetected = false;
                        Debug.DrawRay(rayCastOrigin.position, direction, Color.red);
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
        if (curTeammateState == TeammateState.AtWorkplace)
            energy -= energyStatConfig.decrease * energyDecreaseScaleWhenWorking * Time.deltaTime;
        else
            energy -= energyStatConfig.decrease * Time.deltaTime;

        bladder -= bladderStatConfig.decrease * Time.deltaTime;
        hunger -= hungerStatConfig.decrease * Time.deltaTime;

        // try acting on low stat if cooldown reached
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
        Debug.Log("Acting on low energy");
        // TODO: 
    }

    private void OnLowBladder()
    {
        Debug.Log("Acting on low bladder");
        // TODO: 
    }

    private void OnLowHunger()
    {
        Debug.Log("Acting on low hunger");
        // TODO: 

        if (curTeammateState == TeammateState.AtWorkplace)
        {

        }
    }

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

        Debug.Log($"WalkSpeed set to: {walkSpeed}");

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

        Debug.Log($"Going to destination: {curDestination}");
    }

    private void UpdateUI()
    {
        teammateStateText.text = $"Teammate state: {curTeammateState}";
        // TODO: 
    }

    // Draw places
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;

        Gizmos.DrawCube(workplace.transform.position, new Vector3(0.4f, 0.4f, 0.4f));

        //FoV
        Color c = new Color(0, 0, 0.6f, 0.4f);
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

[System.Serializable]
    public class EdiblePreference
    {
        public EdibleData.EdibleType type;
        public float weight;
    }
