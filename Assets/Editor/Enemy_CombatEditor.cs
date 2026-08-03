using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Enemy_Combat))]
public class Enemy_CombatEditor : Editor
{
    private SerializedProperty damageProp;
    private SerializedProperty cooldownTimeProp;
    private SerializedProperty canDamageProp;
    private SerializedProperty attackPointProp;
    private SerializedProperty playerLayerProp;
    private SerializedProperty forceProp;
    private SerializedProperty stunTimeProp;
    private SerializedProperty canCombatProp;
    private Enemy_Combat combat;

    private void OnEnable()
    {
        // 获取序列化属性
        damageProp = serializedObject.FindProperty("damage");
        cooldownTimeProp = serializedObject.FindProperty("cooldownTime");
        canDamageProp = serializedObject.FindProperty("CanDamage");
        attackPointProp = serializedObject.FindProperty("AttackPoint");
        playerLayerProp = serializedObject.FindProperty("playerLayer");
        forceProp = serializedObject.FindProperty("Force");
        stunTimeProp = serializedObject.FindProperty("stunTime");
        canCombatProp = serializedObject.FindProperty("CanCompat");
        combat = (Enemy_Combat)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("战斗参数", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // 伤害值
        EditorGUILayout.PropertyField(damageProp, new GUIContent("伤害值", "对玩家造成的伤害量（负数表示伤害，正数表示治疗）"));

        // 冷却时间
        EditorGUILayout.PropertyField(cooldownTimeProp, new GUIContent("冷却时间", "两次造成伤害之间的最小间隔（秒）"));

        // 能否造成伤害（私有字段，一般用于调试）
        EditorGUILayout.PropertyField(canDamageProp, new GUIContent("能否造成伤害", "是否允许当前造成伤害（通常由冷却自动控制，仅在调试时手动修改）"));

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("攻击判定", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // 攻击点 Transform
        EditorGUILayout.PropertyField(attackPointProp, new GUIContent("攻击点", "攻击判定的中心点位置（通常为武器或攻击特效的挂点）"));

        // 攻击范围（只读，从父物体获取）
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("攻击范围", "实际攻击半径（从父物体的 Enemy_Movement 脚本获取）"), GUILayout.Width(EditorGUIUtility.labelWidth));
        GUI.enabled = false;
        EditorGUILayout.FloatField(combat.AttackRange);
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        // 玩家层
        EditorGUILayout.PropertyField(playerLayerProp, new GUIContent("玩家层", "玩家角色所在的 Layer，用于碰撞检测"));

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("击退效果", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // 击退力
        EditorGUILayout.PropertyField(forceProp, new GUIContent("击退力", "攻击命中后对玩家施加的击退力度（矢量大小）"));

        // 眩晕时间
        EditorGUILayout.PropertyField(stunTimeProp, new GUIContent("眩晕时间", "玩家被击中后无法移动的时间（秒）"));
        
        //是否具有碰撞伤害
        EditorGUILayout.PropertyField(canCombatProp, new GUIContent("是否具有碰撞伤害", "关闭后玩家与敌人碰撞将没有碰撞伤害"));

        serializedObject.ApplyModifiedProperties();
    }
}