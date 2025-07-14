using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private float _moveOffset = 1920f;

    [SerializeField] private RectTransform _fullCanvas;
    [SerializeField] private PlayableDirector _introDirector;
    [SerializeField] private float _durationIntro = 9f;

    public void OnClickStartButton() {
        SoundManager.Instance.PlaySound("ButtonClick");
        _fullCanvas.DOAnchorPosX(-_moveOffset, 1f).SetEase(Ease.OutBack).OnComplete(() => {
            _introDirector.Play();
            Invoke("LoadGameScene", _durationIntro);
        });
    }

    public void LoadGameScene() {
        _introDirector.Stop();
        SceneManager.LoadScene("Game");
    }

    public void OnClickHelpButton() {
         SoundManager.Instance.PlaySound("ButtonClick");
        _fullCanvas.DOAnchorPosX(_moveOffset, 1f).SetEase(Ease.OutBack).OnComplete(() => {
        });
    }

    public void OnClickBackButton() {
        SoundManager.Instance.PlaySound("ButtonClick");
        _fullCanvas.DOAnchorPosX(0, 1f).SetEase(Ease.OutBack);
    }

    public void OnClickExitButton() {
        SoundManager.Instance.PlaySound("ButtonClick");
        Application.Quit();
    }
}
