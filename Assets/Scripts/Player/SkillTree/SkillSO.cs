using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "NewSkill",menuName = "SkillTree/Skill")]
public class SkillSO : ScriptableObject
{
    public string SkillName;//¼¼ÄÜÃû
    public int maxLevel;
    public Sprite skillIcon;
    public string skillDescribe;
}
