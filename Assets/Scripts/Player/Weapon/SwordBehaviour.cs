using UnityEngine;

public class SwordBehaviour : MeleeWeapon
{
    [SerializeField]
    private BoxCollider swordCol;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EnableSwordCollider()
    {
        swordCol.enabled = true;
    }

    public void DisableSwordCollider()
    {
        swordCol.enabled = false;
        hitColliders.Clear();
    }
}
