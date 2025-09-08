using UnityEngine;
using UnityEngine.UI;
using TMPro; // Required for TextMeshPro

public class LevelUpUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button optionButtonPrefab; // Changed to Button
    [SerializeField] private Transform optionsContainer;

    [Header("Skill Settings")]
    [SerializeField] private int numberOfOptions = 3;

    private SkillManager skillManager;

    void Awake()
    {
        if (panel != null)
        {
            panel.SetActive(false); // Start hidden
        }
        skillManager = FindObjectOfType<SkillManager>();
    }

    public void ShowLevelUpOptions()
    {
        if (panel == null || optionButtonPrefab == null || optionsContainer == null)
        {
            Debug.LogError("LevelUpUI references missing! Panel, Option Button Prefab, or Options Container is null.");
            return;
        }

        panel.SetActive(true);
        Time.timeScale = 0f; // Pause the game

        // Clear previous options
        foreach (Transform child in optionsContainer)
        {
            Destroy(child.gameObject);
        }

        // Generate and display new options
        GenerateOptions();
    }

    void GenerateOptions()
    {
        int currentLevel = 1;
        var player = FindObjectOfType<PlayerHealth>();
        if (player != null) currentLevel = player.GetCurrentLevel();

        System.Collections.Generic.List<SkillType> availableSkills = skillManager != null ?
            skillManager.GetEligibleSkills(currentLevel) :
            new System.Collections.Generic.List<SkillType>();

        Debug.Log($"[LevelUpUI] Generating options. PlayerLevel={currentLevel}, Available={availableSkills.Count}");
        System.Collections.Generic.List<SkillType> chosenSkills = new System.Collections.Generic.List<SkillType>();

        for (int i = 0; i < numberOfOptions; i++)
        {
            if (availableSkills.Count == 0) break;

            int randomIndex = Random.Range(0, availableSkills.Count);
            SkillType chosenSkill = availableSkills[randomIndex];
            chosenSkills.Add(chosenSkill);
            availableSkills.RemoveAt(randomIndex);

            CreateOptionButton(chosenSkill);
        }
        Debug.Log($"LevelUpUI spawned {chosenSkills.Count} option buttons.");
    }

    void CreateOptionButton(SkillType skillType)
    {
        Button button = Instantiate(optionButtonPrefab, optionsContainer);
        
        // SkillManager'dan skill açıklamasını al
        string buttonText = skillManager != null ? skillManager.GetSkillDescription(skillType) : skillType.ToString();
        
        // Try to get TextMeshProUGUI first, then regular Text
        TextMeshProUGUI buttonTextTMP = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonTextTMP != null)
        {
            buttonTextTMP.text = buttonText;
        }
        else
        {
            Text buttonTextRegular = button.GetComponentInChildren<Text>();
            if (buttonTextRegular != null)
            {
                buttonTextRegular.text = buttonText;
            }
            else
            {
                Debug.LogWarning("No Text or TextMeshProUGUI component found on button prefab child.");
            }
        }

        button.onClick.AddListener(() => SelectOption(skillType));
    }

    void SelectOption(SkillType skillType)
    {
        skillManager?.ApplySkillEffect(skillType);
        HideLevelUpOptions();
    }

    public void HideLevelUpOptions()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
        Time.timeScale = 1f; // Resume the game
    }
}
