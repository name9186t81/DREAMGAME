using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Networking.Utils
{
    [CreateAssetMenu(fileName = "ProcessorGenerator", menuName = "Networking/ProcessorGeneratorDescriptor")]
    public class PackageProcessorGenerator : ScriptableObject
    {
        private enum Permission
        {
            Server,
            Client,
            Both
        }
        [SerializeField] private string _filePath;
        [SerializeField] private PackageType _type;
        [SerializeField] private Permission _permission;
        [SerializeField] private bool _separateClientAndServerMethods;
        [SerializeField] private bool _serverConnectionValidation;

        public void Generate()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => (typeof(IPackage)).IsAssignableFrom(t));
            Type choosenType = null;
            foreach(var type in types)
            {
                var attribute = type.GetCustomAttribute<PackageAttribute>();
                if(attribute == null || attribute.PackageType != _type)
                {
                    continue;
                }

                choosenType = type;
                break;
            }

            if(choosenType == null)
            {
                Debug.LogError("Cant find package for " + _type.ToString() + " total types searched - " + types.Count());
                return;
            }

            string name = _type.ToString() + "Processor.cs";
            string fullPath = Path.Combine(Application.dataPath + "/" + _filePath, name);
            if (File.Exists(fullPath))
            {
                Debug.LogWarning($"Package of a type {_type} already exists");
            }

            string s = GenerateHeader();
            s = GenerateBaseMethod(s, choosenType);
            s = CloseClass(s);

            if (!Directory.Exists(Path.GetDirectoryName(fullPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            }

            using (var stream = File.Exists(fullPath) ? File.Open(fullPath, FileMode.Truncate) : File.Open(fullPath, FileMode.CreateNew))
            {
                byte[] buffer = Encoding.Default.GetBytes(s);
                stream.Write(buffer);
            }
        }

        private string GenerateHeader()
        {
            string res = string.Empty;
            res = $"//the following code was partly auto-generated\nusing Networking.Packages;\nusing System;\nusing System.Net;\nusing System.Threading;\nusing System.Threading.Tasks;\n\nnamespace Networking\n{{\n\t[Processor(PackageType.{_type.ToString()}{(_permission == Permission.Both ? "" : _permission == Permission.Server ? "ProcessorAttribute.ProcessorType.Server" : "ProcessorAttribute.ProcessorType.Client")})]\n\tpublic sealed class {_type.ToString()}Processor : IPackageProcessor\n\t{{\n";
            return res;
        }

        private string GenerateBaseMethod(string s, Type type)
        {
            s += "\t\tpublic Task<bool> Process(ReadOnlySpan<byte> data, CancellationTokenSource cts, IPEndPoint sender, ListenerBase receiver)\n\t\t{\n";
            s += $"\t\t\t{type.Name} package = new {type.Name}();\n";
            s += $"\t\t\tpackage.Deserialize(data, package.GetOffset());\n";

            if (_separateClientAndServerMethods && _permission == Permission.Both)
            {
                s += "\t\t\tif(receiver is Server server)\n";
                s += "\t\t\t\tSolveForServer(package, cts, sender, server);\n";
                s += "\t\t\tif(receiver is Client client)\n";
                s += "\t\t\t\tSolveForClient(package, cts, sender, client);\n";
                s += "\t\t\treturn Task.FromResult(true);\n";
                s += "\t\t}\n";
                s = GenerateAdditionalMethod(s, type, true);
                s = GenerateAdditionalMethod(s, type, false);
            }
            else
            {
                if (_serverConnectionValidation && _permission != Permission.Client)
                {
                    s += "\t\t\tif (receiver is Server server && !server.IsUserConnected(sender))\n\t\t\t{\n";
                    s += "\t\t\t\tserver.DebugMessageWarning(\"Received package from unknown IP - \" + sender.ToString(), ListenerBase.DebugLevel.Low);\n";
                    s += "\t\t\t\treturn Task.FromResult(false);\n\t\t\t}\n";
                }
                s += "\t\t\treturn Task.FromResult(true);\n";
                s += "\t\t}\n";
            }

            return s;
        }

        private string GenerateAdditionalMethod(string s, Type type, bool isServer)
        {
            s += "\n";
            s += $"\t\tprivate void SolveFor{(isServer ? "Server" : "Client")}({type.Name} package, CancellationTokenSource cts, IPEndPoint sender, {(isServer ? "Server server" : "Client client")})\n\t\t{{";
            if(_serverConnectionValidation && isServer)
            {
                s += "\t\t\tif (!server.IsUserConnected(sender))\n\t\t\t{\n";
                s += "\t\t\t\tserver.DebugMessageWarning(\"Received package from unknown IP - \" + sender.ToString(), ListenerBase.DebugLevel.Low);\n";
                s += "\t\t\t\treturn;\n\t\t\t}\n";
            }
            s += "\t\t\t\n";
            s += "\t\t}\n";
            return s;
        }

        private string CloseClass(string s)
        {
            s += "\t}\n}";
            return s;
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(PackageProcessorGenerator))]
        private sealed class GeneratorEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if (GUILayout.Button("GenerateProcessor"))
                {
                    ((PackageProcessorGenerator)target).Generate();
                }
            }
        }
#endif
    }
}
