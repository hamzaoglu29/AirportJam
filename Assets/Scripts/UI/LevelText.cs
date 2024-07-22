using TMPro;
using UnityEngine;

public class LevelText : MonoBehaviour
{
    private TextMeshProUGUI _levelText;
    private void Awake()
    {
        
        _levelText = GetComponent<TextMeshProUGUI>();
        _levelText.text = "Level "+ PlayerPrefs.GetInt("LevelIndex",GridManager.LevelIndex);
    }
}
