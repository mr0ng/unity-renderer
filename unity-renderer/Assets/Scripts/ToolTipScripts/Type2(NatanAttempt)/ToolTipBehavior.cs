using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ToolTipBehavior : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private GameObject _toolTip;
    private float _waitTime = 0;

    private void Start()
    {
        _toolTip.SetActive(false);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowTip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _toolTip.SetActive(false);
    }

    private void ShowTip()
    {
        _toolTip.SetActive(true);
    }
}
