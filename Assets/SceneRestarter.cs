using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public void RestartScene()
    {
        // 1. Ensure time is running (just in case)
        Time.timeScale = 1f;

        // 2. Reload the entire scene
        // This instantly kills the alarm, resets the fire, and puts you back at the start.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}