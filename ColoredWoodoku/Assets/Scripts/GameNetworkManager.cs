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

    private bool isWaitingForOthers = false; 
    
    // For synchronizing random shapes - using NetworkList instead of arrays
    private NetworkList<int> syncedShapeIndices;
    private NetworkList<int> syncedShapeColors;
        
    // For syncing explosion colors
    private NetworkVariable<int> syncedExplosionColor = new NetworkVariable<int>(
        0, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server);
        
    // Flag to indicate if shapes have been synchronized
    private NetworkVariable<bool> shapesReadyToUse = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
        
    // Flag to track initial shapes generation
    private bool initialShapesGenerated = false;

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
        
        // Initialize the NetworkLists
        syncedShapeIndices = new NetworkList<int>();
        syncedShapeColors = new NetworkList<int>();
        
        if (waitingText != null)
        {
            waitingText.gameObject.SetActive(false);
        }
    }
    
    private void Start()
    {
        // Register for network variable change events
        if (syncedShapeIndices != null)
        {
            syncedShapeIndices.OnListChanged += OnSyncedShapesChanged;
        }
        
        if (shapesReadyToUse != null)
        {
            shapesReadyToUse.OnValueChanged += OnShapesReadyChanged;
        }
        
        // Subscribe to client connection event
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }
    
    private void OnDestroy()
    {
        // Unregister from network events to prevent memory leaks
        if (syncedShapeIndices != null)
        {
            syncedShapeIndices.OnListChanged -= OnSyncedShapesChanged;
        }
        
        if (shapesReadyToUse != null)
        {
            shapesReadyToUse.OnValueChanged -= OnShapesReadyChanged;
        }
        
        // Unsubscribe from client connection event
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
    
    // Event fired when a client connects
    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client connected: {clientId}");
        
        if (IsServer)
        {
            // If this is the host and shapes are already generated,
            // trigger a refresh for the new client
            if (initialShapesGenerated)
            {
                Debug.Log("Re-syncing shapes for newly connected client");
                
                // Reset flag to ensure change is detected
                shapesReadyToUse.Value = false;
                
                // Small delay to ensure network variables are updated
                StartCoroutine(SyncShapesForNewClient(0.5f));
            }
            else
            {
                // Shapes aren't generated yet, generate them now
                Debug.Log("Generating initial shapes for first connected client");
                GenerateInitialShapes();
            }
        }
    }
    
    private IEnumerator SyncShapesForNewClient(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Re-sync shapes to ensure they match on all clients
        if (syncedShapeIndices.Count > 0)
        {
            // Shapes already exist, let's not regenerate
            // Just set ready flag to true to trigger refresh
            shapesReadyToUse.Value = true;
            Debug.Log("Re-synced existing shapes to new client");
        }
        else
        {
            // No shapes exist yet, generate them
            GenerateAndSyncRandomShapes();
            shapesReadyToUse.Value = true;
            Debug.Log("Generated new shapes for new client");
        }
    }
    
    // Generate initial shapes once when host starts
    private void GenerateInitialShapes()
    {
        if (!IsServer || initialShapesGenerated) return;
        
        Debug.Log("Generating initial shapes on host");
        
        // First set ready to false to ensure change is detected
        shapesReadyToUse.Value = false;
        
        // Generate shapes
        GenerateAndSyncRandomShapes();
        
        // Mark initial shapes as generated
        initialShapesGenerated = true;
        
        // Set shapes as ready with small delay
        StartCoroutine(SetShapesReadyAfterDelay(0.5f));
    }
    
    private void OnShapesReadyChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            // Shapes are ready to use, let's get new shapes
            Debug.Log("Shapes are ready signal received - requesting initial shapes");
            GameEvents.RequestNewShapeMethod();
        }
    }
    
    private void OnSyncedShapesChanged(NetworkListEvent<int> changeEvent)
    {
        Debug.Log($"SyncedShapes changed: {changeEvent.Type}");
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
            
            // Initial shapes will be generated when clients connect
            // so we don't need to do it here
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
        
        Debug.Log("Client started - waiting for shapes from server");
    }

    public void LocalPlayerFinishedPlacingShapes()
    {        
        isWaitingForOthers = true;
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

    private void InitializeServerState()
    {
         playersFinished.Clear();
    }

    private void HandlePlayerFinishedOnServer(ulong clientId)
    {
        if (!IsServer) return; 

        playersFinished[clientId] = true;

        if (NetworkManager.Singleton.ConnectedClientsList.Count >= expectedPlayers && 
            playersFinished.Count == NetworkManager.Singleton.ConnectedClientsList.Count &&
            playersFinished.Values.All(finished => finished))
        {
            Debug.Log("All players finished. Generating new shapes on server");
            
            // Reset the shapes ready flag first to ensure clients know new shapes are coming
            shapesReadyToUse.Value = false;
            
            // Generate and sync random shapes before requesting new shapes
            GenerateAndSyncRandomShapes();
            
            // Also sync the explosion color
            SyncExplosionColor();
            
            // Set flag to indicate shapes are ready
            // Küçük bir gecikme ekleyelim ki istemciler tüm değişiklikleri algılayabilsin
            StartCoroutine(SetShapesReadyAfterDelay(0.2f));
            
            // Tell all clients that all players have finished
            AllPlayersFinishedClientRpc();
            
            // Reset server state for next round
            InitializeServerState(); 
        }
    }
    
    private System.Collections.IEnumerator SetShapesReadyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        shapesReadyToUse.Value = true;
        Debug.Log("Shapes ready set to TRUE after delay");
    }
    
    // Generate random shapes on the server and sync them to clients
    private void GenerateAndSyncRandomShapes()
    {
        if (!IsServer) 
        {
            Debug.LogWarning("Attempted to generate shapes on client. This should only happen on server!");
            return;
        }
        
        Debug.Log("Server is generating random shapes");
        
        ShapeStorage shapeStorage = ShapeStorage.Instance;
        if (shapeStorage == null) 
        {
            Debug.LogError("ShapeStorage not found!");
            return;
        }
        
        int shapesCount = shapeStorage.ShapeList.Count;
        
        // Clear previous data
        syncedShapeIndices.Clear();
        syncedShapeColors.Clear();
        
        for (int i = 0; i < shapesCount; i++)
        {
            // Generate random shape index
            int shapeIndex = Random.Range(0, shapeStorage.shapeData.Count);
            
            // Generate random color (0=Blue, 1=Green, 2=Yellow)
            int shapeColor = Random.Range(0, 3);
            
            Debug.Log($"Server generated shape {i}: Type={shapeIndex}, Color={shapeColor}");
            
            // Add to NetworkLists
            syncedShapeIndices.Add(shapeIndex);
            syncedShapeColors.Add(shapeColor);
        }
    }
    
    // Sync the explosion color for color squares
    private void SyncExplosionColor()
    {
        if (!IsServer) return;
        
        // Get current explosion color and sync it
        Shape.ShapeColor explosionColor = GameEvents.LastExplosionColor;
        int colorInt = 0; // Default to Blue
        
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
    
    // Method for setting the explosion color based on synced value
    public void ApplySyncedExplosionColor()
    {
        if (syncedExplosionColor.Value < 0)
        {
            GameEvents.SetLastExplosionColorMethod(Shape.ShapeColor.None);
            return;
        }
        
        Shape.ShapeColor color = Shape.ShapeColor.Blue; // Default
        
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
    
    // Method for ShapeStorage to get the synced random shape index
    public int GetSyncedShapeIndex(int shapePosition)
    {
        if (syncedShapeIndices == null || syncedShapeIndices.Count <= shapePosition) 
        {
            Debug.LogWarning($"Requested shape index {shapePosition} but only {(syncedShapeIndices != null ? syncedShapeIndices.Count : 0)} are available");
            // Return random as fallback if not synced yet
            return Random.Range(0, ShapeStorage.Instance.shapeData.Count);
        }
        
        return syncedShapeIndices[shapePosition];
    }
    
    // Method for ShapeStorage to get the synced random color
    public Shape.ShapeColor GetSyncedShapeColor(int shapePosition)
    {
        if (syncedShapeColors == null || syncedShapeColors.Count <= shapePosition) 
        {
            Debug.LogWarning($"Requested shape color {shapePosition} but only {(syncedShapeColors != null ? syncedShapeColors.Count : 0)} are available");
            // Return random as fallback
            return (Shape.ShapeColor)Random.Range(0, 3);
        }
        
        return (Shape.ShapeColor)syncedShapeColors[shapePosition];
    }
    
    // Check if shapes are ready to use (for clients)
    public bool AreShapesReadyToUse()
    {
        return shapesReadyToUse.Value;
    }

    [ClientRpc]
    private void AllPlayersFinishedClientRpc()
    {
        Debug.Log("Received AllPlayersFinishedClientRpc");
        
        // All players have finished, now we can show new shapes
        isWaitingForOthers = false;
        ShowWaitingMessageLocally(false); 
        
        // Apply synced explosion color
        ApplySyncedExplosionColor();
        
        // Şekiller zaten hazır olduğu için ve OnShapesReadyChanged tarafından otomatik tetikleneceği için
        // burada ayrıca GameEvents.RequestNewShapeMethod() çağırmaya gerek yok
    }

    private void ShowWaitingMessageLocally(bool show)
    {
        if (waitingText == null)
        {
            return;
        }

        if (show)
        {
            waitingText.text = "Shapes placed! Waiting for other player...";
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