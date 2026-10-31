using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ganss.Xss;

namespace VerichatCRM.API.Middleware;

/// <summary>
/// Strips HTML markup out of the string values in an incoming JSON body, before
/// MVC binds and validates it.
/// </summary>
/// <remarks>
/// <para>
/// This has to be middleware rather than an action filter: model validation runs
/// during binding, which is before every filter, so a filter sanitises values the
/// validators have already passed. That gap is not theoretical - a supplier name of
/// <c>&lt;b&gt;&lt;/b&gt;</c> satisfies "not empty" as submitted and then sanitises
/// down to an empty string, which the service happily saves as a purchase order
/// with no supplier. Sanitising first means the rules see the value that gets
/// stored.
/// </para>
/// <para>
/// Two limits keep the pass from corrupting good data. Only values that actually
/// contain <c>&lt;</c> are rewritten, because the sanitiser re-escapes
/// <c>&amp;</c> and would otherwise silently break query strings in image URLs.
/// And credential-shaped properties are left alone: rewriting a password that
/// happens to hold an angle bracket locks its owner out, and hashes and tokens are
/// never markup.
/// </para>
/// <para>
/// The trade-off is text that legitimately contains <c>&lt;</c> - "Cable &lt;2m" -
/// loses it. That is the price of storing what is rendered; the staff app, the Expo
/// client and the generated PDFs all re-emit these strings.
/// </para>
/// </remarks>
public sealed class RequestBodySanitisationMiddleware
{
    /// <summary>Bodies larger than this are passed through untouched.</summary>
    private const int MaxSanitisedBytes = 512 * 1024;

    private const int MaxDepth = 8;

    private static readonly string[] UntouchedPropertyNames =
    [
        "Password", "PasswordHash", "Token", "AccessToken", "RefreshToken",
        "Secret", "Signature", "DeviceToken", "ApiKey"
    ];

    private static readonly HtmlSanitizer Sanitizer = CreateSanitizer();

    private readonly RequestDelegate _next;

    public RequestBodySanitisationMiddleware(RequestDelegate next) => _next = next;

    private static HtmlSanitizer CreateSanitizer()
    {
        var sanitizer = new HtmlSanitizer
        {
            // Keep the words, discard the element: <b>Acme</b> must become Acme.
            KeepChildNodes = true
        };

        // Nothing is trusted. The aim is removal, not a safe subset of markup.
        sanitizer.AllowedTags.Clear();
        sanitizer.AllowedAttributes.Clear();
        sanitizer.AllowedCssProperties.Clear();
        sanitizer.AllowedSchemes.Clear();

        return sanitizer;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var length = context.Request.ContentLength;
        if (context.Request.HasJsonContentType() && length is > 0 and <= MaxSanitisedBytes)
        {
            var body = await new StreamReader(context.Request.Body, Encoding.UTF8).ReadToEndAsync();
            // The stream is spent the moment it is read, so it has to be replaced
            // even when nothing was sanitised - otherwise MVC binds an empty body
            // and every JSON request fails as "input does not contain any JSON
            // tokens". Rewriting is the exception; passing the body through
            // unchanged is the common path.
            var bytes = Encoding.UTF8.GetBytes(Rewrite(body) ?? body);
            context.Request.Body = new MemoryStream(bytes);
            context.Request.ContentLength = bytes.Length;
        }

        await _next(context);
    }

    /// <summary>Returns the replacement body, or null when nothing needed changing.</summary>
    private static string? Rewrite(string body)
    {
        JsonNode? root;
        try
        {
            root = JsonNode.Parse(body);
        }
        catch (JsonException)
        {
            // Malformed JSON is MVC's to report, not this middleware's.
            return null;
        }

        switch (root)
        {
            case null:
                return null;

            // [FromBody] string newStatus arrives as a bare JSON string, which
            // has no container to rewrite it inside.
            case JsonValue bare when bare.TryGetValue<string>(out var text):
                var strippedBare = Strip(text);
                return strippedBare == text ? null : JsonSerializer.Serialize(strippedBare);

            default:
                return SanitiseContainer(root, 0) ? root.ToJsonString() : null;
        }
    }

    private static bool SanitiseContainer(JsonNode node, int depth)
    {
        if (depth > MaxDepth) return false;
        var changed = false;

        if (node is JsonObject obj)
        {
            // Copied because assigning back into the object while enumerating its
            // members would invalidate the enumerator.
            foreach (var member in obj.ToArray())
            {
                if (UntouchedPropertyNames.Contains(member.Key, StringComparer.OrdinalIgnoreCase))
                    continue;

                if (TryRewrite(member.Value, out var stripped))
                {
                    obj[member.Key] = stripped;
                    changed = true;
                }
                else if (member.Value is not null)
                {
                    changed |= SanitiseContainer(member.Value, depth + 1);
                }
            }
        }
        else if (node is JsonArray array)
        {
            for (var i = 0; i < array.Count; i++)
            {
                // Held in a local: the null test and the call have to read the
                // same element, and the compiler cannot prove that an indexer
                // which returned non-null once still does.
                var element = array[i];
                if (TryRewrite(element, out var stripped))
                {
                    array[i] = stripped;
                    changed = true;
                }
                else if (element is not null)
                {
                    changed |= SanitiseContainer(element, depth + 1);
                }
            }
        }

        return changed;
    }

    private static bool TryRewrite(JsonNode? node, out string stripped)
    {
        stripped = string.Empty;
        if (node is JsonValue value && value.TryGetValue<string>(out var text))
        {
            stripped = Strip(text);
            return stripped != text;
        }

        return false;
    }

    private static string Strip(string value)
        => value.Contains('<') ? Sanitizer.Sanitize(value) : value;
}
