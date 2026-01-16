using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EmojiBubbleController : MonoBehaviour
{
    [Header("UI References")]
    public UIDocument uiDocument;
    public Sprite bubbleBackgroundSprite;

    [Header("Testing")]
    public Sprite testEmoji;

    [Header("Animation Settings")]
    public float fadeDuration = 0.15f;

    private VisualElement emojiFaceElement;
    private VisualElement bubbleElement;
    private Coroutine currentAnimation;

    private void OnEnable()
    {
        var root = uiDocument.rootVisualElement;

        bubbleElement = root.Q<VisualElement>("SpeechBubble");
        emojiFaceElement = root.Q<VisualElement>("EmojiFace");

        if (emojiFaceElement != null)
        {
            emojiFaceElement.style.transitionProperty = new List<StylePropertyName> { new StylePropertyName("opacity") };
            emojiFaceElement.style.transitionDuration = new List<TimeValue> { new TimeValue(fadeDuration, TimeUnit.Second) };
            emojiFaceElement.style.transitionTimingFunction = new List<EasingFunction> { EasingMode.EaseInOut };
        }

        if (bubbleElement != null && bubbleBackgroundSprite != null)
        {
            bubbleElement.style.backgroundImage = new StyleBackground(bubbleBackgroundSprite);
        }

        if (testEmoji != null) SetEmoji(testEmoji);
    }

    public void SetEmoji(Sprite newEmoji)
    {
        if (currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(SwapEmojiRoutine(newEmoji));
    }

    private IEnumerator SwapEmojiRoutine(Sprite newEmoji)
    {
        if (emojiFaceElement != null)
        {
            emojiFaceElement.style.opacity = 0;
            
            yield return new WaitForSeconds(fadeDuration);

            emojiFaceElement.style.backgroundImage = new StyleBackground(newEmoji);
            
            emojiFaceElement.style.opacity = 1;
        }
    }
}