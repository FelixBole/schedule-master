using UnityEngine;
using Slax.Schedule;

public class ScheduleMasterDemoStarter : MonoBehaviour
{
    public TimeManager TimeManager;

    void Start()
    {
        Debug.Log("Initializing & Starting Time Manager, you will need a script like this one to start the Time in your game.");
        TimeManager.Initialize();
        TimeManager.Play();
    }
}
