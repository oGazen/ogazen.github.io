using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field)]
public class DisableFieldAttribute : PropertyAttribute
{
    public DisableFieldAttribute() { }
}
