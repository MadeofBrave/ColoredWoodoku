using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class GridStateManager : NetworkBehaviour
{
    public static GridStateManager Instance { get; private set; }
    
    [System.Serializable]
    public struct GridSquareState
    {
        public int index;
        public bool isOccupied;
        public int colorIndex;
    }
    
    private List<GridSquareState> localGridState = new List<GridSquareState>();
    
    private List<GridSquareState> remoteGridState = new List<GridSquareState>();
    
    private Grid gridReference;
    
    public GameObject opponentsBoardPanel;
    public TextMeshProUGUI opponentsBoardText;
    
    private OpponentGridVisualizer gridVisualizer;
    
    public Transform gridVisualizationContainer;
    public GameObject gridSquarePrefab;
    private List<GameObject> visualGridSquares = new List<GameObject>();
    
    private void Awake()
    {
        gameObject.SetActive(true);
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            localGridState = new List<GridSquareState>();
            remoteGridState = new List<GridSquareState>();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    private void OnEnable()
    {
        if (Grid.Instance != null)
        {
            FindAndAssignGridReference();
        }
    }
    
    private void OnDisable()
    {
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (opponentsBoardPanel == null)
        {
            opponentsBoardPanel = GameObject.Find("OpponentsGrid");
        }
        
        if (opponentsBoardText == null && opponentsBoardPanel != null)
        {
            opponentsBoardText = opponentsBoardPanel.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        }
        
        FindAndAssignGridReference();
        
        FindOrCreateGridVisualizer();
    }
    
    private void FindOrCreateGridVisualizer()
    {
        if (gridVisualizer != null) return;
        
        if (opponentsBoardPanel != null)
        {
            gridVisualizer = opponentsBoardPanel.GetComponentInChildren<OpponentGridVisualizer>();
            
            if (gridVisualizer == null)
            {
                
                GameObject visualizerObj = new GameObject("OpponentGridVisualizer");
                visualizerObj.transform.SetParent(opponentsBoardPanel.transform, false);
                
                gridVisualizer = visualizerObj.AddComponent<OpponentGridVisualizer>();
                
                GameObject gridContainerObj = new GameObject("GridContainer");
                gridContainerObj.transform.SetParent(visualizerObj.transform, false);
                
                RectTransform containerRect = gridContainerObj.AddComponent<RectTransform>();
                containerRect.anchoredPosition = new Vector2(10, 200); 
                containerRect.sizeDelta = new Vector2(270, 270); 
                
                gridVisualizer.gridContainer = gridContainerObj.transform;
                
                if (gridSquarePrefab == null)
                {
                    GameObject prefab = new GameObject("GridSquarePrefab");
                    RectTransform rect = prefab.AddComponent<RectTransform>();
                    rect.sizeDelta = new Vector2(28, 28);
                    
                    UnityEngine.UI.Image image = prefab.AddComponent<UnityEngine.UI.Image>();
                    image.color = new Color(0.8f, 0.8f, 0.8f, 0.2f);
                    if (image != null)
                    {
                        
                            image.raycastTarget = false; 
                        
                    }
                    
                    gridSquarePrefab = prefab;
                    gridVisualizer.gridSquarePrefab = prefab;
                    
                    prefab.SetActive(false);
                    DontDestroyOnLoad(prefab);
                    
                }
                else
                {
                    gridVisualizer.gridSquarePrefab = gridSquarePrefab;
                }
            }
            else
            {
                if (gridVisualizer.gridSquarePrefab == null && gridSquarePrefab != null)
                {
                    gridVisualizer.gridSquarePrefab = gridSquarePrefab;
                }
            }
        }
    }
    
    private void Start()
    {
        FindAndAssignGridReference();
        
        Transform canvas = GameObject.Find("Canvas")?.transform;
        if (canvas != null)
        {
            Transform opponentsGrid = canvas.Find("OpponentsGrid");
            if (opponentsGrid != null)
            {
                Transform opponentsBoardTr = opponentsGrid.Find("OpponentsBoard");
                if (opponentsBoardTr != null)
                {
                    opponentsBoardPanel = opponentsBoardTr.gameObject;
                    
                    TextMeshProUGUI[] texts = opponentsBoardPanel.GetComponentsInChildren<TextMeshProUGUI>(true);
                    if (texts.Length > 0)
                    {
                        opponentsBoardText = texts[0];
                    }
                   
                }
            }
        }
        
        if (opponentsBoardText == null)
        {
            TextMeshProUGUI[] allTextComponents = FindObjectsOfType<TextMeshProUGUI>(true);
            foreach (var text in allTextComponents)
            {
                
                if (GetGameObjectPath(text.gameObject).Contains("OpponentsBoard"))
                {
                    opponentsBoardText = text;
                    opponentsBoardPanel = text.transform.parent.gameObject;
                    break;
                }
            }
        }
        
        if (opponentsBoardText != null)
        {
            
            if (opponentsBoardPanel != null && !opponentsBoardPanel.activeSelf)
            {
                opponentsBoardPanel.SetActive(true);
            }
        }
        
        InvokeRepeating("FindAndAssignGridReference", 2f, 5f);
    }
    
    private void FindAndAssignGridReference()
    {
        if (gridReference == null)
        {
            gridReference = Grid.Instance;
            
            if (gridReference == null)
            {
                gridReference = FindObjectOfType<Grid>();
            }
        }
    }
    
    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        
        return path;
    }
    public void CollectLocalGridState()
    {
        if (gridReference == null)
        {
            return;
        }
        
        localGridState.Clear();

        if (gridReference._GridSquares.Count == 0 && Application.isPlaying)
        {
        }

        for (int i = 0; i < gridReference._GridSquares.Count; i++)
        {
            if (gridReference._GridSquares[i] == null)
            {
                continue;
            }
            var gridSquareComponent = gridReference._GridSquares[i].GetComponent<GridSquare>();
            
            if (gridSquareComponent == null)
            {
                GridSquareState errorState = new GridSquareState
                {
                    index = i,
                    isOccupied = false,
                    colorIndex = -1
                };
                localGridState.Add(errorState);
                continue;
            }
            
            GridSquareState squareState = new GridSquareState
            {
                index = i,
                isOccupied = gridSquareComponent.SquareOccupied,
                colorIndex = ConvertShapeColorToIndex(gridSquareComponent.OccupiedColor)
            };
            
            localGridState.Add(squareState);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    public void SendGridStateToServerRpc(int[] squareIndices, bool[] occupiedStates, int[] colorIndices, ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        ulong receiverId = senderId == 0 ? 1ul : 0ul; 
        ReceiveGridStateClientRpc(squareIndices, occupiedStates, colorIndices, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { receiverId }
            }
        });
    }
    
    [ClientRpc]
    public void ReceiveGridStateClientRpc(int[] squareIndices, bool[] occupiedStates, int[] colorIndices, ClientRpcParams clientRpcParams = default)
    {
        remoteGridState.Clear();
        
        int doluKareSayisi = 0;
        
        for (int i = 0; i < squareIndices.Length; i++)
        {
            bool isDolu = occupiedStates[i] && colorIndices[i] >= 0;
            
            GridSquareState squareState = new GridSquareState
            {
                index = squareIndices[i],
                isOccupied = occupiedStates[i],
                colorIndex = colorIndices[i]
            };
            
            if (isDolu)
            {
                doluKareSayisi++;
                int row = squareIndices[i] / 9;
                int col = squareIndices[i] % 9;
            }
            
            remoteGridState.Add(squareState);
        }
        
        DisplayComparisonPanel();
    }
    
    public void DisplayOpponentBoard()
    {
        DisplayComparisonPanel();
    }
    
    public void ShareGridState()
    {
        if (gridReference == null)
        {
            FindAndAssignGridReference();
            
            if (gridReference == null)
            {
                return;
            }
        }
        
        CollectLocalGridState();
        
        try
        {
            int occupiedCount = localGridState.Count(s => s.isOccupied);
            if (occupiedCount == 0)
            {
                if (gridReference != null && gridReference._GridSquares.Count > 0)
                {
                }
            }
            
            int[] squareIndices = new int[localGridState.Count];
            bool[] occupiedStates = new bool[localGridState.Count];
            int[] colorIndices = new int[localGridState.Count];
            
            for (int i = 0; i < localGridState.Count; i++)
            {
                squareIndices[i] = localGridState[i].index;
                occupiedStates[i] = localGridState[i].isOccupied;
                colorIndices[i] = localGridState[i].colorIndex;
            }
            
            SendGridStateToServerRpc(squareIndices, occupiedStates, colorIndices);
        }
        catch (System.Exception e)
        {
        }
    }
    
    private int ConvertShapeColorToIndex(Shape.ShapeColor color)
    {
        switch (color)
        {
            case Shape.ShapeColor.Blue:
                return 0;
            case Shape.ShapeColor.Green:
                return 1;
            case Shape.ShapeColor.Yellow:
                return 2;
            case Shape.ShapeColor.Joker:
                return 3;
            case Shape.ShapeColor.None:
            default:
                return -1;
        }
    }
    
    
    private void DisplayComparisonPanel()
    {
        VisualizeOpponentGridOnPanel();
    }
    
    private void VisualizeOpponentGridOnPanel()
    {
        if (opponentsBoardPanel != null)
        {
            opponentsBoardPanel.SetActive(true);
            
            int doluKareSayisi = 0;
            foreach (var state in remoteGridState)
            {
                if (state.isOccupied && state.colorIndex >= 0)
                {
                    doluKareSayisi++;
                    int satir = state.index / 9;
                    int sutun = state.index % 9;
                }
            }
            
            FindOrCreateGridVisualizer();
            
            if (gridVisualizer != null)
            {
                gridVisualizer.gameObject.SetActive(true);
                
                var filtered = new List<GridSquareState>();
                foreach (var state in remoteGridState)
                {
                    if (state.isOccupied && state.colorIndex >= 0)
                    {
                        filtered.Add(state);
                    }
                }
                
                gridVisualizer.UpdateVisualGrid(filtered);
                
            }
            else
            {
                CreateVisualGrid();
            }
        }
    }
    
    private void CreateVisualGrid()
    {
        ClearVisualGrid();
        
        if (gridVisualizationContainer == null)
        {
            gridVisualizationContainer = new GameObject("GridVisualizationContainer").transform;
            gridVisualizationContainer.SetParent(opponentsBoardPanel.transform, false);
            
            RectTransform containerRect = gridVisualizationContainer.gameObject.AddComponent<RectTransform>();
            containerRect.anchoredPosition = new Vector2(0, -50); 
            containerRect.sizeDelta = new Vector2(270, 270);
        }
        if (gridSquarePrefab == null)
        {
            
            gridSquarePrefab = new GameObject("GridSquarePrefab");
            gridSquarePrefab.AddComponent<RectTransform>().sizeDelta = new Vector2(28, 28);
            UnityEngine.UI.Image image = gridSquarePrefab.AddComponent<UnityEngine.UI.Image>();
            image.color = Color.white;
            
            gridSquarePrefab.SetActive(false);
        }
        
        const int gridSize = 9;
        float cellSize = 30f;
        float startX = -gridSize * cellSize / 2 + cellSize / 2;
        float startY = gridSize * cellSize / 2 - cellSize / 2;
        
        
        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                int index = row * gridSize + col;
                
                GridSquareState? squareState = null;
                foreach (var state in remoteGridState)
                {
                    if (state.index == index)
                    {
                        squareState = state;
                        break;
                    }
                }
                
                GameObject square = Instantiate(gridSquarePrefab, gridVisualizationContainer);
                square.SetActive(true);
                visualGridSquares.Add(square);
                
                RectTransform rectTransform = square.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(
                    startX + col * cellSize, 
                    startY - row * cellSize
                );
                
                UnityEngine.UI.Image squareImage = square.GetComponent<UnityEngine.UI.Image>();
                
                if (squareState.HasValue && squareState.Value.isOccupied)
                {
                    switch (squareState.Value.colorIndex)
                    {
                        case 0: 
                            squareImage.color = new Color(0.2f, 0.4f, 1f, 1f);
                            break;
                        case 1:
                            squareImage.color = new Color(0.2f, 1f, 0.2f, 1f);
                            break;
                        case 2: 
                            squareImage.color = new Color(1f, 1f, 0.2f, 1f);
                            break;
                        case 3: 
                            squareImage.color = new Color(1f, 0.2f, 1f, 1f);
                            break;
                        default:
                            squareImage.color = new Color(0.8f, 0.8f, 0.8f, 0.7f);
                            break;
                    }
                }
                else
                {
                    squareImage.color = new Color(0.8f, 0.8f, 0.8f, 0.2f);
                }
            }
        }
    }
    
    private void ClearVisualGrid()
    {
        foreach (var square in visualGridSquares)
        {
            Destroy(square);
        }
        visualGridSquares.Clear();
    }
    
    private System.Collections.IEnumerator HidePanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (opponentsBoardText != null)
        {
            opponentsBoardText.text = "Rakip tahtası";
        }
    }

    public void LocalPlayerFinishedPlacingShapes()
    {
        GridStateManager.Instance?.ShareGridState();
    }
} 