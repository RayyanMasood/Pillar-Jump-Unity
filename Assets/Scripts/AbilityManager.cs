using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public List<Ability> abilities = new List<Ability>();
    public TextMeshProUGUI abilityTimerText;
    public float landingDotDuration = 10f; // Public variable to control the duration of the LandingDotAbility

    private Ability activeAbility;
    private float abilityTimeLeft;

    private void Start()
    {
        // Initialize the abilities list with the specified duration
        abilities.Add(new LandingDotAbility(landingDotDuration));
        // Future abilities can be added here

        // Hide the timer text initially
        if (abilityTimerText != null)
        {
            abilityTimerText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("abilityTimerText is not assigned in the AbilityManager.");
        }
    }

    private void Update()
    {
        if (activeAbility != null)
        {
            if (activeAbility is TimedAbility timedAbility)
            {
                abilityTimeLeft -= Time.deltaTime;

                if (abilityTimerText != null)
                {
                    abilityTimerText.text = "Landing Dot Time Left: " + Mathf.Ceil(abilityTimeLeft).ToString() + "s";
                }

                if (abilityTimeLeft <= 0)
                {
                    DeactivateAbility();
                }
            }

            activeAbility.UpdateAbility();
        }
    }

    public void ActivateRandomAbility()
    {
        if (abilities.Count == 0)
        {
            Debug.Log("No abilities available to activate.");
            return;
        }

        int randomIndex = Random.Range(0, abilities.Count);
        activeAbility = abilities[randomIndex];
        abilities.RemoveAt(randomIndex);
        activeAbility.Activate(this);

        if (activeAbility is TimedAbility timedAbility)
        {
            abilityTimeLeft = timedAbility.Duration;

            if (abilityTimerText != null)
            {
                abilityTimerText.gameObject.SetActive(true);
            }
        }
    }

    public void DeactivateAbility()
    {
        if (activeAbility != null)
        {
            activeAbility.Deactivate();
            abilities.Add(activeAbility);
            

            if (abilityTimerText != null)
            {
                abilityTimerText.gameObject.SetActive(false);
            }
            activeAbility = null;
        }
    }
}

public abstract class Ability
{
    public abstract void Activate(AbilityManager manager);
    public abstract void Deactivate();
    public virtual void UpdateAbility() { }
}

public abstract class TimedAbility : Ability
{
    public float Duration { get; private set; }

    protected TimedAbility(float duration)
    {
        Duration = duration;
    }
}

public class LandingDotAbility : TimedAbility
{
    public LandingDotAbility(float duration) : base(duration) { }

    public override void Activate(AbilityManager manager)
    {
        if (TrajectoryManager.Instance != null)
        {
            TrajectoryManager.Instance.enableLandingDot = true;
            Debug.Log("Landing Dot Ability Activated.");
        }
        else
        {
            Debug.LogError("TrajectoryManager.Instance is null. Ensure TrajectoryManager is properly initialized.");
        }
    }

    public override void Deactivate()
    {
        if (TrajectoryManager.Instance != null)
        {
            TrajectoryManager.Instance.enableLandingDot = false;
            Debug.Log("Landing Dot Ability Deactivated.");
        }
        else
        {
            Debug.LogError("TrajectoryManager.Instance is null. Ensure TrajectoryManager is properly initialized.");
        }
    }
}
