using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum EffectType {
    LevelUp,
    IceItem,
    HealItem,
    RocketItem,
}

[System.Serializable]
public class EffectData
{
    [Header("Effect Settings")]
    public EffectType effectType;
    public Image effectImage;
    
    [Header("Animation Settings")]
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 0.5f;
    public float holdDuration = 1.0f;
    public bool loop = false;
    public int loopCount = -1; // -1 for infinite
    public Ease easeType = Ease.InOutSine;
    
    [Header("Camera Shake Settings")]
    public bool enableCameraShake = false;
    public float shakeIntensity = 1f;
    public float shakeDuration = 0.5f;
    public float shakeFrequency = 0.1f;
}

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    [Header("Effect Settings")]
    [SerializeField] private List<EffectData> effectDataList = new List<EffectData>();
    
    [Header("Camera Shake")]
    [SerializeField] private Cinemachine.CinemachineVirtualCamera virtualCamera;
    [SerializeField] private bool _globalCameraShakeEnabled = false; // 전역 카메라 쉐이크 설정
    
    private Dictionary<EffectType, EffectData> effectDataDict = new Dictionary<EffectType, EffectData>();
    private Dictionary<EffectType, Sequence> activeEffectSequences = new Dictionary<EffectType, Sequence>();

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
            InitializeEffectData();
        } else {
            Destroy(gameObject);
        }
    }

    private void InitializeEffectData()
    {
        effectDataDict.Clear();
        foreach (var effectData in effectDataList)
        {
            if (effectData.effectImage != null)
            {
                effectDataDict[effectData.effectType] = effectData;
                // 초기 상태 설정
                Color color = effectData.effectImage.color;
                color.a = 0f;
                effectData.effectImage.color = color;
            }
        }
    }

    public void PlayEffect(EffectType effectType)
    {
        if (!effectDataDict.ContainsKey(effectType))
        {
            Debug.LogWarning($"Effect {effectType} not found in EffectDataList!");
            return;
        }

        EffectData effectData = effectDataDict[effectType];
        
        // 이미 실행 중인 이펙트가 있다면 중지
        if (activeEffectSequences.ContainsKey(effectType))
        {
            activeEffectSequences[effectType].Kill();
            activeEffectSequences.Remove(effectType);
        }

        // 새로운 이펙트 시작
        Sequence effectSequence = CreateEffectSequence(effectData);
        activeEffectSequences[effectType] = effectSequence;
    }

    public void StopEffect(EffectType effectType)
    {
        if (activeEffectSequences.ContainsKey(effectType))
        {
            activeEffectSequences[effectType].Kill();
            activeEffectSequences.Remove(effectType);
            
            // 이펙트 이미지 알파값을 0으로 설정
            if (effectDataDict.ContainsKey(effectType))
            {
                EffectData effectData = effectDataDict[effectType];
                Color color = effectData.effectImage.color;
                color.a = 0f;
                effectData.effectImage.color = color;
            }
        }
    }

    public void StopAllEffects()
    {
        foreach (var sequence in activeEffectSequences.Values)
        {
            if (sequence != null)
            {
                sequence.Kill();
            }
        }
        activeEffectSequences.Clear();

        // 모든 이펙트 이미지 알파값을 0으로 설정
        foreach (var effectData in effectDataList)
        {
            if (effectData.effectImage != null)
            {
                Color color = effectData.effectImage.color;
                color.a = 0f;
                effectData.effectImage.color = color;
            }
        }
    }

    private Sequence CreateEffectSequence(EffectData effectData)
    {
        Sequence sequence = DOTween.Sequence();

        Image effectImage = effectData.effectImage;
        if (effectImage != null) 
        {
            // Fade In
            sequence.Append(effectImage.DOFade(1f, effectData.fadeInDuration).SetEase(effectData.easeType));
            // Hold
            sequence.AppendInterval(effectData.holdDuration);
            // Fade Out
            sequence.Append(effectImage.DOFade(0f, effectData.fadeOutDuration).SetEase(effectData.easeType));
        }
        
        // 카메라 쉐이크 실행 (이펙트 시작과 동시에)
        if (_globalCameraShakeEnabled && effectData.enableCameraShake)
        {
            // 이펙트 시작과 동시에 쉐이크 실행
            sequence.Insert(0f, DOTween.Sequence().OnComplete(() => {
                StartCoroutine(CameraShakeCoroutine(effectData.shakeIntensity, effectData.shakeDuration, effectData.shakeFrequency));
            }));
        }
        
        // 루프 설정
        if (effectData.loop)
        {
            if (effectData.loopCount > 0)
            {
                sequence.SetLoops(effectData.loopCount);
            }
            else
            {
                sequence.SetLoops(-1); // 무한 루프
            }
        }
        
        // 시퀀스 완료 후 정리
        sequence.OnComplete(() => {
            if (activeEffectSequences.ContainsValue(sequence))
            {
                var key = GetKeyByValue(activeEffectSequences, sequence);
                if (key.HasValue)
                {
                    activeEffectSequences.Remove(key.Value);
                }
            }
        });
        
        return sequence;
    }

    private EffectType? GetKeyByValue(Dictionary<EffectType, Sequence> dict, Sequence value)
    {
        foreach (var kvp in dict)
        {
            if (kvp.Value == value)
            {
                return kvp.Key;
            }
        }
        return null;
    }

    private IEnumerator CameraShakeCoroutine(float intensity, float duration, float frequency)
    {
        if (virtualCamera == null) 
        {
            Debug.LogWarning("Virtual Camera is not assigned to EffectManager!");
            yield break;
        }

        Cinemachine.CinemachineBasicMultiChannelPerlin noise = 
            virtualCamera.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
        
        if (noise == null) 
        {
            Debug.LogWarning("Cinemachine Basic Multi Channel Perlin component not found on Virtual Camera! Please add this component.");
            yield break;
        }

        if (noise.m_NoiseProfile == null)
        {
            Debug.LogWarning("Noise Profile is not assigned to Cinemachine Basic Multi Channel Perlin! Please assign a noise profile.");
            yield break;
        }

        float elapsed = 0f;
        float originalAmplitudeGain = noise.m_AmplitudeGain;
        float originalFrequencyGain = noise.m_FrequencyGain;

        while (elapsed < duration)
        {
            noise.m_AmplitudeGain = intensity;
            noise.m_FrequencyGain = frequency;
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 원래 값으로 복원
        noise.m_AmplitudeGain = originalAmplitudeGain;
        noise.m_FrequencyGain = originalFrequencyGain;
    }

    // 인스펙터에서 이펙트 데이터 추가/제거를 위한 메서드
    [ContextMenu("Add New Effect")]
    private void AddNewEffect()
    {
        EffectData newEffect = new EffectData();
        effectDataList.Add(newEffect);
    }

    [ContextMenu("Remove Last Effect")]
    private void RemoveLastEffect()
    {
        if (effectDataList.Count > 0)
        {
            effectDataList.RemoveAt(effectDataList.Count - 1);
        }
    }

    // 전역 카메라 쉐이크 설정
    public void SetGlobalCameraShake(bool enabled)
    {
        _globalCameraShakeEnabled = enabled;
    }

    // 런타임에서 이펙트 데이터 동적 추가
    public void AddEffectData(EffectData effectData)
    {
        if (effectData.effectImage != null)
        {
            effectDataList.Add(effectData);
            effectDataDict[effectData.effectType] = effectData;
            
            // 초기 상태 설정
            Color color = effectData.effectImage.color;
            color.a = 0f;
            effectData.effectImage.color = color;
        }
    }

    // 런타임에서 이펙트 데이터 제거
    public void RemoveEffectData(EffectType effectType)
    {
        if (effectDataDict.ContainsKey(effectType))
        {
            StopEffect(effectType);
            effectDataList.RemoveAll(x => x.effectType == effectType);
            effectDataDict.Remove(effectType);
        }
    }
}
