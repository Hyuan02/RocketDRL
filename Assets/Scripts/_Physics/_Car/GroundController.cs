using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class GroundController : MonoBehaviour
{

    private Rigidbody _rBody;
    private CarManager _instance;

    private WheelController[] _wheels;

    private void Start()
    {
        _rBody = this.GetComponent<Rigidbody>();
        _instance = this.GetComponent<CarManager>();
        _wheels = this.GetComponentsInChildren<WheelController>();
    }

    private void FixedUpdate()
    {
        //ApplyDownForce();
        CalculateDriftDrag();
        _instance.stats.forwardAcceleration = CalcForwardForce(_instance.GetForwardSignal(), _instance.GetBoostSignal());
        _instance.stats.currentSteerAngle = CalculateSteerAngle(_instance.GetTurnSignal());
        //ApplyRotOnWheels();

    }

    private void CalculateDriftDrag()
    {
        float currentDriftDrag = _instance.GetDriftSignal() ? _instance.stats.wheelSideFrictionDrift : _instance.stats.wheelSideFriction;
        _instance.stats.currentWheelSideFriction = Mathf.MoveTowards(_instance.stats.currentWheelSideFriction, currentDriftDrag, Time.deltaTime * _instance.stats.driftTime);
    }

    private void ApplyDownForce()
    {
        if (_instance.carState == CarStates.AllWheelsSurface || _instance.carState == CarStates.AllWheelsGround)
            _rBody.AddForce(-transform.up * 5, ForceMode.Acceleration);
    }

    private void ApplyRotOnWheels()
    {
        foreach(WheelController wheel in _wheels)
        {
            wheel.RotateWheels(_instance.stats.currentSteerAngle);
        }
    }

    private float CalcForwardForce(float throttleInput, bool boostInput)
    {
        // Throttle
        float forwardAcceleration = 0;

        forwardAcceleration = (boostInput? 1 : throttleInput) * GetForwardAcceleration(_instance.stats.forwardSpeedAbs);

        if (_instance.stats.forwardSpeedSign != Mathf.Sign(throttleInput) && throttleInput != 0)
        {
            forwardAcceleration += -1 * _instance.stats.forwardSpeedSign * Constants.BREAK_STRENGTH;
        }

        return forwardAcceleration;
    }


    static float GetForwardAcceleration(float speed)
    {
        float throttle = 0;

        if(speed > Constants.Instance.MaxSpeed)
        {
            throttle = 0;
        }
        else if(speed > Constants.Instance.IntermediateSpeed)
        {
            throttle = RoboUtils.Scale(14f, 14.1f, 1.6f, 0, speed);
        }
        else
        {
            throttle = RoboUtils.Scale(0, 14, 16, 1.6f, speed);
        }

        return throttle;
    }


    static float GetTurnRadius(float forwardSpeed)
    {
        float turnRadius = 0;

        var curvature = RoboUtils.Scale(0, 5, 0.0069f, 0.00398f, forwardSpeed);

        if (forwardSpeed >= 500 / 100)
            curvature = RoboUtils.Scale(5, 10, 0.00398f, 0.00235f, forwardSpeed);

        if (forwardSpeed >= 1000 / 100)
            curvature = RoboUtils.Scale(10, 15, 0.00235f, 0.001375f, forwardSpeed);

        if (forwardSpeed >= 1500 / 100)
            curvature = RoboUtils.Scale(15, 17.5f, 0.001375f, 0.0011f, forwardSpeed);

        if (forwardSpeed >= 1750 / 100)
            curvature = RoboUtils.Scale(17.5f, 23, 0.0011f, 0.00088f, forwardSpeed);

        turnRadius = 1 / (curvature * 100);
        return turnRadius;
    }

    private float CalculateSteerAngle(float turnInput)
    {
        float curvature = 1 / GetTurnRadius(_instance.stats.forwardSpeedAbs);
        return turnInput * curvature * _instance.stats.turnRadiusCoefficient;
    }


}
