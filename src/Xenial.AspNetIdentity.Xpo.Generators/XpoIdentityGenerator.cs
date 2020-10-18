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

        protected virtual IEnumerable<(string attributeFieldName, string propertyName, bool isAggregated, string[] additionalAttributes)> ManyFields { get; }

        public void Execute(GeneratorExecutionContext context)
        {
            var attributeSource = SourceText.From(AttributeText, Encoding.UTF8);

            context.AddSource(AttributeFullName, attributeSource);

            var options = (context.Compilation as CSharpCompilation).SyntaxTrees[0].Options as CSharpParseOptions;
            var compilation = context.Compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(attributeSource, options));

            var attributeSymbol = compilation.GetTypeByMetadataName(AttributeFullName);

#if DEBUG
            // if (!System.Diagnostics.Debugger.IsAttached)
            // {
            //     System.Diagnostics.Debugger.Launch();
            // }
#endif

            if (context.SyntaxReceiver is SyntaxReceiver syntaxReceiver)
            {
                var roleType = syntaxReceiver.ClassToAugment;

                var model = compilation.GetSemanticModel(roleType.SyntaxTree);
                var classSymbol = (INamedTypeSymbol)model.GetDeclaredSymbol(roleType);

                var attributes = classSymbol.GetAttributes();
                var attributeData = attributes.Single(ad => ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default));

                var userTypeSymbol = attributeData.NamedArguments.FirstOrDefault(a => a.Key == "UserType");

                var textWriter = new StringWriter();
                var writer = new IndentedTextWriter(textWriter);

                if (roleType.Parent.IsKind(SyntaxKind.NamespaceDeclaration) && roleType.Parent is NamespaceDeclarationSyntax namespaceDeclarationSyntax)
                {
                    writer.WriteLine("using System;");
                    writer.WriteLine();
                    writer.WriteLine("using DevExpress.Xpo;");
                    writer.WriteLine();
                    writer.WriteLine("using Microsoft.AspNetCore.Identity;");
                    writer.WriteLine();
                    using (new CurlyIndenter(writer, $"namespace {namespaceDeclarationSyntax.Name}"))
                    using (new CurlyIndenter(writer, $"partial class {roleType.Identifier}"))
                    {
                        writer.Indent++;
                        foreach (var (fieldType, fieldName, size, additionalAttributes) in Fields)
                        {
                            writer.WriteLine($"private {ToType(fieldType)} {LowerCaseFirstLetter(fieldName)};");
                            writer.WriteLine($"[Persistent(\"{fieldName}\")]");
                            if (size > 0)
                            {
                                writer.WriteLine($"[Size({size})]");
                            }
                            foreach (var additionalAttribute in additionalAttributes)
                            {
                                writer.WriteLine($"[{additionalAttribute}]");
                            }
                            var propertyDeclaration = $"public {ToType(fieldType)} {fieldName} {{ get => {LowerCaseFirstLetter(fieldName)}; set => SetPropertyValue(\"{fieldName}\", ref {LowerCaseFirstLetter(fieldName)}, value); }}";
                            writer.WriteLine(propertyDeclaration);
                            writer.WriteLine();
                        }

                        if (userTypeSymbol.Key is not null && userTypeSymbol.Value is TypedConstant userTypedConstant)
                        {
                            var userTypeName = userTypedConstant.Value.ToString();
                            writer.WriteLine();
                            var propertyDeclaration = $"public XPCollection<{userTypeName}> Users => GetCollection<{userTypeName}>(\"Users\");";
                            writer.WriteLine("[Association]");
                            writer.WriteLine(propertyDeclaration);
                            writer.WriteLine();
                        }

                        writer.Indent--;
                    }
                    var source = textWriter.ToString();
                    var sourceFileName = $"{roleType.Identifier}.XPIdentityRole.generated.cs";
                    context.AddSource(sourceFileName, SourceText.From(source, Encoding.UTF8));
                    File.WriteAllText($@"C:\F\tmp\{sourceFileName}", source);
                }
            }
        }

        public void Initialize(GeneratorInitializationContext context)
           => context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

        private class SyntaxReceiver : ISyntaxReceiver
        {
            public ClassDeclarationSyntax ClassToAugment { get; private set; }

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is ClassDeclarationSyntax cds
                    && cds.AttributeLists.Count > 0
                    && cds.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword))
                )
                {
                    ClassToAugment = cds;
                }
            }
        }

        private string LowerCaseFirstLetter(string str)
        {
            if (str != string.Empty && char.IsUpper(str[0]))
            {
                str = char.ToLower(str[0]) + str.Substring(1);
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
