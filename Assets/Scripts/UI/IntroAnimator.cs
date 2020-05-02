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
            GameManager.Instance.Player.transform.position = 
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
            GameManager.Instance.StartNewGame();
            yield break;
        }

        GameManager.Instance.Player.AnimatingIntro = true;
        GameManager.Instance.Player.Respawn(false);

        yield return AnimatePlayer(GameManager.Instance.GetOffsetScreenPoint(new Vector2(0, -1f)), introPosition.position, 2f);
        yield return new WaitUntil(() => SkipIntro);

        GameManager.Instance.ResetGame();
        UIController.Instance.ShowGameUI();

        yield return AnimatePlayer(GameManager.Instance.Player.transform.position, startPosition.position, 0.5f, skippable: false);

        GameManager.Instance.Player.AnimatingIntro = false;
        GameManager.Instance.StartNewGame();
    }
}
