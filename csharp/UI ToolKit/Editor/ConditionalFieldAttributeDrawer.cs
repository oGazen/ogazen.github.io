using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ConditionalFieldAttribute))]
public class ConditionalFieldAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ConditionalFieldAttribute constomattribute = (ConditionalFieldAttribute)attribute;
        bool isShow = this.IsShow(constomattribute, property);
        if (isShow)
        { EditorGUI.PropertyField(position, property, label); }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ConditionalFieldAttribute constomattribute = (ConditionalFieldAttribute)attribute;
        bool isShow = this.IsShow(constomattribute, property);
        if (isShow) return base.GetPropertyHeight(property, label);
        else return EditorGUIUtility.standardVerticalSpacing * -1;
    }

    private bool IsShow(ConditionalFieldAttribute attribute, SerializedProperty property)
    {
        string propertyPath = property.propertyPath;
        string enum_propertyPath = propertyPath.Replace(property.name, attribute.EnumStringName);
        SerializedProperty serializedProperty = property.serializedObject.FindProperty(enum_propertyPath);

        if (serializedProperty != null)
        {
            var index_select = serializedProperty.enumValueIndex;
            for (int i = 0; i < attribute.Type_value.Length; i++)
            {
                var item = attribute.Type_value[i];
                if (index_select.Equals(item)) return true;
            }
        }

        return false;
    }

}
