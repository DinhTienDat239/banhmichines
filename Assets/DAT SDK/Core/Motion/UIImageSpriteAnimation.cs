using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIImageSpriteAnimation : MonoBehaviour
{
    [SerializeField]
    Image _UIImageObject;
    [SerializeField]
    bool _autoStart = false;
    [SerializeField]
    bool _loop = true;
    [SerializeField]
    bool _caculateByDuration;
    [SerializeField]
    bool _caculateByFPS;
    [SerializeField]
    float _duration;
    [SerializeField]
    int _animationFPS;
    [SerializeField]
    List<Sprite> _spriteList = new List<Sprite>();
    [SerializeField]
    bool _useUnscaledTime = false;

    float _timePerFrame;
    Coroutine _playRoutine;
    // Removed WaitForSeconds cache to allow precise duration-based stepping
    // Start is called before the first frame update
    private void Awake()
    {
        if (_UIImageObject == null)
        {
            _UIImageObject = GetComponent<Image>();
        }

        RecalculateTiming();
    }
    void Start()
    {
        if (_UIImageObject != null && _spriteList != null && _spriteList.Count > 0)
        {
            _UIImageObject.sprite = _spriteList[0];
        }
        if (_autoStart)
        {
            PlayAnimation();
        }
    }

    public void PlayAnimation()
    {
        if (_playRoutine != null)
        {
            StopCoroutine(_playRoutine);
            _playRoutine = null;
        }

        if (_UIImageObject == null || _spriteList == null || _spriteList.Count == 0 || _timePerFrame <= 0f)
        {
            return;
        }

        _playRoutine = StartCoroutine(PlayAnimationIE());
    }

    public void StopAnimation()
    {
        if (_playRoutine != null)
        {
            StopCoroutine(_playRoutine);
            _playRoutine = null;
        }
    }

    IEnumerator PlayAnimationIE()
    {
        if (_spriteList == null || _spriteList.Count == 0 || _timePerFrame <= 0f)
        {
            yield break;
        }

        if (_caculateByDuration)
        {
            do
            {
                int frameCount = _spriteList.Count;
                float perFrame = frameCount > 0 ? _duration / frameCount : 0f;
                if (perFrame <= 0f)
                {
                    yield break;
                }

                if (_useUnscaledTime)
                {
                    var wait = new WaitForSecondsRealtime(perFrame);
                    for (int i = 0; i < frameCount; i++)
                    {
                        _UIImageObject.sprite = _spriteList[i];
                        yield return wait;
                    }
                }
                else
                {
                    var wait = new WaitForSeconds(perFrame);
                    for (int i = 0; i < frameCount; i++)
                    {
                        _UIImageObject.sprite = _spriteList[i];
                        yield return wait;
                    }
                }

            } while (_loop);
        }
        else // FPS mode
        {
            do
            {
                int index = 0;
                float accumulator = 0f;
                float timePerFrameLocal = _timePerFrame;
                int frameCount = _spriteList.Count;

                // hiển thị khung đầu tiên
                _UIImageObject.sprite = _spriteList[0];

                while (index < frameCount - 1)
                {
                    yield return null;
                    accumulator += Time.deltaTime;

                    if (timePerFrameLocal > 0f)
                    {
                        int steps = Mathf.FloorToInt(accumulator / timePerFrameLocal);
                        if (steps > 0)
                        {
                            index = Mathf.Min(index + steps, frameCount - 1);
                            accumulator -= steps * timePerFrameLocal;
                            _UIImageObject.sprite = _spriteList[index];
                        }
                    }
                    else
                    {
                        break;
                    }
                }

            } while (_loop);
        }

        _playRoutine = null;
    }

    void OnValidate()
    {
        RecalculateTiming();
    }

    void RecalculateTiming()
    {
        if (_caculateByDuration)
        {
            int frameCount = _spriteList != null ? _spriteList.Count : 0;
            if (frameCount > 0 && _duration > 0f)
            {
                _timePerFrame = _duration / frameCount;
            }
            else
            {
                _timePerFrame = 0f;
            }
        }
        else if (_caculateByFPS)
        {
            _timePerFrame = _animationFPS > 0 ? 1f / _animationFPS : 0f;
        }
        else
        {
            _timePerFrame = 0f;
        }
    }

    private void OnDisable()
    {
        StopAnimation();
    }

    private void OnDestroy()
    {
        StopAnimation();
    }
}
