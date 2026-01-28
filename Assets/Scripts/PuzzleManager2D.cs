using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PuzzleManager2D : MonoBehaviour
{
    public static PuzzleManager2D instance;

    [Header("Door")]
    public GameObject door; // Reference to the door GameObject (can disable to "open")

    [Header("Order Settings")]
    private List<int> correctOrder = new List<int>(); // The correct sequence (will be randomized)
    private List<int> currentOrder = new List<int>(); // Player's current input sequence

    [Header("Plates")]
    public PressurePlateLevel4[] plates; // All pressure plates in the scene

    private bool puzzleSolved = false;
    
    [SerializeField] private AudioSource audioSource;

    // Public property to access the correct order from other scripts
    public List<int> CorrectOrder => correctOrder;

    private void Awake()
    {
        instance = this;

        // Generate random correct order
        GenerateRandomOrder();

        // Optional: Automatically find all plates in the scene (you can also assign manually)
        if (plates == null || plates.Length == 0)
        {
            plates = FindObjectsOfType<PressurePlateLevel4>();
        }
    }

    /// <summary>
    /// Generates a random order for the puzzle (e.g., 0,2,1,3 or 1,2,3,0)
    /// </summary>
    private void GenerateRandomOrder()
    {
        // Create a list with IDs 0, 1, 2, 3
        List<int> availableIDs = new List<int> { 0, 1, 2, 3 };
        
        // Shuffle the list using Fisher-Yates algorithm
        for (int i = availableIDs.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = availableIDs[i];
            availableIDs[i] = availableIDs[randomIndex];
            availableIDs[randomIndex] = temp;
        }

        correctOrder = availableIDs;
        
        Debug.Log($"🔢 Random puzzle order generated: {string.Join(", ", correctOrder)}");
    }

    /// <summary>
    /// Get the correct order as a string (useful for displaying hints)
    /// </summary>
    public string GetCorrectOrderString()
    {
        return string.Join(" → ", correctOrder);
    }

    /// <summary>
    /// Get the position of a specific plate ID in the correct order (0-indexed)
    /// </summary>
    public int GetPlatePosition(int plateID)
    {
        return correctOrder.IndexOf(plateID);
    }

    public void PlatePressed(int id)
    {
        if (puzzleSolved)
            return; // Ignore input if puzzle is already solved

        currentOrder.Add(id);

        // If player pressed more plates than needed → wrong, reset
        if (currentOrder.Count > correctOrder.Count)
        {
            ResetPuzzle();
            return;
        }

        // Check current order against the correct sequence so far
        for (int i = 0; i < currentOrder.Count; i++)
        {
            if (currentOrder[i] != correctOrder[i])
            {
                ResetPuzzle();
                return;
            }
        }

        // If all plates are pressed in correct order → puzzle solved
        if (currentOrder.Count == correctOrder.Count)
        {
            PuzzleSuccess();
        }
    }

    private void PuzzleSuccess()
    {
        Debug.Log("✅ Puzzle Solved!");
        puzzleSolved = true;

        if (audioSource != null)
            audioSource.Play();
        
        // Open the door (disable or trigger animation)
        if (door != null)
            door.SetActive(false);

        // Lock all plates in their pressed color
        foreach (PressurePlateLevel4 plate in plates)
        {
            if (plate == null) continue;

            if (correctOrder.Contains(plate.plateID))
                plate.LockPressedColor();
        }
    }

    private void ResetPuzzle()
    {
        Debug.Log("❌ Wrong order! Resetting puzzle...");
        currentOrder.Clear();

        // Reset all plates to idle color
        foreach (PressurePlateLevel4 plate in plates)
        {
            if (plate == null) continue;
            plate.UnlockAndReset();
        }
    }

    /// <summary>
    /// Optional: Manually regenerate a new random order (useful for testing or replay)
    /// </summary>
    public void RegenerateOrder()
    {
        if (!puzzleSolved)
        {
            GenerateRandomOrder();
            ResetPuzzle();
        }
    }
}