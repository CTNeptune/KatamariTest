using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class BallMovement : MonoBehaviour
{
    public CinemachineFreeLook _FreeLookCamera;

    private KatamariInput mInput;

    private void Start()
    {
        mInput = new KatamariInput();
        mInput.Ball.MoveBall.performed += OnMoveBall;
        mInput.Ball.MoveCamera.performed += OnMoveCamera;

        mInput.Enable();
    }

    private void OnMoveBall(InputAction.CallbackContext obj)
    {
        throw new NotImplementedException();
    }

    private void OnMoveCamera(InputAction.CallbackContext obj)
    {
        Vector2 inputDelta = obj.ReadValue<Vector2>();

        _FreeLookCamera.m_XAxis.Value += inputDelta.x;
        _FreeLookCamera.m_YAxis.Value += inputDelta.y;
    }
}
