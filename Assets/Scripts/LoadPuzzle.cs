using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadPuzzle : MonoBehaviour
{
    static float rhealth;
    static float rtimer;
    public string loadLevel;
    private static string sceneName;
    // Start is called before the first frame update
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == "Player")
        {
            setScene();
            SceneManager.LoadSceneAsync(loadLevel);
        }
    }
    private void setScene()
    {
        Scene newlyLoadedScene = SceneManager.GetActiveScene();
        sceneName = newlyLoadedScene.name;
    }
    public static string getScene()
    {
        return sceneName;
    }
    public static void setHealth(float health)
    {
        rhealth = health;
    }
    public static void setTimer(float timer)
    {
        rtimer = timer;
    }
    public static float getHealth()
    {
        return rhealth;
    }
    public static float getTimer()
    {
        return rtimer;
    }
}
