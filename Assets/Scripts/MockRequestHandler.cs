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
    
    [Header("Instance Costs")]
    [Tooltip("Cost of the EC2 instance (in dollars)")]
    [SerializeField] private float ec2Cost = 0.5f;
    [Tooltip("Cost of the RDB instance (in dollars)")]
    [SerializeField] private float rdbCost = 2.6f;
    [Tooltip("Cost of the ALB instance (in dollars)")]
    [SerializeField] private float albCost = 1.2f;

    [Header("Instance Existence")]
    [Tooltip("Whether the EC2 instance exists")]
    [SerializeField] private bool ec2Exists = true;
    [Tooltip("Whether the RDB instance exists")]
    [SerializeField] private bool rdbExists = true;
    [Tooltip("Whether the ALB instance exists")]
    [SerializeField] private bool albExists = true;

    // Public properties to access current states
    public InstanceState EC2State => ec2State;
    public InstanceState RDBState => rdbState;
    public InstanceState ALBState => albState;
    
    // Public properties to access existence status
    public bool EC2Exists => ec2Exists;
    public bool RDBExists => rdbExists;
    public bool ALBExists => albExists;

    private void Awake()
    {
        // Initialize mock AWS components with their states and costs
        ec2Component = new AwsComponent("arn:aws:ec2:us-west-2:123456789012:instance/i-1234567890abcdef0", "Mock-EC2", InstanceType.EC2, ec2State, ec2Cost);
        rdbComponent = new AwsComponent("arn:aws:rds:us-west-2:123456789012:db:mock-db", "Mock-RDB", InstanceType.RDS, rdbState, rdbCost);
        albComponent = new AwsComponent("arn:aws:elasticloadbalancing:us-west-2:123456789012:loadbalancer/app/mock-alb/1234567890abcdef", "Mock-ALB", InstanceType.ALB, albState, albCost);
    }

    /// <summary>
    /// Mock implementation of GetAwsComponents that returns predefined components with their current states
    /// </summary>
    public override IEnumerator GetAwsComponents(Action<List<AwsComponent>> callback)
    {
        // Simulate network delay
        yield return new WaitForSeconds(0.5f);

        // Update components with current states and costs
        ec2Component = new AwsComponent(ec2Component.Arn, ec2Component.InstanceName, ec2Component.IType, ec2State, ec2Cost);
        rdbComponent = new AwsComponent(rdbComponent.Arn, rdbComponent.InstanceName, rdbComponent.IType, rdbState, rdbCost);
        albComponent = new AwsComponent(albComponent.Arn, albComponent.InstanceName, albComponent.IType, albState, albCost);

        // Create a list of mock AWS components based on existence flags
        List<AwsComponent> components = new List<AwsComponent>();
        
        if (ec2Exists)
        {
            components.Add(ec2Component);
        }
        
        if (rdbExists)
        {
            components.Add(rdbComponent);
        }
        
        if (albExists)
        {
            components.Add(albComponent);
        }

        // Return the mock components
        callback(components);
    }
    
    /// <summary>
    /// Toggles the existence of the EC2 instance
    /// </summary>
    public void ToggleEC2Existence()
    {
        ec2Exists = !ec2Exists;
        Debug.Log($"EC2 instance {(ec2Exists ? "added" : "removed")}");
    }
    
    /// <summary>
    /// Toggles the existence of the RDB instance
    /// </summary>
    public void ToggleRDBExistence()
    {
        rdbExists = !rdbExists;
        Debug.Log($"RDB instance {(rdbExists ? "added" : "removed")}");
    }
    
    /// <summary>
    /// Toggles the existence of the ALB instance
    /// </summary>
    public void ToggleALBExistence()
    {
        albExists = !albExists;
        Debug.Log($"ALB instance {(albExists ? "added" : "removed")}");
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

    /// <summary>
    /// Mock implementation of Chat that simulates a response from the /chat endpoint
    /// </summary>
    public override IEnumerator Chat(string arn, Action<string> callback)
    {
        // Simulate network delay
        yield return new WaitForSeconds(0.5f);

        string responseMessage = "";

        // Generate a mock response based on the component type and state
        if (arn == ec2Component.Arn)
        {
            responseMessage = GetMockResponseForEC2();
        }
        else if (arn == rdbComponent.Arn)
        {
            responseMessage = GetMockResponseForRDB();
        }
        else if (arn == albComponent.Arn)
        {
            responseMessage = GetMockResponseForALB();
        }
        else
        {
            Debug.LogWarning($"Unknown ARN: {arn}");
            callback(null);
            yield break;
        }

        // Add the assistant's response to the chat log
        ChatMessage assistantMessage = new ChatMessage("assistant", responseMessage);
        
        // Use reflection to access the private chatLog field in the base class
        System.Reflection.FieldInfo chatLogField = typeof(RequestHandler).GetField("chatLog", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (chatLogField != null)
        {
            List<ChatMessage> chatLog = (List<ChatMessage>)chatLogField.GetValue(this);
            chatLog.Add(assistantMessage);
        }

        // Return the mock response
        callback(responseMessage);
    }

    /// <summary>
    /// Mock implementation of Talk that simulates a response from the /talk endpoint
    /// </summary>
    public override IEnumerator Talk(string arn, string msg, Action<string> callback)
    {
        // Simulate network delay
        yield return new WaitForSeconds(0.5f);

        // Use reflection to access the private chatLog field in the base class
        System.Reflection.FieldInfo chatLogField = typeof(RequestHandler).GetField("chatLog", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (chatLogField != null)
        {
            List<ChatMessage> chatLog = (List<ChatMessage>)chatLogField.GetValue(this);
            chatLog.Add(new ChatMessage("user", msg));
        }

        string responseMessage = "";

        // Generate a mock response based on the component type, state, and user message
        if (arn == ec2Component.Arn)
        {
            responseMessage = GetMockResponseForEC2WithMessage(msg);
        }
        else if (arn == rdbComponent.Arn)
        {
            responseMessage = GetMockResponseForRDBWithMessage(msg);
        }
        else if (arn == albComponent.Arn)
        {
            responseMessage = GetMockResponseForALBWithMessage(msg);
        }
        else
        {
            Debug.LogWarning($"Unknown ARN: {arn}");
            callback(null);
            yield break;
        }

        // Add the assistant's response to the chat log
        ChatMessage assistantMessage = new ChatMessage("assistant", responseMessage);
        if (chatLogField != null)
        {
            List<ChatMessage> chatLog = (List<ChatMessage>)chatLogField.GetValue(this);
            chatLog.Add(assistantMessage);
        }

        // Return the mock response
        callback(responseMessage);
    }

    /// <summary>
    /// Gets a mock response for the EC2 component based on its current state
    /// </summary>
    private string GetMockResponseForEC2()
    {
        switch (ec2State)
        {
            case InstanceState.LOW:
                return "EC2インスタンスの負荷は低いです。余裕があります！";
            case InstanceState.MIDDLE:
                return "EC2インスタンスの負荷は普通です。問題なく動作しています。";
            case InstanceState.HIGH:
                return "EC2インスタンスの負荷が高いです！スケールアウトを検討してください。";
            default:
                return "EC2インスタンスの状態を確認できません。";
        }
    }

    /// <summary>
    /// Gets a mock response for the RDB component based on its current state
    /// </summary>
    private string GetMockResponseForRDB()
    {
        switch (rdbState)
        {
            case InstanceState.LOW:
                return "RDBの負荷は低いです。クエリの実行に問題はありません。";
            case InstanceState.MIDDLE:
                return "RDBの負荷は普通です。通常通り動作しています。";
            case InstanceState.HIGH:
                return "RDBの負荷が高いです！クエリの最適化やレプリカの追加を検討してください。";
            default:
                return "RDBの状態を確認できません。";
        }
    }

    /// <summary>
    /// Gets a mock response for the ALB component based on its current state
    /// </summary>
    private string GetMockResponseForALB()
    {
        switch (albState)
        {
            case InstanceState.LOW:
                return "ALBのトラフィックは少ないです。余裕があります。";
            case InstanceState.MIDDLE:
                return "ALBのトラフィックは普通です。問題なく動作しています。";
            case InstanceState.HIGH:
                return "ALBのトラフィックが多いです！バックエンドのスケールアウトを検討してください。";
            default:
                return "ALBの状態を確認できません。";
        }
    }

    /// <summary>
    /// Gets a mock response for the EC2 component based on its current state and user message
    /// </summary>
    private string GetMockResponseForEC2WithMessage(string userMessage)
    {
        if (userMessage.Contains("調子") || userMessage.Contains("状態"))
        {
            return GetMockResponseForEC2();
        }
        else if (userMessage.Contains("スケール") || userMessage.Contains("拡張"))
        {
            return "EC2インスタンスをスケールアウトするには、Auto Scaling Groupを設定するか、手動で新しいインスタンスを追加できます。";
        }
        else
        {
            return "こんにちは！EC2インスタンスです。何かお手伝いできることはありますか？";
        }
    }

    /// <summary>
    /// Gets a mock response for the RDB component based on its current state and user message
    /// </summary>
    private string GetMockResponseForRDBWithMessage(string userMessage)
    {
        if (userMessage.Contains("調子") || userMessage.Contains("状態"))
        {
            return GetMockResponseForRDB();
        }
        else if (userMessage.Contains("バックアップ"))
        {
            return "RDBのバックアップは自動的に毎日取得されています。手動でバックアップを取ることもできます。";
        }
        else
        {
            return "こんにちは！RDBインスタンスです。データベースに関して何かお手伝いできることはありますか？";
        }
    }

    /// <summary>
    /// Gets a mock response for the ALB component based on its current state and user message
    /// </summary>
    private string GetMockResponseForALBWithMessage(string userMessage)
    {
        if (userMessage.Contains("調子") || userMessage.Contains("状態"))
        {
            return GetMockResponseForALB();
        }
        else if (userMessage.Contains("ターゲット") || userMessage.Contains("ルーティング"))
        {
            return "ALBのターゲットグループとルーティングルールは正常に設定されています。現在、すべてのターゲットは正常です。";
        }
        else
        {
            return "こんにちは！ALBです。ロードバランシングに関して何かお手伝いできることはありますか？";
        }
    }
}