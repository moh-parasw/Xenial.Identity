using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Xenial.AspNetIdentity.Xpo.Generators
{
    [Generator]
    public class XpoIdentityRoleGenerator : ISourceGenerator
    {
        private const string attributeText = @"
using System;
namespace Xenial.AspNetIdentity.Xpo
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class XPIdentityRoleAttribute : Attribute
    {
        public XPIdentityRoleAttribute() { }

        public Type UserType { get; set; }
        public Type ClaimsType { get; set; }
    }
}
";
        public void Initialize(GeneratorInitializationContext context)
            => context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

        public void Execute(GeneratorExecutionContext context)
        {
            context.AddSource("XPIdentityRoleAttribute", SourceText.From(attributeText, Encoding.UTF8));

            CSharpParseOptions options = (context.Compilation as CSharpCompilation).SyntaxTrees[0].Options as CSharpParseOptions;
            Compilation compilation = context.Compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(attributeText, Encoding.UTF8), options));

            // get the newly bound attribute, and INotifyPropertyChanged
            INamedTypeSymbol attributeSymbol = compilation.GetTypeByMetadataName("Xenial.AspNetIdentity.Xpo.XPIdentityRoleAttribute");

#if DEBUG
            if (!System.Diagnostics.Debugger.IsAttached)
            {
              System.Diagnostics.Debugger.Launch();
            }
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
                    var fields = new[]
                    {
                        (typeof(string), "Name", 50, new [] { "Indexed(Unique = true)" }),
                        (typeof(string), "NormalizedName", 50, new [] { "Indexed" }),
                    };

                    writer.WriteLine("using System;");
                    writer.WriteLine();
                    writer.WriteLine("using DevExpress.Xpo;");
                    writer.WriteLine();
                    writer.WriteLine("using Microsoft.AspNetCore.Identity;");
                    writer.WriteLine();
                    using (new CurlyIndenter(writer, $"namespace {namespaceDeclarationSyntax.Name}"))
                    using (new CurlyIndenter(writer, $"partial class {roleType.Identifier}"))
                    {
                        string LowerCaseFirstLetter(string str)
                        {
                            if (str != string.Empty && char.IsUpper(str[0]))
                            {
                                str = char.ToLower(str[0]) + str.Substring(1);
                            }
                            return str;
                        }

                        string ToType(Type type)
                        {
                            if (Nullable.GetUnderlyingType(type) != null)
                            {
                                return $"Nullable<{Nullable.GetUnderlyingType(type).FullName}>";
                            }
                            return type.FullName;
                        }

                        writer.Indent++;
                        foreach (var (fieldType, fieldName, size, additionalAttributes) in fields)
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

                        if(userTypeSymbol.Key is not null && userTypeSymbol.Value is TypedConstant userTypedConstant)
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
    }
}
