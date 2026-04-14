using System.Collections.Generic;
using UnityEngine;

public class DoorLatchController : MonoBehaviour
{
    [Header("Plates & Requirement")]
    [Tooltip("Add any number of PressurePlate2D components here.")]
    public List<PressurePlate2D> plates = new List<PressurePlate2D>();

    [Tooltip("How many plates must be pressed to open. <=0 means ALL plates.")]
    public int requiredPressedCount = 0;

    [Header("Target")]
    [Tooltip("The laser/door objects to disable when opened.")]
    public List<GameObject> doorsToDisable = new List<GameObject>();

    [Header("Behavior")]
    [Tooltip("If true, once opened it stays open even if plates are released.")]
    public bool permanentOnceOpened = true;

    [SerializeField] public bool isOpen;
    
    [SerializeField] private AudioSource audioSource;

    private void OnEnable()
    {
        Subscribe(true);
        Recompute();
    }

    private void OnDisable()
    {
        //Subscribe(false);
    }

    private void Subscribe(bool add)
    {
        if (plates == null) return;

        foreach (var p in plates)
        {
            if (!p) continue;
            if (add) p.OnPressChanged += OnPlateChanged;
            else p.OnPressChanged -= OnPlateChanged;
        }
    }

    private void OnPlateChanged(bool _)
    {
        Recompute();
    }

    private int PressedCount()
    {
        if (plates == null) return 0;

        int count = 0;
        foreach (var p in plates)
            if (p && p.IsPressed) count++;
        return count;
    }

    private bool RequirementMet()
    {
        int total = plates?.Count ?? 0;
        if (total == 0) return false;

        int need = (requiredPressedCount <= 0) ? total : Mathf.Min(requiredPressedCount, total);
        return PressedCount() >= need;
    }

    // Returns true if ALL lasers have been permakilled
    private bool AllPermakilled()
    {
        if (doorsToDisable == null || doorsToDisable.Count == 0) return false;

        foreach (var door in doorsToDisable)
        {
            if (door == null) continue;
            var lazer = door.GetComponent<LazerCollision>();
            if (lazer == null || !lazer.isPermakilled) return false;
        }
        return true;
    }

    private void Recompute()
    {
        if (doorsToDisable == null || doorsToDisable.Count == 0) return;

        // If all lasers are already permanently killed, nothing to do
        if (AllPermakilled()) return;

        if (permanentOnceOpened)
        {
            if (isOpen) return;
            if (RequirementMet())
            {
                if (audioSource != null)
                    audioSource.Play();

                isOpen = true;
                foreach (var door in doorsToDisable)
                    if (door) door.SetActive(false);
            }
        }
        else
        {
            bool openNow = RequirementMet();

            if (!isOpen && openNow)
            {
                if (audioSource != null)
                    audioSource.Play();
            }

            if (openNow != isOpen)
            {
                isOpen = openNow;
                foreach (var door in doorsToDisable)
                {
                    if (door == null) continue;
                    var lazer = door.GetComponent<LazerCollision>();
                    if (openNow || lazer == null || !lazer.isPermakilled)
                        door.SetActive(!openNow);
                }
            }
        }
    }
}