using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EmojisController : MonoBehaviour
{
    [Header("UI References")]
    public UIDocument uiDocument;
    public Sprite bubbleBackgroundSprite;

    [Header("Emojis")]
    public bool isYellow;
    public Sprite currentEmoji;
    [SerializeField] private Sprite emoji_interesting;
    [SerializeField] private Sprite emoji_thinking_b;
    [SerializeField] private Sprite emoji_thinking_y;
    [SerializeField] private Sprite emoji_smiling_b;
    [SerializeField] private Sprite emoji_smiling_y;
    [SerializeField] private Sprite emoji_exciting_b;
    [SerializeField] private Sprite emoji_exciting_y;
    [SerializeField] private Sprite emoji_super_exciting_b;
    [SerializeField] private Sprite emoji_super_exciting_y;
    [SerializeField] private Sprite emoji_loving_b;
    [SerializeField] private Sprite emoji_loving_y;
    [SerializeField] private Sprite emoji_awkwardly_b;
    [SerializeField] private Sprite emoji_awkwardly_y;
    [SerializeField] private Sprite emoji_lucky_b;
    [SerializeField] private Sprite emoji_lucky_y;
    [SerializeField] private Sprite emoji_super_sad_b;
    [SerializeField] private Sprite emoji_super_sad_y;
    [SerializeField] private Sprite emoji_sad_b;
    [SerializeField] private Sprite emoji_sad_y;

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

        if (currentEmoji != null) SetEmoji(currentEmoji);
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