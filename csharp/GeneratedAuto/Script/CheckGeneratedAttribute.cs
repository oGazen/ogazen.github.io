using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class CheckGeneratedAttribute : Attribute
{

}

public class CheckGeneratedVisiblyAttribute : PropertyAttribute
{
    public string boolname;

    public CheckGeneratedVisiblyAttribute(string boolfieldname)
    {
        this.boolname = boolfieldname;
    }
}
