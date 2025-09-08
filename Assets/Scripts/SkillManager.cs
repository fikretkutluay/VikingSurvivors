using UnityEngine;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour
{
    [Header("References (Auto-found if null)")]
    [SerializeField] private AttackSystem attackSystem;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerHealth playerHealth;
    
    [Header("Magnet Skill Settings")]
    [SerializeField] private MagnetAttractor magnetAttractor;
    
    [Header("Crow Shield Settings")]
    [SerializeField] private CrowShieldController crowShieldController; // lives on player; tuning is on the component


    
    [Header("Skill Levels (Debug)")]
    [SerializeField] private string skillLevelsDisplay = ""; // Debug için
    [SerializeField] private bool debugAlwaysShowMagnet = false; // TEST: Magnet'i her zaman göster

    private Dictionary<SkillType, int> skillLevels = new Dictionary<SkillType, int>();
    private bool magnetSkillUnlocked = false;
    
    // All selectable skills live here; UI will query eligibility from this manager
    private static readonly SkillType[] selectableSkills = new SkillType[]
    {
        SkillType.FireballPower,
        SkillType.FireballSpeed,
        SkillType.FireballCount,
        SkillType.SwordPower,
        SkillType.SwordSpeed,
        SkillType.MoveSpeed,
        SkillType.CrowShield
    };


    void Awake()
    {
        if (attackSystem == null) attackSystem = FindObjectOfType<AttackSystem>();
        if (playerMovement == null) playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerHealth == null) playerHealth = FindObjectOfType<PlayerHealth>();
        if (magnetAttractor == null && playerMovement != null)
        {
            magnetAttractor = playerMovement.GetComponent<MagnetAttractor>();
            if (magnetAttractor == null)
            {
                magnetAttractor = playerMovement.gameObject.AddComponent<MagnetAttractor>();
                magnetAttractor.SetActive(false);
            }
        }
        
        // Initialize all skill levels to 0
        foreach (SkillType skillType in System.Enum.GetValues(typeof(SkillType)))
        {
            skillLevels[skillType] = 0;
        }
        
        // CrowShield referansını (inactive child olsa bile) bul
        if (crowShieldController == null && playerMovement != null)
        {
            crowShieldController = playerMovement.GetComponentInChildren<CrowShieldController>(true);
            if (crowShieldController == null)
            {
                crowShieldController = FindObjectOfType<CrowShieldController>(true);
            }
        }
        
        // Magnet skill'i başlangıçta kapalı
        magnetSkillUnlocked = false;
    }
    
    void Update() { }
    


    public void ApplySkillEffect(SkillType skillType)
    {
        // Increase skill level
        if (skillLevels.ContainsKey(skillType))
        {
            skillLevels[skillType]++;
        }
        else
        {
            skillLevels[skillType] = 1;
        }

        // Apply the skill effect based on current level
        switch (skillType)
        {
            // Fireball Skills
            case SkillType.FireballPower:
                attackSystem?.IncreaseFireballDamage((int)GetSkillBonus(skillType));
                break;
            case SkillType.FireballSpeed:
                attackSystem?.IncreaseFireballSpeed(GetSkillBonus(skillType));
                break;
            case SkillType.FireballCount:
                attackSystem?.IncreaseFireballCount((int)GetSkillBonus(skillType));
                break;
                
            // Sword Skills
            case SkillType.SwordPower:
                attackSystem?.IncreaseSwordDamage((int)GetSkillBonus(skillType));
                break;
            case SkillType.SwordSpeed:
                attackSystem?.IncreaseSwordSpeed(GetSkillBonus(skillType));
                break;
                
            // General Skills
            case SkillType.MoveSpeed:
                playerMovement?.IncreaseMoveSpeed(GetSkillBonus(skillType));
                break;
            case SkillType.CrowShield:
                ApplyCrowShieldUpgrade();
                break;
            default:
                Debug.LogWarning($"Unknown skill type: {skillType}");
                break;
        }
        
        // Update debug display
        UpdateSkillLevelsDisplay();
        
        Debug.Log($"Applied {skillType} - Level {skillLevels[skillType]}");
    }
    
    private void UpdateSkillLevelsDisplay()
    {
        skillLevelsDisplay = "";
        foreach (var skill in skillLevels)
        {
            skillLevelsDisplay += $"{skill.Key}: Lv.{skill.Value}\n";
        }
    }

    private float GetSkillBonus(SkillType skillType)
    {
        int level = skillLevels.ContainsKey(skillType) ? skillLevels[skillType] : 0;
        
        // Her level için artan bonus
        switch (skillType)
        {
            case SkillType.FireballPower:
                return 15f + (level * 10f); // Base 15 + 10 per level (15, 25, 35, 45...)
                
            case SkillType.SwordPower:
                return 10f + (level * 7.5f); // Base 10 + 7.5 per level (10, 17.5, 25, 32.5, 40...)
                
            case SkillType.FireballSpeed:
                return 0.35f; // Her level -0.35 saniye azalt
                
            case SkillType.SwordSpeed:
                return 0.25f; // Her level -0.25 saniye azalt
                
            case SkillType.FireballCount:
                // İstenen dizilim: 1 -> 2 -> 4 ve biter
                // IncreaseFireballCount(base + amount) kullanıyor, base=1 olduğundan
                // level 1'de amount=1 (1+1=2), level 2'de amount=3 (1+3=4)
                if (level <= 1) return 1f; // Lv1: 2 alev topu
                return 3f;                  // Lv2: 4 alev topu
                
            case SkillType.MoveSpeed:
                return 0.5f + (level * 0.2f); // Base 0.5 + 0.2 per level
                
            default:
                return 1f;
        }
    }

    public int GetSkillLevel(SkillType skillType)
    {
        return skillLevels.ContainsKey(skillType) ? skillLevels[skillType] : 0;
    }

    public string GetSkillDescription(SkillType skillType)
    {
        int level = GetSkillLevel(skillType);
        string baseDesc = GetSkillBaseDescription(skillType);
        return $"{baseDesc} Lv.{level + 1}";
    }
    


    private string GetSkillBaseDescription(SkillType skillType)
    {
        switch (skillType)
        {
            case SkillType.FireballPower: return "Fireball Power";
            case SkillType.FireballSpeed: return "Fireball Speed";
            case SkillType.FireballCount: return "Fireball Count";
            case SkillType.SwordPower: return "Sword Power";
            case SkillType.SwordSpeed: return "Sword Speed";
            case SkillType.MoveSpeed: return "Move Speed";
            case SkillType.CrowShield: return "Crow Shield";
            default: return "Unknown Skill";
        }
    }
    
    // --- Selection API for LevelUpUI ---
    public System.Collections.Generic.List<SkillType> GetEligibleSkills(int currentPlayerLevel)
    {
        var eligible = new System.Collections.Generic.List<SkillType>();
        foreach (var skill in selectableSkills)
        {
            if (IsSkillEligible(skill, currentPlayerLevel))
            {
                eligible.Add(skill);
            }
        }
        return eligible;
    }
    
    public bool IsSkillEligible(SkillType skillType, int currentPlayerLevel)
    {
        // Fireball skills unlock at level 4
        if (skillType == SkillType.FireballPower || skillType == SkillType.FireballSpeed || skillType == SkillType.FireballCount)
        {
            if (currentPlayerLevel < 4)
            {
                return false; // Fireball skills level 4'ten önce çıkmaz
            }
        }
        
        // Fireball Speed maksimum 3 level
        if (skillType == SkillType.FireballSpeed)
        {
            int currentLevel = skillLevels.ContainsKey(skillType) ? skillLevels[skillType] : 0;
            return currentLevel < 3;
        }
        
        // Fireball Count maksimum 2 level (sonda 4 alev topu)
        if (skillType == SkillType.FireballCount)
        {
            int currentLevel = skillLevels.ContainsKey(skillType) ? skillLevels[skillType] : 0;
            return currentLevel < 2; // Level 2'den sonra çıkmasın
        }
        
        // Sword Speed maksimum 3 level
        if (skillType == SkillType.SwordSpeed)
        {
            int currentLevel = skillLevels.ContainsKey(skillType) ? skillLevels[skillType] : 0;
            return currentLevel < 3;
        }
        
        // Crow Shield unlock at level 7, maksimum 5 level
        if (skillType == SkillType.CrowShield)
        {
            if (currentPlayerLevel < 7)
            {
                return false; // Level 7'den önce çıkmaz
            }
            int currentLevel = skillLevels.ContainsKey(skillType) ? skillLevels[skillType] : 0;
            return currentLevel < 5; // Maksimum 5 level
        }
        
        return true;
    }
    
    // ---- Magnet Skill Methods ----
    private void UnlockMagnetSkill()
    {
        magnetSkillUnlocked = true;
        if (magnetAttractor == null && playerMovement != null)
        {
            magnetAttractor = playerMovement.GetComponent<MagnetAttractor>();
            if (magnetAttractor == null)
            {
                magnetAttractor = playerMovement.gameObject.AddComponent<MagnetAttractor>();
            }
        }
        if (magnetAttractor != null) magnetAttractor.SetActive(true);
        Debug.Log($"🎯 Magnet Skill unlocked and activated on player!");
    }
    
    public bool IsMagnetSkillUnlocked()
    {
        return magnetSkillUnlocked;
    }
    
    // ---- Crow Shield Skill Methods ----
    private void ApplyCrowShieldUpgrade()
    {
        if (crowShieldController == null && playerMovement != null)
        {
            crowShieldController = playerMovement.GetComponent<CrowShieldController>();
        }
        
        if (crowShieldController == null && playerMovement != null)
        {
            crowShieldController = playerMovement.GetComponentInChildren<CrowShieldController>(true);
            if (crowShieldController == null)
            {
                crowShieldController = FindObjectOfType<CrowShieldController>(true);
            }
        }
        
        if (crowShieldController != null)
        {
            int currentLevel = skillLevels.ContainsKey(SkillType.CrowShield) ? skillLevels[SkillType.CrowShield] : 0;
            
            // İlk level'da aktif et
            if (currentLevel == 1)
            {
                crowShieldController.ActivateShield();
            }
            
            // Level'a göre upgrade et
            crowShieldController.UpgradeShield(currentLevel);
        }
    }
}
