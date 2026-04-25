using Networking;
using Networking.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Networking {
    [CreateAssetMenu(fileName = "PackageGeneratorDescriptor", menuName = "Networking/PackageGeneratorDescriptor")]
    public sealed class PackageGeneratorDescriptor : ScriptableObject
    {
        private enum DefaultTypes
        {
            Int,
            Short,
            Long,
            Float,
            Double,
            Byte,
            Char,
            String,
            Vector2,
            Vector3
        }

        [Serializable]
        private struct TypeDesctiptor
        {
            public string Name;
            public bool IsArray;
            public string CustomStructTypeName;
            public DefaultTypes DefaultType;
        }
        [SerializeField] private PackageType _type;
        [SerializeField] private PackageFlags _flags;
        [SerializeField, Tooltip("Path starting with Assets/")] private string _pathFromCore;
        [SerializeField] private TypeDesctiptor[] _descriptors;

        public void Create()
        {
            string path = Application.dataPath + "/" + _pathFromCore;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var types = GetAllTypes();
            var names = GetAllNames();

            PackageGenerator.GeneratePackage(path, _type, _flags, names, types);
            AssetDatabase.Refresh();
        }

        private IReadOnlyList<string> GetAllNames()
        {
            var list = new List<string>();
            foreach (var type in _descriptors)
            {
                list.Add(type.Name);
            }
            return list;
        }
        private Type[] GetAllTypes()
        {
            Type[] types = new Type[_descriptors.Length];
            for(int i = 0; i < _descriptors.Length; i++)
            {
                types[i] = GetType(_descriptors[i]);
            }
            return types;
        }

        private Type GetType(TypeDesctiptor descriptor)
        {
            Type res = null;
            if (string.IsNullOrEmpty(descriptor.CustomStructTypeName))
            {
                switch (descriptor.DefaultType)
                {
                    case DefaultTypes.Int:
                        {
                            res = typeof(int);
                            break;
                        }
                    case DefaultTypes.Short:
                        {
                            res = typeof(short);
                            break;
                        }
                    case DefaultTypes.Long:
                        {
                            res = typeof(long);
                            break;
                        }
                    case DefaultTypes.Float:
                        {
                            res = typeof(float);
                            break;
                        }
                    case DefaultTypes.Double:
                        {
                            res = typeof(double);
                            break;
                        }
                    case DefaultTypes.Char:
                        {
                            res = typeof(char);
                            break;
                        }
                    case DefaultTypes.Byte:
                        {
                            res = typeof(byte);
                            break;
                        }
                    case DefaultTypes.String:
                        {
                            res = typeof(string);
                            break;
                        }
                    case DefaultTypes.Vector2:
                        {
                            res = typeof(Vector2);
                            break;
                        }
                    case DefaultTypes.Vector3:
                        {
                            res = typeof(Vector3);
                            break;
                        }
                }
            }
            else
            {
                var type = Assembly.GetExecutingAssembly().GetTypes().First(t => (t.Name == descriptor.CustomStructTypeName));
                if (type == null)
                {
                    Debug.LogError("Cannot find type with a name: " +  descriptor.CustomStructTypeName);
                    return null;
                }
                if (!type.IsValueType)
                {
                    Debug.LogError(type.FullName + " is not a struct");
                    return null;
                }

                res = type;
            }

            if (descriptor.IsArray)
            {
                res = res.MakeArrayType();
            }

            return res;
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(PackageGeneratorDescriptor))]
    public sealed class DescriptorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("GeneratePackage"))
            {
                ((PackageGeneratorDescriptor)target).Create();
            }
        }
    }
#endif
}