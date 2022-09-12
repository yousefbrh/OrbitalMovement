using System;
using System.Collections;
using System.Collections.Generic;
using Lean.Touch;
using Sirenix.OdinInspector;
using UnityEngine;

public class OrbitalMovement : MonoBehaviour
{
    [SerializeField] private Transform centerTransform;
    [SerializeField] private MovementAxis movementAxis;
    [SerializeField] private TouchType touchType;
    [SerializeField] private float movementSpeed;
    
    [SerializeField] private bool hasRotationLimit;
    [ShowIf("hasRotationLimit")] [SerializeField] 
    private float minLimitedAngel;
    [ShowIf("hasRotationLimit")] [SerializeField] 
    private float maxLimitedAngel;

    [SerializeField] private bool hasStartingPosition;

    [ShowIf("hasStartingPosition")] [SerializeField]
    private Transform startingPositionTransform;
    
    private float _currentAngle;
    private float _deltaRotation;
    private bool _canMove;

    private void Start()
    {
        InitializeLean();
        SetupStartingPosition();
    }

    private void InitializeLean()
    {
        LeanTouch.OnFingerDown += ActivateMovement;
        LeanTouch.OnFingerUp += DeactivateMovement;
        LeanTouch.OnFingerUpdate += OrbitalMove;
    }

    private void SetupStartingPosition()
    {
        if (hasStartingPosition)
            transform.position = startingPositionTransform.position;
    }

    //In case that you want to initialize variables with script
    public void Initialize(Transform centerTransform)
    {
        this.centerTransform = centerTransform;
    }

    public void ActivateMovement(LeanFinger leanFinger)
    {
        _canMove = true;
    }

    public void DeactivateMovement(LeanFinger leanFinger)
    {
        _canMove = false;
    }

    //Main function for orbital movement
    public void OrbitalMove(LeanFinger leanFinger)
    {
        if (!_canMove) return;

        var screenDeltaAxisValue = GetScreenDeltaAxisValue(leanFinger);
        
        _deltaRotation = screenDeltaAxisValue * movementSpeed * Time.deltaTime;
        
        var newRotation = _currentAngle + _deltaRotation;

        CheckLimitAngle(newRotation);

        _currentAngle += _deltaRotation;
            
        var centerPosition = centerTransform.position;
        
        var axisForRotation = GetAxisForRotation();
        
        transform.RotateAround(centerPosition, axisForRotation, _deltaRotation);
    }

    private void CheckLimitAngle(float newRotation)
    {
        if (hasRotationLimit)
            LimitAngle(newRotation);
        else
            NoAngleLimitation(newRotation);
    }

    private void NoAngleLimitation(float newRotation)
    {
        if (newRotation > 360) _currentAngle -= 360;
        if (newRotation < -360) _currentAngle -= -360;
    }

    private void LimitAngle(float newRotation)
    {
        if (newRotation > maxLimitedAngel) _deltaRotation = maxLimitedAngel - _currentAngle;
        if (newRotation < minLimitedAngel) _deltaRotation = minLimitedAngel - _currentAngle;
    }

    private float GetScreenDeltaAxisValue(LeanFinger leanFinger)
    {
        return touchType switch
        {
            TouchType.RightToLeft => leanFinger.ScreenDelta.x,
            TouchType.UpToDown => leanFinger.ScreenDelta.y,
            _ => leanFinger.ScreenDelta.x
        };
    }

    private Vector3 GetAxisForRotation()
    {
        return movementAxis switch
        {
            MovementAxis.X => -Vector3.right,
            MovementAxis.Y => -Vector3.up,
            MovementAxis.Z => -Vector3.forward,
            _ => -Vector3.up
        };
    }
    
    private enum MovementAxis
    {
        X,
        Y,
        Z
    }
    
    private enum TouchType
    {
        UpToDown,
        RightToLeft
    }
}
