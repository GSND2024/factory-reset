using UnityEngine;
using UnityEngine.SceneManagement; 

public class Level4Trigger : MonoBehaviour
{
    [SerializeField] private GameObject laser1;
    [SerializeField] private GameObject laser2;
    [SerializeField] private GameObject robot;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GlobalGameState.stateSaver[2] = GlobalGameState.lazerHitRobot2;
            GlobalGameState.stateSaver[3] = (GlobalGameState.isRobotHacked2);

            if (!GlobalGameState.lazerHitRobot2 && (!laser1.activeSelf || !laser2.activeSelf || (!IsNearlyEqual(robot.transform.position.x, -4.547f) || (!IsNearlyEqual(robot.transform.position.y, -2.131f)))))
                GlobalGameState.isRobotSaved = true;
            GlobalGameState.stateSaver[4] = (GlobalGameState.isRobotSaved);
            GlobalGameState.isLevel3 = false;
            SceneManager.LoadScene("PrototypeLevel4");
        }
    }

    private bool IsNearlyEqual(float a, float b, float tolerance = 0.1f)
    {
        return Mathf.Abs(a - b) <= tolerance;
    }
}
