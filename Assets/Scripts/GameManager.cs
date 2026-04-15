using UnityEngine;
using Luna.Unity;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public WordSearchGrid grid;
    
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        LifeCycle.GameStarted();
    }

    public void OnGameComplete()
    {
        LifeCycle.GameEnded();
        Debug.Log("Game Complete!");
    }
}
