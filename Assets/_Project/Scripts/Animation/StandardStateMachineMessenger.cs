using UnityEngine;
using GameDB;

public enum AnimationType { Attack, Motion, Ability, Reload, ReloadStart, ReloadEnd }

[System.Serializable]
public struct StateMachineParameters
{
    [Tooltip("What type of animation is this?")]
    public AnimationType animationType;
    [Tooltip("Can the character queue up other actions (attacking, interaction, etc) while this animation is playing?")]
    public bool canAct;
    [Tooltip("How fast can the character move while this animation is playing?  (0 = stopped)")]
    [Range(0, 1)]
    public float motionMultiplier;
    [Tooltip("The base stamina used when this move is executed")]
    public float staminaCost;
    [Tooltip("1 = normal, 2 = twice, 0.5 = half, etc")]
    public float damageMultiplier;
    [Tooltip("Is this a charging attack?")]
    public bool charging;
    [Tooltip("Damage Multiplier increases by this much every second while charging")]
    public float chargeMultiplier;
    [Tooltip("Maximum damage multiplier that can be gained through charging")]
    public float maxChargeMultiplier;
    [Tooltip("How to modify the hitbox size for this animation")]
    public Vector3 hitboxSizeOffset;
    [HideInInspector]
    public AnimatorStateInfo stateInfo;
    [HideInInspector]
    public int layerIndex;
}

public class StandardStateMachineMessenger : StateMachineBehaviour
{
    public StateMachineParameters parameters;

    bool attemptedToFindChar = false;
    Character cachedChar = null;

    //Utility Methods
    bool canSendMessage(Animator animator)
    {
        if (cachedChar != null)
            return true;

        if (attemptedToFindChar)
            return false;

        attemptedToFindChar = true;
        cachedChar = animator.GetComponent<Character>();
        return canSendMessage(animator);
    }

    //State Methods
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        if (canSendMessage(animator))
        {
            parameters.stateInfo = stateInfo;
            parameters.layerIndex = layerIndex;
            cachedChar.StandardAnimatorStateEnter(parameters);
        }
    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        if (canSendMessage(animator))
        {
            parameters.stateInfo = stateInfo;
            parameters.layerIndex = layerIndex;
            cachedChar.StandardAnimatorStateExit(parameters);
        }
    }
}