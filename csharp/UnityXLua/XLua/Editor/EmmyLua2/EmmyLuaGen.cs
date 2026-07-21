using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class EmmyLuaGen
{
    private static string m_path_gen = "/LuaScripts/IntelliSenseCode";

    // Lua 注释类型
    public enum LuaAnnotation
    {
        _class,
        _field,
        _param,
        _return,
        _type,
        _overlaod,
    }
    
    
    
    private class MemberInfoCompareName : IEqualityComparer<MemberInfo>
    {
        public bool Equals(MemberInfo a, MemberInfo b)
        {
            return a?.Name == b?.Name;
        }

        public int GetHashCode(MemberInfo obj)
        {
            return obj.GetHashCode();
        }
    }
    
    
    
    
    private static List<Type> m_cs_types = new List<Type>();
    


    private static Dictionary<LuaAnnotation, string> m_lua_annotations = new Dictionary<LuaAnnotation, string>()
    {
        { LuaAnnotation._class, "---@class {0} : {1}" },
        { LuaAnnotation._field, "---@field {0} {1}" },
        { LuaAnnotation._param, "---@param {0} {1}" },
        { LuaAnnotation._return, "---@return {0}" },
        { LuaAnnotation._type, "---@type {0}" },
        { LuaAnnotation._overlaod,"---@overload fun({0}):{1}" },
    };
    



    private static Dictionary<Type, string> m_lua_basic_types = new Dictionary<Type, string>()
    {
        {typeof(sbyte),"number"},
        {typeof(byte),"number"},
        {typeof(short),"number"},
        {typeof(ushort),"number"},
        {typeof(int),"number"},
        {typeof(uint),"number"},
        {typeof(float),"number"},
        {typeof(double),"number"},
        
        {typeof(bool),"boolean"},
        {typeof(string),"string"},
    };
    
    
    

    private static string GetLuaStr_(Type type)
    {
        return GetLuaStr(type).Replace('.','_');
    }
    
    
    private static string GetLuaStr(Type type)
    {
        var name = type.FullName;
        if (type.IsGenericType || type.IsConstructedGenericType)
        {
            name = name.Split('`')[0];
        }
        return $"CS.{name}";
    }
    


    private static string GetLuaStrWhenLuaType(Type type)
    {
        string name = null;
        name = m_lua_basic_types.TryGetValue(type, out name) ? name : GetLuaStr(type);
        return name;
    }




    private static string CheckLuaKeyword(string str)
    {
        if (str.Equals("end"))
        {
            return "_end";
        }
        return str;
    }



    
    
    // 方法
    private static bool IsHaveGenericParameter(MethodBase methodBase)
    {
        var _params =  methodBase.GetParameters();
        foreach (var _param in _params)
        {
            if (_param.ParameterType.IsGenericType) return true;
        }
        
        return false;
    }


    private static bool IsHaveAddressPointerParameter(MethodBase methodBase)
    {
        var _params =  methodBase.GetParameters();
        foreach (var _param in _params)
        {
            if (_param.ParameterType.Name.Contains('*') || _param.ParameterType.IsPointer) return true;
        }
        
        return false;
    }
    

    private static bool IsHaveRetunGenericParameter(MethodInfo methodInfo)
    {
        return methodInfo.ReturnType.IsGenericType;
    }




    private static bool IsinheritMember(MemberInfo _memberInfo,ref MemberInfo[] _memberInfos)
    {
        MemberInfoCompareName comparer = new MemberInfoCompareName();
        return _memberInfos.Contains(_memberInfo,comparer);
    }
    
    
    
    
    
    
    [MenuItem("XLua/Clear IntelliSenseCode")]
    private static void ClearGenerate()
    {
        var lua_intelliSense_path = CommonTool.GetBaseAssetPath() + m_path_gen;
        DirectoryInfo directoryInfo = Directory.CreateDirectory(lua_intelliSense_path);
        FileInfo[] fileInfos = directoryInfo.GetFiles();
        for (int i = 0; i < fileInfos.Length; i++)
        {
            var file = fileInfos[i];
            file.Delete();
        }
        
        AssetDatabase.Refresh();
    }

    
    

    [MenuItem("XLua/Gen IntelliSenseCode")]
    private static void Generate()
    {
        Debug.Log("start generate lua intelliScese code");
        var types_list = ExampleConfig.LuaCallCSharp;

        for (int i = 0; i < types_list.Count; i++)
        {
            var type = types_list[i];
            CheckNestedType(type);
        }
        
        
        var str_assembly = Generate_Assembly();
        File.WriteAllText($"{CommonTool.GetBaseAssetPath()}{m_path_gen}/_Assembly_Lua.lua.txt",str_assembly);
        
        
        m_cs_types.Clear();
        AssetDatabase.Refresh();
        Debug.Log("generate lua intelliScese code finish");
    }



    private static string Generate_Type(Type type)
    {
        StringBuilder str_builder = new StringBuilder();
        str_builder.Clear();

        
        // TODO 继承剔除
        var members = type.BaseType == null ? new MemberInfo[0] : type.BaseType.GetMembers();
        
        
        
        var _calss_str = GetLuaStr(type);
        var _class_base_str = type.BaseType == null ? null : GetLuaStr(type.BaseType);
        var _class = string.Format(m_lua_annotations[LuaAnnotation._class],_calss_str,_class_base_str);
        str_builder.AppendLine(_class);
        
        var fields = type.GetFields();
        foreach (var field in fields)
        {
            CheckNestedType(field.FieldType);
            if(IsinheritMember(field,ref members)) continue;
            str_builder.AppendLine(string.Format(m_lua_annotations[LuaAnnotation._field],field.Name,GetLuaStrWhenLuaType(field.FieldType)));
        }
        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            CheckNestedType(property.PropertyType);
            if(IsinheritMember(property,ref members)) continue;
            str_builder.AppendLine(string.Format(m_lua_annotations[LuaAnnotation._field],property.Name,GetLuaStrWhenLuaType(property.PropertyType)));
        }
        str_builder.AppendLine();
        
        
        
        var methods = type.GetMethods();
        var method_return_params = new List<string>();
        foreach (var method in methods)
        {
            if(!method.IsPublic) continue;
            
            if(method.IsGenericMethod || method.IsGenericExtension()) continue;
            
            if(IsHaveGenericParameter(method)) continue;
            
            if(IsHaveAddressPointerParameter(method)) continue;
            
            if(IsHaveRetunGenericParameter(method)) continue;
            
            if(method.IsOperator()) continue;
            
            if(method.Attributes.HasFlag(MethodAttributes.SpecialName)) continue;
            
            if(IsinheritMember(method,ref members)) continue;
            
            
            // TODO 泛型函数 重载函数
            bool isStatic = method.IsStatic;
            method_return_params.Clear();

            // 参数表
            var method_params = method.GetParameters();
            var method_str_sign = isStatic ? "." : ":";
            var method_return_sign = $"_{_calss_str}{method_str_sign}{method.Name}";
            var method_str = $"function {_calss_str}{method_str_sign}{method.Name}";
            var method_param_str = "(";
            for (int j = 0; j < method_params.Length; j++)
            {
                var param = method_params[j];
                CheckNestedType(param.ParameterType);
                
                string param_type_name = GetLuaStrWhenLuaType(param.ParameterType);
                string param_name = CheckLuaKeyword(param.Name);

                
                // Ref Out 参数处理
                if (param.ParameterType.IsByRef)
                {
                    method_return_params.Add(param_type_name);
                }
                else if(param.IsOut)
                {
                    method_return_params.Add(param_type_name);
                    continue;
                }
                
                
                str_builder.AppendLine(string.Format(m_lua_annotations[LuaAnnotation._param], param_name, param_type_name));
                if (j == method_params.Length - 1) method_param_str += $" {param_name}";
                else method_param_str += $" {param_name},";
            }
            method_param_str += ")";

            
            // 返回值
            var method_return = method.ReturnType;
            CheckNestedType(method_return);
            method_return_params.Insert(0,GetLuaStrWhenLuaType(method_return));

            if (isStatic)
            {
                if (method_return_params.Count == 1 && method_return.Name == "Void")
                    method_param_str += $"\n    {method_return_sign}{method_param_str}\n";
                else method_param_str += $"\n    return {method_return_sign}{method_param_str}\n";
            }

            method_param_str += " end";
            method_str += method_param_str;
            
            
            for (int j = 0; j < method_return_params.Count; j++)
            {
                var return_str = method_return_params[j];
                str_builder.AppendLine(string.Format(m_lua_annotations[LuaAnnotation._return],return_str));
            }
            
            str_builder.AppendLine(method_str);
            str_builder.AppendLine(null);
        }
        
        
        return str_builder.ToString();
    }



    private static void CheckNestedType(Type type)
    {
        if(type.IsGenericType) return;
        
        
        if(type.IsPointer) return;
        
        
        if(type.IsInterface) return;
        
        
        if(!(type.IsClass || type.IsValueType)) return;
        

        
        if(type.FullName.Contains('&') 
           || type.FullName.Contains('+') // TODO 内部类型？
           || type.FullName.Contains("Security")
           || type.FullName.Contains("Threading")
           || type.FullName.Contains("Experimental")
           || type.FullName.Contains("Reflection")
           || type.FullName.Contains("SafeHandles")
           || type.FullName.Contains("Assemblies")) return;

        
        
        
        
        if (type.IsArray)
        {
            type = type.GetElementType();
        }
        
        
        if (!m_cs_types.Contains(type))
        {
            m_cs_types.Add(type);
            var str_newfile = Generate_Type(type);
            File.WriteAllTextAsync($"{CommonTool.GetBaseAssetPath()}{m_path_gen}/{GetLuaStr_(type)}.lua.txt",str_newfile);
        }
    }
    


    private static string Generate_Assembly()
    {
        String txt = "_CS = CS;\nCS = {};\nsetmetatable(CS,{ __index = _CS});\n\n";
        
        
        for (int i = 0; i < m_cs_types.Count; i++)
        {
            var type = m_cs_types[i];
            var _namespace = type.Namespace;
            if (string.IsNullOrEmpty(_namespace)) continue;

            var _namespace_arr = _namespace.Split('.');
            var _txt = "";
            
            for (int j = 0; j < _namespace_arr.Length; j++)
            {
                var name = _namespace_arr[j];
                _txt = string.IsNullOrEmpty(_txt) ? name : _txt+"."+name;
                
                var txt_addination = $"CS.{_txt} = {{}}";
                if (txt.Contains(txt_addination)) continue;
                txt += $"{txt_addination}\n";
                txt += $"setmetatable(CS.{_txt},{{ __index = _CS.{_txt}}} )\n\n";
            }
        }

        
        
        
        for (int i = 0; i < m_cs_types.Count; i++)
        {
            var type = m_cs_types[i];
            var type_name = type.FullName;
           
            
            var methods_constructors = type.GetConstructors();
            var constructor_index = 0;
            foreach (var constrctor in methods_constructors)
            {
                if(!constrctor.IsPublic) continue;
            
                if(constrctor.IsGenericMethod) continue;
            
                if(IsHaveGenericParameter(constrctor)) continue;
                
                if(IsHaveAddressPointerParameter(constrctor)) continue;
                
                // 参数表
                var method_params = constrctor.GetParameters();
                var method_param_str = "";
                for (int j = 0; j < method_params.Length; j++)
                {
                    var param = method_params[j];
                    string param_type_name = GetLuaStrWhenLuaType(param.ParameterType);
                    string param_name = CheckLuaKeyword(param.Name);
                    
                    if (j == method_params.Length - 1) method_param_str += $" {param_name}:{param_type_name}";
                    else method_param_str += $" {param_name}:{param_type_name},";
                }
                var method_str = string.Format(m_lua_annotations[LuaAnnotation._overlaod],method_param_str,GetLuaStrWhenLuaType(type));
                txt += $"{method_str}\n";
            }
            
            
            
            var txt_addination = $"CS.{type_name} = {{}}";
            if (txt.Contains(txt_addination)) continue;
            txt += string.Format(m_lua_annotations[LuaAnnotation._type],$"CS.{type_name}\n");
            txt += $"{txt_addination}\n";
            txt += $"setmetatable(CS.{type_name},{{\n    __index = _CS.{type_name},";
            if (methods_constructors.Length > 0)
            {
                txt += $"\n    __call = function(self,...) return _CS.{type_name}(...) end,";
            }
            txt += "\n})\n\n";
        }
        
        
        return txt;
    }
    
    
}
