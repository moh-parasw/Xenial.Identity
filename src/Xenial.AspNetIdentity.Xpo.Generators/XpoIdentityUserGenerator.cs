using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
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
    public class XpoIdentityUserGenerator : ISourceGenerator
    {
        private const string attributeText = @"
using System;
namespace Xenial.AspNetIdentity.Xpo
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class XPIdentityUserAttribute : Attribute
    {
        public XPIdentityUserAttribute() { }
    }
}
";
        public void Initialize(GeneratorInitializationContext context)
            => context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());

        public void Execute(GeneratorExecutionContext context)
        {
            context.AddSource("IdentityUserAttribute", SourceText.From(attributeText, Encoding.UTF8));
#if DEBUG
            //if (!Debugger.IsAttached)
            //{
            //    Debugger.Launch();
            //}
#endif

            if (context.SyntaxReceiver is SyntaxReceiver syntaxReceiver)
            {
                var userType = syntaxReceiver.ClassToAugment;

                var textWriter = new StringWriter();
                var writer = new IndentedTextWriter(textWriter);

                if (userType.Parent.IsKind(SyntaxKind.NamespaceDeclaration) && userType.Parent is NamespaceDeclarationSyntax namespaceDeclarationSyntax)
                {
                    var fields = new[]
                    {
                        (typeof(string), "UserName", 250, new [] { "Indexed(Unique = true)", "ProtectedPersonalData" }),
                        (typeof(string), "NormalizedUserName", 250, new [] { "Indexed" }),
                        (typeof(string), "Email", 250, new [] { "Indexed(Unique = true)", "ProtectedPersonalData" }),
                        (typeof(string), "NormalizedEmail", 250, new [] { "Indexed" }),
                        (typeof(bool), "EmailConfirmed", 0, new [] { "PersonalData" }),
                        (typeof(string), "PasswordHash", 50000, new string[0]),
                        (typeof(string), "SecurityStamp", 500, new string[0]),
                        (typeof(string), "PhoneNumber", 50, new [] { "ProtectedPersonalData"}),
                        (typeof(bool), "PhoneNumberConfirmed", 0, new [] { "PersonalData" }),
                        (typeof(bool), "TwoFactorEnabled", 0, new string [0]),
                        (typeof(DateTime?), "LockoutEnd", 0, new string [0]),
                        (typeof(bool), "LockoutEnabled", 0, new string [0]),
                        (typeof(int), "AccessFailedCount", 0, new string [0]),
                    };

                    writer.WriteLine("using System;");
                    writer.WriteLine();
                    writer.WriteLine("using DevExpress.Xpo;");
                    writer.WriteLine();
                    writer.WriteLine("using Microsoft.AspNetCore.Identity;");
                    writer.WriteLine();
                    using (new CurlyIndenter(writer, $"namespace {namespaceDeclarationSyntax.Name}"))
                    using (new CurlyIndenter(writer, $"partial class {userType.Identifier}"))
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
                        writer.Indent--;
                    }
                    var source = textWriter.ToString();

                    context.AddSource($"{userType.Identifier}.generated.cs", SourceText.From(source, Encoding.UTF8));
                    File.WriteAllText(@"C:\F\tmp\1.cs", source);
                }
            }
        }

        private class SyntaxReceiver : ISyntaxReceiver
        {
            public ClassDeclarationSyntax ClassToAugment { get; private set; }

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {

                // Business logic to decide what we're interested in goes here
                if (syntaxNode is ClassDeclarationSyntax cds
                    && cds.AttributeLists.Count > 0
                    && cds.Modifiers.Any(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword))
                )
                {
                    ClassToAugment = cds;
                }
            }
        }
    }
}
