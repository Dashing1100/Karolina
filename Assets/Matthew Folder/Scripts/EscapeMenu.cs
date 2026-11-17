using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
public class EscapeMenu : MonoBehaviour
{
    public GameObject escapemenu;
    public GameObject escapemenubackground;
    public GameObject settingsmenu;
    public GameObject resume;
    public bool IsPaused;

    public void MainMenu()
    {
        Time.timeScale = 1;
        escapemenu.SetActive(false);
        escapemenubackground.SetActive(false);
        IsPaused = false;
        settingsmenu.SetActive(false);
        SceneManager.LoadScene("Title Screen");
    }

    public void Quits()
    {
        Resume();
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void PauseUnPause(InputAction.CallbackContext ctx)
    {
        if (ctx.started == false)
            return;

        if (IsPaused == false)
        {
            Pause();
        }
        else
        {
            Resume();
        }
    }

    public void Pause()
    {
        escapemenu.SetActive(true);
        escapemenubackground.SetActive(true);
        Time.timeScale = 0;
        IsPaused = true;
    }
    public void Resume()
    {
        escapemenu.SetActive(false);
        escapemenubackground.SetActive(false);
        settingsmenu.SetActive(false);
        Time.timeScale = 1f;
        IsPaused = false;
    }

}
