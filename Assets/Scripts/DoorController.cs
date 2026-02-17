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
    [Tooltip("The laser/door root to disable when opened.")]
    public GameObject doorToDisable;

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
        Subscribe(false);
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

    private void Recompute()
    {
        if (!doorToDisable) return;

        if (permanentOnceOpened)
        {
            if (isOpen) return;
            if (RequirementMet())
            {
                if (audioSource != null)
                {
                    audioSource.Play();
                }
                isOpen = true;
                doorToDisable.SetActive(false); // permanently open
            }
        }
        else
        {
            bool openNow = RequirementMet();

            // Only react if the state actually changes
            if (!isOpen && openNow)
            {
                if (audioSource != null)
                {
                    audioSource.Play();
                }
            }

            if (openNow != isOpen)
            {
                isOpen = openNow;
                doorToDisable.SetActive(!openNow);
            }
        }
    }
}
