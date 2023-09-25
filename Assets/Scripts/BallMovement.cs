using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class BallMovement : MonoBehaviour
{
    public Camera _Camera;
    public CinemachineFreeLook _FreeLookComponent;
    public Rigidbody _BallRigidbody;
    public float _BallSpeed;

    private KatamariInput mInput;
    private Vector2 mMoveInput;

    /// <summary>
    /// In start we create a new instance of KatamariInput, a class generated with Unity's new Input System.
    /// Subscribe our OnMoveBall and OnMoveCamera methods to the MoveBall and MoveCamera events raised from KatamariInput, then enable the input.
    /// </summary>
    private void Start()
    {
        mInput = new KatamariInput();
        mInput.Ball.MoveBall.performed += OnMoveBall;
        mInput.Ball.MoveCamera.performed += OnMoveCamera;

        mInput.Enable();
    }

    /// <summary>
    /// Updates the movement input vector. Subscribed to the mInput.Ball.MoveBall.Performed event.
    /// </summary>
    /// <param name="inCallback"></param>
    private void OnMoveBall(InputAction.CallbackContext inCallback)
    {
        mMoveInput = inCallback.ReadValue<Vector2>();
    }

    /// <summary>
    /// Continually add force to the sphere based on mMoveInput
    /// </summary>
    private void Update()
    {
        if (mMoveInput == Vector2.zero)
            return;

        Vector3 moveDirection = CalculateRelativeMoveDirection(mMoveInput);
        Vector3 finalMove = moveDirection * _BallSpeed * mMoveInput.magnitude;

        _BallRigidbody.AddForce(finalMove, ForceMode.Force);
    }

    /// <summary>
    /// Calculates how the ball should be moved relative to the camera's direction
    /// </summary>
    /// <param name="inputDirection"></param>
    /// <returns></returns>
    private Vector3 CalculateRelativeMoveDirection(Vector2 inputDirection)
    {
        Vector3 cameraForward = _Camera.transform.forward;
        Vector3 cameraRight = _Camera.transform.right;

        Vector3 moveDirection = (cameraForward * inputDirection.y) + (cameraRight * inputDirection.x);
        moveDirection.y = 0f;

        moveDirection.Normalize();

        return moveDirection;
    }

    /// <summary>
    /// Moves the camera. Subscribed to the mInput.Ball.MoveCamera.performed event.
    /// </summary>
    /// <param name="inCallback"></param>
    private void OnMoveCamera(InputAction.CallbackContext inCallback)
    {
        Vector2 inputDelta = inCallback.ReadValue<Vector2>();

        _FreeLookComponent.m_XAxis.Value += inputDelta.x;
        _FreeLookComponent.m_YAxis.Value += inputDelta.y;
    }
}
