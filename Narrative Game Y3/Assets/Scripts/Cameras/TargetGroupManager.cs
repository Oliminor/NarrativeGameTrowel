using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetGroupManager : MonoBehaviour
{
    [SerializeField] CinemachineTargetGroup targetGroup;

    public void SetFocusTarget(int target)
    {
        targetGroup.m_Targets[target].weight = 1f;
    }

    public void SetFocusTarget(int target, int target2)
    {
        targetGroup.m_Targets[target].weight = 1f;
        targetGroup.m_Targets[target2].weight = 1f;
    }

    public void SetFocusTarget(int target, int target2, int target3)
    {
        targetGroup.m_Targets[target].weight = 1f;
        targetGroup.m_Targets[target2].weight = 1f;
        targetGroup.m_Targets[target3].weight = 1f;
    }

    public void ResetFocus()
    {
        for(int i = 0; i < targetGroup.m_Targets.Length; i++)
        {
            targetGroup.m_Targets[i].weight = 0f;
        }
    }
}