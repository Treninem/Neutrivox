using System.Globalization;
using System.Text.RegularExpressions;
using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record StructuredTextCompileResult(bool Success, IReadOnlyList<string> Errors, int Statements);

/// <summary>
/// Compiles the Neutrivox ST subset to the same instruction IR used by FBD/LD and simulation.
/// Supported statements: assignment, NOT, AND/OR/XOR, =, &gt;, &lt;, SET(x), RESET(x).
/// Parentheses around a complete expression and literal TRUE/FALSE/numbers are accepted.
/// </summary>
public sealed class StructuredTextCompilerService
{
    private static readonly Regex Identifier = new("^[A-Za-z_А-Яа-яЁё][A-Za-z0-9_А-Яа-яЁё.]*$", RegexOptions.Compiled);

    public StructuredTextCompileResult Compile(AutomationProject project, string source)
    {
        var errors = new List<string>();
        var compiled = new List<LogicInstruction>();
        var statements = SplitStatements(source).ToList();
        var line = 0;
        foreach (var raw in statements)
        {
            line++;
            var statement = StripComment(raw).Trim();
            if (statement.Length == 0) continue;
            if (TryCall(statement, "SET", out var setTarget))
            {
                if (!ValidIdentifier(setTarget)) errors.Add($"Statement {line}: invalid SET target '{setTarget}'.");
                else compiled.Add(new LogicInstruction { Kind = LogicInstructionKind.Set, Target = setTarget, Comment = "ST" });
                continue;
            }
            if (TryCall(statement, "RESET", out var resetTarget))
            {
                if (!ValidIdentifier(resetTarget)) errors.Add($"Statement {line}: invalid RESET target '{resetTarget}'.");
                else compiled.Add(new LogicInstruction { Kind = LogicInstructionKind.Reset, Target = resetTarget, Comment = "ST" });
                continue;
            }

            var assign = statement.IndexOf(":=", StringComparison.Ordinal);
            if (assign <= 0)
            {
                errors.Add($"Statement {line}: expected ':=', SET(...) or RESET(...).");
                continue;
            }
            var target = statement[..assign].Trim();
            var expression = TrimOuterParentheses(statement[(assign + 2)..].Trim());
            if (!ValidIdentifier(target))
            {
                errors.Add($"Statement {line}: invalid assignment target '{target}'.");
                continue;
            }
            if (!TryCompileExpression(target, expression, out var instruction, out var error))
            {
                errors.Add($"Statement {line}: {error}");
                continue;
            }
            compiled.Add(instruction!);
        }

        if (errors.Count > 0) return new(false, errors, compiled.Count);

        project.Logic.Networks.Clear();
        var network = new LogicNetwork { Name = "ST Main", Enabled = true };
        network.Instructions.AddRange(compiled);
        project.Logic.Networks.Add(network);
        project.Logic.StructuredTextSource = source;
        project.Logic.EditorMode = LogicEditorMode.StructuredText;
        return new(true, [], compiled.Count);
    }

    public string Render(AutomationProject project)
    {
        var lines = new List<string>();
        foreach (var network in project.Logic.Networks.Where(x => x.Enabled))
        {
            lines.Add($"// {network.Name}");
            foreach (var i in network.Instructions)
                lines.Add(RenderInstruction(i));
            lines.Add(string.Empty);
        }
        return string.Join(Environment.NewLine, lines).TrimEnd();
    }

    private static bool TryCompileExpression(string target, string expression, out LogicInstruction? instruction, out string error)
    {
        instruction = null;
        error = string.Empty;
        expression = TrimOuterParentheses(expression.Trim());
        if (expression.StartsWith("NOT ", StringComparison.OrdinalIgnoreCase))
        {
            var operand = TrimOuterParentheses(expression[4..].Trim());
            if (!ValidOperand(operand)) { error = $"invalid NOT operand '{operand}'."; return false; }
            instruction = new() { Kind = LogicInstructionKind.Not, Target = target, SourceA = operand, Comment = "ST" };
            return true;
        }

        foreach (var (token, kind) in new[]
        {
            (" AND ", LogicInstructionKind.And),
            (" OR ", LogicInstructionKind.Or),
            (" XOR ", LogicInstructionKind.Xor),
            (" = ", LogicInstructionKind.CompareEqual),
            (" > ", LogicInstructionKind.CompareGreater),
            (" < ", LogicInstructionKind.CompareLess)
        })
        {
            var index = expression.IndexOf(token, StringComparison.OrdinalIgnoreCase);
            if (index <= 0) continue;
            var left = TrimOuterParentheses(expression[..index].Trim());
            var right = TrimOuterParentheses(expression[(index + token.Length)..].Trim());
            if (!ValidOperand(left) || !ValidOperand(right))
            {
                error = $"invalid operands in '{expression}'.";
                return false;
            }
            instruction = new() { Kind = kind, Target = target, SourceA = left, SourceB = right, Comment = "ST" };
            return true;
        }

        if (!ValidOperand(expression))
        {
            error = $"unsupported expression '{expression}'. Use one operation per assignment in this release.";
            return false;
        }
        instruction = new() { Kind = LogicInstructionKind.Copy, Target = target, SourceA = expression, Comment = "ST" };
        return true;
    }

    private static string RenderInstruction(LogicInstruction i) => i.Kind switch
    {
        LogicInstructionKind.Copy => $"{i.Target} := {i.SourceA};",
        LogicInstructionKind.Not => $"{i.Target} := NOT {i.SourceA};",
        LogicInstructionKind.And => $"{i.Target} := {i.SourceA} AND {i.SourceB};",
        LogicInstructionKind.Or => $"{i.Target} := {i.SourceA} OR {i.SourceB};",
        LogicInstructionKind.Xor => $"{i.Target} := {i.SourceA} XOR {i.SourceB};",
        LogicInstructionKind.Set => $"SET({i.Target});",
        LogicInstructionKind.Reset => $"RESET({i.Target});",
        LogicInstructionKind.CompareEqual => $"{i.Target} := {i.SourceA} = {i.SourceB};",
        LogicInstructionKind.CompareGreater => $"{i.Target} := {i.SourceA} > {i.SourceB};",
        LogicInstructionKind.CompareLess => $"{i.Target} := {i.SourceA} < {i.SourceB};",
        _ => $"// Unsupported {i.Kind}"
    };

    private static IEnumerable<string> SplitStatements(string source)
    {
        var normalized = source.Replace("\r\n", "\n").Replace('\r', '\n');
        var buffer = string.Empty;
        foreach (var line in normalized.Split('\n'))
        {
            var clean = StripComment(line);
            if (string.IsNullOrWhiteSpace(clean)) continue;
            buffer = string.IsNullOrWhiteSpace(buffer) ? clean.Trim() : buffer + " " + clean.Trim();
            while (buffer.Contains(';'))
            {
                var index = buffer.IndexOf(';');
                yield return buffer[..index];
                buffer = buffer[(index + 1)..].Trim();
            }
        }
        if (!string.IsNullOrWhiteSpace(buffer)) yield return buffer;
    }

    private static string StripComment(string value)
    {
        var index = value.IndexOf("//", StringComparison.Ordinal);
        return index >= 0 ? value[..index] : value;
    }

    private static bool TryCall(string statement, string name, out string argument)
    {
        argument = string.Empty;
        if (!statement.StartsWith(name + "(", StringComparison.OrdinalIgnoreCase) || !statement.EndsWith(')')) return false;
        argument = statement[(name.Length + 1)..^1].Trim();
        return true;
    }

    private static bool ValidIdentifier(string value) => Identifier.IsMatch(value);
    private static bool ValidOperand(string value) => ValidIdentifier(value) || IsLiteral(value);
    internal static bool IsLiteral(string value) =>
        value.Equals("TRUE", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("FALSE", StringComparison.OrdinalIgnoreCase) ||
        double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _);

    private static string TrimOuterParentheses(string value)
    {
        while (value.Length >= 2 && value[0] == '(' && value[^1] == ')' && Balanced(value[1..^1]))
            value = value[1..^1].Trim();
        return value;
    }

    private static bool Balanced(string value)
    {
        var depth = 0;
        foreach (var c in value)
        {
            if (c == '(') depth++;
            if (c == ')' && --depth < 0) return false;
        }
        return depth == 0;
    }
}
