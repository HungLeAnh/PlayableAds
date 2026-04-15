using UnityEngine;
using Luna.Unity;

public class CTAButton : MonoBehaviour
{
    public void OpenStore()
    {
        LifeCycle.GameEnded();
        Playable.InstallFullGame();
    }
}
