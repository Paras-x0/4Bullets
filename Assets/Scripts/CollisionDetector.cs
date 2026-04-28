using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    public GameObject listeningUI;
    public PlayerMovement playerMovement;

    void Start()
    {
        // Make sure UI is hidden at start
        listeningUI.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("F pressed");
            EnterListeningMode();
        }
    }

    void EnterListeningMode()
    {
        Debug.Log("Entering Listening Mode");

        listeningUI.SetActive(true);

        if (playerMovement != null)
            playerMovement.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}