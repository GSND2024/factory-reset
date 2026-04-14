using System;
using UnityEngine;

public class LazerCollision : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    // Set to true when permanently killed by a box or robot
    public bool isPermakilled = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Robot"))
        {
            audioSource.Play();
            isPermakilled = true;

            if (GlobalGameState.isLevel1 || GlobalGameState.isLevel2)
            {
                GlobalGameState.lazerHitRobot = true;
                GlobalGameState.destroyCount += 1;
                Debug.Log($"TalkCount: {GlobalGameState.talkCount}, HackCount: {GlobalGameState.hackCount}, , destroyCount: {GlobalGameState.destroyCount}");
                Debug.Log("LazerHitRobot");
            }

            if (GlobalGameState.isLevel3)
            {
                GlobalGameState.lazerHitRobot2 = true;
                GlobalGameState.destroyCount += 1;
                Debug.Log($"TalkCount: {GlobalGameState.talkCount}, HackCount: {GlobalGameState.hackCount}, , destroyCount: {GlobalGameState.destroyCount}");
            }

            if (GlobalGameState.isLevel0)
            {
                GlobalGameState.destroyCount += 1;
                Debug.Log($"TalkCount: {GlobalGameState.talkCount}, HackCount: {GlobalGameState.hackCount}, , destroyCount: {GlobalGameState.destroyCount}");
            }

            if (HackManager.Instance)
                HackManager.Instance.NotifyActorKilled(other.gameObject);

            other.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
        else if (other.CompareTag("Box"))
        {
            audioSource.Play();
            isPermakilled = true;
            other.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}