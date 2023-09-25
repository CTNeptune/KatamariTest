using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using UnityEngine.Rendering.PostProcessing;

public class BallMovement : MonoBehaviour
{
    public Camera _Camera;
    public CinemachineFreeLook _FreeLookComponent;
    public Rigidbody _BallRigidbody;
    public float _BallSpeed;

    public SphereCollider _BallCollider;

    public LayerMask _NormalCameraLayerMask;
    public LayerMask _InverseCameraLayerMask;

    public int _PostCollectPickupLayer; //This should align with the "CollectedPickups" layer
    public float _SizeAdjustPadding = 1.05f;
    public float _PickupSpeedScale = 2f;

    private KatamariInput mInput;
    private Vector2 mMoveInput;

    private List<Pickup> mCollectedPickups = new List<Pickup>();

    private bool mInverseCollisionEnabled;
    public int _PlayerLayer; //This should align with the "Player" layer
    public int _PickupsLayer; //This should align with the "Pickups" layer
    public int _InversePickupsLayer; //This should align with the "InversePickups" layer
    public int _PortalLayer; //This should align with the "Portal" layer

    public PostProcessVolume _NormalPostProcessVolume;
    public PostProcessVolume _InvertedPostProcessVolume;
    public float lerpDuration = 2f;

    private LensDistortion mNormalLensDistortion;
    private LensDistortion mInvertedLensDistortion;

    private bool mAbleToSwitchDimension;

    /// <summary>
    /// In start we create a new instance of KatamariInput, a class generated with Unity's new Input System.
    /// Subscribe our OnMoveBall and OnMoveCamera methods to the MoveBall and MoveCamera events raised from KatamariInput, then enable the input.
    /// </summary>
    private void Start()
    {
        mInput = new KatamariInput();
        mInput.Ball.MoveBall.performed += OnMoveBall;
        mInput.Ball.MoveCamera.performed += OnMoveCamera;
        mInput.Ball.SwitchDimension.performed += OnSwitchDimension;

        mInput.Enable();

        Physics.IgnoreLayerCollision(_PlayerLayer, _InversePickupsLayer, true);
        Physics.IgnoreLayerCollision(_InversePickupsLayer, _PlayerLayer, true);
        _Camera.cullingMask = _NormalCameraLayerMask;

        if (!_NormalPostProcessVolume || !_InvertedPostProcessVolume)
        {
            Debug.LogError("Missing a post processing volume on Player!");
            return;
        }

        _NormalPostProcessVolume.profile.TryGetSettings(out mNormalLensDistortion);
        _InvertedPostProcessVolume.profile.TryGetSettings(out mInvertedLensDistortion);
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer != _PickupsLayer && collision.gameObject.layer != _InversePickupsLayer)
            return;

        Pickup pickup = collision.gameObject.GetComponent<Pickup>();
        
        if (!pickup || mCollectedPickups.Contains(pickup))
            return;

        ProcessCollision(pickup);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != _PortalLayer)
            return;

        mAbleToSwitchDimension = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != _PortalLayer)
            return;

        mAbleToSwitchDimension = false;
    }

    private void ProcessCollision(Pickup inPickup)
    {
        if (!IsCollidedObjectBigger(inPickup._Collider))
            return;

        AddPickupToBall(inPickup);
        AdjustBallSize();
        _BallRigidbody.mass += inPickup._Rigidbody.mass;
        _BallSpeed += inPickup._Rigidbody.mass * _PickupSpeedScale;

        GamePlayManager.pInstance.AddToScore(_BallCollider.radius);
    }

    private bool IsCollidedObjectBigger(Collider collider)
    {
        Vector3 colliderSize = collider.bounds.size;
        if (colliderSize.x < _BallCollider.bounds.size.x || colliderSize.y < _BallCollider.bounds.size.y || colliderSize.z < _BallCollider.bounds.size.z)
            return true;

        Vector3 colliderScale = collider.transform.localScale;
        if (colliderScale.x < transform.localScale.x || colliderScale.y < transform.localScale.y || colliderScale.z < transform.localScale.z)
            return true;

        return false;
    }

    private void AddPickupToBall(Pickup inPickup)
    {
        inPickup.gameObject.layer = _PostCollectPickupLayer;
        inPickup.transform.SetParent(transform, true);

        FixedJoint joint = inPickup.gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = _BallRigidbody;

        inPickup._Rigidbody.useGravity = false;
        mCollectedPickups.Add(inPickup);
    }

    private void AdjustBallSize()
    {
        Bounds totalBounds = _BallCollider.bounds;

        for (int i = 1; i < mCollectedPickups.Count; i++)
            totalBounds.Encapsulate(mCollectedPickups[i]._Collider.bounds);

        float scaleFactor = totalBounds.extents.magnitude / _BallCollider.bounds.extents.magnitude;

        _BallCollider.radius *= scaleFactor * _SizeAdjustPadding;
    }

    private void OnSwitchDimension(InputAction.CallbackContext obj)
    {
        if (!mAbleToSwitchDimension)
            return;

        SwitchPhysics();
        SwitchCamera();
        SwitchPostProcessVolume();
    }

    private void SwitchPhysics()
    {
        mInverseCollisionEnabled = !mInverseCollisionEnabled;

        if (mInverseCollisionEnabled)
        {
            Physics.IgnoreLayerCollision(_PlayerLayer, _PickupsLayer, true);
            Physics.IgnoreLayerCollision(_PlayerLayer, _InversePickupsLayer, false);
        }
        else
        {
            Physics.IgnoreLayerCollision(_PlayerLayer, _PickupsLayer, false);
            Physics.IgnoreLayerCollision(_PlayerLayer, _InversePickupsLayer, true);
        }
    }

    private void SwitchCamera()
    {
        _Camera.cullingMask = mInverseCollisionEnabled ? _InverseCameraLayerMask : _NormalCameraLayerMask;
    }

    private void SwitchPostProcessVolume()
    {
        _NormalPostProcessVolume.enabled = !mInverseCollisionEnabled;
        _InvertedPostProcessVolume.enabled = mInverseCollisionEnabled;
        StartCoroutine(AdjustLensDistortion());
    }

    IEnumerator AdjustLensDistortion()
    {
        mNormalLensDistortion.intensity.value = -100f;
        mInvertedLensDistortion.intensity.value = -100f;

        float elapsedTime = 0f;
        float startValue = -100f;
        float endValue = 0f;

        while (elapsedTime < lerpDuration)
        {
            mNormalLensDistortion.intensity.value = Mathf.Lerp(startValue, endValue, elapsedTime / lerpDuration);
            mInvertedLensDistortion.intensity.value = Mathf.Lerp(startValue, endValue, elapsedTime / lerpDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mNormalLensDistortion.intensity.value = endValue;
        mInvertedLensDistortion.intensity.value = endValue;
    }
}
