using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    public Animator anim;
    public AnimatorSO animSO;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        animSO.Animator = anim;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchWeapon()
    {
        animSO.Animator = anim;
    }
}
