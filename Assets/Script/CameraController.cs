using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    private CinemachineVirtualCamera _virtualCamera;

    void Awake() {
        _virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    public void SetTarget(Transform target) {
        _virtualCamera.Follow = target;
    }
}
