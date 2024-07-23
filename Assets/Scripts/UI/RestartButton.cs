using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RestartButton : MonoBehaviour
{
    private void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClicked);
            Debug.Log("Restart button initialized successfully.");
        }
        else
        {
            Debug.LogError("Button component not found on the RestartButton GameObject.");
        }
    }

    private void OnButtonClicked()
    {
        Debug.Log("Restart button clicked. Attempting to reload the Gameplay scene.");
        SceneManager.LoadScene("Gameplay");
    }

    private void OnDestroy()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }
    }
}
