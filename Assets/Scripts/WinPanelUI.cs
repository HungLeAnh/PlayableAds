using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class WinPanelUI : MonoBehaviour, IPointerClickHandler
{
    private Action onPanelClicked;

    public void Setup(Action onClick)
    {
        onPanelClicked = onClick;
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onPanelClicked?.Invoke();
    }
}
