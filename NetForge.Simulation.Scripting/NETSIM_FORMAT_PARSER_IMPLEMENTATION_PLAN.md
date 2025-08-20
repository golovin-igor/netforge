# NetSim Format Parser Implementation Plan

## Project Overview

This document outlines the implementation plan for the NetSim scripting format parser within the `NetForge.Simulation.Scripting` project. The parser will enable automated network simulation, configuration, and testing through declarative `.netsim` scripts.

## Architecture Overview

### Core Components

```
NetForge.Simulation.Scripting/
├── Core/
│   ├── Lexer/                    # Tokenization and lexical analysis
│   ├── Parser/                   # Syntax analysis and AST generation  
│   ├── Interpreter/              # Script execution engine
│   └── Runtime/                  # Runtime environment and context
├── Language/
│   ├── Syntax/                   # Grammar definitions and syntax rules
│   ├── Semantics/                # Semantic analysis and type checking
│   └── Builtins/                 # Built-in functions and commands
├── Integration/
│   ├── NetForge/                 # NetForge.Player integration
│   ├── Devices/                  # Device management integration
│   └── Testing/                  # Test framework integration
├── Extensions/
│   ├── Libraries/                # Standard library procedures
│   ├── Templates/                # Configuration templates
│   └── Plugins/                  # Extensibility framework
└── Tools/
    ├── CLI/                      # Command-line script executor
    ├── Debugger/                 # Script debugging tools
    └── Formatter/                # Code formatting and linting
```

## Implementation Phases

### Phase 1: Foundation Layer (Weeks 1-3)

#### 1.1 Project Setup and Basic Infrastructure

**Deliverables:**
- `NetForge.Simulation.Scripting.csproj` with proper dependencies
- Basic project structure and namespaces
- Unit testing framework setup
- CI/CD pipeline configuration

**Dependencies:**
- .NET 9.0 runtime
- ANTLR4 or similar parser generator (optional)
- xUnit for testing
- Reference to `NetForge.Simulation.Common`

**Implementation Details:**
```csharp
// NetForge.Simulation.Scripting/NetForge.Simulation.Scripting.csproj
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <AssemblyTitle>NetForge Network Simulation Scripting Engine</AssemblyTitle>
  </PropertyGroup>
  
  <ItemGroup>
    <PackageReference Include="Antlr4.Runtime.Standard" Version="4.13.1" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="9.0.0" />
  </ItemGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\NetForge.Simulation.Common\NetForge.Simulation.Common.csproj" />
  </ItemGroup>
</Project>
```

#### 1.2 Lexical Analysis (Tokenizer)

**Purpose:** Break input text into tokens for parsing

**Key Classes:**
```csharp
namespace NetForge.Simulation.Scripting.Core.Lexer
{
    public enum TokenType
    {
        // Literals
        Identifier, StringLiteral, NumberLiteral, BooleanLiteral,
        IpAddressLiteral, NetworkLiteral,
        
        // Keywords
        If, Else, For, While, Function, Template, Test, Configure,
        CreateDevice, Link, Assert, Import, Export,
        
        // Operators
        Assignment, Equals, NotEquals, LessThan, GreaterThan,
        Plus, Minus, Multiply, Divide, Modulo,
        
        // Symbols
        LeftBrace, RightBrace, LeftParen, RightParen,
        LeftBracket, RightBracket, Comma, Semicolon, Dot,
        
        // Special
        Variable, Interpolation, Comment, Newline, EOF
    }
    
    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public string SourceFile { get; set; }
    }
    
    public class Lexer
    {
        private readonly string _input;
        private int _position;
        private int _line;
        private int _column;
        
        public Lexer(string input, string sourceFile = null) { }
        
        public IEnumerable<Token> Tokenize() { }
        public Token NextToken() { }
        
        private bool IsIdentifierStart(char c) { }
        private bool IsIdentifierPart(char c) { }
        private bool IsDigit(char c) { }
        private bool IsIpAddress(string text) { }
        private string ReadString() { }
        private string ReadNumber() { }
        private string ReadComment() { }
    }
}
```

**Key Features:**
- String interpolation support (`${variable}`)
- IP address and network literal recognition
- Multi-line comment support (`/* */`)
- Variable identification (`$variable`)
- Metadata annotation support (`@name`, `#!`)

#### 1.3 Abstract Syntax Tree (AST) Definitions

**Purpose:** Define the structure of parsed NetSim scripts

**AST Node Hierarchy:**
```csharp
namespace NetForge.Simulation.Scripting.Core.Parser.AST
{
    public abstract class AstNode
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public string SourceFile { get; set; }
        
        public abstract void Accept(IAstVisitor visitor);
        public abstract T Accept<T>(IAstVisitor<T> visitor);
    }
    
    // Program structure
    public class ScriptNode : AstNode
    {
        public List<MetadataNode> Metadata { get; set; } = new();
        public List<ImportNode> Imports { get; set; } = new();
        public List<VariableDeclarationNode> Variables { get; set; } = new();
        public List<StatementNode> Statements { get; set; } = new();
    }
    
    public class MetadataNode : AstNode
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
    
    // Statements
    public abstract class StatementNode : AstNode { }
    
    public class DeviceCreationNode : StatementNode
    {
        public string Vendor { get; set; }
        public ExpressionNode HostnameExpression { get; set; }
        public Dictionary<string, ExpressionNode> Options { get; set; } = new();
    }
    
    public class LinkCreationNode : StatementNode
    {
        public ExpressionNode Device1 { get; set; }
        public ExpressionNode Interface1 { get; set; }
        public ExpressionNode Device2 { get; set; }
        public ExpressionNode Interface2 { get; set; }
        public Dictionary<string, ExpressionNode> Properties { get; set; } = new();
    }
    
    public class ConfigurationNode : StatementNode
    {
        public ExpressionNode DeviceExpression { get; set; }
        public List<CommandNode> Commands { get; set; } = new();
    }
    
    public class CommandNode : AstNode
    {
        public string Command { get; set; }
        public List<ExpressionNode> Arguments { get; set; } = new();
    }
    
    // Control flow
    public class IfStatementNode : StatementNode
    {
        public ExpressionNode Condition { get; set; }
        public List<StatementNode> ThenStatements { get; set; } = new();
        public List<StatementNode> ElseStatements { get; set; } = new();
    }
    
    public class ForLoopNode : StatementNode
    {
        public string IteratorVariable { get; set; }
        public ExpressionNode IterableExpression { get; set; }
        public List<StatementNode> Body { get; set; } = new();
    }
    
    public class WhileLoopNode : StatementNode
    {
        public ExpressionNode Condition { get; set; }
        public List<StatementNode> Body { get; set; } = new();
    }
    
    // Functions and templates
    public class FunctionDeclarationNode : StatementNode
    {
        public string Name { get; set; }
        public List<ParameterNode> Parameters { get; set; } = new();
        public string ReturnType { get; set; }
        public List<StatementNode> Body { get; set; } = new();
    }
    
    public class TemplateDeclarationNode : StatementNode
    {
        public string Name { get; set; }
        public List<ParameterNode> Parameters { get; set; } = new();
        public List<StatementNode> Body { get; set; } = new();
    }
    
    // Expressions
    public abstract class ExpressionNode : AstNode { }
    
    public class VariableReferenceNode : ExpressionNode
    {
        public string VariableName { get; set; }
    }
    
    public class LiteralNode : ExpressionNode
    {
        public object Value { get; set; }
        public Type ValueType { get; set; }
    }
    
    public class BinaryOperationNode : ExpressionNode
    {
        public ExpressionNode Left { get; set; }
        public string Operator { get; set; }
        public ExpressionNode Right { get; set; }
    }
    
    public class FunctionCallNode : ExpressionNode
    {
        public string FunctionName { get; set; }
        public List<ExpressionNode> Arguments { get; set; } = new();
    }
    
    public class InterpolatedStringNode : ExpressionNode
    {
        public List<StringSegment> Segments { get; set; } = new();
    }
    
    public class StringSegment
    {
        public bool IsLiteral { get; set; }
        public string Text { get; set; }
        public ExpressionNode Expression { get; set; }
    }
}
```

### Phase 2: Parser Implementation (Weeks 4-6)

#### 2.1 Recursive Descent Parser

**Purpose:** Convert tokens into AST

**Grammar Definition (EBNF-style):**
```ebnf
script              ::= metadata* import* variable_declaration* statement*

metadata            ::= '@' identifier string_literal

import              ::= 'import' string_literal ('as' identifier)?

variable_declaration ::= '$' identifier '=' expression

statement           ::= device_creation 
                      | link_creation
                      | configuration
                      | control_flow
                      | function_declaration
                      | template_declaration
                      | test_block
                      | expression_statement

device_creation     ::= 'create-device' identifier expression option_block?

link_creation       ::= 'link' expression expression expression expression option_block?

configuration       ::= 'configure' expression command_block

command_block       ::= '{' command* '}'

command             ::= string_literal | template_call | control_flow

control_flow        ::= if_statement | for_loop | while_loop

if_statement        ::= 'if' '(' expression ')' statement_block ('else' statement_block)?

for_loop            ::= 'for' '$' identifier 'in' expression statement_block

while_loop          ::= 'while' '(' expression ')' statement_block

expression          ::= logical_or

logical_or          ::= logical_and ('||' logical_and)*

logical_and         ::= equality ('&&' equality)*

equality            ::= comparison (('==' | '!=') comparison)*

comparison          ::= addition (('<' | '>' | '<=' | '>=') addition)*

addition            ::= multiplication (('+' | '-') multiplication)*

multiplication      ::= unary (('*' | '/' | '%') unary)*

unary               ::= ('!' | '-')? primary

primary             ::= literal
                      | variable_reference  
                      | function_call
                      | '(' expression ')'
                      | array_literal
                      | object_literal

literal             ::= string_literal | number_literal | boolean_literal | ip_address_literal

array_literal       ::= '[' (expression (',' expression)*)? ']'

object_literal      ::= '{' (identifier ':' expression (',' identifier ':' expression)*)? '}'
```

**Parser Implementation:**
```csharp
namespace NetForge.Simulation.Scripting.Core.Parser
{
    public class NetSimParser
    {
        private readonly List<Token> _tokens;
        private int _position;
        private readonly List<ParseError> _errors;
        
        public NetSimParser(IEnumerable<Token> tokens) { }
        
        public ParseResult Parse() 
        {
            try
            {
                var script = ParseScript();
                return new ParseResult(script, _errors);
            }
            catch (ParseException ex)
            {
                _errors.Add(new ParseError(ex.Message, ex.Token));
                return new ParseResult(null, _errors);
            }
        }
        
        private ScriptNode ParseScript() { }
        private List<MetadataNode> ParseMetadata() { }
        private List<ImportNode> ParseImports() { }
        private StatementNode ParseStatement() { }
        private DeviceCreationNode ParseDeviceCreation() { }
        private LinkCreationNode ParseLinkCreation() { }
        private ConfigurationNode ParseConfiguration() { }
        private IfStatementNode ParseIfStatement() { }
        private ForLoopNode ParseForLoop() { }
        private ExpressionNode ParseExpression() { }
        private ExpressionNode ParseLogicalOr() { }
        private ExpressionNode ParseLogicalAnd() { }
        private ExpressionNode ParseEquality() { }
        private ExpressionNode ParseComparison() { }
        private ExpressionNode ParseAddition() { }
        private ExpressionNode ParseMultiplication() { }
        private ExpressionNode ParseUnary() { }
        private ExpressionNode ParsePrimary() { }
        
        private Token Peek() { }
        private Token Advance() { }
        private bool Check(TokenType type) { }
        private bool Match(params TokenType[] types) { }
        private Token Consume(TokenType type, string message) { }
        private void Synchronize() { }
    }
    
    public class ParseResult
    {
        public ScriptNode Script { get; }
        public List<ParseError> Errors { get; }
        public bool IsSuccess => Errors.Count == 0 && Script != null;
        
        public ParseResult(ScriptNode script, List<ParseError> errors) { }
    }
    
    public class ParseError
    {
        public string Message { get; }
        public Token Token { get; }
        public int Line => Token?.Line ?? 0;
        public int Column => Token?.Column ?? 0;
        
        public ParseError(string message, Token token) { }
    }
}
```

#### 2.2 Error Recovery and Reporting

**Advanced Error Handling:**
```csharp
public class ErrorRecovery
{
    public static void RecoverFromError(NetSimParser parser, TokenType[] synchronizationTokens)
    {
        // Skip tokens until we find a synchronization point
        while (!parser.IsAtEnd() && !parser.Check(synchronizationTokens))
        {
            parser.Advance();
        }
    }
    
    public static ParseError CreateDetailedError(Token token, string expected, string found)
    {
        return new ParseError(
            $"Expected {expected}, but found {found} at line {token.Line}, column {token.Column}",
            token
        );
    }
}
```

### Phase 3: Semantic Analysis (Weeks 7-8)

#### 3.1 Symbol Table and Scope Management

**Purpose:** Track variables, functions, and their scopes

```csharp
namespace NetForge.Simulation.Scripting.Core.Semantics
{
    public class Symbol
    {
        public string Name { get; set; }
        public SymbolType Type { get; set; }
        public object Value { get; set; }
        public bool IsConstant { get; set; }
        public int DeclarationLine { get; set; }
        public int DeclarationColumn { get; set; }
    }
    
    public enum SymbolType
    {
        Variable, Function, Template, Device, Interface
    }
    
    public class Scope
    {
        private readonly Dictionary<string, Symbol> _symbols = new();
        private readonly Scope _parent;
        
        public Scope(Scope parent = null) { }
        
        public void Define(string name, Symbol symbol) { }
        public Symbol Get(string name) { }
        public bool IsDefined(string name) { }
        public void Assign(string name, object value) { }
    }
    
    public class SymbolTable
    {
        private readonly Stack<Scope> _scopes = new();
        
        public void PushScope() { }
        public void PopScope() { }
        public void Define(string name, Symbol symbol) { }
        public Symbol Get(string name) { }
        public void Assign(string name, object value) { }
    }
}
```

#### 3.2 Type System and Validation

```csharp
public class TypeChecker : IAstVisitor
{
    private readonly SymbolTable _symbolTable;
    private readonly List<SemanticError> _errors;
    
    public TypeChecker(SymbolTable symbolTable) { }
    
    public List<SemanticError> CheckTypes(ScriptNode script)
    {
        _errors.Clear();
        script.Accept(this);
        return _errors;
    }
    
    public void VisitDeviceCreation(DeviceCreationNode node)
    {
        // Validate vendor name
        // Check hostname expression type
        // Validate option types
    }
    
    public void VisitVariableDeclaration(VariableDeclarationNode node)
    {
        // Check if variable already exists
        // Validate expression type
        // Add to symbol table
    }
    
    public void VisitFunctionCall(FunctionCallNode node)
    {
        // Check if function exists
        // Validate argument count and types
        // Return appropriate type
    }
}
```

### Phase 4: Runtime Environment (Weeks 9-11)

#### 4.1 Script Execution Engine

**Purpose:** Execute parsed and validated NetSim scripts

```csharp
namespace NetForge.Simulation.Scripting.Core.Runtime
{
    public class ScriptExecutor
    {
        private readonly INetForgeContext _netForgeContext;
        private readonly IRuntimeEnvironment _runtime;
        private readonly ILogger<ScriptExecutor> _logger;
        
        public ScriptExecutor(
            INetForgeContext netForgeContext,
            IRuntimeEnvironment runtime,
            ILogger<ScriptExecutor> logger) { }
        
        public async Task<ExecutionResult> ExecuteAsync(
            ScriptNode script, 
            ExecutionOptions options = null)
        {
            var context = new ExecutionContext(_netForgeContext, options);
            var interpreter = new ScriptInterpreter(context, _runtime);
            
            try
            {
                await interpreter.ExecuteAsync(script);
                return ExecutionResult.Success();
            }
            catch (RuntimeException ex)
            {
                _logger.LogError(ex, "Script execution failed");
                return ExecutionResult.Failure(ex.Message, ex.Line, ex.Column);
            }
        }
    }
    
    public class ScriptInterpreter : IAstVisitor
    {
        private readonly ExecutionContext _context;
        private readonly IRuntimeEnvironment _runtime;
        
        public ScriptInterpreter(ExecutionContext context, IRuntimeEnvironment runtime) { }
        
        public async Task ExecuteAsync(ScriptNode script)
        {
            // Execute imports first
            foreach (var import in script.Imports)
            {
                await ExecuteImport(import);
            }
            
            // Initialize variables
            foreach (var variable in script.Variables)
            {
                await ExecuteVariableDeclaration(variable);
            }
            
            // Execute statements
            foreach (var statement in script.Statements)
            {
                await ExecuteStatement(statement);
            }
        }
        
        public async Task ExecuteStatement(StatementNode statement)
        {
            switch (statement)
            {
                case DeviceCreationNode deviceNode:
                    await ExecuteDeviceCreation(deviceNode);
                    break;
                case LinkCreationNode linkNode:
                    await ExecuteLinkCreation(linkNode);
                    break;
                case ConfigurationNode configNode:
                    await ExecuteConfiguration(configNode);
                    break;
                case IfStatementNode ifNode:
                    await ExecuteIfStatement(ifNode);
                    break;
                case ForLoopNode forNode:
                    await ExecuteForLoop(forNode);
                    break;
                default:
                    throw new RuntimeException($"Unknown statement type: {statement.GetType()}");
            }
        }
        
        private async Task ExecuteDeviceCreation(DeviceCreationNode node)
        {
            var hostname = await EvaluateExpression(node.HostnameExpression);
            var options = new Dictionary<string, object>();
            
            foreach (var option in node.Options)
            {
                options[option.Key] = await EvaluateExpression(option.Value);
            }
            
            var device = await _context.NetForgeContext.CreateDeviceAsync(
                node.Vendor, 
                hostname.ToString(), 
                options);
            
            _context.Runtime.DefineVariable($"device_{hostname}", device);
        }
        
        private async Task ExecuteConfiguration(ConfigurationNode node)
        {
            var deviceName = await EvaluateExpression(node.DeviceExpression);
            var device = _context.NetForgeContext.GetDevice(deviceName.ToString());
            
            var commands = new List<string>();
            foreach (var command in node.Commands)
            {
                var commandText = await EvaluateCommand(command);
                commands.Add(commandText);
            }
            
            await device.ExecuteCommandsAsync(commands);
        }
        
        private async Task<object> EvaluateExpression(ExpressionNode expression)
        {
            return expression switch
            {
                LiteralNode literal => literal.Value,
                VariableReferenceNode variable => _context.Runtime.GetVariable(variable.VariableName),
                BinaryOperationNode binary => await EvaluateBinaryOperation(binary),
                FunctionCallNode function => await EvaluateFunctionCall(function),
                InterpolatedStringNode interpolated => await EvaluateInterpolatedString(interpolated),
                _ => throw new RuntimeException($"Unknown expression type: {expression.GetType()}")
            };
        }
    }
}
```

#### 4.2 Built-in Function Library

**Core Built-in Functions:**
```csharp
namespace NetForge.Simulation.Scripting.Language.Builtins
{
    public static class BuiltinFunctions
    {
        private static readonly Dictionary<string, BuiltinFunction> _functions = new()
        {
            ["print"] = new PrintFunction(),
            ["ping"] = new PingFunction(),
            ["wait"] = new WaitFunction(),
            ["assert"] = new AssertFunction(),
            ["range"] = new RangeFunction(),
            ["read_file"] = new ReadFileFunction(),
            ["write_file"] = new WriteFileFunction(),
            ["http_get"] = new HttpGetFunction(),
            ["http_post"] = new HttpPostFunction(),
            ["sql_query"] = new SqlQueryFunction(),
            ["exec"] = new ExecFunction(),
            ["format"] = new FormatFunction(),
            ["length"] = new LengthFunction(),
            ["split"] = new SplitFunction(),
            ["join"] = new JoinFunction()
        };
        
        public static BuiltinFunction Get(string name) => _functions.TryGetValue(name, out var func) ? func : null;
        public static IEnumerable<string> GetNames() => _functions.Keys;
    }
    
    public abstract class BuiltinFunction
    {
        public abstract string Name { get; }
        public abstract int MinArguments { get; }
        public abstract int MaxArguments { get; }
        
        public abstract Task<object> ExecuteAsync(List<object> arguments, ExecutionContext context);
        
        protected void ValidateArgumentCount(List<object> arguments)
        {
            if (arguments.Count < MinArguments || arguments.Count > MaxArguments)
            {
                throw new RuntimeException(
                    $"Function {Name} expects {MinArguments}-{MaxArguments} arguments, got {arguments.Count}");
            }
        }
    }
    
    public class PrintFunction : BuiltinFunction
    {
        public override string Name => "print";
        public override int MinArguments => 1;
        public override int MaxArguments => int.MaxValue;
        
        public override Task<object> ExecuteAsync(List<object> arguments, ExecutionContext context)
        {
            ValidateArgumentCount(arguments);
            
            var message = string.Join(" ", arguments.Select(arg => arg?.ToString() ?? ""));
            context.Logger.LogInformation(message);
            Console.WriteLine(message);
            
            return Task.FromResult<object>(null);
        }
    }
    
    public class PingFunction : BuiltinFunction
    {
        public override string Name => "ping";
        public override int MinArguments => 2;
        public override int MaxArguments => 4; // source, destination, count, timeout
        
        public override async Task<object> ExecuteAsync(List<object> arguments, ExecutionContext context)
        {
            ValidateArgumentCount(arguments);
            
            var sourceDevice = arguments[0].ToString();
            var destinationDevice = arguments[1].ToString();
            var count = arguments.Count > 2 ? Convert.ToInt32(arguments[2]) : 4;
            var timeout = arguments.Count > 3 ? Convert.ToInt32(arguments[3]) : 5000;
            
            var source = context.NetForgeContext.GetDevice(sourceDevice);
            var destination = context.NetForgeContext.GetDevice(destinationDevice);
            
            var result = await source.PingAsync(destination.GetPrimaryIpAddress(), count, timeout);
            
            return new
            {
                success = result.Success,
                sent = result.Sent,
                received = result.Received,
                lost = result.Lost,
                min_time = result.MinTime,
                max_time = result.MaxTime,
                avg_time = result.AvgTime
            };
        }
    }
    
    public class WaitFunction : BuiltinFunction
    {
        public override string Name => "wait";
        public override int MinArguments => 1;
        public override int MaxArguments => 1;
        
        public override async Task<object> ExecuteAsync(List<object> arguments, ExecutionContext context)
        {
            ValidateArgumentCount(arguments);
            
            var seconds = Convert.ToInt32(arguments[0]);
            await Task.Delay(seconds * 1000);
            
            return null;
        }
    }
}
```

### Phase 5: NetForge Integration (Weeks 12-13)

#### 5.1 NetForge.Player Integration

**Integration Interface:**
```csharp
namespace NetForge.Simulation.Scripting.Integration.NetForge
{
    public interface INetForgeContext
    {
        Task<INetworkDevice> CreateDeviceAsync(string vendor, string hostname, Dictionary<string, object> options);
        INetworkDevice GetDevice(string hostname);
        IEnumerable<INetworkDevice> GetAllDevices();
        Task CreateLinkAsync(string device1, string interface1, string device2, string interface2, Dictionary<string, object> properties);
        Task<bool> TestConnectivityAsync(string sourceDevice, string destinationDevice);
        Task SaveScenarioAsync(string filename);
        Task LoadScenarioAsync(string filename);
    }
    
    public class NetForgePlayerContext : INetForgeContext
    {
        private readonly INetworkManager _networkManager;
        private readonly IDeviceFactory _deviceFactory;
        private readonly ILogger<NetForgePlayerContext> _logger;
        
        public NetForgePlayerContext(
            INetworkManager networkManager,
            IDeviceFactory deviceFactory,
            ILogger<NetForgePlayerContext> logger) { }
        
        public async Task<INetworkDevice> CreateDeviceAsync(
            string vendor, 
            string hostname, 
            Dictionary<string, object> options)
        {
            _logger.LogInformation($"Creating {vendor} device: {hostname}");
            
            var device = _deviceFactory.CreateDevice(vendor, hostname);
            
            // Apply options
            if (options.TryGetValue("management_ip", out var managementIp))
            {
                device.SetManagementIp(managementIp.ToString());
            }
            
            if (options.TryGetValue("location", out var location))
            {
                device.SetLocation(location.ToString());
            }
            
            _networkManager.AddDevice(device);
            return device;
        }
        
        public INetworkDevice GetDevice(string hostname)
        {
            var device = _networkManager.GetDevice(hostname);
            if (device == null)
            {
                throw new RuntimeException($"Device '{hostname}' not found");
            }
            return device;
        }
        
        public async Task CreateLinkAsync(
            string device1, string interface1, 
            string device2, string interface2, 
            Dictionary<string, object> properties)
        {
            _logger.LogInformation($"Creating link: {device1}:{interface1} -> {device2}:{interface2}");
            
            var dev1 = GetDevice(device1);
            var dev2 = GetDevice(device2);
            
            var link = _networkManager.CreateLink(dev1, interface1, dev2, interface2);
            
            // Apply link properties
            if (properties.TryGetValue("bandwidth", out var bandwidth))
            {
                link.SetBandwidth(Convert.ToInt32(bandwidth));
            }
            
            if (properties.TryGetValue("delay", out var delay))
            {
                link.SetDelay(Convert.ToInt32(delay));
            }
            
            await _networkManager.EstablishLinkAsync(link);
        }
    }
}
```

#### 5.2 Command-Line Interface

**Script Execution CLI:**
```csharp
namespace NetForge.Simulation.Scripting.Tools.CLI
{
    public class ScriptRunner
    {
        private readonly ILogger<ScriptRunner> _logger;
        private readonly IServiceProvider _serviceProvider;
        
        public ScriptRunner(ILogger<ScriptRunner> logger, IServiceProvider serviceProvider) { }
        
        public async Task<int> RunScriptAsync(string[] args)
        {
            var options = ParseArguments(args);
            
            if (options.ShowHelp)
            {
                ShowHelp();
                return 0;
            }
            
            if (string.IsNullOrEmpty(options.ScriptPath))
            {
                Console.WriteLine("Error: Script path is required");
                return 1;
            }
            
            try
            {
                var scriptContent = await File.ReadAllTextAsync(options.ScriptPath);
                var result = await ExecuteScriptAsync(scriptContent, options);
                
                if (result.IsSuccess)
                {
                    Console.WriteLine("Script executed successfully");
                    return 0;
                }
                else
                {
                    Console.WriteLine($"Script execution failed: {result.Error}");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Script execution failed");
                Console.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }
        
        private async Task<ExecutionResult> ExecuteScriptAsync(string scriptContent, CommandLineOptions options)
        {
            // Lexical analysis
            var lexer = new Lexer(scriptContent, options.ScriptPath);
            var tokens = lexer.Tokenize().ToList();
            
            // Parsing
            var parser = new NetSimParser(tokens);
            var parseResult = parser.Parse();
            
            if (!parseResult.IsSuccess)
            {
                foreach (var error in parseResult.Errors)
                {
                    Console.WriteLine($"Parse error at line {error.Line}: {error.Message}");
                }
                return ExecutionResult.Failure("Parse errors occurred");
            }
            
            // Semantic analysis
            var symbolTable = new SymbolTable();
            var typeChecker = new TypeChecker(symbolTable);
            var semanticErrors = typeChecker.CheckTypes(parseResult.Script);
            
            if (semanticErrors.Count > 0)
            {
                foreach (var error in semanticErrors)
                {
                    Console.WriteLine($"Semantic error at line {error.Line}: {error.Message}");
                }
                return ExecutionResult.Failure("Semantic errors occurred");
            }
            
            // Execution
            var netForgeContext = _serviceProvider.GetRequiredService<INetForgeContext>();
            var runtime = _serviceProvider.GetRequiredService<IRuntimeEnvironment>();
            var executor = new ScriptExecutor(netForgeContext, runtime, _logger);
            
            var executionOptions = new ExecutionOptions
            {
                Timeout = options.Timeout,
                MaxMemory = options.MaxMemory,
                DebugMode = options.DebugMode,
                Variables = options.Variables
            };
            
            return await executor.ExecuteAsync(parseResult.Script, executionOptions);
        }
        
        private CommandLineOptions ParseArguments(string[] args) { }
        private void ShowHelp() { }
    }
    
    public class CommandLineOptions
    {
        public string ScriptPath { get; set; }
        public bool ShowHelp { get; set; }
        public int Timeout { get; set; } = 300; // seconds
        public long MaxMemory { get; set; } = 1024 * 1024 * 1024; // 1GB
        public bool DebugMode { get; set; }
        public Dictionary<string, object> Variables { get; set; } = new();
    }
}

// Program.cs for standalone script runner
namespace NetForge.Simulation.Scripting.Tools.CLI
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var services = new ServiceCollection()
                .AddLogging(builder => builder.AddConsole())
                .AddSingleton<INetForgeContext, NetForgePlayerContext>()
                .AddSingleton<IRuntimeEnvironment, RuntimeEnvironment>()
                .AddSingleton<ScriptRunner>()
                .BuildServiceProvider();
            
            var runner = services.GetRequiredService<ScriptRunner>();
            return await runner.RunScriptAsync(args);
        }
    }
}
```

### Phase 6: Testing and Validation (Weeks 14-15)

#### 6.1 Unit Testing Framework

```csharp
namespace NetForge.Simulation.Scripting.Tests
{
    public class LexerTests
    {
        [Fact]
        public void Tokenize_SimpleScript_ProducesCorrectTokens()
        {
            var input = """
                $hostname = "Router1"
                create-device cisco ${hostname}
                """;
            
            var lexer = new Lexer(input);
            var tokens = lexer.Tokenize().ToList();
            
            Assert.Equal(TokenType.Variable, tokens[0].Type);
            Assert.Equal("hostname", tokens[0].Value);
            Assert.Equal(TokenType.Assignment, tokens[1].Type);
            Assert.Equal(TokenType.StringLiteral, tokens[2].Type);
            Assert.Equal("Router1", tokens[2].Value);
        }
        
        [Fact]
        public void Tokenize_IpAddress_RecognizedAsLiteral()
        {
            var input = "192.168.1.1";
            var lexer = new Lexer(input);
            var tokens = lexer.Tokenize().ToList();
            
            Assert.Equal(TokenType.IpAddressLiteral, tokens[0].Type);
            Assert.Equal("192.168.1.1", tokens[0].Value);
        }
    }
    
    public class ParserTests
    {
        [Fact]
        public void Parse_DeviceCreation_ProducesCorrectAST()
        {
            var tokens = new List<Token>
            {
                new() { Type = TokenType.Identifier, Value = "create-device" },
                new() { Type = TokenType.Identifier, Value = "cisco" },
                new() { Type = TokenType.StringLiteral, Value = "Router1" },
                new() { Type = TokenType.EOF }
            };
            
            var parser = new NetSimParser(tokens);
            var result = parser.Parse();
            
            Assert.True(result.IsSuccess);
            Assert.Single(result.Script.Statements);
            Assert.IsType<DeviceCreationNode>(result.Script.Statements[0]);
            
            var deviceNode = (DeviceCreationNode)result.Script.Statements[0];
            Assert.Equal("cisco", deviceNode.Vendor);
        }
    }
    
    public class InterpreterTests
    {
        private readonly Mock<INetForgeContext> _mockContext;
        private readonly Mock<IRuntimeEnvironment> _mockRuntime;
        
        public InterpreterTests()
        {
            _mockContext = new Mock<INetForgeContext>();
            _mockRuntime = new Mock<IRuntimeEnvironment>();
        }
        
        [Fact]
        public async Task Execute_DeviceCreation_CallsNetForgeContext()
        {
            var script = new ScriptNode
            {
                Statements = new List<StatementNode>
                {
                    new DeviceCreationNode
                    {
                        Vendor = "cisco",
                        HostnameExpression = new LiteralNode { Value = "Router1" }
                    }
                }
            };
            
            var context = new ExecutionContext(_mockContext.Object, null);
            var interpreter = new ScriptInterpreter(context, _mockRuntime.Object);
            
            await interpreter.ExecuteAsync(script);
            
            _mockContext.Verify(c => c.CreateDeviceAsync("cisco", "Router1", It.IsAny<Dictionary<string, object>>()), Times.Once);
        }
    }
}
```

#### 6.2 Integration Testing

```csharp
public class IntegrationTests : IClassFixture<NetForgeTestFixture>
{
    private readonly NetForgeTestFixture _fixture;
    
    public IntegrationTests(NetForgeTestFixture fixture)
    {
        _fixture = fixture;
    }
    
    [Fact]
    public async Task ExecuteCompleteScript_CreatesDevicesAndLinks()
    {
        var script = """
            create-device cisco R1
            create-device cisco R2
            link R1 GigabitEthernet0/0 R2 GigabitEthernet0/0
            
            configure R1 {
              "enable"
              "configure terminal"
              "interface GigabitEthernet0/0"
              "ip address 192.168.1.1 255.255.255.252"
              "no shutdown"
            }
            
            test connectivity {
              ping R1 R2 --count 3
              assert ping.success
            }
            """;
        
        var executor = _fixture.GetScriptExecutor();
        var result = await executor.ExecuteScriptFromStringAsync(script);
        
        Assert.True(result.IsSuccess);
        
        var devices = _fixture.NetworkManager.GetAllDevices();
        Assert.Equal(2, devices.Count());
        Assert.Contains(devices, d => d.Name == "R1");
        Assert.Contains(devices, d => d.Name == "R2");
        
        var links = _fixture.NetworkManager.GetAllLinks();
        Assert.Single(links);
    }
}
```

### Phase 7: Documentation and Examples (Weeks 16)

#### 7.1 API Documentation

- Complete XML documentation for all public APIs
- Usage examples for each major component
- Integration guides for NetForge.Player
- Performance tuning recommendations

#### 7.2 Example Scripts Library

```
NetForge.Simulation.Scripting/Examples/
├── Basic/
│   ├── simple_network.netsim
│   ├── device_configuration.netsim
│   └── connectivity_test.netsim
├── Advanced/
│   ├── enterprise_network.netsim
│   ├── multi_vendor_setup.netsim
│   └── performance_testing.netsim
├── Templates/
│   ├── cisco_baseline.netsim
│   ├── juniper_baseline.netsim
│   └── security_hardening.netsim
└── Testing/
    ├── regression_suite.netsim
    ├── load_testing.netsim
    └── failover_testing.netsim
```

## Technical Requirements

### Dependencies
- .NET 9.0 or later
- ANTLR4 runtime (optional, for advanced parsing)
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Logging
- xUnit (testing)
- Moq (mocking)

### Performance Targets
- **Parsing Speed**: 1000+ lines per second
- **Memory Usage**: < 100MB for typical scripts
- **Execution Speed**: Near real-time for most operations
- **Concurrency**: Support for parallel execution

### Quality Gates
- **Code Coverage**: > 90%
- **Unit Tests**: Comprehensive test suite
- **Integration Tests**: End-to-end scenarios
- **Documentation**: Complete API documentation
- **Performance Tests**: Benchmarking suite

## Deployment Strategy

### Packaging
- NuGet package for library components
- Standalone executable for command-line usage
- Integration package for NetForge.Player
- Docker image for containerized execution

### Versioning
- Semantic versioning (SemVer)
- Backward compatibility guarantees
- Migration guides for major versions
- Deprecation warnings for API changes

## Future Enhancements

### Phase 2 Features (Future)
- **IDE Integration**: Syntax highlighting and IntelliSense
- **Debugger**: Step-through debugging capabilities
- **Performance Profiler**: Script performance analysis
- **Visual Editor**: Drag-and-drop script creation
- **Plugin System**: Custom function libraries
- **Cloud Integration**: Remote script execution
- **Advanced Testing**: Property-based testing support

This implementation plan provides a comprehensive roadmap for creating a full-featured NetSim scripting language and parser that integrates seamlessly with NetForge.Player while providing powerful automation capabilities for network simulation and testing.