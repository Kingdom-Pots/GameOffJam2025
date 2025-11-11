using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRestartHelper : MonoBehaviour
{
    /// <summary>
    /// Restarts the currently active scene.
    /// </summary>
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
