using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FlagManager : MonoBehaviour
{
    public static FlagManager Instance { get; private set; }

    [System.Serializable]
    public class NamedFlag
    {
        [Header("Flag Settings")]
        public string flagName;
        [HideInInspector] public bool isActive;
        [HideInInspector] public int count;
        public int threshold = 5;
        
        [Header("Events")]
        [Tooltip("Event triggered when the flag becomes active")]
        public UnityEvent onFlagActivated;
        
        [Tooltip("Event triggered each time count increases (optional)")]
        public UnityEvent onCountIncreased;
    }

    [SerializeField] private List<NamedFlag> flags = new List<NamedFlag>();
    private Dictionary<string, NamedFlag> flagDict = new Dictionary<string, NamedFlag>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFlagDictionary();
        }
    }

    private void InitializeFlagDictionary()
    {
        foreach (var namedFlag in flags)
        {
            if (!flagDict.ContainsKey(namedFlag.flagName))
            {
                flagDict.Add(namedFlag.flagName, namedFlag);
            }
        }
    }

    public void AddToFlag(string flagName)
    {
        if (flagDict.TryGetValue(flagName, out NamedFlag flag))
        {
            flag.count++;
            
            // Invoke count increased event if it has listeners
            flag.onCountIncreased?.Invoke();
            
            // Check if flag should be activated
            if (!flag.isActive && flag.count >= flag.threshold)
            {
                flag.isActive = true;
                Debug.Log($"Flag '{flagName}' activated at count {flag.count}.");
                
                // Invoke activation event
                flag.onFlagActivated?.Invoke();
            }
        }
        else
        {
            Debug.LogWarning($"Flag '{flagName}' not found.");
        }
    }

    public bool IsFlagActive(string flagName)
    {
        if (flagDict.TryGetValue(flagName, out NamedFlag flag))
        {
            return flag.isActive;
        }
        return false;
    }

    public int GetFlagCount(string flagName)
    {
        if (flagDict.TryGetValue(flagName, out NamedFlag flag))
        {
            return flag.count;
        }
        return 0;
    }

    public void ResetFlag(string flagName)
    {
        if (flagDict.TryGetValue(flagName, out NamedFlag flag))
        {
            flag.count = 0;
            flag.isActive = false;
            Debug.Log($"Flag '{flagName}' reset.");
        }
        else
        {
            Debug.LogWarning($"Flag '{flagName}' not found.");
        }
    }
}