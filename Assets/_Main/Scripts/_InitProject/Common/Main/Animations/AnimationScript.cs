using UnityEngine;

/// <summary>
/// All kind of animation states that you can trigger with <see cref="AnimationScript"/>
/// </summary>
public enum AnimationStates
{
    Idle01 = 0,
    Idle10 = 9,

    Walk01 = 10,
    // ..
    Walk10 = 19,

    Upgrade = 20,

    Victory = 30,

    Hit = 40,

    Loss = 50,
}

/// <summary>
/// Animator wrapper to handle animations. <br>
/// Has basic state switch logic by <see cref="AnimationStates"/> </br>
/// </summary>
[AddComponentMenu("Common/Animation/Animation Script")]
[RequireComponent(typeof(Animator))]
public class AnimationScript : MonoBehaviour {
    /// <summary>
    /// Each animation state should have it's own <see cref="AnimationStates"/> enum value condition <br>
    /// This property returns currently running <see cref="AnimationStates"/> in animator </br>
    /// </summary>
    public AnimationStates CurrentAnimState {
        get => _currentAnimState;
        set => SetAnimationState(value);
    }

    protected Animator _animator;
    public Animator Animator => _animator;

    private AnimationStates _currentAnimState = AnimationStates.Idle01;
    private float _defaultAnimatorSpeed;

    private static readonly int AnimationStateIntegerID = Animator.StringToHash("AnimState");
    private static readonly int AnyStateTriggerID = Animator.StringToHash("AnyState");

    private void Awake() {
        _animator = GetComponent<Animator>();
        _defaultAnimatorSpeed = _animator.speed;
    }

    /// <summary>
    /// Switches current animation state in animator by <paramref name="animationState"/>
    /// </summary>
    public void SetAnimationState(AnimationStates animationState) {
        _animator.SetInteger(AnimationStateIntegerID, (int)animationState);
        _animator.SetTrigger(AnyStateTriggerID);

        _currentAnimState = animationState;
    }

    public void SetAnimatorSpeed(float speed) {
        _animator.speed = speed;
    }

    public void SetAnimatorDefaultSpeed() {
        _animator.speed = _defaultAnimatorSpeed;
    }
	
    public virtual void ResetState(AnimationStates defaultState = AnimationStates.Idle01) {
        _currentAnimState = defaultState;
    }

    public float GetCurrentStateNormalizedTime() {
        return _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }
}
