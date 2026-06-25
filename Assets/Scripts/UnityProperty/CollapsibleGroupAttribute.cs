using UnityEngine;
using UnityEditor;
public class CollapsibleGroupAttribute : PropertyAttribute
{
    public string groupName;
    public int groupOrder;
    public int subgroupOrder;

    public CollapsibleGroupAttribute(string groupName, int groupOrder = 0, int subgroupOrder = 0)
    {
        this.groupName = groupName;
        this.groupOrder = groupOrder;
        this.subgroupOrder = subgroupOrder;
    }
}
