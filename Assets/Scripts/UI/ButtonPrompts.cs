using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ButtonPrompts : MonoBehaviour
{
    public static ButtonPrompts instance;
    [SerializeField] StarterAssetsInputs inputs;
    [SerializeField] NarrativeHandler narrativeHandler;

    [Header("Movement Prompt")]
    [SerializeField] GameObject movePrompt;
    private Animator movePromptAnimator;
    private string movePromptState;
    public bool hasMoved;

    [Header("Tongue Combat Prompt")]
    [SerializeField] GameObject tonguePrompt;
    private Animator tonguePromptAnimator;
    private string tonguePromptState;
    public bool hasUsedTongue;

    [Header("Target Prompt")]
    [SerializeField] GameObject targetPrompt;
    private Animator targetPromptAnimator;
    private string targetPromptState;
    public bool hasTargeted;


    [Header("Croak Combat Prompt")]
    [SerializeField] GameObject croakPrompt;
    private Animator croakPromptAnimator;
    private string croakPromptState;
    public bool hasUsedCroak;

    // Animation States
    const string FADE_IN = "Fade_In";
    const string FADE_OUT = "Fade_Out";

    private Coroutine movePromptRoutine;

    // Basic Singleton
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(instance);
        }
    }

    private void Start()
    {
        // Get Animators
        movePromptAnimator = movePrompt.GetComponent<Animator>();
        tonguePromptAnimator = tonguePrompt.GetComponent<Animator>();
        targetPromptAnimator = targetPrompt.GetComponent<Animator>();
        croakPromptAnimator = croakPrompt.GetComponent<Animator>();

        // Movement Prompt
        movePromptRoutine = StartCoroutine(ShowMovementPrompt(15.0f));
    }

    private void Update()
    {
        if (inputs.move != Vector2.zero)
        {
            hasMoved = true;
            if (movePromptState == FADE_IN)
            {
                // Play the animation
                movePromptAnimator.Play(FADE_OUT);
            }
        }

        if (inputs.holdingTongue)
        {
            hasUsedTongue = true;
            if (tonguePromptState == FADE_IN)
            {
                // Play the animation
                tonguePromptAnimator.Play(FADE_OUT);
            }
        }

        if (inputs.lockOnEnemy)
        {
            hasTargeted = true;
            if (targetPromptState == FADE_IN)
            {
                // Play the animation
                targetPromptAnimator.Play(FADE_OUT);
            }
        }

        if (inputs.pAttack || inputs.hAttack)
        {
            hasUsedCroak = true;
            if (croakPromptState == FADE_IN)
            {
                // Play the animation
                croakPromptAnimator.Play(FADE_OUT);
            }
        }
    }

    public void InvokeCombatPrompts()
    {
        StartCoroutine(ShowTonguePrompt(2.0f));
        StartCoroutine(ShowTargetPrompt(5.0f));
    }

    public void InvokeCroakPrompt()
    {
        StartCoroutine(ShowCroakPrompt(2.0f));
    }

    IEnumerator ShowMovementPrompt(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!hasMoved)
        {
            movePrompt.SetActive(true);
            movePromptState = FADE_IN;
            movePromptAnimator.Play(FADE_IN);
        }
    }

    IEnumerator ShowTonguePrompt(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!hasUsedTongue)
        {
            tonguePrompt.SetActive(true);
            tonguePromptState = FADE_IN;
            tonguePromptAnimator.Play(FADE_IN);
        }
    }

    IEnumerator ShowTargetPrompt(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!hasTargeted)
        {
            targetPrompt.SetActive(true);
            targetPromptState = FADE_IN;
            targetPromptAnimator.Play(FADE_IN);
        }
    }

    IEnumerator ShowCroakPrompt(float delay)
    {
        yield return new WaitForSeconds(delay);
        hasUsedCroak = false; // Because people will hit the main button in the narrative sadly
        yield return new WaitForSeconds(delay);
        if (!hasUsedCroak)
        {
            croakPrompt.SetActive(true);
            croakPromptState = FADE_IN;
            croakPromptAnimator.Play(FADE_IN);
        }
    }
}
