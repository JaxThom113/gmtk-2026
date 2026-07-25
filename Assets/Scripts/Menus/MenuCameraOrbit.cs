using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class MenuCameraOrbit : MonoBehaviour
{
    [Header("Camera Movement Settings")]
    [SerializeField] private float orbitSpeed;

    private CinemachineOrbitalFollow orbitalFollow;

    void Awake()
    {
        orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
    }

    void Update()
    {
        orbitalFollow.HorizontalAxis.Value += orbitSpeed * Time.deltaTime;
    }
}