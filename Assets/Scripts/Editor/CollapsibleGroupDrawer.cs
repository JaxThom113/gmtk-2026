using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

[CustomEditor(typeof(Object), true)]
[CanEditMultipleObjects]
public class CollapsibleGroupDrawer : Editor
{
    private Dictionary<string, bool> foldouts = new Dictionary<string, bool>();

    class GroupInstance
    {
        public int order;
        public int suborder;
        public List<SerializedProperty> fields = new List<SerializedProperty>();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var fields = GetAllFields(target.GetType());

        // Key: groupName, Value: List of group instances with order, suborder, fields
        Dictionary<string, List<GroupInstance>> groups = new Dictionary<string, List<GroupInstance>>();

        List<SerializedProperty> ungrouped = new List<SerializedProperty>();

        string currentGroupName = null;
        int currentGroupOrder = 0;
        int currentGroupSuborder = 0;

        SerializedProperty prop = serializedObject.GetIterator();
        bool enterChildren = true;

        while (prop.NextVisible(enterChildren))
        {
            enterChildren = false;

            var field = fields.FirstOrDefault(f => f.Name == prop.name);
            var groupAttr = field?.GetCustomAttribute<CollapsibleGroupAttribute>();

            if (groupAttr != null)
            {
                currentGroupName = groupAttr.groupName;
                currentGroupOrder = groupAttr.groupOrder;
                currentGroupSuborder = groupAttr.subgroupOrder;

                if (!groups.ContainsKey(currentGroupName))
                {
                    groups[currentGroupName] = new List<GroupInstance>();
                }

                groups[currentGroupName].Add(new GroupInstance
                {
                    order = currentGroupOrder,
                    suborder = currentGroupSuborder,
                    fields = new List<SerializedProperty> { prop.Copy() }
                });
            }
            else if (currentGroupName != null)
            {
                // Add field to last group instance of currentGroupName (the most recent start)
                var instances = groups[currentGroupName];
                instances[instances.Count - 1].fields.Add(prop.Copy());
            }
            else
            {
                ungrouped.Add(prop.Copy());
            }
        }

        // Draw ungrouped fields first (order = 0)
        foreach (var p in ungrouped)
        {
            EditorGUILayout.PropertyField(p, true);
        }

        // Sort groups by order ascending
        var orderedGroupNames = groups.Keys.OrderBy(name =>
        {
            // Use smallest order among instances of this groupName to sort groupNames
            return groups[name].Min(inst => inst.order);
        });

        foreach (var groupName in orderedGroupNames)
        {
            if (!foldouts.ContainsKey(groupName))
                foldouts[groupName] = true;

            foldouts[groupName] = EditorGUILayout.Foldout(foldouts[groupName], groupName, true);

            if (foldouts[groupName])
            {
                EditorGUI.indentLevel++;

                // For this group name, sort instances by suborder ascending
                var instances = groups[groupName].OrderBy(inst => inst.suborder);

                // Draw fields from each instance in order
                foreach (var inst in instances)
                {
                    foreach (var p in inst.fields)
                    {
                        EditorGUILayout.PropertyField(p, true);
                    }
                }

                EditorGUI.indentLevel--;
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private static FieldInfo[] GetAllFields(System.Type type)
    {
        List<FieldInfo> fields = new List<FieldInfo>();
        while (type != null && type != typeof(MonoBehaviour))
        {
            fields.AddRange(type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
            type = type.BaseType;
        }
        return fields.ToArray();
    }
}
