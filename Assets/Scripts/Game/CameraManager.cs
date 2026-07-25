using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    [Header("Camera References")]
    [SerializeField] private List<CinemachineCamera> cameras;

    private int currentCamera;

    private void Start()
    {
        currentCamera = 0;

        for (int i = 0; i < cameras.Count; i++)
            cameras[i].Priority = (i == currentCamera) ? 10 : 0;
    }

    public void ActivateCamera(int index)
    {
        if (index == currentCamera)
            return;

        cameras[currentCamera].Priority = 0;
        cameras[index].Priority = 10;

        currentCamera = index;
    }
}
