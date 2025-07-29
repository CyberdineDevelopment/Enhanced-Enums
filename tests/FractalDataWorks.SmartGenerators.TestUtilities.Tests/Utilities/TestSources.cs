namespace FractalDataWorks.SmartGenerators.TestUtilities.Tests.Utilities;

/// <summary>
/// Contains test source code used across tests.
/// </summary>
internal static class TestSources
{
    #region Simple Classes
    public const string SimpleClassSource = """

                                            namespace TestNamespace;

                                            /// <summary>
                                            /// A simple test class.
                                            /// </summary>
                                            public class SimpleClass
                                            {
                                                /// <summary>
                                                /// Gets or sets the name.
                                                /// </summary>
                                                public string Name { get; set; }
                                            
                                                /// <summary>
                                                /// A test method.
                                                /// </summary>
                                                /// <param name="input">The input string.</param>
                                                /// <returns>A result string.</returns>
                                                public string TestMethod(string input)
                                                {
                                                    return $"Processed: {input}";
                                                }
                                            }
                                            """;

    public const string ComplexClassSource = """

                                             namespace TestNamespace;

                                             /// <summary>
                                             /// A more complex test class.
                                             /// </summary>
                                             public class ComplexClass : BaseClass, ITestInterface
                                             {
                                                 private readonly string _value;
                                             
                                                 /// <summary>
                                                 /// Initializes a new instance of the <see cref="ComplexClass"/> class.
                                                 /// </summary>
                                                 /// <param name="value">The initial value.</param>
                                                 public ComplexClass(string value)
                                                 {
                                                     _value = value;
                                                 }
                                             
                                                 /// <summary>
                                                 /// Gets the value.
                                                 /// </summary>
                                                 public string Value => _value;
                                             
                                                 /// <summary>
                                                 /// A test method implementing the interface.
                                                 /// </summary>
                                                 /// <param name="value">The input value.</param>
                                                 /// <returns>A result value.</returns>
                                                 public int ProcessValue(int value)
                                                 {
                                                     return value * 2;
                                                 }
                                             }
                                             """;

    public const string MultipleClassesSource = """

                                                namespace TestNamespace;

                                                public class FirstClass
                                                {
                                                    public string Property1 { get; set; }
                                                }

                                                public class SecondClass
                                                {
                                                    public int Property2 { get; set; }
                                                }
                                                """;

    public const string NamespaceSource = """

                                          namespace FirstNamespace;

                                          public class ClassA { }

                                          namespace SecondNamespace;

                                          public class ClassB { }
                                          """;
    #endregion

    #region Generator Test Sources
    public const string AttributeClassSource = """

                                               namespace TestNamespace;

                                               [GenerateCode]
                                               public class AttributeClass
                                               {
                                                   public string Name { get; set; }
                                               }
                                               """;

    public const string NoAttributeClassSource = """

                                                 namespace TestNamespace;

                                                 public class NoAttributeClass
                                                 {
                                                     public string Value { get; set; }
                                                 }
                                                 """;

    public const string PersonClassSource = """

                                             namespace TestNamespace;

                                             [GenerateEquals]
                                             public class Person
                                             {
                                                 public string FirstName { get; set; }
                                                 public string LastName { get; set; }
                                                 public int Age { get; set; }
                                             }
                                             """;

    public const string ProductClassSource = """

                                              namespace TestNamespace;

                                              [GenerateEquals]
                                              public class Product
                                              {
                                                  public int Id { get; set; }
                                                  public string Name { get; set; }
                                                  public decimal Price { get; set; }
                                                  public string Category { get; set; }
                                                  public bool IsAvailable { get; set; }
                                              }
                                              """;

    public const string GenerateCodeAttributeSource = """

                                                      namespace TestNamespace;

                                                      [System.AttributeUsage(System.AttributeTargets.Class)]
                                                      public class GenerateCodeAttribute : System.Attribute
                                                      {
                                                      }
                                                      """;

    public const string GenerateEqualsAttributeSource = """

                                                        namespace TestNamespace;

                                                        [System.AttributeUsage(System.AttributeTargets.Class)]
                                                        public class GenerateEqualsAttribute : System.Attribute
                                                        {
                                                        }
                                                        """;
    #endregion

    #region Method and Class Tests
    public const string ClassWithMembersSource = """

                                                 namespace TestNamespace;

                                                 public class TestClass
                                                 {
                                                     private string _field;
                                                     
                                                     public TestClass(string value)
                                                     {
                                                         _field = value;
                                                     }
                                                     
                                                     public string Property { get; set; }
                                                     
                                                     public int GetLength()
                                                     {
                                                         return _field.Length;
                                                     }
                                                     
                                                     public void SetValue(string value)
                                                     {
                                                         _field = value;
                                                     }
                                                 }
                                                 """;

    public const string MethodsSource = """

                                        namespace TestNamespace;

                                        public class TestClass
                                        {
                                            public void SimpleMethod()
                                            {
                                                // Method implementation
                                            }
                                            
                                            public string GetValue()
                                            {
                                                return "Value";
                                            }
                                            
                                            public int Add(int a, int b)
                                            {
                                                return a + b;
                                            }
                                            
                                            protected virtual bool Validate(string input, ValidationOptions options)
                                            {
                                                return !string.IsNullOrEmpty(input);
                                            }
                                            
                                            private static void LogMessage(string message)
                                            {
                                                // Log implementation
                                            }
                                            
                                            public static TestClass Create(string name)
                                            {
                                                return new TestClass { Name = name };
                                            }
                                            
                                            public string Name { get; set; }
                                        }
                                        """;

    public const string InvocableClassSource = """

                                               namespace TestNamespace;

                                               public class Calculator
                                               {
                                                   public static int Add(int a, int b)
                                                   {
                                                       return a + b;
                                                   }
                                                   
                                                   public string FormatResult(int value)
                                                   {
                                                       return $"Result: {value}";
                                                   }
                                               }
                                               """;
    #endregion
}
