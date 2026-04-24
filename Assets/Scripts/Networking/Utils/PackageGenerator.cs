using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace Networking.Utils
{
    public static class PackageGenerator
    {
        public static void GeneratePackage<T1>(string path, PackageType type, PackageFlags flags, IReadOnlyList<string> names) => GeneratePackage(path, type, flags, names, typeof(T1));
        public static void GeneratePackage<T1, T2>(string path, PackageType type, PackageFlags flags, IReadOnlyList<string> names) => GeneratePackage(path, type, flags, names, typeof(T1), typeof(T2));
        public static void GeneratePackage<T1, T2, T3>(string path, PackageType type, PackageFlags flags, IReadOnlyList<string> names) => GeneratePackage(path, type, flags, names, typeof(T1), typeof(T2), typeof(T3));
        public static void GeneratePackage<T1, T2, T3, T4>(string path, PackageType type, PackageFlags flags, IReadOnlyList<string> names) => GeneratePackage(path, type, flags, names, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        public static void GeneratePackage<T1, T2, T3, T4, T5>(string path, PackageType type, PackageFlags flags, IReadOnlyList<string> names) => GeneratePackage(path, type, flags, names, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        public static void GeneratePackage<T1, T2, T3, T4, T5, T6>(string path, PackageType type, PackageFlags flags, IReadOnlyList<string> names) => GeneratePackage(path, type, flags, names, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        public static void GeneratePackage<T1, T2, T3, T4, T5, T6, T7>(string path, PackageType type, PackageFlags flags, IReadOnlyList<string> names) => GeneratePackage(path, type, flags, names, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        public static void GeneratePackage<T1, T2, T3, T4, T5, T6, T7, T8>(string path, PackageType type, PackageFlags flags, IReadOnlyList<string> names) => GeneratePackage(path, type, flags, names, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        public static void GeneratePackage<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string path, PackageType type, PackageFlags flags, IReadOnlyList<string> names) => GeneratePackage(path, type, flags, names, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
        public static void GeneratePackage<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(string path, PackageType type, PackageFlags flags, IReadOnlyList<string> names) => GeneratePackage(path, type, flags, names, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));

        public static void GeneratePackage(string path, PackageType type, PackageFlags flags, IReadOnlyList<string> names, params Type[] data)
        {
            string name = type.ToString() + "Package.cs";
            string fullPath = Path.Combine(path, name);
            if (File.Exists(fullPath))
            {
                Debug.LogWarning($"Package of a type {type} already exists");
            }

            if (!ValidateObjects(data, out int totalSize, out bool isSizeVariable))
            {
                Debug.LogError("Failed to generate package");
                return;
            }

            names = ValidateList(names);
            string res = GenerateUpperHeader(type, flags, totalSize, isSizeVariable);
            GenerateMiddlePart(ref res, data, type.ToString() + "Package", names);

            using (var stream = File.Exists(fullPath) ? File.Open(fullPath, FileMode.Truncate) : File.Open(fullPath, FileMode.CreateNew))
            {
                byte[] buffer = Encoding.Default.GetBytes(res);
                stream.Write(buffer);
            }
        }

        public static string GenerateUpperHeader(PackageType type, PackageFlags flags, int size, bool isSizeVariable)
        {
            string packageTypeName = Enum.GetName(typeof(PackageType), type);
            string flagsFormatted = FormatFlags(flags);
            string res = "";
            res += "//the following code was auto-generated.\n";
            res += "#pragma warning disable IDE\n\n";
            res += "using System;\nusing System.Runtime.InteropServices;\nusing System.Text;\n";
            res += "\n";
            res += "namespace Networking.Packages\n";
            res += "{\n";
            res += $"\t[PackageAttribute({flagsFormatted}, PackageType.{packageTypeName})]\n";
            res += $"\tpublic sealed class {type.ToString() + "Package"} : IPackage\n";
            res += "\t{\n";
            res += $"\t\tpublic PackageType Type => PackageType.{packageTypeName};\n";
            res += $"\t\tpublic PackageFlags Flags => {flagsFormatted};\n";
            if (isSizeVariable)
            {
                res += "\t\tpublic int DataSize => VariableSize + CONSTANT_DATA_SIZE;\n";
                res += $"\t\tprivate const int CONSTANT_DATA_SIZE = {size};\n";
            }
            else
            {
                res += $"\t\tpublic int DataSize => {size};\n";
            }
            res += "\n";
            return res;
        }

        public static void GenerateMiddlePart(ref string upperPart, Type[] data, string className, IReadOnlyList<string> names)
        {
            Dictionary<Type, int> usedNames = new Dictionary<Type, int>();
            List<string> stringNames = new List<string>();
            List<int> arraySizes = new List<int>();
            List<string> arrayNames = new List<string>();

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].IsPrimitive || data[i].IsValueType)
                {
                    string typeName = data[i].Name + "Gen";

                    if (usedNames.TryGetValue(data[i], out int num))
                    {
                        typeName += num.ToString();
                        usedNames[data[i]]++;
                    }
                    else
                    {
                        usedNames.Add(data[i], 1);
                    }
                    TryGetName(i, names, out typeName);

                    upperPart += "\t\tpublic " + data[i].FullName + " " + typeName + ";\n";
                }
                else if (data[i] == typeof(string))
                {
                    string name = "StringGen";
                    if (usedNames.TryGetValue(typeof(string), out int num))
                    {
                        name += num.ToString();
                    }
                    else
                    {
                        usedNames.Add(typeof(string), 1);
                    }

                    TryGetName(i, names, out name);
                    upperPart += "\t\tpublic " + "short" + " " + name + "Size;\n";
                    upperPart += "\t\tpublic " + "string" + " " + name + ";\n";
                    stringNames.Add(name);
                }
                else if (data[i].IsArray)
                {
                    string name = "ArrayGen";
                    if (usedNames.TryGetValue(typeof(Array), out int num))
                    {
                        name += num.ToString();
                    }
                    else
                    {
                        usedNames.Add(typeof(Array), 1);
                    }
                    TryGetName(i, names, out name);

                    var element = data[i].GetElementType();
                    upperPart += "\t\tprivate " + "short" + " " + name + "Size;\n";
                    upperPart += "\t\tpublic " + element.FullName + "[] " + name + ";\n";
                    arrayNames.Add(name);
                    arraySizes.Add(Marshal.SizeOf(data[i].GetElementType()));
                }
            }

            if (arrayNames.Count > 0 || stringNames.Count > 0)
            {
                string variableSize = "\t\tprivate int VariableSize => ";
                for (int i = 0; i < arrayNames.Count; i++)
                {
                    if (i > 0)
                    {
                        variableSize += " + ";
                    }
                    variableSize += arrayNames[i] + ".Length * " + arraySizes[i];
                }

                for (int i = 0; i < stringNames.Count; i++)
                {
                    if (i > 0 || arrayNames.Count > 0)
                    {
                        variableSize += " + ";
                    }
                    variableSize += $"Encoding.Unicode.GetBytes({stringNames[i]}).Length";
                }

                variableSize += ";\n";
                upperPart += variableSize;
            }
            upperPart += "\n";

            upperPart += $"\t\tpublic {className}()" + "{}\n";
            if (data.Length > 0)
            {
                upperPart += $"\t\tpublic {className}(";
                usedNames = new Dictionary<Type, int>();
                for (int i = 0; i < data.Length; i++)
                {
                    string varName = data[i].FullName + "Gen";
                    var type = data[i];
                    if (data[i].IsArray)
                    {
                        varName = data[i].GetElementType() + "arrayGen";
                        type = typeof(Array);
                    }

                    if (usedNames.TryGetValue(type, out int num))
                    {
                        varName += num.ToString();
                        usedNames[type]++;
                    }
                    else
                    {
                        usedNames.Add(type, 1);
                    }
                    var splitted = varName.Split('.');
                    string last = char.ToLower(splitted[splitted.Length - 1][0]) + splitted[splitted.Length - 1].Substring(1);
                    string res = "";
                    for (int j = 0; j < splitted.Length - 1; ++j)
                    {
                        res += splitted[j];
                    }
                    res += last;
                    varName = res;

                    if (i > 0)
                    {
                        upperPart += ", ";
                    }
                    if (TryGetName(i, names, out varName))
                    {
                        varName = char.ToLower(varName[0]) + varName.Substring(1);
                    }

                    upperPart += data[i].FullName + " " + varName;
                }
                upperPart += ")\n";
                upperPart += "\t\t{\n";
                usedNames = new Dictionary<Type, int>();
                for (int i = 0; i < data.Length; i++)
                {
                    string varName = data[i].FullName + "Gen";
                    string fullName = data[i].Name + "Gen";
                    var type = data[i];
                    if (data[i].IsArray)
                    {
                        varName = data[i].GetElementType() + "arrayGen";
                        fullName = "ArrayGen";
                        type = typeof(Array);
                    }

                    if (usedNames.TryGetValue(type, out int num))
                    {
                        varName += num.ToString();
                        fullName += num.ToString();
                        usedNames[type]++;
                    }
                    else
                    {
                        usedNames.Add(type, 1);
                    }
                    var splitted = varName.Split('.');
                    string last = char.ToLower(splitted[splitted.Length - 1][0]) + splitted[splitted.Length - 1].Substring(1);
                    string res = "";
                    for (int j = 0; j < splitted.Length - 1; ++j)
                    {
                        res += splitted[j];
                    }
                    res += last;
                    varName = res;

                    if (TryGetName(i, names, out fullName))
                    {
                        varName = char.ToLower(fullName[0]) + fullName.Substring(1);
                    }
                    upperPart += $"\t\t\t{fullName} = {varName};\n";
                }
                upperPart += "\t\t}\n";
            }
            usedNames = GenereteSerializeMethod(ref upperPart, data, names);
            usedNames = GenerateDeserializeMethod(ref upperPart, data, names);
            upperPart += "\t}\n";
            upperPart += "}\n";
        }

        private static Dictionary<Type, int> GenerateDeserializeMethod(ref string upperPart, Type[] data, IReadOnlyList<string> names)
        {
            Dictionary<Type, int> usedNames = new Dictionary<Type, int>();
            upperPart += "\t\tpublic void Deserialize(ReadOnlySpan<byte> data, int offset)\n";
            upperPart += "\t\t{\n";
            upperPart += "\t\t\tint localOffset = 0;\n";
            for (int i = 0; i < data.Length; i++)
            {
                var type = data[i];
                if (type.IsPrimitive)
                {
                    string typeName = data[i].Name + "Gen";

                    if (usedNames.TryGetValue(data[i], out int num))
                    {
                        typeName += num.ToString();
                        usedNames[data[i]]++;
                    }
                    else
                    {
                        usedNames.Add(data[i], 1);
                    }
                    TryGetName(i, names, out typeName);

                    if (type == typeof(float))
                    {
                        upperPart += $"\t\t\t{typeName} = BitConverter.Int32BitsToSingle(BitConverter.ToInt32(data[(offset + localOffset)..]));\n";
                        upperPart += $"\t\t\tlocalOffset += sizeof({data[i].Name});\n";
                    }
                    else if (type == typeof(double))
                    {
                        upperPart += $"\t\t\t{typeName} = BitConverter.Int64BitsToSingle(BitConverter.ToInt64(data[(offset + localOffset)..]));\n";
                        upperPart += $"\t\t\tlocalOffset += sizeof({data[i].Name});\n";
                    }
                    else if (type == typeof(byte))
                    {
                        upperPart += $"\t\t\t{typeName} = data[offset + localOffset];\n";
                        upperPart += $"\t\t\tlocalOffset++;\n";
                    }
                    else if (type == typeof(char))
                    {
                        upperPart += $"\t\t\t{typeName} = (char)data[offset + localOffset];\n";
                        upperPart += $"\t\t\tlocalOffset++;\n";
                    }
                    else
                    {
                        upperPart += $"\t\t\t{typeName} = BitConverter.To{type.Name}(data[(offset + localOffset)..]);\n";
                        upperPart += $"\t\t\tlocalOffset += sizeof({data[i].Name});\n";
                    }
                }
                else if (type == typeof(string))
                {
                    string typeName = "StringGen";

                    if (usedNames.TryGetValue(typeof(string), out int num))
                    {
                        typeName += num.ToString();
                        usedNames[data[i]]++;
                    }
                    else
                    {
                        usedNames.Add(data[i], 1);
                    }
                    TryGetName(i, names, out typeName);

                    upperPart += $"\t\t\t{typeName + "Size"} = BitConverter.ToInt16(data[(offset + localOffset)..]);\n";
                    upperPart += $"\t\t\tlocalOffset += sizeof(short);\n";
                    upperPart += $"\t\t\tif({typeName + "Size"} > 0)\n";
                    upperPart += "\t\t\t{\n";
                    upperPart += $"\t\t\t\t{typeName} = Encoding.Unicode.GetString(data[(offset + localOffset)..]);\n";
                    upperPart += $"\t\t\t\tlocalOffset += Encoding.Unicode.GetBytes({typeName}).Length;\n";
                    upperPart += "\t\t\t}\n";
                }
                else if (type.IsArray)
                {
                    string typeName = "ArrayGen";

                    if (usedNames.TryGetValue(typeof(Array), out int num))
                    {
                        typeName += num.ToString();
                        usedNames[typeof(Array)]++;
                    }
                    else
                    {
                        usedNames.Add(typeof(Array), 1);
                    }
                    TryGetName(i, names, out typeName);

                    upperPart += $"\t\t\t{typeName + "Size"} = BitConverter.ToInt16(data[(offset + localOffset)..]);\n";
                    upperPart += $"\t\t\tlocalOffset += sizeof(short);\n";
                    upperPart += $"\t\t\tif({typeName + "Size"} > 0)\n";
                    upperPart += "\t\t\t{\n";
                    if (type.GetElementType() == typeof(byte))
                    {
                        upperPart += $"\t\t\t\tvar split = data[(offset + localOffset)..(offset + localOffset + {typeName + "Size"})];\n";
                        upperPart += $"\t\t\t\t{typeName} = data.ToArray();\n";
                    }
                    else
                    {
                        upperPart += $"\t\t\t\tvar {typeName}Cast = MemoryMarshal.Cast<byte, {type.GetElementType().FullName}>(data[(offset + localOffset)..(offset + localOffset + {typeName + "Size"} * sizeof({type.GetElementType().FullName}))]);\n";
                        upperPart += $"\t\t\t\t{typeName} = {typeName}Cast.ToArray();\n";
                        upperPart += $"\t\t\t\tlocalOffset += {typeName + "Size"} * Marshal.SizeOf(typeof({type.GetElementType().FullName}));\n";
                    }
                    upperPart += "\t\t\t}\n";
                }
            }

            upperPart += "\t\t}\n";
            return usedNames;
        }

        private static Dictionary<Type, int> GenereteSerializeMethod(ref string upperPart, Type[] data, IReadOnlyList<string> names)
        {
            Dictionary<Type, int> usedNames = new Dictionary<Type, int>();
            upperPart += "\t\tpublic void Serialize(byte[] data, int offset)\n";
            upperPart += "\t\t{\n";
            upperPart += "\t\t\tint localOffset = 0;\n";
            for (int i = 0; i < data.Length; i++)
            {
                var type = data[i];
                if (type.IsPrimitive)
                {
                    string typeName = data[i].Name + "Gen";

                    if (usedNames.TryGetValue(data[i], out int num))
                    {
                        typeName += num.ToString();
                        usedNames[data[i]]++;
                    }
                    else
                    {
                        usedNames.Add(data[i], 1);
                    }
                    TryGetName(i, names, out typeName);

                    if (type == typeof(char))
                    {
                        upperPart += $"\t\t\tdata[offset + localOffset] = (byte){typeName};\n";
                        upperPart += $"\t\t\tlocalOffset++;\n";
                    }
                    else if (type == typeof(byte))
                    {
                        upperPart += $"\t\t\tdata[offset + localOffset] = {typeName};\n";
                        upperPart += $"\t\t\tlocalOffset++;\n";
                    }
                    else if (type == typeof(float))
                    {
                        upperPart += $"\t\t\tBitConverter.SingleToInt32Bits({typeName}).Convert(data, offset + localOffset);\n";
                        upperPart += $"\t\t\tlocalOffset += sizeof({data[i].Name});\n";
                    }
                    else if (type == typeof(double))
                    {
                        upperPart += $"\t\t\tBitConverter.DoubleToInt64Bits({typeName}).Convert(data, offset + localOffset);\n";
                        upperPart += $"\t\t\tlocalOffset += sizeof({data[i].Name});\n";
                    }
                    else
                    {
                        upperPart += $"\t\t\t{typeName}.Convert(data, offset + localOffset);\n";
                        upperPart += $"\t\t\tlocalOffset += sizeof({data[i].Name});\n";
                    }
                }
                else if (type == typeof(string))
                {
                    string typeName = "StringGen";

                    if (usedNames.TryGetValue(typeof(string), out int num))
                    {
                        typeName += num.ToString();
                        usedNames[data[i]]++;
                    }
                    else
                    {
                        usedNames.Add(data[i], 1);
                    }
                    TryGetName(i, names, out typeName);

                    upperPart += $"\t\t\t{typeName + "Size"} = (short){typeName}.Length;\n";
                    upperPart += $"\t\t\t{typeName + "Size"}.Convert(data, offset + localOffset);\n";
                    upperPart += $"\t\t\tlocalOffset += sizeof(short);\n";
                    upperPart += $"\t\t\tvar bytes{typeName} = Encoding.Unicode.GetBytes({typeName});\n";
                    upperPart += $"\t\t\tArray.Copy(bytes{typeName}, 0, data, offset + localOffset, bytes{typeName}.Length);\n";
                    upperPart += $"\t\t\tlocalOffset += bytes{typeName}.Length;\n";
                }
                else if (type.IsArray)
                {
                    string typeName = "ArrayGen";

                    if (usedNames.TryGetValue(typeof(Array), out int num))
                    {
                        typeName += num.ToString();
                        usedNames[typeof(Array)]++;
                    }
                    else
                    {
                        usedNames.Add(typeof(Array), 1);
                    }
                    var elementType = data[i].GetElementType();
                    TryGetName(i, names, out typeName);

                    upperPart += $"\t\t\t{typeName + "Size"} = (short){typeName}.Length;\n";
                    upperPart += $"\t\t\t{typeName + "Size"}.Convert(data, offset + localOffset);\n";
                    upperPart += $"\t\t\tlocalOffset += sizeof(short);\n";
                    upperPart += $"\t\t\tif({typeName + "Size"} > 0)\n";
                    upperPart += "\t\t\t{\n";
                    upperPart += $"\t\t\t\tint singleSize{typeName} = Marshal.SizeOf({typeName}[0]);\n";
                    upperPart += $"\t\t\t\tfor(int i = 0; i < {typeName}.Length; ++i)\n";
                    upperPart += "\t\t\t\t{\n";
                    if (elementType.IsPrimitive)
                    {
                        if (elementType == typeof(float))
                        {
                            upperPart += $"\t\t\t\t\tBitConverter.SingleToInt32Bits({typeName}[i]).Convert(data, offset + localOffset);\n";
                        }
                        else if (elementType == typeof(double))
                        {
                            upperPart += $"\t\t\t\t\tBitConverter.DoubleToInt64Bits({typeName}[i]).Convert(data, offset + localOffset);\n";
                        }
                        else if (elementType == typeof(char))
                        {
                            upperPart += $"\t\t\t\t\tdata[i] = (byte){typeName}[i];\n";
                        }
                        else
                        {
                            upperPart += $"\t\t\t\t\t{typeName}[i].Convert(data, offset + localOffset);\n";
                        }
                        upperPart += $"\t\t\t\t\tlocalOffset += singleSize{typeName};\n";
                    }//todo add structs support
                    upperPart += "\t\t\t\t}\n";
                    upperPart += "\t\t\t}\n";
                }
            }
            upperPart += "\t\t}\n";
            upperPart += "\t\t\n";
            return usedNames;
        }

        public static string FormatFlags(PackageFlags flags)
        {
            if (flags == PackageFlags.None) { return "PackageFlags.None"; }

            string result = "";
            int num = 1;
            for(int i = 0; i < 32; i++)
            {
                if(((int)flags & num) != 0)
                {
                    if (!string.IsNullOrEmpty(result))
                    {
                        result += " | "; 
                    }
                    result += "PackageFlags." + Enum.GetName(typeof(PackageFlags), num);
                }
                num <<= 1;
            }

            return result;
        }

        private static bool TryGetName(int ind, IReadOnlyList<string> names, out string name)
        {
            name = "";
            if (names.Count <= ind || string.IsNullOrEmpty(names[ind])) return false;
            else
            {
                name = names[ind];
                return true;
            }
        }

        private static IReadOnlyList<string> ValidateList(IReadOnlyList<string> names)
        {
            if(names == null) { return new string[0]; }

            Dictionary<string, int> usedNames = new Dictionary<string, int>();
            List<string> result = new List<string>();
            for(int i = 0; i < names.Count; i++)
            {
                string name = names[i];
                if(usedNames.TryGetValue(name, out int num))
                {
                    usedNames[name]++;
                    name += num.ToString();
                }
                else
                {
                    usedNames.Add(name, num);
                }

                if (char.IsNumber(name[0]))
                {
                    char first = name[0];
                    string numName = "";
                    switch (first)
                    {
                        case '0':
                            {
                                numName = "Zero";
                                break;
                            }
                        case '1':
                            {
                                numName = "One";
                                break;
                            }
                        case '2':
                            {
                                numName = "Two";
                                break;
                            }
                        case '3':
                            {
                                numName = "Three";
                                break;
                            }
                        case '4':
                            {
                                numName = "Four";
                                break;
                            }
                        case '5':
                            {
                                numName = "Five";
                                break;
                            }
                        case '6':
                            {
                                numName = "Six";
                                break;
                            }
                        case '7':
                            {
                                numName = "Seven";
                                break;
                            }
                        case '8':
                            {
                                numName = "Eight";
                                break;
                            }
                        case '9':
                            {
                                numName = "Nine";
                                break;
                            }
                    }

                    name = numName + name.Substring(1);
                }
                else if (char.IsUpper(name[0]))
                {
                    name = char.ToUpper(name[0]) + name.Substring(1);
                }

                result.Add(name);
            }

            return result;
        }

        private static bool ValidateObjects(Type[] data, out int totalSize, out bool isSizeVariable)
        {
            bool res = true;
            totalSize = 0;
            isSizeVariable = false;

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].IsPrimitive || data[i] == typeof(string))
                {
                    if (data[i] == typeof(string))
                    {
                        totalSize += sizeof(short);
                        isSizeVariable = true;
                        continue;
                    }
                    totalSize += Marshal.SizeOf(data[i]);
                    continue;
                }

                if (data[i].IsArray)
                {
                    Type elementType = data[i].GetElementType();
                    if(!elementType.IsPrimitive && !elementType.IsValueType)
                    {
                        Debug.LogError($"Array of type {elementType} cannot be serialized");
                        res = false;
                        continue;
                    }

                    totalSize += sizeof(short);
                    isSizeVariable = true;
                    continue;
                }

                if (!data[i].IsValueType)
                {
                    res = false;
                    Debug.LogError($"Type {data[i].Name} cannot be serialized");
                }
                else
                {
                    totalSize += Marshal.SizeOf(data[i]);
                }
            }

            return res;
        }
    }
}