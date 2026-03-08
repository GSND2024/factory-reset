using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotAnimation : MonoBehaviour
{
    [Header("Animation Frames")]
    public Sprite spriteA;
    public Sprite spriteB;

    [Header("Timing")]
    public float delay = 0.3f;

    [Header("Target Settings")]
    [SerializeField] private string targetTag = "Robot";
    [SerializeField] private string excludedName = "AI";

    private List<SpriteRenderer> targets = new List<SpriteRenderer>();
    private bool useA = true;

    void Start()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(targetTag);

        foreach (GameObject obj in objects)
        {
            if (obj.name == excludedName) continue;

            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                targets.Add(sr);
            }
        }

        StartCoroutine(AlternateSprites());
    }

    IEnumerator AlternateSprites()
    {
        while (true)
        {
            foreach (SpriteRenderer sr in targets)
            {
                sr.sprite = useA ? spriteA : spriteB;
            }

            useA = !useA;

            yield return new WaitForSeconds(delay);
        }
    }
}