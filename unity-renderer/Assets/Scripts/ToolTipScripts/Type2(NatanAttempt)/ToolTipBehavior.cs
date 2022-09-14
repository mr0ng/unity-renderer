using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ToolTipBehavior : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private GameObject _toolTip;
    private float _waitTime = 0.5f;

    private void Start()
    {
        _toolTip.SetActive(false);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(StartTimer());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        _toolTip.SetActive(false);
    }

    private void ShowTip()
    {
        _toolTip.SetActive(true);
    }

    private IEnumerator StartTimer()
    {
        yield return new WaitForSeconds(_waitTime);
       ShowTip();
    }
}
