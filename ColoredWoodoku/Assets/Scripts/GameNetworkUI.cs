using UnityEngine;
using UnityEngine.UI;

public class GameNetworkUI : MonoBehaviour
{
    public Button hostButton;
    public Button clientButton;
    public Button endServerButton;
    public GameObject networkPanel;

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
        GameNetworkManager.Instance?.StartHost();
    }

    private void OnClientButtonClicked()
    {
        GameNetworkManager.Instance?.StartClient();
    }

    private void OnEndServerButtonClicked()
    {
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