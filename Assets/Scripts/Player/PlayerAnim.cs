using UnityEngine;

public class PlayerAnim : MonoBehaviour
{
    [SerializeField]
    private AnimatorSO currentAnim;
    private Animator anim;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        anim = currentAnim.Animator;
        currentAnim.onValueChanged += (sender, e) =>
        {
            anim = currentAnim.Animator;
        };
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayAttack()
    {
        anim.Play("SwordSwing");
    }
}
