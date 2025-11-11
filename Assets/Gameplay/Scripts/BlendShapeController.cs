using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Controls blend shape weights with smooth transitions and organized sets
/// </summary>
public class BlendShapeController : MonoBehaviour
{
    [Header("Blend Shape Sets")]
    [SerializeField] private List<BlendShapeSet> blendShapeSets = new List<BlendShapeSet>();
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    
    // Public properties for runtime access
    public IReadOnlyList<BlendShapeSet> BlendShapeSets => blendShapeSets;
    public int SetCount => blendShapeSets?.Count ?? 0;

    #region Unity Lifecycle
    
    private void Awake()
    {
        ValidateConfiguration();
        InitializeAllSets();
    }
    
    private void Update()
    {
        UpdateAllSets();
        
        if (showDebugInfo)
            LogDebugInfo();
    }
    
    #endregion

    #region Public API
    
    /// <summary>
    /// Adds weight to a specific blend shape set
    /// </summary>
    /// <param name="setName">Name of the set to modify</param>
    /// <param name="customAmount">Optional custom amount to add (uses set default if null)</param>
    public void AddWeight(string setName, float? customAmount = null)
    {
        var set = GetSet(setName);
        if (set != null)
        {
            set.AddWeight(customAmount);
        }
        else
        {
            LogWarning($"BlendShape set '{setName}' not found");
        }
    }
    
    /// <summary>
    /// Sets the target weight for a blend shape set
    /// </summary>
    public void SetTargetWeight(string setName, float weight)
    {
        var set = GetSet(setName);
        if (set != null)
        {
            set.SetTargetWeight(weight);
        }
        else
        {
            LogWarning($"BlendShape set '{setName}' not found");
        }
    }
    
    /// <summary>
    /// Starts decay for a specific blend shape set
    /// </summary>
    public void StartDecay(string setName)
    {
        var set = GetSet(setName);
        if (set != null)
        {
            set.StartDecay();
        }
        else
        {
            LogWarning($"BlendShape set '{setName}' not found");
        }
    }
    
    /// <summary>
    /// Resets a blend shape set to its default weight
    /// </summary>
    public void ResetSet(string setName)
    {
        var set = GetSet(setName);
        if (set != null)
        {
            set.Reset();
        }
        else
        {
            LogWarning($"BlendShape set '{setName}' not found");
        }
    }
    
    /// <summary>
    /// Gets current weight of a blend shape set
    /// </summary>
    public float GetCurrentWeight(string setName)
    {
        var set = GetSet(setName);
        return set?.CurrentWeight ?? 0f;
    }
    
    /// <summary>
    /// Gets all available set names
    /// </summary>
    public string[] GetSetNames()
    {
        return blendShapeSets?.Select(s => s.setName).ToArray() ?? new string[0];
    }
    
    #endregion
    
    #region Unity Event Methods (Single Parameter)
    
    /// <summary>
    /// Unity Event friendly method - Adds default weight amount to a blend shape set
    /// Perfect for UI buttons, triggers, and other Unity Events
    /// </summary>
    [System.ComponentModel.Description("Adds the default weight amount to the specified blend shape set")]
    public void TriggerAddWeight(string setName)
    {
        AddWeight(setName);
    }
    
    /// <summary>
    /// Unity Event friendly method - Starts decay for a blend shape set
    /// Perfect for UI buttons, triggers, and other Unity Events
    /// </summary>
    [System.ComponentModel.Description("Starts decay animation for the specified blend shape set")]
    public void TriggerDecay(string setName)
    {
        var set = GetSet(setName);
        if (set != null)
        {
            StartCoroutine(DelayedDecay(setName, set.decayDelay));
        }
        else
        {
            LogWarning($"BlendShape set '{setName}' not found");
        }
    }
    
    /// <summary>
    /// Unity Event friendly method - Resets a blend shape set to default weight
    /// Perfect for UI buttons, triggers, and other Unity Events
    /// </summary>
    [System.ComponentModel.Description("Resets the specified blend shape set to its default weight")]
    public void TriggerReset(string setName)
    {
        ResetSet(setName);
    }
    
    /// <summary>
    /// Unity Event friendly method - Sets blend shape to 25% weight
    /// Perfect for UI buttons, triggers, and other Unity Events
    /// </summary>
    [System.ComponentModel.Description("Sets the blend shape set to 25% weight")]
    public void TriggerLowWeight(string setName)
    {
        var set = GetSet(setName);
        if (set != null)
        {
            set.SetTargetWeight(25f);
            StartCoroutine(DelayedDecay(setName, set.decayDelay));
        }
        else
        {
            LogWarning($"BlendShape set '{setName}' not found");
        }
    }
    
    /// <summary>
    /// Unity Event friendly method - Sets blend shape to 50% weight
    /// Perfect for UI buttons, triggers, and other Unity Events
    /// </summary>
    [System.ComponentModel.Description("Sets the blend shape set to 50% weight")]
    public void TriggerMediumWeight(string setName)
    {
        var set = GetSet(setName);
        if (set != null)
        {
            set.SetTargetWeight(50f);
            StartCoroutine(DelayedDecay(setName, set.decayDelay));
        }
        else
        {
            LogWarning($"BlendShape set '{setName}' not found");
        }
    }
    
    /// <summary>
    /// Unity Event friendly method - Sets blend shape to 75% weight
    /// Perfect for UI buttons, triggers, and other Unity Events
    /// </summary>
    [System.ComponentModel.Description("Sets the blend shape set to 75% weight")]
    public void TriggerHighWeight(string setName)
    {
        var set = GetSet(setName);
        if (set != null)
        {
            set.SetTargetWeight(75f);
            StartCoroutine(DelayedDecay(setName, set.decayDelay));
        }
        else
        {
            LogWarning($"BlendShape set '{setName}' not found");
        }
    }
    
    /// <summary>
    /// Unity Event friendly method - Sets blend shape to maximum weight
    /// Perfect for UI buttons, triggers, and other Unity Events
    /// </summary>
    [System.ComponentModel.Description("Sets the blend shape set to its maximum weight")]
    public void TriggerMaxWeight(string setName)
    {
        var set = GetSet(setName);
        if (set != null)
        {
            set.SetTargetWeight(set.maxWeight);
            StartCoroutine(DelayedDecay(setName, set.decayDelay));
        }
        else
        {
            LogWarning($"BlendShape set '{setName}' not found");
        }
    }
    
    /// <summary>
    /// Unity Event friendly method - Toggles blend shape between default and maximum weight
    /// Perfect for UI buttons, triggers, and other Unity Events
    /// </summary>
    [System.ComponentModel.Description("Toggles the blend shape set between default and maximum weight")]
    public void TriggerToggle(string setName)
    {
        var set = GetSet(setName);
        if (set != null)
        {
            // If close to default, go to max. If close to max or anything else, go to default
            bool isAtDefault = Mathf.Approximately(set.CurrentWeight, set.defaultWeight);
            set.SetTargetWeight(isAtDefault ? set.maxWeight : set.defaultWeight);
            
            // Only apply decay delay when activating (going to max weight)
            if (isAtDefault)
            {
                StartCoroutine(DelayedDecay(setName, set.decayDelay));
            }
        }
        else
        {
            LogWarning($"BlendShape set '{setName}' not found");
        }
    }
    
    /// <summary>
    /// Unity Event friendly method - Pulses the blend shape (quick max then decay)
    /// Perfect for UI buttons, triggers, and other Unity Events
    /// </summary>
    [System.ComponentModel.Description("Pulses the blend shape set (quick activation then decay)")]
    public void TriggerPulse(string setName)
    {
        var set = GetSet(setName);
        if (set != null)
        {
            set.SetTargetWeight(set.maxWeight);
            StartCoroutine(DelayedDecay(setName, set.decayDelay));
        }
        else
        {
            LogWarning($"BlendShape set '{setName}' not found");
        }
    }
    
    /// <summary>
    /// Unity Event friendly method - Activates blend shape and auto-decays after delay
    /// Perfect for UI buttons, triggers, and other Unity Events
    /// </summary>
    [System.ComponentModel.Description("Activates the blend shape set and starts auto-decay")]
    public void TriggerActivateAndDecay(string setName)
    {
        var set = GetSet(setName);
        if (set != null)
        {
            set.AddWeight();
            StartCoroutine(DelayedDecay(setName, set.decayDelay));
        }
        else
        {
            LogWarning($"BlendShape set '{setName}' not found");
        }
    }
    
    #endregion

    #region Private Methods
    
    private void ValidateConfiguration()
    {
        if (blendShapeSets == null || blendShapeSets.Count == 0)
        {
            LogWarning("No blend shape sets configured");
            return;
        }
        
        // Check for duplicate names
        var duplicates = blendShapeSets
            .GroupBy(s => s.setName)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);
            
        foreach (var duplicate in duplicates)
        {
            LogError($"Duplicate blend shape set name found: '{duplicate}'");
        }
        
        // Validate each set
        foreach (var set in blendShapeSets)
        {
            set.Validate();
        }
    }
    
    private void InitializeAllSets()
    {
        foreach (var set in blendShapeSets)
        {
            set?.Initialize();
        }
    }
    
    private void UpdateAllSets()
    {
        foreach (var set in blendShapeSets)
        {
            set?.UpdateWeights();
        }
    }
    
    private BlendShapeSet GetSet(string setName)
    {
        if (string.IsNullOrEmpty(setName)) return null;
        
        return blendShapeSets?.Find(s => 
            string.Equals(s.setName, setName, System.StringComparison.OrdinalIgnoreCase));
    }
    
    private void LogDebugInfo()
    {
        foreach (var set in blendShapeSets)
        {
            if (set != null && !Mathf.Approximately(set.CurrentWeight, set.DefaultWeight))
            {
                Debug.Log($"[BlendShape] {set.setName}: {set.CurrentWeight:F1} -> {set.TargetWeight:F1}");
            }
        }
    }
    
    private System.Collections.IEnumerator DelayedDecay(string setName, float delay)
    {
        yield return new WaitForSeconds(delay);
        StartDecay(setName);
    }
    
    private void LogWarning(string message) => Debug.LogWarning($"[BlendShapeController] {message}", this);
    private void LogError(string message) => Debug.LogError($"[BlendShapeController] {message}", this);
    
    #endregion
}

[System.Serializable]
public class BlendShapeSet
{
    [System.Serializable]
    public class RendererData
    {
        [Header("Renderer Configuration")]
        public SkinnedMeshRenderer renderer;
        
        [Header("Blend Shape Selection")]
        [Tooltip("Indices of blend shapes to control (leave empty to auto-populate in editor)")]
        public int[] blendShapeIndices;
        
        [Header("Blend Shape Names (Read-only)")]
        [SerializeField, TextArea(2, 4)] 
        private string blendShapeNames = "Click 'Refresh Names' to populate";
        
        public string BlendShapeNames => blendShapeNames;
        
        public bool IsValid => renderer != null && renderer.sharedMesh != null && 
                              blendShapeIndices != null && blendShapeIndices.Length > 0;
        
        public void RefreshBlendShapeNames()
        {
            if (!renderer || !renderer.sharedMesh) 
            {
                blendShapeNames = "No renderer or mesh assigned";
                return;
            }
            
            if (blendShapeIndices == null || blendShapeIndices.Length == 0)
            {
                blendShapeNames = "No blend shape indices assigned";
                return;
            }
            
            var names = new List<string>();
            var mesh = renderer.sharedMesh;
            
            foreach (int index in blendShapeIndices)
            {
                if (index >= 0 && index < mesh.blendShapeCount)
                {
                    names.Add($"{index}: {mesh.GetBlendShapeName(index)}");
                }
                else
                {
                    names.Add($"{index}: INVALID INDEX");
                }
            }
            
            blendShapeNames = string.Join("\n", names);
        }
        
        public void AutoPopulateIndices()
        {
            if (!renderer || !renderer.sharedMesh) return;
            
            var mesh = renderer.sharedMesh;
            var indices = new int[mesh.blendShapeCount];
            for (int i = 0; i < mesh.blendShapeCount; i++)
            {
                indices[i] = i;
            }
            blendShapeIndices = indices;
            RefreshBlendShapeNames();
        }
        
        public bool ValidateIndices()
        {
            if (!renderer || !renderer.sharedMesh || blendShapeIndices == null) 
                return false;
            
            var mesh = renderer.sharedMesh;
            foreach (int index in blendShapeIndices)
            {
                if (index < 0 || index >= mesh.blendShapeCount)
                {
                    Debug.LogError($"Invalid blend shape index {index} on {renderer.name}. " +
                                 $"Valid range: 0-{mesh.blendShapeCount - 1}");
                    return false;
                }
            }
            return true;
        }
    }

    [Header("Set Configuration")]
    [Tooltip("Unique identifier for this blend shape set")]
    public string setName = "New Set";
    
    [Space]
    [Tooltip("Renderers and their associated blend shapes to control")]
    public List<RendererData> rendererData = new List<RendererData>();
    
    [Header("Weight Configuration")]
    [Range(0, 100), Tooltip("Maximum weight this set can reach")]
    public float maxWeight = 100f;
    
    [Range(0, 100), Tooltip("Amount to add when AddWeight() is called")]
    public float addAmount = 25f;
    
    [Range(0, 100), Tooltip("Default/resting weight for this set")]
    public float defaultWeight = 0f;
    
    [Header("Animation")]
    [Tooltip("If true, weights smoothly interpolate to target values. If false, weights snap instantly.")]
    public bool useInterpolation = true;
    
    [Tooltip("Speed at which weights interpolate to target values (only used when Use Interpolation is enabled)")]
    public float interpolationSpeed = 5f;
    
    [Tooltip("If true, weights will automatically decay to default over time")]
    public bool autoDecay = false;
    
    [Tooltip("Time in seconds before auto-decay begins")]
    public float decayDelay = 1f;
    
    [Header("Runtime Info (Read-only)")]
    [SerializeField, Range(0, 100)] 
    private float currentWeight;
    
    [SerializeField, Range(0, 100)]
    private float targetWeight;
    
    private float lastUpdateTime;
    private bool isDecaying;
    
    // Public properties
    public float CurrentWeight => currentWeight;
    public float TargetWeight => targetWeight;
    public float DefaultWeight => defaultWeight;
    public bool IsAnimating => !Mathf.Approximately(currentWeight, targetWeight);

    public void Initialize()
    {
        currentWeight = defaultWeight;
        targetWeight = defaultWeight;
        lastUpdateTime = Time.time;
        isDecaying = false;
        ApplyWeights();
    }

    public void UpdateWeights()
    {
        // Handle auto-decay
        if (autoDecay && !isDecaying && !Mathf.Approximately(currentWeight, defaultWeight))
        {
            if (Time.time - lastUpdateTime >= decayDelay)
            {
                StartDecay();
            }
        }
        
        // Interpolate or snap to target
        if (!Mathf.Approximately(currentWeight, targetWeight))
        {
            if (useInterpolation)
            {
                // Smooth interpolation
                currentWeight = Mathf.MoveTowards(currentWeight, targetWeight, 
                               interpolationSpeed * Time.deltaTime);
            }
            else
            {
                // Instant snap
                currentWeight = targetWeight;
            }
            ApplyWeights();
        }
    }

    public void AddWeight(float? customAmount = null)
    {
        float amount = customAmount ?? addAmount;
        currentWeight = Mathf.Clamp(currentWeight + amount, 0f, maxWeight);
        targetWeight = currentWeight;
        lastUpdateTime = Time.time;
        isDecaying = false;
        ApplyWeights();
    }
    
    public void SetTargetWeight(float weight)
    {
        targetWeight = Mathf.Clamp(weight, 0f, maxWeight);
        lastUpdateTime = Time.time;
        isDecaying = targetWeight <= defaultWeight;
    }

    public void StartDecay()
    {
        targetWeight = defaultWeight;
        isDecaying = true;
    }
    
    public void Reset()
    {
        currentWeight = defaultWeight;
        targetWeight = defaultWeight;
        lastUpdateTime = Time.time;
        isDecaying = false;
        ApplyWeights();
    }

    public void Validate()
    {
        if (string.IsNullOrEmpty(setName))
        {
            Debug.LogError("BlendShape set has no name assigned");
        }
        
        if (rendererData == null || rendererData.Count == 0)
        {
            Debug.LogWarning($"BlendShape set '{setName}' has no renderer data");
            return;
        }
        
        foreach (var data in rendererData)
        {
            if (data?.renderer == null)
            {
                Debug.LogError($"BlendShape set '{setName}' has null renderer reference");
                continue;
            }
            
            if (!data.ValidateIndices())
            {
                Debug.LogError($"BlendShape set '{setName}' has invalid indices on renderer '{data.renderer.name}'");
            }
        }
    }

    private void ApplyWeights()
    {
        foreach (var data in rendererData)
        {
            if (!data?.IsValid ?? true) continue;

            foreach (int index in data.blendShapeIndices)
            {
                data.renderer.SetBlendShapeWeight(index, currentWeight);
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(BlendShapeController))]
public class BlendShapeControllerEditor : Editor
{
    private SerializedProperty blendShapeSets;
    private SerializedProperty showDebugInfo;
    
    private void OnEnable()
    {
        blendShapeSets = serializedObject.FindProperty("blendShapeSets");
        showDebugInfo = serializedObject.FindProperty("showDebugInfo");
    }
    
    public override void OnInspectorGUI()
    {
        var controller = (BlendShapeController)target;
        serializedObject.Update();
        
        // Header section
        EditorGUILayout.PropertyField(showDebugInfo);
        EditorGUILayout.Space(5);
        
        // Blend Shape Sets
        EditorGUILayout.LabelField("Blend Shape Sets", EditorStyles.boldLabel);
        
        if (blendShapeSets.arraySize == 0)
        {
            EditorGUILayout.HelpBox("No blend shape sets configured. Add sets to control blend shapes.", MessageType.Info);
        }
        
        // Draw each set with custom layout
        for (int i = 0; i < blendShapeSets.arraySize; i++)
        {
            DrawBlendShapeSet(blendShapeSets.GetArrayElementAtIndex(i), i);
        }
        
        // Add/Remove buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add New Set"))
        {
            blendShapeSets.arraySize++;
            var newSet = blendShapeSets.GetArrayElementAtIndex(blendShapeSets.arraySize - 1);
            newSet.FindPropertyRelative("setName").stringValue = $"Set {blendShapeSets.arraySize}";
            newSet.FindPropertyRelative("maxWeight").floatValue = 100f;
            newSet.FindPropertyRelative("addAmount").floatValue = 25f;
            newSet.FindPropertyRelative("defaultWeight").floatValue = 0f;
            newSet.FindPropertyRelative("useInterpolation").boolValue = true;
            newSet.FindPropertyRelative("interpolationSpeed").floatValue = 5f;
        }
        
        EditorGUI.BeginDisabledGroup(blendShapeSets.arraySize == 0);
        if (GUILayout.Button("Remove Last Set"))
        {
            blendShapeSets.arraySize--;
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // Editor Tools
        EditorGUILayout.LabelField("Editor Tools", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Validate All Sets"))
        {
            foreach (var set in controller.BlendShapeSets)
            {
                set?.Validate();
            }
        }
        
        EditorGUILayout.Space(5);
        
        // Runtime controls (only show during play mode)
        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);
            
            foreach (var set in controller.BlendShapeSets)
            {
                if (set == null) continue;
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(set.setName, GUILayout.Width(100));
                EditorGUILayout.LabelField($"{set.CurrentWeight:F1}", GUILayout.Width(40));
                
                if (GUILayout.Button("Add", GUILayout.Width(50)))
                {
                    controller.AddWeight(set.setName);
                }
                
                if (GUILayout.Button("Decay", GUILayout.Width(50)))
                {
                    controller.StartDecay(set.setName);
                }
                
                if (GUILayout.Button("Reset", GUILayout.Width(50)))
                {
                    controller.ResetSet(set.setName);
                }
                
                EditorGUILayout.EndHorizontal();
            }
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void DrawBlendShapeSet(SerializedProperty setProperty, int index)
    {
        var setName = setProperty.FindPropertyRelative("setName");
        var rendererData = setProperty.FindPropertyRelative("rendererData");
        var maxWeight = setProperty.FindPropertyRelative("maxWeight");
        var addAmount = setProperty.FindPropertyRelative("addAmount");
        var defaultWeight = setProperty.FindPropertyRelative("defaultWeight");
        var useInterpolation = setProperty.FindPropertyRelative("useInterpolation");
        var interpolationSpeed = setProperty.FindPropertyRelative("interpolationSpeed");
        var autoDecay = setProperty.FindPropertyRelative("autoDecay");
        var decayDelay = setProperty.FindPropertyRelative("decayDelay");
        var currentWeight = setProperty.FindPropertyRelative("currentWeight");
        var targetWeight = setProperty.FindPropertyRelative("targetWeight");
        
        // Foldout header
        var headerRect = EditorGUILayout.GetControlRect();
        var foldoutRect = new Rect(headerRect.x, headerRect.y, headerRect.width - 60, headerRect.height);
        var deleteRect = new Rect(headerRect.x + headerRect.width - 55, headerRect.y, 55, headerRect.height);
        
        setProperty.isExpanded = EditorGUI.Foldout(foldoutRect, setProperty.isExpanded, 
            $"Set {index + 1}: {setName.stringValue}", true);
        
        if (GUI.Button(deleteRect, "Delete"))
        {
            blendShapeSets.DeleteArrayElementAtIndex(index);
            return;
        }
        
        if (!setProperty.isExpanded) return;
        
        EditorGUI.indentLevel++;
        
        // Basic configuration
        EditorGUILayout.PropertyField(setName, new GUIContent("Set Name"));
        
        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Weight Configuration", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(maxWeight);
        EditorGUILayout.PropertyField(addAmount);
        EditorGUILayout.PropertyField(defaultWeight);
        
        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(useInterpolation);
        
        // Only show interpolation speed if interpolation is enabled
        var useInterpolationProp = setProperty.FindPropertyRelative("useInterpolation");
        if (useInterpolationProp.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(interpolationSpeed);
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.PropertyField(autoDecay);
        if (autoDecay.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(decayDelay);
            EditorGUI.indentLevel--;
        }
        
        // Runtime info (read-only during play mode)
        if (Application.isPlaying)
        {
            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField("Runtime Info", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(currentWeight);
            EditorGUILayout.PropertyField(targetWeight);
            EditorGUI.EndDisabledGroup();
        }
        
        EditorGUILayout.Space(5);
        
        // Renderer Data section
        EditorGUILayout.LabelField("Renderer Data", EditorStyles.boldLabel);
        
        if (rendererData.arraySize == 0)
        {
            EditorGUILayout.HelpBox("No renderers assigned. Add renderers to control their blend shapes.", MessageType.Warning);
        }
        
        // Draw each renderer data entry
        for (int j = 0; j < rendererData.arraySize; j++)
        {
            DrawRendererData(rendererData.GetArrayElementAtIndex(j), j);
        }
        
        // Renderer data buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Renderer"))
        {
            rendererData.arraySize++;
        }
        
        EditorGUI.BeginDisabledGroup(rendererData.arraySize == 0);
        if (GUILayout.Button("Remove Last"))
        {
            rendererData.arraySize--;
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
        
        EditorGUI.indentLevel--;
        EditorGUILayout.Space(10);
    }
    
    private void DrawRendererData(SerializedProperty rendererDataProperty, int index)
    {
        var renderer = rendererDataProperty.FindPropertyRelative("renderer");
        var indices = rendererDataProperty.FindPropertyRelative("blendShapeIndices");
        var names = rendererDataProperty.FindPropertyRelative("blendShapeNames");
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // Header with delete button
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Renderer {index + 1}", EditorStyles.boldLabel);
        if (GUILayout.Button("×", GUILayout.Width(20), GUILayout.Height(16)))
        {
            var rendererDataArray = rendererDataProperty.serializedObject.FindProperty(rendererDataProperty.propertyPath.Replace($".Array.data[{index}]", ""));
            rendererDataArray.DeleteArrayElementAtIndex(index);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }
        EditorGUILayout.EndHorizontal();
        
        // Renderer field
        EditorGUILayout.PropertyField(renderer, new GUIContent("Skinned Mesh Renderer"));
        
        var rendererComponent = renderer.objectReferenceValue as SkinnedMeshRenderer;
        bool hasValidRenderer = rendererComponent != null && rendererComponent.sharedMesh != null;
        
        if (!hasValidRenderer)
        {
            EditorGUILayout.HelpBox("Assign a SkinnedMeshRenderer with a mesh to continue.", MessageType.Warning);
        }
        else
        {
            var mesh = rendererComponent.sharedMesh;
            
            // Blend shape info
            EditorGUILayout.LabelField($"Available Blend Shapes: {mesh.blendShapeCount}");
            
            // Indices array with utility buttons
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(indices, new GUIContent("Blend Shape Indices"), true);
            EditorGUILayout.EndHorizontal();
            
            // Utility buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Auto-Populate All"))
            {
                indices.arraySize = mesh.blendShapeCount;
                for (int i = 0; i < mesh.blendShapeCount; i++)
                {
                    indices.GetArrayElementAtIndex(i).intValue = i;
                }
                RefreshBlendShapeNames(rendererComponent, indices, names);
            }
            
            if (GUILayout.Button("Clear All"))
            {
                indices.arraySize = 0;
                names.stringValue = "No indices assigned";
            }
            
            if (GUILayout.Button("Refresh Names"))
            {
                RefreshBlendShapeNames(rendererComponent, indices, names);
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Show blend shape names
            if (!string.IsNullOrEmpty(names.stringValue))
            {
                var style = new GUIStyle(EditorStyles.textArea)
                {
                    wordWrap = true,
                    fontSize = 10
                };
                
                EditorGUILayout.LabelField("Blend Shape Names:", EditorStyles.miniLabel);
                EditorGUILayout.TextArea(names.stringValue, style, GUILayout.MinHeight(40));
            }
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(3);
    }
    
    private void RefreshBlendShapeNames(SkinnedMeshRenderer renderer, SerializedProperty indices, SerializedProperty names)
    {
        if (renderer == null || renderer.sharedMesh == null)
        {
            names.stringValue = "No renderer or mesh assigned";
            return;
        }
        
        if (indices.arraySize == 0)
        {
            names.stringValue = "No blend shape indices assigned";
            return;
        }
        
        var namesList = new List<string>();
        var mesh = renderer.sharedMesh;
        
        for (int i = 0; i < indices.arraySize; i++)
        {
            int index = indices.GetArrayElementAtIndex(i).intValue;
            if (index >= 0 && index < mesh.blendShapeCount)
            {
                namesList.Add($"{index}: {mesh.GetBlendShapeName(index)}");
            }
            else
            {
                namesList.Add($"{index}: INVALID INDEX");
            }
        }
        
        names.stringValue = string.Join("\n", namesList);
    }
}
#endif