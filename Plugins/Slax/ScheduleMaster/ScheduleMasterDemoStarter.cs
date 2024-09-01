using UnityEngine;
using Slax.Schedule;

public class ScheduleMasterDemoStarter : MonoBehaviour
{
    public TimeManager TimeManager;

    [Header("Tracked Time Configuration")]
    public TimeConfigurationSO GameTimeConfiguration;
    public bool UseGameTimeConfiguration = false;

    void Start()
    {
        Debug.Log("Initializing & Starting Time Manager, you will need a script like this one to start the Time in your game.");

        if (UseGameTimeConfiguration)
        {
            if (GameTimeConfiguration == null)
            {
                Debug.LogError("You must provide a TimeConfigurationSO to use game time configuration.");
                return;
            }

            TimeManager.InitializeFromConfiguration(GameTimeConfiguration);
        }
        else
        {
            TimeManager.Initialize();
        }

        TimeManager.Play();
    }
}
