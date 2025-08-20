# NetForge.Simulation.Scripting - Project Structure

This document outlines the complete project structure for the NetForge.Simulation.Scripting project, including all directories, files, and their purposes.

## Directory Structure

```
NetForge.Simulation.Scripting/
├── 📁 Core/                              # Core scripting engine components
│   ├── 📁 Lexer/                         # Tokenization and lexical analysis
│   │   ├── 📄 ILexer.cs                  # Lexer interface
│   │   ├── 📄 Lexer.cs                   # Main lexer implementation
│   │   ├── 📄 Token.cs                   # Token definitions
│   │   ├── 📄 TokenType.cs               # Token type enumeration
│   │   ├── 📄 LexerException.cs          # Lexer-specific exceptions
│   │   └── 📄 LexerOptions.cs            # Lexer configuration options
│   ├── 📁 Parser/                        # Syntax parsing and AST generation
│   │   ├── 📁 AST/                       # Abstract Syntax Tree nodes
│   │   │   ├── 📄 AstNode.cs             # Base AST node class
│   │   │   ├── 📄 ScriptNode.cs          # Root script node
│   │   │   ├── 📄 StatementNodes.cs      # Statement AST nodes
│   │   │   ├── 📄 ExpressionNodes.cs     # Expression AST nodes
│   │   │   ├── 📄 DeclarationNodes.cs    # Declaration AST nodes
│   │   │   ├── 📄 ControlFlowNodes.cs    # Control flow AST nodes
│   │   │   └── 📄 IAstVisitor.cs         # Visitor pattern interface
│   │   ├── 📄 IParser.cs                 # Parser interface
│   │   ├── 📄 NetSimParser.cs            # Main recursive descent parser
│   │   ├── 📄 ParseResult.cs             # Parser result container
│   │   ├── 📄 ParseError.cs              # Parser error information
│   │   ├── 📄 ParseException.cs          # Parser exceptions
│   │   └── 📄 ErrorRecovery.cs           # Error recovery utilities
│   ├── 📁 Interpreter/                   # Script execution engine
│   │   ├── 📄 IScriptInterpreter.cs      # Interpreter interface
│   │   ├── 📄 ScriptInterpreter.cs       # Main script interpreter
│   │   ├── 📄 ScriptExecutor.cs          # High-level script executor
│   │   ├── 📄 ExecutionContext.cs        # Execution context container
│   │   ├── 📄 ExecutionOptions.cs        # Execution configuration
│   │   ├── 📄 ExecutionResult.cs         # Execution result container
│   │   ├── 📄 RuntimeException.cs        # Runtime exceptions
│   │   └── 📄 CallStack.cs               # Function call stack management
│   └── 📁 Runtime/                       # Runtime environment and context
│       ├── 📄 IRuntimeEnvironment.cs     # Runtime environment interface
│       ├── 📄 RuntimeEnvironment.cs      # Main runtime environment
│       ├── 📄 VariableStore.cs           # Variable storage and management
│       ├── 📄 FunctionRegistry.cs        # Function registration system
│       ├── 📄 Scope.cs                   # Scope management
│       ├── 📄 SymbolTable.cs             # Symbol table for variables/functions
│       └── 📄 RuntimeValue.cs            # Runtime value wrapper
├── 📁 Language/                          # Language definitions and semantics
│   ├── 📁 Syntax/                        # Grammar definitions and syntax rules
│   │   ├── 📄 Grammar.ebnf               # Extended BNF grammar definition
│   │   ├── 📄 SyntaxRules.cs             # Syntax validation rules
│   │   ├── 📄 Keywords.cs                # Language keyword definitions
│   │   ├── 📄 Operators.cs               # Operator definitions and precedence
│   │   └── 📄 LiteralParsers.cs          # Literal value parsers
│   ├── 📁 Semantics/                     # Semantic analysis and type checking
│   │   ├── 📄 ISemanticAnalyzer.cs       # Semantic analyzer interface
│   │   ├── 📄 SemanticAnalyzer.cs        # Main semantic analyzer
│   │   ├── 📄 TypeChecker.cs             # Type checking implementation
│   │   ├── 📄 TypeSystem.cs              # Type system definitions
│   │   ├── 📄 SemanticError.cs           # Semantic error information
│   │   ├── 📄 SemanticException.cs       # Semantic exceptions
│   │   └── 📄 ReferenceResolver.cs       # Variable/function reference resolution
│   └── 📁 Builtins/                      # Built-in functions and commands
│       ├── 📄 BuiltinFunctions.cs        # Built-in function registry
│       ├── 📄 BuiltinFunction.cs         # Base built-in function class
│       ├── 📁 Network/                   # Network-specific functions
│       │   ├── 📄 PingFunction.cs        # Ping implementation
│       │   ├── 📄 TraceRouteFunction.cs  # Traceroute implementation
│       │   ├── 📄 ConfigureFunction.cs   # Device configuration function
│       │   └── 📄 TestFunction.cs        # Network testing functions
│       ├── 📁 Utilities/                 # Utility functions
│       │   ├── 📄 PrintFunction.cs       # Print/logging functions
│       │   ├── 📄 WaitFunction.cs        # Delay/wait functions
│       │   ├── 📄 StringFunctions.cs     # String manipulation
│       │   ├── 📄 MathFunctions.cs       # Mathematical functions
│       │   └── 📄 CollectionFunctions.cs # Array/object functions
│       └── 📁 IO/                        # I/O operations
│           ├── 📄 FileOperations.cs      # File read/write functions
│           ├── 📄 HttpFunctions.cs       # HTTP client functions
│           ├── 📄 DatabaseFunctions.cs   # Database operations
│           └── 📄 ExternalCommands.cs    # External command execution
├── 📁 Integration/                       # External system integration
│   ├── 📁 NetForge/                      # NetForge.Player integration
│   │   ├── 📄 INetForgeContext.cs        # NetForge integration interface
│   │   ├── 📄 NetForgePlayerContext.cs   # NetForge.Player implementation
│   │   ├── 📄 NetworkManager.cs          # Network management wrapper
│   │   ├── 📄 DeviceManager.cs           # Device management wrapper
│   │   └── 📄 EventBridge.cs             # Event system integration
│   ├── 📁 Devices/                       # Device management integration
│   │   ├── 📄 IDeviceProvider.cs         # Device provider interface
│   │   ├── 📄 DeviceFactory.cs           # Device creation factory
│   │   ├── 📄 VendorSpecifics/           # Vendor-specific implementations
│   │   │   ├── 📄 CiscoProvider.cs       # Cisco device provider
│   │   │   ├── 📄 JuniperProvider.cs     # Juniper device provider
│   │   │   ├── 📄 AristaProvider.cs      # Arista device provider
│   │   │   └── 📄 GenericProvider.cs     # Generic device provider
│   │   ├── 📄 CommandTranslator.cs       # Command syntax translation
│   │   └── 📄 ConfigurationValidator.cs  # Configuration validation
│   └── 📁 Testing/                       # Test framework integration
│       ├── 📄 ITestFramework.cs          # Test framework interface
│       ├── 📄 TestFramework.cs           # Main testing implementation
│       ├── 📄 TestCase.cs                # Individual test case
│       ├── 📄 TestSuite.cs               # Test suite container
│       ├── 📄 TestResult.cs              # Test execution results
│       ├── 📄 AssertionEngine.cs         # Assertion validation
│       └── 📄 TestReporter.cs            # Test result reporting
├── 📁 Extensions/                        # Extensibility framework
│   ├── 📁 Libraries/                     # Standard procedure libraries
│   │   ├── 📄 CommonProcedures.netsim    # Common utility procedures
│   │   ├── 📄 CiscoProcedures.netsim     # Cisco-specific procedures
│   │   ├── 📄 JuniperProcedures.netsim   # Juniper-specific procedures
│   │   ├── 📄 SecurityBaseline.netsim    # Security configuration templates
│   │   ├── 📄 RoutingProtocols.netsim    # Routing protocol configurations
│   │   └── 📄 NetworkTesting.netsim      # Network testing procedures
│   ├── 📁 Templates/                     # Configuration templates
│   │   ├── 📄 ITemplate.cs               # Template interface
│   │   ├── 📄 TemplateEngine.cs          # Template processing engine
│   │   ├── 📄 TemplateRegistry.cs        # Template registration system
│   │   ├── 📄 ConfigurationTemplate.cs  # Configuration template base
│   │   └── 📁 Vendor/                    # Vendor-specific templates
│   │       ├── 📄 CiscoTemplates.cs      # Cisco configuration templates
│   │       ├── 📄 JuniperTemplates.cs    # Juniper configuration templates
│   │       └── 📄 AristaTemplates.cs     # Arista configuration templates
│   └── 📁 Plugins/                       # Plugin framework
│       ├── 📄 IPlugin.cs                 # Plugin interface
│       ├── 📄 PluginManager.cs           # Plugin management system
│       ├── 📄 PluginLoader.cs            # Dynamic plugin loading
│       ├── 📄 PluginMetadata.cs          # Plugin information
│       └── 📄 IFunctionPlugin.cs         # Function plugin interface
├── 📁 Tools/                             # Development and runtime tools
│   ├── 📁 CLI/                           # Command-line script executor
│   │   ├── 📄 Program.cs                 # Main entry point
│   │   ├── 📄 ScriptRunner.cs            # Script execution runner
│   │   ├── 📄 CommandLineOptions.cs     # CLI argument parsing
│   │   ├── 📄 ConsoleLogger.cs           # Console logging implementation
│   │   └── 📄 NetForge.Simulation.Scripting.CLI.csproj
│   ├── 📁 Debugger/                      # Script debugging tools
│   │   ├── 📄 IDebugger.cs               # Debugger interface
│   │   ├── 📄 ScriptDebugger.cs          # Script debugging implementation
│   │   ├── 📄 Breakpoint.cs              # Breakpoint management
│   │   ├── 📄 DebugSession.cs            # Debug session management
│   │   ├── 📄 VariableInspector.cs       # Variable inspection
│   │   └── 📄 CallStackInspector.cs      # Call stack inspection
│   └── 📁 Formatter/                     # Code formatting and linting
│       ├── 📄 IFormatter.cs              # Formatter interface
│       ├── 📄 ScriptFormatter.cs         # Script code formatter
│       ├── 📄 FormattingOptions.cs       # Formatting configuration
│       ├── 📄 ScriptLinter.cs            # Script linting and validation
│       ├── 📄 LintingRule.cs             # Individual linting rules
│       └── 📄 CodeAnalyzer.cs            # Static code analysis
├── 📁 Examples/                          # Example scripts and templates
│   ├── 📁 Basic/                         # Basic examples for learning
│   │   ├── 📄 hello_world.netsim         # Simple hello world example
│   │   ├── 📄 simple_network.netsim      # Basic two-device network
│   │   ├── 📄 device_configuration.netsim # Device configuration example
│   │   ├── 📄 connectivity_test.netsim   # Basic connectivity testing
│   │   └── 📄 variable_usage.netsim      # Variable and interpolation example
│   ├── 📁 Intermediate/                  # Intermediate complexity examples
│   │   ├── 📄 multi_vendor_setup.netsim  # Multi-vendor network setup
│   │   ├── 📄 routing_protocols.netsim   # OSPF/BGP configuration
│   │   ├── 📄 vlan_configuration.netsim  # VLAN setup and management
│   │   ├── 📄 control_flow.netsim        # Loops and conditionals
│   │   └── 📄 functions_templates.netsim # Custom functions and templates
│   ├── 📁 Advanced/                      # Advanced examples
│   │   ├── 📄 enterprise_network.netsim  # Large enterprise network
│   │   ├── 📄 datacenter_fabric.netsim   # Data center network fabric
│   │   ├── 📄 wan_deployment.netsim      # WAN deployment scenario
│   │   ├── 📄 performance_testing.netsim # Network performance testing
│   │   ├── 📄 failover_testing.netsim    # Failover and redundancy testing
│   │   └── 📄 security_validation.netsim # Security configuration validation
│   ├── 📁 Templates/                     # Reusable templates
│   │   ├── 📄 cisco_baseline.netsim      # Cisco device baseline
│   │   ├── 📄 juniper_baseline.netsim    # Juniper device baseline  
│   │   ├── 📄 security_hardening.netsim  # Security hardening template
│   │   ├── 📄 monitoring_setup.netsim    # SNMP monitoring setup
│   │   └── 📄 backup_configuration.netsim # Configuration backup procedures
│   └── 📄 README.md                      # Examples documentation
├── 📁 Tests/                             # Comprehensive test suite
│   ├── 📁 Unit/                          # Unit tests
│   │   ├── 📁 Core/                      # Core component tests
│   │   │   ├── 📄 LexerTests.cs          # Lexer unit tests
│   │   │   ├── 📄 ParserTests.cs         # Parser unit tests
│   │   │   ├── 📄 InterpreterTests.cs    # Interpreter unit tests
│   │   │   └── 📄 RuntimeTests.cs        # Runtime unit tests
│   │   ├── 📁 Language/                  # Language feature tests
│   │   │   ├── 📄 SyntaxTests.cs         # Syntax validation tests
│   │   │   ├── 📄 SemanticsTests.cs      # Semantic analysis tests
│   │   │   └── 📄 BuiltinTests.cs        # Built-in function tests
│   │   └── 📁 Integration/               # Integration layer tests
│   │       ├── 📄 NetForgeIntegrationTests.cs # NetForge integration tests
│   │       └── 📄 DeviceProviderTests.cs # Device provider tests
│   ├── 📁 Integration/                   # Integration tests
│   │   ├── 📄 EndToEndTests.cs           # Complete script execution tests
│   │   ├── 📄 ScenarioTests.cs           # Real-world scenario tests
│   │   ├── 📄 PerformanceTests.cs        # Performance benchmarking
│   │   └── 📄 NetForgePlayerTests.cs     # NetForge.Player integration tests
│   ├── 📁 Fixtures/                      # Test fixtures and data
│   │   ├── 📄 TestScripts/               # Test script files
│   │   ├── 📄 ExpectedResults/           # Expected test outputs
│   │   └── 📄 MockData/                  # Mock data for testing
│   └── 📁 TestUtilities/                 # Test utility classes
│       ├── 📄 MockNetForgeContext.cs     # Mock NetForge context
│       ├── 📄 TestExecutionContext.cs    # Test execution context
│       └── 📄 ScriptTestHelper.cs        # Script testing utilities
├── 📁 Documentation/                     # Project documentation
│   ├── 📁 API/                           # API documentation
│   │   ├── 📄 README.md                  # API overview
│   │   ├── 📄 Core.md                    # Core components API
│   │   ├── 📄 Language.md                # Language features API
│   │   ├── 📄 Integration.md             # Integration API
│   │   └── 📄 Extensions.md              # Extensions API
│   ├── 📁 Guides/                        # User guides
│   │   ├── 📄 GettingStarted.md          # Getting started guide
│   │   ├── 📄 ScriptingGuide.md          # Complete scripting guide
│   │   ├── 📄 IntegrationGuide.md        # Integration guide
│   │   ├── 📄 BestPractices.md           # Best practices and patterns
│   │   └── 📄 Troubleshooting.md         # Troubleshooting guide
│   ├── 📁 Tutorials/                     # Step-by-step tutorials
│   │   ├── 📄 BasicNetworking.md         # Basic networking tutorial
│   │   ├── 📄 AdvancedScenarios.md       # Advanced scenarios tutorial
│   │   └── 📄 TestingFramework.md        # Testing framework tutorial
│   └── 📁 Reference/                     # Reference materials
│       ├── 📄 LanguageReference.md       # Complete language reference
│       ├── 📄 FunctionReference.md       # Built-in functions reference
│       ├── 📄 ErrorReference.md          # Error codes and messages
│       └── 📄 ConfigurationReference.md  # Configuration options reference
├── 📄 NetForge.Simulation.Scripting.csproj # Main project file
├── 📄 README.md                          # Main project README (created)
├── 📄 NETSIM_SCRIPTING_FORMAT.md         # Language specification (created)
├── 📄 NETSIM_FORMAT_PARSER_IMPLEMENTATION_PLAN.md # Implementation plan (created)
├── 📄 PROJECT_STRUCTURE.md               # This file
├── 📄 CHANGELOG.md                       # Project changelog
├── 📄 CONTRIBUTING.md                    # Contributing guidelines
├── 📄 LICENSE                            # Project license
├── 📄 .gitignore                         # Git ignore rules
├── 📄 .editorconfig                      # Editor configuration
├── 📄 global.json                        # .NET global configuration
└── 📄 Directory.Build.props              # MSBuild properties
```

## File Purposes and Responsibilities

### Core Components (`Core/`)

**Lexer** - Tokenization and lexical analysis of NetSim scripts
- Converts raw script text into structured tokens
- Handles string interpolation, comments, and special literals
- Provides error reporting for lexical issues

**Parser** - Syntax analysis and AST generation
- Parses token streams into Abstract Syntax Tree (AST)
- Implements recursive descent parsing with error recovery
- Validates syntax according to NetSim grammar rules

**Interpreter** - Script execution engine
- Executes parsed AST nodes using visitor pattern
- Manages execution context and variable scoping
- Handles control flow, function calls, and error handling

**Runtime** - Runtime environment and variable management
- Provides variable storage and scope management
- Implements function registry and built-in function system
- Manages execution state and call stack

### Language Definitions (`Language/`)

**Syntax** - Grammar definitions and parsing rules
- Contains formal grammar specification in EBNF format
- Defines language keywords, operators, and precedence rules
- Provides syntax validation utilities

**Semantics** - Type checking and semantic analysis
- Validates variable references and function calls
- Implements type checking system for expressions
- Resolves symbols and detects semantic errors

**Builtins** - Built-in function library
- Provides comprehensive library of networking functions
- Implements utility functions for string, math, and I/O operations
- Extensible framework for adding custom built-in functions

### Integration Layer (`Integration/`)

**NetForge** - Integration with NetForge.Player
- Provides bridge between scripting engine and NetForge simulation
- Implements device and network management interfaces
- Handles event system integration

**Devices** - Device management and vendor abstraction
- Abstracts device creation and management across vendors
- Provides command translation for vendor-specific syntax
- Validates configurations against device capabilities

**Testing** - Test framework integration
- Implements comprehensive testing framework for network validation
- Provides assertion engine and test result reporting
- Supports both unit testing and integration testing

### Extensions Framework (`Extensions/`)

**Libraries** - Standard procedure libraries
- Provides reusable NetSim script libraries for common tasks
- Includes vendor-specific procedure collections
- Supports modular script development

**Templates** - Configuration templates
- Implements template engine for dynamic configuration generation
- Provides vendor-specific configuration templates
- Supports parameterized and conditional templates

**Plugins** - Plugin framework for extensibility
- Enables dynamic loading of custom function libraries
- Provides plugin registration and management system
- Supports third-party extensions

### Development Tools (`Tools/`)

**CLI** - Command-line script execution
- Standalone executable for running NetSim scripts
- Supports command-line arguments and variable passing
- Provides comprehensive logging and error reporting

**Debugger** - Script debugging utilities
- Interactive debugger for step-through script execution
- Breakpoint management and variable inspection
- Call stack analysis and execution flow visualization

**Formatter** - Code formatting and analysis
- Automatic code formatting according to style guidelines
- Linting and static analysis for code quality
- Style validation and best practice enforcement

## Dependencies and Relationships

### Internal Dependencies
```
CLI → Core → Language → Extensions
     ↓      ↓         ↓
Integration → Testing → Templates
```

### External Dependencies
- **.NET 9.0**: Target framework
- **NetForge.Simulation.Common**: Core simulation interfaces
- **Microsoft.Extensions.DependencyInjection**: Dependency injection
- **Microsoft.Extensions.Logging**: Logging abstraction
- **System.Text.Json**: JSON serialization
- **ANTLR4** (Optional): Advanced parsing capabilities

### Optional Dependencies
- **Newtonsoft.Json**: Alternative JSON library
- **System.Data.SqlClient**: Database integration
- **Microsoft.Extensions.Http**: HTTP client functionality

## Build Configuration

### Project References
```xml
<ProjectReference Include="..\NetForge.Simulation.Common\NetForge.Simulation.Common.csproj" />
<ProjectReference Include="..\NetForge.Simulation.Protocols.Common\NetForge.Simulation.Protocols.Common.csproj" />
```

### Package References
```xml
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="9.0.0" />
<PackageReference Include="System.Text.Json" Version="9.0.0" />
<PackageReference Include="Antlr4.Runtime.Standard" Version="4.13.1" />
```

### Build Targets
- **Debug**: Development builds with full debugging information
- **Release**: Optimized builds for production deployment
- **Test**: Test-specific builds with code coverage instrumentation

This comprehensive project structure provides a solid foundation for implementing the NetSim scripting language and parser according to the implementation plan.