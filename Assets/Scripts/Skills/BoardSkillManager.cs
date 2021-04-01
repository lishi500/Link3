using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardSkillManager : Singleton<BoardSkillManager>
{
    public BoardSkill GetBoardSkill(System.Type skillType) {
        BoardSkill skill = (BoardSkill) gameObject.GetComponent(skillType);
        if (skill == null) { 
           return (BoardSkill) gameObject.AddComponent(skillType);
        }

        return skill;
    }
}
