using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class UIButtonScale : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
{
    private RectTransform rect;
    private Vector3 normalScale;
    private Coroutine scaleRoutine;

    public float hoverScale = 1.05f;
    public float pressScale = 0.95f;
    public float speed = 12f;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        normalScale = rect.localScale;
    }

    void ScaleTo(Vector3 target)
    {
        if (scaleRoutine != null)
            StopCoroutine(scaleRoutine);

        scaleRoutine = StartCoroutine(ScaleRoutine(target));
    }

    IEnumerator ScaleRoutine(Vector3 target)
    {
        while (Vector3.Distance(rect.localScale, target) > 0.001f)
        {
            rect.localScale = Vector3.Lerp(
                rect.localScale,
                target,
                Time.unscaledDeltaTime * speed   // 🔥 FIX
            );
            yield return null;
        }
        rect.localScale = target;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ScaleTo(normalScale * hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ScaleTo(normalScale);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ScaleTo(normalScale * pressScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ScaleTo(normalScale * hoverScale);
    }
}
