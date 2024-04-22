using UnityEngine;
using UnityEngine.UI;

public class IntroCutsceneHandler : SceneTransitionHandler
{
    [SerializeField]
    private Sprite[] sprites;


    protected override void Start()
    {
        base.Start();
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
                        StartCoroutine(Wait.WaitThen(0.3f, () =>
                        {
                            snakeSpawners[0].SpawnChungusnake();
                        }));
                    }));
                }));
            })
        );
    }


    private void SetSprite(int index)
    {
        GetComponent<Image>().sprite = sprites[index];
    }
}
