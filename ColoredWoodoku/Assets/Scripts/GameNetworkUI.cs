using UnityEngine;
using UnityEngine.UI;

public class GameNetworkUI : MonoBehaviour
{
    public Button hostButton;
    public Button clientButton;
    public Button endServerButton;
    public GameObject networkPanel; // Bu UI panelinin kendisi

    void Start()
    {
        if (hostButton != null)
            hostButton.onClick.AddListener(OnHostButtonClicked);

        if (clientButton != null)
            clientButton.onClick.AddListener(OnClientButtonClicked);

        if (endServerButton != null)
        {
            endServerButton.onClick.AddListener(OnEndServerButtonClicked);
            endServerButton.gameObject.SetActive(false);
        }
    }

    private void OnHostButtonClicked()
    {
        Debug.Log("Host Button Clicked!");
        GameNetworkManager.Instance?.StartHost();
        // Panel gizleme işini GameNetworkManager yapacak
    }

    private void OnClientButtonClicked()
    {
        Debug.Log("Client Button Clicked!");
        GameNetworkManager.Instance?.StartClient();
        // Panel gizleme işini GameNetworkManager yapacak
    }

    private void OnEndServerButtonClicked()
    {
        Debug.Log("End Server Button Clicked!");
        GameNetworkManager.Instance?.ShutdownServer();
    }

    public void ShowPanel(bool show)
    {
        if (networkPanel != null)
        {
            networkPanel.SetActive(show);
            if (show && endServerButton != null) 
            {
                endServerButton.gameObject.SetActive(false);
            }
        }
    }

    public void ActivateEndServerButton()
    {
        if (endServerButton != null) 
        {
            endServerButton.gameObject.SetActive(true);
        }
    }
} 