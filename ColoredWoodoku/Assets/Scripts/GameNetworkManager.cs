using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System.Collections;

public class GameNetworkManager : NetworkBehaviour
{
    public static GameNetworkManager Instance { get; private set; }
    public TextMeshProUGUI waitingText;

    private Dictionary<ulong, bool> playersFinished = new Dictionary<ulong, bool>();
    private int expectedPlayers = 2;
    private Dictionary<ulong, bool> gridStatesReceived = new Dictionary<ulong, bool>();

    private bool isWaitingForOthers = false; 
    
    private NetworkList<int> syncedShapeIndices;
    private NetworkList<int> syncedShapeColors;
    private NetworkVariable<int> syncedExplosionColor = new NetworkVariable<int>(
        0, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server);
        
    private NetworkVariable<bool> shapesReadyToUse = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
        
    private bool initialShapesGenerated = false;

    private NetworkVariable<bool> gridStateManagerSpawned = new NetworkVariable<bool>(false);

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        syncedShapeIndices = new NetworkList<int>();
        syncedShapeColors = new NetworkList<int>();
        
        if (waitingText != null)
        {
            waitingText.gameObject.SetActive(false);
        }
    }
    
    private void Start()
    {
        if (syncedShapeIndices != null)
        {
            syncedShapeIndices.OnListChanged += OnSyncedShapesChanged;
        }
        
        if (shapesReadyToUse != null)
        {
            shapesReadyToUse.OnValueChanged += OnShapesReadyChanged;
        }
        
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnDestroy()
    {
        if (syncedShapeIndices != null)
        {
            syncedShapeIndices.OnListChanged -= OnSyncedShapesChanged;
        }
        
        if (shapesReadyToUse != null)
        {
            shapesReadyToUse.OnValueChanged -= OnShapesReadyChanged;
        }
        
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
    
    private void OnClientConnected(ulong clientId)
    {
        
        if (IsServer)
        {
            if (initialShapesGenerated)
            {
                
                shapesReadyToUse.Value = false;
                StartCoroutine(SyncShapesForNewClient(0.5f));
            }
            else
            {
                GenerateInitialShapes();
            }
        }
    }
    
    private IEnumerator SyncShapesForNewClient(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (syncedShapeIndices.Count > 0)
        {
            shapesReadyToUse.Value = true;
        }
        else
        {
            GenerateAndSyncRandomShapes();
            shapesReadyToUse.Value = true;
        }
    }
    
    private void GenerateInitialShapes()
    {
        if (!IsServer || initialShapesGenerated) return;
        
        shapesReadyToUse.Value = false;
        GenerateAndSyncRandomShapes();
        
        initialShapesGenerated = true;
        
        StartCoroutine(SetShapesReadyAfterDelay(0.5f));
    }
    
    private void OnShapesReadyChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            GameEvents.RequestNewShapeMethod();
        }
    }

    private void OnSyncedShapesChanged(NetworkListEvent<int> changeEvent)
    {
    }

    public void StartHost()
    {
        if (NetworkManager.Singleton == null)
        {
            return; 
        }
        NetworkManager.Singleton.StartHost();
        
        var networkUI = FindObjectOfType<GameNetworkUI>();
        if (networkUI == null)
        {
            return;
        }
        networkUI?.ShowPanel(false);
        
        networkUI?.ActivateEndServerButton(); 
        
        if (IsServer)
        {
            InitializeServerState();
            
            SpawnGridStateManager();
        }
    }

    private void SpawnGridStateManager()
    {
        if (!IsServer) return;
        
        GridStateManager existingManager = FindObjectOfType<GridStateManager>();
        
        if (existingManager == null)
        {
            GameObject gridStateObj = new GameObject("GridStateManager");
            GridStateManager gridStateManager = gridStateObj.AddComponent<GridStateManager>();
            
            var networkObject = gridStateObj.AddComponent<NetworkObject>();
            networkObject.Spawn();
            
            gridStateManagerSpawned.Value = true;
            
        }
        else
        {
            if (existingManager.gameObject.GetComponent<NetworkObject>() == null)
            {
                existingManager.gameObject.AddComponent<NetworkObject>();
            }
            
            var networkObject = existingManager.gameObject.GetComponent<NetworkObject>();
            if (!networkObject.IsSpawned)
            {
                networkObject.Spawn();
                gridStateManagerSpawned.Value = true;
            }
        }
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        
        var networkUI = FindObjectOfType<GameNetworkUI>();
        if (networkUI == null)
        {
            return;
        }
        networkUI?.ShowPanel(false);
        
    }

    public void LocalPlayerFinishedPlacingShapes()
    {
        ShowWaitingMessageLocally(true);
        InitiateNetworkNotification();
    }

    private void InitiateNetworkNotification()
    {
        if (IsServer) 
        {
            HandlePlayerFinishedOnServer(NetworkManager.Singleton.LocalClientId);
        }
        else if (IsClient) 
        {
            NotifyServerPlayerFinishedServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)] 
    private void NotifyServerPlayerFinishedServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientIdWhoFinished = rpcParams.Receive.SenderClientId;
        HandlePlayerFinishedOnServer(clientIdWhoFinished);
    }

    private void HandlePlayerFinishedOnServer(ulong clientId)
    {
        if (!playersFinished.ContainsKey(clientId))
        {
            playersFinished.Add(clientId, true);
        }
        else
        {
            playersFinished[clientId] = true;
        }
        
        ShowWaitingMessageClientRpc(new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        });
        
        if (playersFinished.Count >= expectedPlayers && playersFinished.Values.All(finished => finished))
        {
            foreach (var playerId in playersFinished.Keys)
            {
                UpdateOpponentGridClientRpc(new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { playerId }
                    }
                });
            }
            
            StartCoroutine(GenerateShapesAfterDelay(2.0f));
        }
    }

    private void InitializeServerState()
    {
        playersFinished.Clear();
        gridStatesReceived.Clear();
    }

    [ClientRpc]
    private void ShowWaitingMessageClientRpc(ClientRpcParams rpcParams = default)
    {
        ShowWaitingMessageLocally(true);
    }

    [ClientRpc]
    private void UpdateOpponentGridClientRpc(ClientRpcParams rpcParams = default)
    {
        if (GridStateManager.Instance != null)
        {
            GridStateManager.Instance.DisplayOpponentBoard();
        }
    }

    private IEnumerator GenerateShapesAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        shapesReadyToUse.Value = false;
        GenerateAndSyncRandomShapes();
        SyncExplosionColor();
        StartCoroutine(SetShapesReadyAfterDelay(0.2f));
        AllPlayersFinishedClientRpc();
        InitializeServerState(); 
    }
    
    [ClientRpc]
    private void RequestGridStateSharingClientRpc(ClientRpcParams rpcParams = default)
    {
        
        
        
            if (!gridStateManagerSpawned.Value)
            {
                StartCoroutine(TryShareGridStateAfterDelay(0.5f));
                return;
            }

            GridStateManager gridStateManager = FindObjectOfType<GridStateManager>();
            if (gridStateManager != null)
            {
                gridStateManager.ShareGridState();
            }
            else
            {
                StartCoroutine(TryShareGridStateAfterDelay(0.5f));
            }
        
       
    }
    
    private System.Collections.IEnumerator TryShareGridStateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        GridStateManager gridStateManager = FindObjectOfType<GridStateManager>();
        if (gridStateManager != null)
        {
            gridStateManager.ShareGridState();
        }
    }
    
    private System.Collections.IEnumerator SetShapesReadyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        shapesReadyToUse.Value = true;
    }
    
    private void GenerateAndSyncRandomShapes()
    {
        if (!IsServer) 
        {
            return;
        }
        
        
        ShapeStorage shapeStorage = ShapeStorage.Instance;
        if (shapeStorage == null) 
        {
            return;
        }
        
        int shapesCount = shapeStorage.ShapeList.Count;
        
        syncedShapeIndices.Clear();
        syncedShapeColors.Clear();
        
        for (int i = 0; i < shapesCount; i++)
        {
            int shapeIndex = Random.Range(0, shapeStorage.shapeData.Count);
            
            int shapeColor = Random.Range(0, 3);
            
            syncedShapeIndices.Add(shapeIndex);
            syncedShapeColors.Add(shapeColor);
        }
    }
    
    private void SyncExplosionColor()
    {
        if (!IsServer) return;
        
        Shape.ShapeColor explosionColor = GameEvents.LastExplosionColor;
        int colorInt = 0;
        
        switch (explosionColor)
        {
            case Shape.ShapeColor.Blue:
                colorInt = 0;
                break;
            case Shape.ShapeColor.Green:
                colorInt = 1;
                break;
            case Shape.ShapeColor.Yellow:
                colorInt = 2;
                break;
            case Shape.ShapeColor.Joker:
                colorInt = 3;
                break;
            case Shape.ShapeColor.None:
                colorInt = -1;
                break;
        }
        
        syncedExplosionColor.Value = colorInt;
    }
    
    public void ApplySyncedExplosionColor()
    {
        if (syncedExplosionColor.Value < 0)
        {
            GameEvents.SetLastExplosionColorMethod(Shape.ShapeColor.None);
            return;
        }
        
        Shape.ShapeColor color = Shape.ShapeColor.Blue;
        
        switch (syncedExplosionColor.Value)
        {
            case 0:
                color = Shape.ShapeColor.Blue;
                break;
            case 1:
                color = Shape.ShapeColor.Green;
                break;
            case 2:
                color = Shape.ShapeColor.Yellow;
                break;
            case 3:
                color = Shape.ShapeColor.Joker;
                break;
        }
        
        GameEvents.SetLastExplosionColorMethod(color);
    }
    
    public int GetSyncedShapeIndex(int shapePosition)
    {
        if (syncedShapeIndices == null || syncedShapeIndices.Count <= shapePosition) 
        {
            return Random.Range(0, ShapeStorage.Instance.shapeData.Count);
        }
        
        return syncedShapeIndices[shapePosition];
    }
    
    public Shape.ShapeColor GetSyncedShapeColor(int shapePosition)
    {
        if (syncedShapeColors == null || syncedShapeColors.Count <= shapePosition) 
        {
            return (Shape.ShapeColor)Random.Range(0, 3);
        }
        
        return (Shape.ShapeColor)syncedShapeColors[shapePosition];
    }
    
    public bool AreShapesReadyToUse()
    {
        return shapesReadyToUse.Value;
    }

    [ClientRpc]
    private void AllPlayersFinishedClientRpc()
    {
        isWaitingForOthers = false;
        ShowWaitingMessageLocally(false);
        ApplySyncedExplosionColor();
    }

    private void ShowWaitingMessageLocally(bool show)
    {
        if (waitingText == null)
        {
            return;
        }

        if (show)
        {
            waitingText.gameObject.SetActive(true);
        }
        else
        {
            waitingText.text = "";
            waitingText.gameObject.SetActive(false);
        }
    }

    public void ShutdownServer()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.Shutdown();
            InitializeServerState();
            initialShapesGenerated = false;
            
            var networkUI = FindObjectOfType<GameNetworkUI>();
             if (networkUI == null)
            {
                return;
            }
            networkUI?.ShowPanel(true); 
        }
    }
} 