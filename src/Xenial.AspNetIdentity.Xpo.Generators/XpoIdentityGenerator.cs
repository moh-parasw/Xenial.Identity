using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Xenial.AspNetIdentity.Xpo.Generators
{
    public abstract class XpoIdentityGenerator : ISourceGenerator
    {
        protected abstract string AttributeText { get; }
        protected abstract string AttributeFullName { get; }
        protected abstract string AttributeName { get; }
        protected abstract string Name { get; }

        protected virtual IEnumerable<(Type type, string name, int size, string[] additionalAttributes)> Fields { get; }

        protected virtual IEnumerable<(string attributeFieldName, string propertyName, bool isAggregated, string[] additionalAttributes)> ManyFields { get { yield break; } }
        protected virtual IEnumerable<(string attributeFieldName, string propertyName, bool isAggregated, string[] additionalAttributes)> OneFields { get { yield break; } }

        public void Execute(GeneratorExecutionContext context)
        {
            var attributeSource = SourceText.From(AttributeText, Encoding.UTF8);

            context.AddSource(AttributeName, attributeSource);

            var options = (context.Compilation as CSharpCompilation).SyntaxTrees[0].Options as CSharpParseOptions;
            var compilation = context.Compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(attributeSource, options));

            var attributeSymbol = compilation.GetTypeByMetadataName(AttributeFullName);


            if (context.SyntaxReceiver is SyntaxReceiver syntaxReceiver)
            {
#if DEBUG

                //if (!System.Diagnostics.Debugger.IsAttached)
                //{
                //    System.Diagnostics.Debugger.Launch();
                //}
#endif

                foreach (var canidate in syntaxReceiver.Canidates)
                {
                    var targetType = canidate;

                    var model = compilation.GetSemanticModel(targetType.SyntaxTree);
                    var classSymbol = model.GetDeclaredSymbol(targetType);

                    var attributes = classSymbol.GetAttributes();
                    var attributeData = attributes.SingleOrDefault(ad => ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default));

                    if (attributeData != null
                        && targetType.Parent.IsKind(SyntaxKind.NamespaceDeclaration)
                        && targetType.Parent is NamespaceDeclarationSyntax namespaceDeclarationSyntax
                    )
                    {
                        var textWriter = new StringWriter();
                        var writer = new IndentedTextWriter(textWriter);

                        writer.WriteLine("using System;");
                        writer.WriteLine();
                        writer.WriteLine("using DevExpress.Xpo;");
                        writer.WriteLine();
                        writer.WriteLine("using Microsoft.AspNetCore.Identity;");
                        writer.WriteLine();
                        using (new CurlyIndenter(writer, $"namespace {namespaceDeclarationSyntax.Name}"))
                        using (new CurlyIndenter(writer, $"partial class {targetType.Identifier}"))
                        {
                            writer.Indent++;
                            foreach (var (fieldType, fieldName, size, additionalAttributes) in Fields)
                            {
                                writer.WriteLine($"private {ToType(fieldType)} {GetFieldName(fieldName)};");
                                writer.WriteLine($"[Persistent(\"{fieldName}\")]");
                                if (size > 0)
                                {
                                    writer.WriteLine($"[Size({size})]");
                                }
                                foreach (var additionalAttribute in additionalAttributes)
                                {
                                    writer.WriteLine($"[{additionalAttribute}]");
                                }
                                var propertyDeclaration = $"public {ToType(fieldType)} {fieldName} {{ get => {GetFieldName(fieldName)}; set => SetPropertyValue(\"{fieldName}\", ref {GetFieldName(fieldName)}, value); }}";
                                writer.WriteLine(propertyDeclaration);
                                writer.WriteLine();
                            }
                            if (attributeData is not null)
                            {
                                foreach (var manyField in ManyFields)
                                {
                                    var manySymbol = attributeData.NamedArguments.FirstOrDefault(a => a.Key == manyField.attributeFieldName);

                                    if (manySymbol.Key is not null && manySymbol.Value is TypedConstant userTypedConstant)
                                    {
                                        var manyTypeName = userTypedConstant.Value.ToString();
                                        writer.WriteLine();
                                        var propertyDeclaration = $"public XPCollection<{manyTypeName}> {manyField.propertyName} => GetCollection<{manyTypeName}>(\"{manyField.propertyName}\");";
                                        writer.WriteLine("[Association]");
                                        if (manyField.isAggregated)
                                        {
                                            writer.WriteLine("[Aggregated]");
                                        }

                                        foreach (var additionalAttribute in manyField.additionalAttributes)
                                        {
                                            writer.WriteLine($"[{additionalAttribute}]");
                                        }

                                        writer.WriteLine(propertyDeclaration);
                                        writer.WriteLine();
                                    }
                                }

                                foreach (var oneField in OneFields)
                                {
                                    var oneSymbol = attributeData.NamedArguments.FirstOrDefault(a => a.Key == oneField.attributeFieldName);

                                    if (oneSymbol.Key is not null && oneSymbol.Value is TypedConstant userTypedConstant)
                                    {
                                        var oneTypeName = userTypedConstant.Value.ToString();
                                        writer.WriteLine();
                                        writer.WriteLine($"private {oneTypeName} {GetFieldName(oneField.propertyName)};");
                                        var propertyDeclaration = $"public {oneTypeName} {oneField.propertyName} {{ get => {GetFieldName(oneField.propertyName)}; set => SetPropertyValue(\"{oneField.propertyName}\", ref {GetFieldName(oneField.propertyName)}, value); }}";
                                        writer.WriteLine($"[Persistent(\"{oneField.propertyName}Id\")]");
                                        writer.WriteLine("[Association]");
                                        if (oneField.isAggregated)
                                        {
                                            writer.WriteLine("[Aggregated]");
                                        }

                                        foreach (var additionalAttribute in oneField.additionalAttributes)
                                        {
                                            writer.WriteLine($"[{additionalAttribute}]");
                                        }

                                        writer.WriteLine(propertyDeclaration);
                                        writer.WriteLine();
                                    }
                                }
                            }

                            writer.Indent--;
                        }
                        var source = textWriter.ToString();
                        var sourceFileName = $"{targetType.Identifier}.{Name}.generated.cs";
                        context.AddSource(sourceFileName, SourceText.From(source, Encoding.UTF8));
                        File.WriteAllText($@"C:\F\tmp\{sourceFileName}", source);
                    }
                }
            }
        }

        public void Initialize(GeneratorInitializationContext context)
           => context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

        private class SyntaxReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> Canidates { get; } = new List<ClassDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is ClassDeclarationSyntax cds
                    && cds.AttributeLists.Count > 0
                    && cds.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword))
                )
                {
                    Canidates.Add(cds);
                }
            }
        }

        private string GetFieldName(string str)
        {
            if (str != string.Empty && char.IsUpper(str[0]))
            {
                str = char.ToLower(str[0]) + str.Substring(1);
            }
            if (str == "value") //Reserved keyword for fieldName
            {
                return "val";
            }
            return str;
        }

        private string ToType(Type type)
        {
            if (Nullable.GetUnderlyingType(type) != null)
            {
                return $"Nullable<{Nullable.GetUnderlyingType(type).FullName}>";
            }
            return type.FullName;
        }
    }
}
