using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroCutsceneHandler : MonoBehaviour
{
    [SerializeField]
    private Sprite[] sprites;

    private bool animationOver = false;


    private void Start()
    {
        StartCoroutine(
            Wait.LoadSceneThenWait(
                "MainMenu",
                IsAnimationOver,
                new WaitForSeconds(0.1f)
            )
        );

        StartCoroutine(
            Wait.WaitThen(0.5f, () =>
            {
                SetSprite(1);
                StartCoroutine(Wait.WaitThen(0.2f, () =>
                {
                    SetSprite(2);
                    StartCoroutine(Wait.WaitThen(0.2f, () =>
                    {
                        SetSprite(3);
                        animationOver = true;
                    }));
                }));
            })
        );
    }

    private void SetSprite(int index)
    {
        GetComponent<Image>().sprite = sprites[index];
    }

    private bool IsAnimationOver() { return animationOver; }
}
