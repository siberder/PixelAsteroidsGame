using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroAnimator : MonoSingleton<IntroAnimator>
{
    public float introDelay = 2f;
    public AnimationCurve shipMovingAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("References")]
    public Transform introPosition;
    public Transform startPosition;

    public bool ShowingIntro { get; set; }
    public bool SkipIntro { get; set; }

    public void ShowIntro()
    {
        StartCoroutine(AnimateSpaceShip_Intro());
    }

    IEnumerator AnimatePlayer(Vector3 from, Vector3 to, float time, bool skippable = true)
    {
        for (float _animTime = 0; _animTime < time; _animTime += Time.deltaTime)
        {
            if(skippable)
            {
                if (SkipIntro) break;
            }

            float t = _animTime / time;
            LevelController.Instance.Player.transform.position = 
                Vector3.Lerp(from, to, shipMovingAnimationCurve.Evaluate(t));
            yield return null;
        }
    }

    // Animating intro
    IEnumerator AnimateSpaceShip_Intro()
    {
        UIController.Instance.ShowMenuScreen();

        if (ShowingIntro)
        {
            yield break;
        }

        ShowingIntro = true;

        SkipIntro = false;

        float time = 0f;
        yield return new WaitUntil(() => (time += Time.deltaTime) >= introDelay || SkipIntro);

        if (SkipIntro)
        {
            LevelController.Instance.StartNewGame();
            yield break;
        }

        LevelController.Instance.Player.AnimatingIntro = true;
        LevelController.Instance.Player.Respawn(false);

        yield return AnimatePlayer(LevelController.Instance.GetOffscreenPoint(new Vector2(0, -1f)), introPosition.position, 2f);
        yield return new WaitUntil(() => SkipIntro);

        LevelController.Instance.ResetGame();
        UIController.Instance.ShowGameUI();

        yield return AnimatePlayer(LevelController.Instance.Player.transform.position, startPosition.position, 0.5f, skippable: false);

        LevelController.Instance.Player.AnimatingIntro = false;
        LevelController.Instance.StartNewGame();
    }
}
