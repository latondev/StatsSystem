#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using GameSystems.Stats;

[CustomEditor(typeof(UnitStatController))]
public class UnitStatControllerEditor : Editor
{
    private UnitStatController controller;
    private GUIStyle titleStyle;
    private GUIStyle headerStyle;
    private GUIStyle statStyle;
    private GUIStyle currentStatStyle;
    private bool isInitialized;

    private bool showStats = true;
    private bool showVitals = true;
    private bool showCombat = true;
    private Vector2 statsScrollPos;
    private float lastRepaintTime;

    private void OnEnable()
    {
        controller = (UnitStatController)target;
        EditorApplication.update += UpdateInspector;
    }

    private void OnDisable()
    {
        EditorApplication.update -= UpdateInspector;
    }

    private void UpdateInspector()
    {
        if (Application.isPlaying)
        {
            float currentTime = (float)EditorApplication.timeSinceStartup;
            if (currentTime - lastRepaintTime >= 0.05f)
            {
                Repaint();
                lastRepaintTime = currentTime;
            }
        }
    }

    private void InitializeStyles()
    {
        if (isInitialized) return;

        titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(0.5f, 1f, 0.9f) }
        };

        headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 13,
            normal = { textColor = Color.white }
        };

        statStyle = new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(10, 10, 8, 8),
            margin = new RectOffset(0, 0, 3, 3)
        };

        currentStatStyle = new GUIStyle(statStyle)
        {
            fontStyle = FontStyle.Bold
        };

        isInitialized = true;
    }

    public override void OnInspectorGUI()
    {
        InitializeStyles();
        DrawDefaultInspector();

        if (controller == null) return;

        EditorGUILayout.Space(10);
        DrawSeparator();
        DrawUnitTitle();

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to use stat controls", MessageType.Info);
            return;
        }

        EditorGUILayout.Space(10);
        DrawSeparator();
        DrawQuickOverview();

        EditorGUILayout.Space(10);
        DrawSeparator();
        DrawCurrentStat();

        EditorGUILayout.Space(10);
        DrawSeparator();
        DrawNavigation();

        EditorGUILayout.Space(10);
        DrawSeparator();
        DrawQuickActions();

        EditorGUILayout.Space(10);
        DrawSeparator();
        DrawVitalStats();

        EditorGUILayout.Space(10);
        DrawSeparator();
        DrawCombatStats();

        EditorGUILayout.Space(10);
        DrawSeparator();
        DrawAllStats();
    }

    private void DrawUnitTitle()
    {
        Color previousBg = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.3f, 0.6f, 0.6f, 0.5f);

        EditorGUILayout.BeginVertical(statStyle);
        GUI.backgroundColor = previousBg;

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField($"⚔️ {controller.UnitName}", titleStyle);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        GUIStyle levelStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(1f, 0.9f, 0.3f) }
        };
        EditorGUILayout.LabelField($"Level {controller.Level}", levelStyle);

        EditorGUILayout.EndVertical();
    }

    private void DrawQuickOverview()
    {
        EditorGUILayout.LabelField("Quick Overview", headerStyle);

        Color previousBg = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.2f, 0.3f, 0.4f, 0.5f);
        EditorGUILayout.BeginVertical(statStyle);
        GUI.backgroundColor = previousBg;

        var hp = controller.Stats.GetStatById("hp");
        var mp = controller.Stats.GetStatById("mp");
        var stamina = controller.Stats.GetStatById("stamina");

        if (hp != null)
        {
            DrawMiniStatBar("❤️ HP", hp, hp.GetStatColor());
        }
        if (mp != null)
        {
            DrawMiniStatBar("💙 MP", mp, mp.GetStatColor());
        }
        if (stamina != null)
        {
            DrawMiniStatBar("💚 STA", stamina, stamina.GetStatColor());
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawMiniStatBar(string label, Stat stat, Color color)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(60));

        Rect barRect = EditorGUILayout.GetControlRect(false, 18);
        float percentage = stat.CurrentValue / stat.MaxValue;

        EditorGUI.DrawRect(barRect, new Color(0.2f, 0.2f, 0.2f));

        Rect fillRect = barRect;
        fillRect.width *= percentage;
        EditorGUI.DrawRect(fillRect, stat.GetStatColor());

        GUIStyle textStyle = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white },
            fontStyle = FontStyle.Bold,
            fontSize = 11
        };
        GUI.Label(barRect, $"{stat.CurrentValue:F0} / {stat.MaxValue:F0} ({percentage * 100:F0}%)", textStyle);
        
        // Đã FIX lỗi móp giao diện bằng cách thêm dòng này
        EditorGUILayout.EndHorizontal();
    }

    private void DrawCurrentStat()
    {
        EditorGUILayout.LabelField("Currently Selected", headerStyle);

        var stat = controller.CurrentStat;
        if (stat == null)
        {
            EditorGUILayout.HelpBox("No stat selected", MessageType.Info);
            return;
        }

        Color bgColor = stat.GetStatColor();
        bgColor.a = 0.3f;

        Color previousBg = GUI.backgroundColor;
        GUI.backgroundColor = bgColor;

        EditorGUILayout.BeginVertical(currentStatStyle);
        GUI.backgroundColor = previousBg;

        EditorGUILayout.BeginHorizontal();
        GUIStyle nameStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 13,
            normal = { textColor = stat.GetStatColor() }
        };
        EditorGUILayout.LabelField($"{stat.GetStatIcon()} {stat.StatName}", nameStyle);
        EditorGUILayout.EndHorizontal();

        if (stat.MaxValue > 0)
        {
            DrawStatBar(stat);
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Base:", GUILayout.Width(60));
            EditorGUILayout.LabelField(stat.BaseValue.ToString("F0"), EditorStyles.boldLabel);

            float finalValue = stat.GetFinalValue();
            EditorGUILayout.LabelField("→", GUILayout.Width(20));
            GUIStyle finalStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = new Color(0.5f, 1f, 0.5f) }
            };
            EditorGUILayout.LabelField(finalValue.ToString("F0"), finalStyle);
            EditorGUILayout.EndHorizontal();
        }

        if (stat.Modifiers.Count > 0)
        {
            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField($"  Modifiers: {stat.Modifiers.Count}", EditorStyles.miniLabel);
        }

        if (stat.CanRegenerate)
        {
            EditorGUILayout.Space(3);
            GUIStyle regenStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = new Color(0.5f, 1f, 0.7f) }
            };
            EditorGUILayout.LabelField($"♻️ Regen: {stat.RegenRate}/s", regenStyle);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawStatBar(Stat stat)
    {
        EditorGUILayout.Space(5);

        Rect barRect = EditorGUILayout.GetControlRect(false, 25);
        float percentage = stat.CurrentValue / stat.MaxValue;

        EditorGUI.DrawRect(barRect, new Color(0.2f, 0.2f, 0.2f));

        Rect fillRect = barRect;
        fillRect.width *= percentage;
        EditorGUI.DrawRect(fillRect, stat.GetStatColor());

        Handles.BeginGUI();
        Handles.color = Color.black;
        Handles.DrawLine(new Vector3(barRect.x, barRect.y), new Vector3(barRect.x + barRect.width, barRect.y));
        Handles.DrawLine(new Vector3(barRect.x, barRect.y + barRect.height), new Vector3(barRect.x + barRect.width, barRect.y + barRect.height));
        Handles.EndGUI();

        GUIStyle textStyle = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white },
            fontStyle = FontStyle.Bold,
            fontSize = 11
        };
        GUI.Label(barRect, $"{stat.CurrentValue:F0} / {stat.MaxValue:F0} ({percentage * 100:F0}%)", textStyle);
    }

    private void DrawNavigation()
    {
        EditorGUILayout.LabelField("Navigation", headerStyle);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("⏮ First", GUILayout.Height(30))) controller.First();
        if (GUILayout.Button("◀ Prev", GUILayout.Height(30))) controller.Previous();
        if (GUILayout.Button("Next ▶", GUILayout.Height(30))) controller.Next();
        if (GUILayout.Button("Last ⏭", GUILayout.Height(30))) controller.Last();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawQuickActions()
    {
        EditorGUILayout.LabelField("Quick Actions", headerStyle);

        EditorGUILayout.BeginHorizontal();

        Color previousBg = GUI.backgroundColor;

        GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
        if (GUILayout.Button("⬆ +10", GUILayout.Height(25))) controller.IncreaseCurrent(10f);

        GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
        if (GUILayout.Button("⬇ -10", GUILayout.Height(25))) controller.DecreaseCurrent(10f);

        GUI.backgroundColor = new Color(0.5f, 0.9f, 1f);
        if (GUILayout.Button("♻️ Restore", GUILayout.Height(25))) controller.RestoreCurrent();

        GUI.backgroundColor = previousBg;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = new Color(0.5f, 1f, 0.8f);
        if (GUILayout.Button("✨ Restore All", GUILayout.Height(25))) controller.RestoreAll();

        GUI.backgroundColor = new Color(1f, 0.9f, 0.3f);
        if (GUILayout.Button("⬆ Level Up", GUILayout.Height(25))) controller.LevelUp();

        GUI.backgroundColor = previousBg;
        EditorGUILayout.EndHorizontal();
    }

    private void DrawVitalStats()
    {
        EditorGUILayout.BeginHorizontal();
        showVitals = EditorGUILayout.Foldout(showVitals, "Vital Stats", true, headerStyle);
        EditorGUILayout.EndHorizontal();

        if (!showVitals) return;

        var vitalStats = controller.Stats.GetVitalStats();
        foreach (var stat in vitalStats)
        {
            DrawStatWithBar(stat, false);
        }
    }

    private void DrawCombatStats()
    {
        EditorGUILayout.BeginHorizontal();
        showCombat = EditorGUILayout.Foldout(showCombat, "Combat Stats", true, headerStyle);
        EditorGUILayout.EndHorizontal();

        if (!showCombat) return;

        var combatStats = controller.Stats.GetCombatStats();
        foreach (var stat in combatStats)
        {
            DrawStatInfo(stat, false);
        }
    }

    private void DrawAllStats()
    {
        EditorGUILayout.BeginHorizontal();
        showStats = EditorGUILayout.Foldout(showStats,
            $"All Stats ({controller.Stats.Count})", true, headerStyle);
        EditorGUILayout.EndHorizontal();

        if (!showStats) return;

        int currentIndex = controller.Stats.GetCurrentIndex();

        statsScrollPos = EditorGUILayout.BeginScrollView(statsScrollPos, GUILayout.MaxHeight(250));

        for (int i = 0; i < controller.Stats.Count; i++)
        {
            var stat = controller.Stats.Stats[i];
            bool isCurrent = i == currentIndex;

            if (stat.MaxValue > 0) DrawStatWithBar(stat, isCurrent);
            else DrawStatInfo(stat, isCurrent);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawStatWithBar(Stat stat, bool isCurrent)
    {
        Color bgColor = isCurrent ? stat.GetStatColor() : new Color(0.3f, 0.3f, 0.3f);
        bgColor.a = 0.3f;

        Color previousBg = GUI.backgroundColor;
        GUI.backgroundColor = bgColor;

        EditorGUILayout.BeginVertical(isCurrent ? currentStatStyle : statStyle);
        GUI.backgroundColor = previousBg;

        EditorGUILayout.BeginHorizontal();

        string prefix = isCurrent ? "➤" : "•";
        EditorGUILayout.LabelField(prefix, GUILayout.Width(20));
        EditorGUILayout.LabelField($"{stat.GetStatIcon()} {stat.StatName}", GUILayout.Width(120));

        Rect miniBarRect = EditorGUILayout.GetControlRect(false, 15);
        float percentage = stat.CurrentValue / stat.MaxValue;

        EditorGUI.DrawRect(miniBarRect, new Color(0.2f, 0.2f, 0.2f));
        Rect fillRect = miniBarRect;
        fillRect.width *= percentage;
        EditorGUI.DrawRect(fillRect, stat.GetStatColor());

        GUIStyle textStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white },
            fontStyle = FontStyle.Bold
        };
        GUI.Label(miniBarRect, $"{stat.CurrentValue:F0}/{stat.MaxValue:F0}", textStyle);

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    private void DrawStatInfo(Stat stat, bool isCurrent)
    {
        Color bgColor = isCurrent ? stat.GetStatColor() : new Color(0.3f, 0.3f, 0.3f);
        bgColor.a = 0.3f;

        Color previousBg = GUI.backgroundColor;
        GUI.backgroundColor = bgColor;

        EditorGUILayout.BeginVertical(isCurrent ? currentStatStyle : statStyle);
        GUI.backgroundColor = previousBg;

        EditorGUILayout.BeginHorizontal();

        string prefix = isCurrent ? "➤" : "•";
        EditorGUILayout.LabelField(prefix, GUILayout.Width(20));
        EditorGUILayout.LabelField($"{stat.GetStatIcon()} {stat.StatName}", GUILayout.Width(120));

        GUIStyle valueStyle = new GUIStyle(EditorStyles.label)
        {
            fontStyle = isCurrent ? FontStyle.Bold : FontStyle.Normal,
            normal = { textColor = isCurrent ? stat.GetStatColor() : Color.white }
        };
        EditorGUILayout.LabelField($"{stat.GetFinalValue():F1}", valueStyle);

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    private void DrawSeparator()
    {
        Rect rect = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
    }
}

#endif
