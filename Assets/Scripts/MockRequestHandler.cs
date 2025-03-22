using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Text;
using Models;

/// <summary>
/// A mock implementation of RequestHandler for debugging purposes.
/// This class simulates API responses without making actual network requests.
/// </summary>
public class MockRequestHandler : RequestHandler
{
    [Header("AWS Components")]
    [SerializeField] private AwsComponent ec2Component;
    [SerializeField] private AwsComponent rdbComponent;
    [SerializeField] private AwsComponent albComponent;

    [Header("Current States")]
    [Tooltip("Current state of the EC2 instance")]
    [SerializeField] private InstanceState ec2State = InstanceState.MIDDLE;
    [Tooltip("Current state of the RDB instance")]
    [SerializeField] private InstanceState rdbState = InstanceState.MIDDLE;
    [Tooltip("Current state of the ALB instance")]
    [SerializeField] private InstanceState albState = InstanceState.MIDDLE;

    // Public properties to access current states
    public InstanceState EC2State => ec2State;
    public InstanceState RDBState => rdbState;
    public InstanceState ALBState => albState;

    private void Awake()
    {
        // Initialize mock AWS components
        ec2Component = new AwsComponent("arn:aws:ec2:us-west-2:123456789012:instance/i-1234567890abcdef0", "Mock-EC2", InstanceType.EC2);
        rdbComponent = new AwsComponent("arn:aws:rds:us-west-2:123456789012:db:mock-db", "Mock-RDB", InstanceType.RDB);
        albComponent = new AwsComponent("arn:aws:elasticloadbalancing:us-west-2:123456789012:loadbalancer/app/mock-alb/1234567890abcdef", "Mock-ALB", InstanceType.ALB);
    }

    /// <summary>
    /// Mock implementation of GetAwsComponents that returns predefined components
    /// </summary>
    public override IEnumerator GetAwsComponents(Action<List<AwsComponent>> callback)
    {
        // Simulate network delay
        yield return new WaitForSeconds(0.5f);

        // Create a list of mock AWS components
        List<AwsComponent> components = new List<AwsComponent>
        {
            ec2Component,
            rdbComponent,
            albComponent
        };

        // Return the mock components
        callback(components);
    }

    /// <summary>
    /// Mock implementation of GetAwsState that returns predefined states based on component ARN
    /// </summary>
    public override IEnumerator GetAwsState(string arn, Action<AwsState> callback)
    {
        // Simulate network delay
        yield return new WaitForSeconds(0.5f);

        // Determine which component is being requested and return the appropriate state
        if (arn == ec2Component.Arn)
        {
            callback(new AwsState(arn, ec2Component.InstanceName, ec2State));
        }
        else if (arn == rdbComponent.Arn)
        {
            callback(new AwsState(arn, rdbComponent.InstanceName, rdbState));
        }
        else if (arn == albComponent.Arn)
        {
            callback(new AwsState(arn, albComponent.InstanceName, albState));
        }
        else
        {
            Debug.LogWarning($"Unknown ARN: {arn}");
            callback(null);
        }
    }

    /// <summary>
    /// Changes the state of the EC2 component
    /// </summary>
    [ContextMenu("Change EC2 State")]
    public void ChangeEC2State()
    {
        ec2State = GetNextState(ec2State);
        Debug.Log($"EC2 state changed to: {ec2State}");
    }

    /// <summary>
    /// Changes the state of the RDB component
    /// </summary>
    [ContextMenu("Change RDB State")]
    public void ChangeRDBState()
    {
        rdbState = GetNextState(rdbState);
        Debug.Log($"RDB state changed to: {rdbState}");
    }

    /// <summary>
    /// Changes the state of the ALB component
    /// </summary>
    [ContextMenu("Change ALB State")]
    public void ChangeALBState()
    {
        albState = GetNextState(albState);
        Debug.Log($"ALB state changed to: {albState}");
    }

    /// <summary>
    /// Gets the next state in the cycle (LOW -> MIDDLE -> HIGH -> LOW)
    /// </summary>
    private InstanceState GetNextState(InstanceState currentState)
    {
        switch (currentState)
        {
            case InstanceState.LOW:
                return InstanceState.MIDDLE;
            case InstanceState.MIDDLE:
                return InstanceState.HIGH;
            case InstanceState.HIGH:
                return InstanceState.LOW;
            default:
                return InstanceState.MIDDLE;
        }
    }
}