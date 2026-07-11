namespace RegexCraft.Core.Analysis;

/// <summary>
/// Recursive-descent structural analyzer producing a rich, expandable analysis tree.
/// Engine-agnostic; prioritizes useful explanations over full flavor fidelity.
/// </summary>
public sealed class RegexAnalysisService : IRegexAnalysisService
{
    public AnalysisNode Analyze(string? pattern)
    {
        pattern ??= string.Empty;
        if (pattern.Length == 0)
        {
            return new AnalysisNode
            {
                Title = "Empty pattern",
                Detail = "Enter a regular expression to see its structure.",
                Kind = AnalysisNodeKind.Root,
                PatternFragment = string.Empty,
                StartIndex = 0,
                Length = 0,
            };
        }

        var root = new AnalysisNode
        {
            Title = "Pattern",
            Detail = $"{pattern.Length} character{(pattern.Length == 1 ? "" : "s")}",
            Kind = AnalysisNodeKind.Root,
            PatternFragment = pattern,
            StartIndex = 0,
            Length = pattern.Length,
        };

        try
        {
            var parser = new Parser(pattern);
            var body = parser.ParseAlternation();
            if (body is not null)
                root.Children.Add(body);

            if (!parser.AtEnd)
            {
                root.Children.Add(new AnalysisNode
                {
                    Title = "Unparsed remainder",
                    Detail = parser.Remainder,
                    Kind = AnalysisNodeKind.Error,
                    IsError = true,
                    PatternFragment = parser.Remainder,
                    StartIndex = parser.Position,
                    Length = pattern.Length - parser.Position,
                });
            }
            else if (parser.HadError && root.Children.Count == 0)
            {
                root.Children.Add(new AnalysisNode
                {
                    Title = "Incomplete pattern",
                    Detail = parser.ErrorMessage ?? "Could not fully parse the pattern.",
                    Kind = AnalysisNodeKind.Incomplete,
                    IsError = true,
                    PatternFragment = pattern,
                    StartIndex = 0,
                    Length = pattern.Length,
                });
            }
        }
        catch (Exception ex)
        {
            root.Children.Add(new AnalysisNode
            {
                Title = "Analysis error",
                Detail = ex.Message,
                Kind = AnalysisNodeKind.Error,
                IsError = true,
            });
        }

        return root;
    }

    private sealed class Parser
    {
        private readonly string _text;
        private int _i;

        public bool HadError { get; private set; }
        public string? ErrorMessage { get; private set; }
        public bool AtEnd => _i >= _text.Length;
        public string Remainder => AtEnd ? string.Empty : _text[_i..];
        public int Position => _i;

        public Parser(string text) => _text = text;

        public AnalysisNode? ParseAlternation()
        {
            var start = _i;
            var left = ParseSequence();
            if (Peek() != '|')
                return left;

            var alt = new AnalysisNode
            {
                Title = "Alternation",
                Detail = "Matches one of the alternatives (left to right)",
                Kind = AnalysisNodeKind.Alternation,
                StartIndex = start,
            };

            if (left is not null)
                alt.Children.Add(WrapBranch(left, 0));

            var branch = 1;
            while (Match('|'))
            {
                var next = ParseSequence();
                if (next is not null)
                    alt.Children.Add(WrapBranch(next, branch));
                else
                    alt.Children.Add(new AnalysisNode
                    {
                        Title = $"Branch {branch}",
                        Detail = "(empty alternative)",
                        Kind = AnalysisNodeKind.Sequence,
                        StartIndex = _i,
                        Length = 0,
                    });
                branch++;
            }

            alt.Detail = $"{branch} alternative{(branch == 1 ? "" : "s")} — matches one of them";
            alt.PatternFragment = _text[start.._i];
            alt.Length = _i - start;
            return alt;
        }

        private static AnalysisNode WrapBranch(AnalysisNode node, int index)
        {
            if (node.Kind == AnalysisNodeKind.Sequence && node.Title.StartsWith("Sequence", StringComparison.Ordinal))
            {
                return new AnalysisNode
                {
                    Title = $"Branch {index}",
                    Detail = node.Detail,
                    Kind = AnalysisNodeKind.Sequence,
                    PatternFragment = node.PatternFragment,
                    StartIndex = node.StartIndex,
                    Length = node.Length,
                    Children = node.Children,
                };
            }

            var wrap = new AnalysisNode
            {
                Title = $"Branch {index}",
                Detail = node.Title,
                Kind = AnalysisNodeKind.Sequence,
                PatternFragment = node.PatternFragment,
                StartIndex = node.StartIndex,
                Length = node.Length,
            };
            wrap.Children.Add(node);
            return wrap;
        }

        private AnalysisNode? ParseSequence()
        {
            var start = _i;
            var nodes = new List<AnalysisNode>();
            while (!AtEnd && Peek() is not ('|' or ')'))
            {
                var atom = ParseQuantified();
                if (atom is null)
                    break;
                nodes.Add(atom);
            }

            if (nodes.Count == 0)
                return null;

            if (nodes.Count == 1)
                return nodes[0];

            var seq = new AnalysisNode
            {
                Title = "Sequence",
                Detail = $"{nodes.Count} parts — matched in order",
                Kind = AnalysisNodeKind.Sequence,
                StartIndex = start,
                Length = _i - start,
            };
            foreach (var n in nodes)
                seq.Children.Add(n);
            seq.PatternFragment = string.Concat(nodes.Select(n => n.PatternFragment));
            return seq;
        }

        private AnalysisNode? ParseQuantified()
        {
            var atom = ParseAtom();
            if (atom is null)
                return null;

            if (AtEnd)
                return atom;

            var q = Peek();
            if (q is '*' or '+' or '?')
            {
                var qStart = atom.StartIndex >= 0 ? atom.StartIndex : _i;
                _i++;
                var lazy = Match('?');
                var possessive = !lazy && Match('+');
                var (name, meaning) = q switch
                {
                    '*' when possessive => ("Zero or more (possessive)", "0+ times, no backtrack"),
                    '*' when lazy => ("Zero or more (lazy)", "0+ times, as few as possible"),
                    '*' => ("Zero or more (greedy)", "0+ times, as many as possible"),
                    '+' when possessive => ("One or more (possessive)", "1+ times, no backtrack"),
                    '+' when lazy => ("One or more (lazy)", "1+ times, as few as possible"),
                    '+' => ("One or more (greedy)", "1+ times, as many as possible"),
                    _ when lazy => ("Optional (lazy)", "0 or 1 time, prefer 0"),
                    _ => ("Optional", "0 or 1 time"),
                };
                var suffix = q + (lazy ? "?" : possessive ? "+" : "");
                return new AnalysisNode
                {
                    Title = name,
                    Detail = meaning,
                    Kind = AnalysisNodeKind.Quantifier,
                    PatternFragment = atom.PatternFragment + suffix,
                    StartIndex = qStart,
                    Length = (atom.Length) + suffix.Length,
                    Children = { atom },
                };
            }

            if (q == '{')
            {
                var braceStart = _i;
                _i++; // {
                while (!AtEnd && Peek() != '}')
                    _i++;
                if (AtEnd)
                {
                    HadError = true;
                    ErrorMessage = "Unclosed quantifier '{'";
                    return new AnalysisNode
                    {
                        Title = "Incomplete quantifier",
                        Detail = ErrorMessage,
                        Kind = AnalysisNodeKind.Incomplete,
                        IsError = true,
                        PatternFragment = _text[braceStart..],
                        StartIndex = atom.StartIndex >= 0 ? atom.StartIndex : braceStart,
                        Length = _text.Length - (atom.StartIndex >= 0 ? atom.StartIndex : braceStart),
                        Children = { atom },
                    };
                }

                _i++; // }
                var lazy = Match('?');
                var possessive = !lazy && Match('+');
                var braceFrag = _text[braceStart.._i] + (lazy ? "?" : possessive ? "+" : "");
                var mode = lazy ? "lazy" : possessive ? "possessive" : "greedy";
                return new AnalysisNode
                {
                    Title = $"Counted quantifier ({mode})",
                    Detail = braceFrag + " — " + DescribeBrace(braceFrag),
                    Kind = AnalysisNodeKind.Quantifier,
                    PatternFragment = atom.PatternFragment + braceFrag,
                    StartIndex = atom.StartIndex >= 0 ? atom.StartIndex : braceStart,
                    Length = (atom.Length) + braceFrag.Length,
                    Children = { atom },
                };
            }

            return atom;
        }

        private static string DescribeBrace(string frag)
        {
            // frag like {2}, {2,}, {2,5}, maybe with ?/+
            var core = frag.TrimEnd('?', '+');
            if (core.Length < 2) return "counted repetition";
            var inner = core[1..^1];
            var parts = inner.Split(',');
            if (parts.Length == 1 && int.TryParse(parts[0], out var n))
                return $"exactly {n} time{(n == 1 ? "" : "s")}";
            if (parts.Length == 2)
            {
                if (string.IsNullOrEmpty(parts[1]))
                    return $"at least {parts[0]} times";
                return $"between {parts[0]} and {parts[1]} times";
            }

            return "counted repetition";
        }

        private AnalysisNode? ParseAtom()
        {
            if (AtEnd)
                return null;

            var c = Peek();

            if (c == '(')
                return ParseGroup();

            if (c == '[')
                return ParseCharacterClass();

            if (c == '\\')
                return ParseEscape();

            if (c is '^' or '$')
            {
                var pos = _i;
                _i++;
                return new AnalysisNode
                {
                    Title = c == '^' ? "Start anchor" : "End anchor",
                    Detail = c == '^'
                        ? "Matches the start of the string (or line with Multiline)"
                        : "Matches the end of the string (or line with Multiline)",
                    Kind = AnalysisNodeKind.Anchor,
                    PatternFragment = c.ToString(),
                    StartIndex = pos,
                    Length = 1,
                };
            }

            if (c == '.')
            {
                var pos = _i;
                _i++;
                return new AnalysisNode
                {
                    Title = "Any character",
                    Detail = "Matches any character (except newline unless Singleline)",
                    Kind = AnalysisNodeKind.Wildcard,
                    PatternFragment = ".",
                    StartIndex = pos,
                    Length = 1,
                };
            }

            if (c is '*' or '+' or '?' or ')' or '|' or '}')
            {
                var pos = _i;
                _i++;
                HadError = true;
                ErrorMessage = $"Unexpected '{c}'";
                return new AnalysisNode
                {
                    Title = $"Unexpected '{c}'",
                    Detail = ErrorMessage,
                    Kind = AnalysisNodeKind.Error,
                    IsError = true,
                    PatternFragment = c.ToString(),
                    StartIndex = pos,
                    Length = 1,
                };
            }

            // Literal run
            var start = _i;
            while (!AtEnd && IsLiteral(Peek()))
                _i++;
            var lit = _text[start.._i];
            return new AnalysisNode
            {
                Title = lit.Length == 1 ? "Literal" : "Literals",
                Detail = lit.Length == 1
                    ? $"Matches the character '{lit}'"
                    : $"Matches the text \"{lit}\"",
                Kind = AnalysisNodeKind.Literal,
                PatternFragment = lit,
                StartIndex = start,
                Length = lit.Length,
            };
        }

        private AnalysisNode ParseGroup()
        {
            var start = _i;
            _i++; // (

            var kind = AnalysisNodeKind.Group;
            var title = "Capturing group";
            string? detail = "Creates a numbered capture";

            if (Match('?'))
            {
                if (Match(':'))
                {
                    kind = AnalysisNodeKind.NonCapturingGroup;
                    title = "Non-capturing group";
                    detail = "Groups without creating a capture";
                }
                else if (Match('='))
                {
                    kind = AnalysisNodeKind.Lookaround;
                    title = "Positive lookahead";
                    detail = "Asserts the pattern matches ahead (zero-width)";
                }
                else if (Match('!'))
                {
                    kind = AnalysisNodeKind.Lookaround;
                    title = "Negative lookahead";
                    detail = "Asserts the pattern does not match ahead (zero-width)";
                }
                else if (Match('<'))
                {
                    if (Match('='))
                    {
                        kind = AnalysisNodeKind.Lookaround;
                        title = "Positive lookbehind";
                        detail = "Asserts the pattern matches behind (zero-width)";
                    }
                    else if (Match('!'))
                    {
                        kind = AnalysisNodeKind.Lookaround;
                        title = "Negative lookbehind";
                        detail = "Asserts the pattern does not match behind (zero-width)";
                    }
                    else
                    {
                        // Named group (?<name>...)
                        var nameStart = _i;
                        while (!AtEnd && Peek() != '>' && Peek() != ')')
                            _i++;
                        var name = _text[nameStart.._i];
                        if (Match('>'))
                        {
                            kind = AnalysisNodeKind.NamedGroup;
                            title = "Named group";
                            detail = string.IsNullOrEmpty(name)
                                ? "(missing name)"
                                : $"Capture name: {name}";
                        }
                        else
                        {
                            HadError = true;
                            ErrorMessage = "Unclosed named group";
                        }
                    }
                }
                else if (Match('>'))
                {
                    kind = AnalysisNodeKind.Group;
                    title = "Atomic group";
                    detail = "Possessive group — no backtracking into contents";
                }
                else if (Match('#'))
                {
                    kind = AnalysisNodeKind.Comment;
                    title = "Comment";
                    var commentStart = _i;
                    while (!AtEnd && Peek() != ')')
                        _i++;
                    detail = _text[commentStart.._i];
                    if (!Match(')'))
                    {
                        HadError = true;
                        ErrorMessage = "Unclosed comment group";
                        return IncompleteGroup(start, null);
                    }

                    return new AnalysisNode
                    {
                        Title = title,
                        Detail = detail,
                        Kind = kind,
                        PatternFragment = _text[start.._i],
                        StartIndex = start,
                        Length = _i - start,
                    };
                }
                else if (Peek() is 'i' or 'm' or 's' or 'x' or 'n' or '-')
                {
                    kind = AnalysisNodeKind.Modifier;
                    title = "Inline modifier";
                    var modStart = _i;
                    while (!AtEnd && Peek() is not (':' or ')'))
                        _i++;
                    detail = $"Flags: {_text[modStart.._i]}";
                    Match(':');
                }
                else
                {
                    kind = AnalysisNodeKind.Group;
                    title = "Special group";
                    detail = "Engine-specific construct";
                }
            }

            var inner = ParseAlternation();
            if (!Match(')'))
            {
                HadError = true;
                ErrorMessage = "Unclosed group '('";
                return IncompleteGroup(start, inner);
            }

            var node = new AnalysisNode
            {
                Title = title,
                Detail = detail,
                Kind = kind,
                PatternFragment = _text[start.._i],
                StartIndex = start,
                Length = _i - start,
            };
            if (inner is not null)
                node.Children.Add(inner);
            return node;
        }

        private AnalysisNode IncompleteGroup(int start, AnalysisNode? inner)
        {
            var incomplete = new AnalysisNode
            {
                Title = "Incomplete group",
                Detail = ErrorMessage,
                Kind = AnalysisNodeKind.Incomplete,
                IsError = true,
                PatternFragment = _text[start..],
                StartIndex = start,
                Length = _text.Length - start,
            };
            if (inner is not null)
                incomplete.Children.Add(inner);
            return incomplete;
        }

        private AnalysisNode ParseCharacterClass()
        {
            var start = _i;
            _i++; // [
            var negated = Match('^');
            var contentStart = _i;
            while (!AtEnd && Peek() != ']')
            {
                if (Peek() == '\\' && _i + 1 < _text.Length)
                    _i += 2;
                else
                    _i++;
            }

            if (!Match(']'))
            {
                HadError = true;
                ErrorMessage = "Unclosed character class '['";
                return new AnalysisNode
                {
                    Title = "Incomplete character class",
                    Detail = ErrorMessage,
                    Kind = AnalysisNodeKind.Incomplete,
                    IsError = true,
                    PatternFragment = _text[start..],
                    StartIndex = start,
                    Length = _text.Length - start,
                };
            }

            var content = _text[contentStart..(_i - 1)];
            return new AnalysisNode
            {
                Title = negated ? "Negated character class" : "Character class",
                Detail = negated
                    ? $"Matches any character except: {Truncate(content, 40)}"
                    : $"Matches one of: {Truncate(content, 40)}",
                Kind = AnalysisNodeKind.CharacterClass,
                PatternFragment = _text[start.._i],
                StartIndex = start,
                Length = _i - start,
            };
        }

        private AnalysisNode ParseEscape()
        {
            var start = _i;
            _i++; // \
            if (AtEnd)
            {
                HadError = true;
                ErrorMessage = "Trailing backslash";
                return new AnalysisNode
                {
                    Title = "Incomplete escape",
                    Detail = ErrorMessage,
                    Kind = AnalysisNodeKind.Incomplete,
                    IsError = true,
                    PatternFragment = "\\",
                    StartIndex = start,
                    Length = 1,
                };
            }

            var c = _text[_i++];
            string title;
            string detail;
            var kind = AnalysisNodeKind.Escape;

            switch (c)
            {
                case 'd':
                    title = "Digit class";
                    detail = "\\d — matches a digit [0-9]";
                    break;
                case 'D':
                    title = "Non-digit class";
                    detail = "\\D — matches a non-digit";
                    break;
                case 'w':
                    title = "Word class";
                    detail = "\\w — matches a word character [A-Za-z0-9_]";
                    break;
                case 'W':
                    title = "Non-word class";
                    detail = "\\W — matches a non-word character";
                    break;
                case 's':
                    title = "Whitespace class";
                    detail = "\\s — matches whitespace";
                    break;
                case 'S':
                    title = "Non-whitespace class";
                    detail = "\\S — matches non-whitespace";
                    break;
                case 'b':
                    title = "Word boundary";
                    detail = "\\b — zero-width word boundary";
                    kind = AnalysisNodeKind.Anchor;
                    break;
                case 'B':
                    title = "Non-boundary";
                    detail = "\\B — zero-width non-boundary";
                    kind = AnalysisNodeKind.Anchor;
                    break;
                case 'A':
                    title = "Absolute start";
                    detail = "\\A — start of string only";
                    kind = AnalysisNodeKind.Anchor;
                    break;
                case 'z':
                    title = "Absolute end";
                    detail = "\\z — end of string only";
                    kind = AnalysisNodeKind.Anchor;
                    break;
                case 'Z':
                    title = "End before final newline";
                    detail = "\\Z — end of string (before final newline)";
                    kind = AnalysisNodeKind.Anchor;
                    break;
                case 'G':
                    title = "Previous match end";
                    detail = "\\G — end of previous match";
                    kind = AnalysisNodeKind.Anchor;
                    break;
                case 'n':
                    title = "Newline escape";
                    detail = "\\n — matches a newline character";
                    break;
                case 't':
                    title = "Tab escape";
                    detail = "\\t — matches a tab character";
                    break;
                case 'r':
                    title = "Carriage return";
                    detail = "\\r — matches CR";
                    break;
                case 'p' or 'P':
                    ParsePropertyBody();
                    title = c == 'p' ? "Unicode property" : "Negated Unicode property";
                    detail = _text[start.._i];
                    break;
                case 'k':
                    if (Match('<'))
                    {
                        while (!AtEnd && Peek() != '>')
                            _i++;
                        Match('>');
                    }

                    title = "Named backreference";
                    detail = _text[start.._i];
                    kind = AnalysisNodeKind.Backreference;
                    break;
                case >= '1' and <= '9':
                    title = $"Backreference \\{c}";
                    detail = $"Matches the same text as group {c}";
                    kind = AnalysisNodeKind.Backreference;
                    break;
                default:
                    title = $"Escaped '{c}'";
                    detail = $"Literal character '{c}'";
                    break;
            }

            return new AnalysisNode
            {
                Title = title,
                Detail = detail,
                Kind = kind,
                PatternFragment = _text[start.._i],
                StartIndex = start,
                Length = _i - start,
            };
        }

        private void ParsePropertyBody()
        {
            if (Match('{'))
            {
                while (!AtEnd && Peek() != '}')
                    _i++;
                Match('}');
            }
        }

        private char Peek() => _i < _text.Length ? _text[_i] : '\0';

        private bool Match(char c)
        {
            if (Peek() != c)
                return false;
            _i++;
            return true;
        }

        private static bool IsLiteral(char c) =>
            c is not ('.' or '^' or '$' or '*' or '+' or '?' or '(' or ')' or '[' or ']' or '{' or '}'
                or '|' or '\\');

        private static string Truncate(string s, int max) =>
            s.Length <= max ? s : s[..max] + "…";
    }
}
