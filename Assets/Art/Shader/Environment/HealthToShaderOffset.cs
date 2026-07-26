using System;
using DG.Tweening;
using UnityEngine;

public class HealthToShaderOffset : MonoBehaviour
{
    [SerializeField] IntSO playerHealthSO;
    [SerializeField] IntSO adjustHealthSO; // PlayerAdjustTimeInt — negative = damage, positive = time gain
    [SerializeField] BoolSO isTimeSlow;
    [SerializeField] Vector2 offsetPerUnit = new Vector2(0.01f, 0f);
    [SerializeField] float secondLength = 1f; // match PlayerSystems secondLength
    [SerializeField] float emissionMin = 1f;
    [SerializeField] float emissionMax = 15f;
    [SerializeField] float dissolveMin = 1.1f;
    [SerializeField] float dissolveMax = 0.75f;
    [SerializeField] float damagePulseDuration = 0.25f;

    // Must match Shader Graph properties set to Global HLSL declaration
    static readonly int OffsetId = Shader.PropertyToID("_Offset");
    static readonly int EmissionMultiplyId = Shader.PropertyToID("_EmissionMultiply");
    static readonly int DissolveAmountId = Shader.PropertyToID("_DissolveAmount");
    static readonly int DamageSliderId = Shader.PropertyToID("_DamageSlider");

    Vector2 _currentOffset;
    float _currentEmission = 1f;
    float _currentDissolve = 1.1f;
    float _currentDamageSlider;
    Tween _offsetTween;
    Tween _damagePulseTween;
    Tween _timeSlowEmissionTween;

    void OnEnable()
    {
        _currentOffset = offsetPerUnit * playerHealthSO.Int;
        _currentEmission = emissionMin;
        _currentDissolve = dissolveMin;
        _currentDamageSlider = 0f;
        Shader.SetGlobalVector(OffsetId, _currentOffset);
        Shader.SetGlobalFloat(EmissionMultiplyId, _currentEmission);
        Shader.SetGlobalFloat(DissolveAmountId, _currentDissolve);
        Shader.SetGlobalFloat(DamageSliderId, _currentDamageSlider);

        playerHealthSO.onValueChanged += OnHealthChanged;
        if (adjustHealthSO != null)
            adjustHealthSO.onValueChanged += OnAdjustHealth;
        if (isTimeSlow != null)
            isTimeSlow.onValueChanged += OnTimeSlowChanged;
    }

    void OnDisable()
    {
        _offsetTween?.Kill();
        _damagePulseTween?.Kill();
        _timeSlowEmissionTween?.Kill();
        Shader.SetGlobalFloat(EmissionMultiplyId, emissionMin);
        Shader.SetGlobalFloat(DissolveAmountId, dissolveMin);
        Shader.SetGlobalFloat(DamageSliderId, 0f);
        if (playerHealthSO != null)
            playerHealthSO.onValueChanged -= OnHealthChanged;
        if (adjustHealthSO != null)
            adjustHealthSO.onValueChanged -= OnAdjustHealth;
        if (isTimeSlow != null)
            isTimeSlow.onValueChanged -= OnTimeSlowChanged;
    }

    void OnHealthChanged(object sender, EventArgs e)
    {
        // Time slow freezes environment time displacement
        if (isTimeSlow != null && isTimeSlow.Bool)
            return;

        Vector2 target = offsetPerUnit * playerHealthSO.Int;
        _offsetTween?.Kill();
        _offsetTween = DOTween.To(
                () => _currentOffset,
                v =>
                {
                    _currentOffset = v;
                    Shader.SetGlobalVector(OffsetId, v);
                },
                target,
                secondLength)
            .SetEase(Ease.Linear);
    }

    void OnTimeSlowChanged(object sender, EventArgs e)
    {
        bool slowed = isTimeSlow.Bool;

        if (slowed)
        {
            // Freeze time displacement where it is
            _offsetTween?.Kill();
            // Hold emission at max for the duration of slow
            _damagePulseTween?.Kill();
            _timeSlowEmissionTween?.Kill();
            _timeSlowEmissionTween = DOTween.To(
                    () => _currentEmission,
                    v =>
                    {
                        _currentEmission = v;
                        Shader.SetGlobalFloat(EmissionMultiplyId, v);
                    },
                    emissionMax,
                    damagePulseDuration)
                .SetEase(Ease.Linear);
        }
        else
        {
            // Revert emission, then catch offset up to current health
            _timeSlowEmissionTween?.Kill();
            _timeSlowEmissionTween = DOTween.To(
                    () => _currentEmission,
                    v =>
                    {
                        _currentEmission = v;
                        Shader.SetGlobalFloat(EmissionMultiplyId, v);
                    },
                    emissionMin,
                    damagePulseDuration)
                .SetEase(Ease.Linear);

            OnHealthChanged(null, EventArgs.Empty);
        }
    }

    void OnAdjustHealth(object sender, EventArgs e)
    {
        int adjust = adjustHealthSO.Int;
        if (adjust == 0)
            return;

        // Emission is owned by time-slow while active
        if (isTimeSlow != null && isTimeSlow.Bool)
            return;

        bool isDamage = adjust < 0;
        _damagePulseTween?.Kill();

        float returnDuration = damagePulseDuration * 3f;
        _currentDamageSlider = isDamage ? 1f : 0f;
        _currentEmission = emissionMax;
        _currentDissolve = dissolveMax;
        Shader.SetGlobalFloat(DamageSliderId, _currentDamageSlider);
        Shader.SetGlobalFloat(EmissionMultiplyId, _currentEmission);
        Shader.SetGlobalFloat(DissolveAmountId, _currentDissolve);

        Sequence pulse = DOTween.Sequence();
        pulse.Join(
            DOTween.To(
                    () => _currentEmission,
                    v =>
                    {
                        _currentEmission = v;
                        Shader.SetGlobalFloat(EmissionMultiplyId, v);
                    },
                    emissionMin,
                    returnDuration)
                .SetEase(Ease.Linear));
        pulse.Join(
            DOTween.To(
                    () => _currentDissolve,
                    v =>
                    {
                        _currentDissolve = v;
                        Shader.SetGlobalFloat(DissolveAmountId, v);
                    },
                    dissolveMin,
                    returnDuration)
                .SetEase(Ease.Linear));

        if (isDamage)
        {
            pulse.Join(
                DOTween.To(
                        () => _currentDamageSlider,
                        v =>
                        {
                            _currentDamageSlider = v;
                            Shader.SetGlobalFloat(DamageSliderId, v);
                        },
                        0f,
                        damagePulseDuration / 1.5f)
                    .SetEase(Ease.Linear));
        }

        _damagePulseTween = pulse;
    }
}
