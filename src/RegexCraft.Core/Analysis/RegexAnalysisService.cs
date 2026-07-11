namespace RegexCraft.Core.Analysis;

/// <summary>
/// Lightweight recursive-descent structural analyzer for common regex constructs.
/// Engine-agnostic; focuses on explanation rather than full flavor fidelity.
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
            };
        }

        var root = new AnalysisNode
        {
            Title = "Pattern",
            Detail = $"Length {pattern.Length}",
            Kind = AnalysisNodeKind.Root,
            PatternFragment = pattern,
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

        public Parser(string text) => _text = text;

        public AnalysisNode? ParseAlternation()
        {
            var left = ParseSequence();
            if (Peek() != '|')
                return left;

            var alt = new AnalysisNode
            {
                Title = "Alternation",
                Detail = "Matches one of the alternatives",
                Kind = AnalysisNodeKind.Alternation,
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
                        Detail = "(empty)",
                        Kind = AnalysisNodeKind.Sequence,
                    });
                branch++;
            }

            alt.PatternFragment = JoinFragments(alt);
            return alt;
        }

        private static AnalysisNode WrapBranch(AnalysisNode node, int index)
        {
            if (node.Kind == AnalysisNodeKind.Sequence)
            {
                node = new AnalysisNode
                {
                    Title = $"Branch {index}",
                    Detail = node.Detail,
                    Kind = AnalysisNodeKind.Sequence,
                    PatternFragment = node.PatternFragment,
                    Children = node.Children,
                };
            }
            else
            {
                var wrap = new AnalysisNode
                {
                    Title = $"Branch {index}",
                    Kind = AnalysisNodeKind.Sequence,
                    PatternFragment = node.PatternFragment,
                };
                wrap.Children.Add(node);
                node = wrap;
            }

            return node;
        }

        private AnalysisNode? ParseSequence()
        {
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
                Detail = $"{nodes.Count} parts",
                Kind = AnalysisNodeKind.Sequence,
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
                _i++;
                var lazy = Match('?');
                var name = q switch
                {
                    '*' => lazy ? "Zero or more (lazy)" : "Zero or more",
                    '+' => lazy ? "One or more (lazy)" : "One or more",
                    _ => lazy ? "Optional (lazy)" : "Optional",
                };
                return new AnalysisNode
                {
                    Title = name,
                    Detail = $"{atom.Title} {q}{(lazy ? "?" : "")}",
                    Kind = AnalysisNodeKind.Quantifier,
                    PatternFragment = atom.PatternFragment + q + (lazy ? "?" : ""),
                    Children = { atom },
                };
            }

            if (q == '{')
            {
                var start = _i;
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
                        PatternFragment = _text[start..],
                        Children = { atom },
                    };
                }

                _i++; // }
                var lazy = Match('?');
                var frag = _text[start.._i] + (lazy ? "?" : "");
                return new AnalysisNode
                {
                    Title = lazy ? "Counted quantifier (lazy)" : "Counted quantifier",
                    Detail = frag,
                    Kind = AnalysisNodeKind.Quantifier,
                    PatternFragment = atom.PatternFragment + frag,
                    Children = { atom },
                };
            }

            return atom;
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
                _i++;
                return new AnalysisNode
                {
                    Title = c == '^' ? "Start anchor" : "End anchor",
                    Detail = c == '^' ? "Matches start of string/line" : "Matches end of string/line",
                    Kind = AnalysisNodeKind.Anchor,
                    PatternFragment = c.ToString(),
                };
            }

            if (c == '.')
            {
                _i++;
                return new AnalysisNode
                {
                    Title = "Any character",
                    Detail = "Matches any character (except newline unless Singleline)",
                    Kind = AnalysisNodeKind.Wildcard,
                    PatternFragment = ".",
                };
            }

            if (c is '*' or '+' or '?' or ')' or '|' or '}')
            {
                // Unexpected quantifier/close — surface as error atom and consume one char
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
                Detail = $"\"{lit}\"",
                Kind = AnalysisNodeKind.Literal,
                PatternFragment = lit,
            };
        }

        private AnalysisNode ParseGroup()
        {
            var start = _i;
            _i++; // (

            var kind = AnalysisNodeKind.Group;
            var title = "Capturing group";
            string? detail = null;

            if (Match('?'))
            {
                if (Match(':'))
                {
                    kind = AnalysisNodeKind.NonCapturingGroup;
                    title = "Non-capturing group";
                }
                else if (Match('='))
                {
                    kind = AnalysisNodeKind.Lookaround;
                    title = "Positive lookahead";
                }
                else if (Match('!'))
                {
                    kind = AnalysisNodeKind.Lookaround;
                    title = "Negative lookahead";
                }
                else if (Match('<'))
                {
                    if (Match('='))
                    {
                        kind = AnalysisNodeKind.Lookaround;
                        title = "Positive lookbehind";
                    }
                    else if (Match('!'))
                    {
                        kind = AnalysisNodeKind.Lookaround;
                        title = "Negative lookbehind";
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
                            detail = name;
                        }
                        else
                        {
                            HadError = true;
                            ErrorMessage = "Unclosed named group";
                        }
                    }
                }
                else if (Peek() is 'i' or 'm' or 's' or 'x' or 'n' or '-' )
                {
                    kind = AnalysisNodeKind.Modifier;
                    title = "Inline modifier";
                    var modStart = _i;
                    while (!AtEnd && Peek() is not (':' or ')'))
                        _i++;
                    detail = _text[modStart.._i];
                    Match(':');
                }
                else
                {
                    // Other (?...) constructs
                    kind = AnalysisNodeKind.Group;
                    title = "Special group";
                }
            }

            var inner = ParseAlternation();
            if (!Match(')'))
            {
                HadError = true;
                ErrorMessage = "Unclosed group '('";
                var incomplete = new AnalysisNode
                {
                    Title = "Incomplete group",
                    Detail = ErrorMessage,
                    Kind = AnalysisNodeKind.Incomplete,
                    IsError = true,
                    PatternFragment = _text[start..],
                };
                if (inner is not null)
                    incomplete.Children.Add(inner);
                return incomplete;
            }

            var node = new AnalysisNode
            {
                Title = title,
                Detail = detail,
                Kind = kind,
                PatternFragment = _text[start.._i],
            };
            if (inner is not null)
                node.Children.Add(inner);
            return node;
        }

        private AnalysisNode ParseCharacterClass()
        {
            var start = _i;
            _i++; // [
            var negated = Match('^');
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
                };
            }

            return new AnalysisNode
            {
                Title = negated ? "Negated character class" : "Character class",
                Detail = _text[start.._i],
                Kind = AnalysisNodeKind.CharacterClass,
                PatternFragment = _text[start.._i],
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
                };
            }

            var c = _text[_i++];
            var (title, kind) = c switch
            {
                'd' => ("Digit class \\d", AnalysisNodeKind.Escape),
                'D' => ("Non-digit \\D", AnalysisNodeKind.Escape),
                'w' => ("Word class \\w", AnalysisNodeKind.Escape),
                'W' => ("Non-word \\W", AnalysisNodeKind.Escape),
                's' => ("Whitespace \\s", AnalysisNodeKind.Escape),
                'S' => ("Non-whitespace \\S", AnalysisNodeKind.Escape),
                'b' => ("Word boundary \\b", AnalysisNodeKind.Anchor),
                'B' => ("Non-boundary \\B", AnalysisNodeKind.Anchor),
                'A' => ("Start of string \\A", AnalysisNodeKind.Anchor),
                'z' => ("End of string \\z", AnalysisNodeKind.Anchor),
                'Z' => ("End of string \\Z", AnalysisNodeKind.Anchor),
                'p' or 'P' => ParseProperty(start, c),
                'k' => ("Named backreference", AnalysisNodeKind.Escape),
                >= '1' and <= '9' => ($"Backreference \\{c}", AnalysisNodeKind.Escape),
                'n' => ("Newline \\n", AnalysisNodeKind.Escape),
                't' => ("Tab \\t", AnalysisNodeKind.Escape),
                'r' => ("Carriage return \\r", AnalysisNodeKind.Escape),
                _ => ($"Escaped '{c}'", AnalysisNodeKind.Escape),
            };

            // Consume \p{...} body if ParseProperty already advanced, else handle \k<name>
            if (c == 'k' && Match('<'))
            {
                while (!AtEnd && Peek() != '>')
                    _i++;
                Match('>');
            }

            return new AnalysisNode
            {
                Title = title,
                Kind = kind,
                PatternFragment = _text[start.._i],
            };
        }

        private (string title, AnalysisNodeKind kind) ParseProperty(int start, char p)
        {
            if (Match('{'))
            {
                while (!AtEnd && Peek() != '}')
                    _i++;
                Match('}');
            }

            var frag = _text[start.._i];
            return (p == 'p' ? $"Unicode property {frag}" : $"Negated Unicode property {frag}",
                AnalysisNodeKind.Escape);
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

        private static string JoinFragments(AnalysisNode node) =>
            string.Join("|", node.Children.Select(c => c.PatternFragment));
    }
}
