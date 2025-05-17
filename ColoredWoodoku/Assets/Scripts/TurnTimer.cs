using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class TurnTimer : NetworkBehaviour
{
    public static TurnTimer Instance { get; private set; }
    
    [SerializeField] private float turnDuration = 30f;
    [SerializeField] private TextMeshProUGUI timerText;
    
    private NetworkVariable<float> currentTime = new NetworkVariable<float>(30f);
    private NetworkVariable<bool> isTimerActive = new NetworkVariable<bool>(false);
    private Coroutine timerCoroutine;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (timerText == null)
        {
            timerText = GameObject.Find("Timer")?.GetComponent<TextMeshProUGUI>();
        }
        
        currentTime.OnValueChanged += OnTimeChanged;
        isTimerActive.OnValueChanged += OnTimerActiveChanged;
    }
    
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        
        currentTime.OnValueChanged -= OnTimeChanged;
        isTimerActive.OnValueChanged -= OnTimerActiveChanged;
    }
    
    private void OnTimeChanged(float previousValue, float newValue)
    {
        UpdateTimerDisplay();
    }
    
    private void OnTimerActiveChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            StartTimerLocally();
        }
        else
        {
            StopTimerLocally();
        }
    }
    
    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(currentTime.Value);
            timerText.text = seconds.ToString();
            
            if (seconds <= 10)
            {
                timerText.color = Color.red;
            }
            else
            {
                timerText.color = Color.white;
            }
        }
    }
    
    private void StartTimerLocally()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        
        timerCoroutine = StartCoroutine(CountdownTimer());
    }
    
    private void StopTimerLocally()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }
    
    public void StartTurn()
    {
        if (IsServer)
        {
            currentTime.Value = turnDuration;
            isTimerActive.Value = true;
            
            StartTurnClientRpc();
        }
        else
        {
            StartTurnServerRpc();
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void StartTurnServerRpc(ServerRpcParams rpcParams = default)
    {
        currentTime.Value = turnDuration;
        isTimerActive.Value = true;
        
        StartTurnClientRpc();
    }
    
    [ClientRpc]
    private void StartTurnClientRpc()
    {
        if (!IsServer)
        {
            UpdateTimerDisplay();
        }
    }
    
    public void PauseTurn()
    {
        if (IsServer)
        {
            isTimerActive.Value = false;
        }
        else
        {
            PauseTurnServerRpc();
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void PauseTurnServerRpc()
    {
        isTimerActive.Value = false;
    }
    
    private IEnumerator CountdownTimer()
    {
        while (currentTime.Value > 0 && isTimerActive.Value)
        {
            yield return new WaitForSeconds(0.1f);
            
            if (IsServer && isTimerActive.Value)
            {
                currentTime.Value -= 0.1f;
            }
            
            if (IsServer && currentTime.Value <= 0)
            {
                isTimerActive.Value = false;
                
                CheckUnfinishedPlayers();
            }
        }
    }
    
    private void CheckUnfinishedPlayers()
    {
        if (!IsServer) return;
        
        GameNetworkManager gameManager = GameNetworkManager.Instance;
        if (gameManager == null) return;
        
        gameManager.CheckAndHandleUnfinishedPlayersOnTimeout();
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void TimerExpiredServerRpc(ulong timeoutPlayerId, ServerRpcParams rpcParams = default)
    {
        ulong loserId = rpcParams.Receive.SenderClientId;
        
        GameNetworkManager.Instance.PlayerTimeoutServerRpc(loserId);
    }
} 