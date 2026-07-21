using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ConditionalFieldAttribute : PropertyAttribute
{
    public string EnumStringName;
    public int[] Type_value;

    public ConditionalFieldAttribute(string enumName, params int[] verifyvalue)
    {
        this.EnumStringName = enumName;
        this.Type_value = verifyvalue;
    }

}