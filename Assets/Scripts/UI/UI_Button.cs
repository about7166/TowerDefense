using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Button : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    private UI ui;
    private UI_Animator uiAnimator;
    private RectTransform myRectTransform;

    [SerializeField] private float showcaseScale = 1.1f;
    [SerializeField] private float scaleUpDuration = 0.25f;

    private Coroutine scaleCoroutine;
    [Space]
    [SerializeField] private UI_TextBlinkEffect myTextBlinkEffect;

    private void Awake()
    {
        ui = GetComponentInParent<UI>();
        uiAnimator = GetComponentInParent<UI_Animator>();
        myRectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        AudioManager.instance?.PlaySFX(ui.onHoverSFX);

        scaleCoroutine = StartCoroutine(uiAnimator.ChangeScaleCo(myRectTransform, showcaseScale, scaleUpDuration));

        if (myTextBlinkEffect != null)
            myTextBlinkEffect.EnableBlink(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        scaleCoroutine = StartCoroutine(uiAnimator.ChangeScaleCo(myRectTransform, 1, scaleUpDuration));

        if (myTextBlinkEffect != null)
            myTextBlinkEffect.EnableBlink(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        AudioManager.instance?.PlaySFX(ui.onClickSFX);
        myRectTransform.localScale = new Vector3(1, 1, 1);
    }
}
