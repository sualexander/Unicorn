using UnityEngine;

public static class Startup 
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void StartGame()
    {
        Object.DontDestroyOnLoad(Object.Instantiate(Resources.Load("GameManager")));
    }
}
