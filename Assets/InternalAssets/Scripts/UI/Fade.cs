using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fade : MonoBehaviour
{
    public Animator animator;

    private void Awake()
    {
        animator.gameObject.SetActive(true);
    }

    private void Start()
    {
        GameManager.Instance.onFadeModeStart.AddListener(StartFade);

    }

    void StartFade(FadeMode fadeMode)
    {
        if(fadeMode == FadeMode.FadeOut)
        {
            animator.SetTrigger("FadeOut");
        }
    }
}
