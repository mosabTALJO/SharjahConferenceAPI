namespace SharjahConferenceAPI.Infrastructure;

using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// HTTP message handler that intercepts communication with Google AI Studio's OpenAI-compatible endpoint.
/// Preserves proprietary `thought_signature` / `extra_content` tokens returned by Gemini models during function calling,
/// and re-injects them into subsequent tool execution messages to satisfy Google's thought signature validation.
/// </summary>
public class GeminiThoughtSignatureHandler : DelegatingHandler
{
    private static readonly ConcurrentDictionary<string, JsonNode> ThoughtSignatureCache = new();

    public GeminiThoughtSignatureHandler() : base(new HttpClientHandler())
    {
    }

    public GeminiThoughtSignatureHandler(HttpMessageHandler innerHandler) : base(innerHandler)
    {
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 1. Inspect outgoing request: Inject cached extra_content if missing from tool_calls
        if (request.Content != null)
        {
            var requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(requestBody) && requestBody.Contains("tool_calls"))
            {
                try
                {
                    var jsonDoc = JsonNode.Parse(requestBody);
                    var messages = jsonDoc?["messages"]?.AsArray();
                    bool modified = false;

                    if (messages != null)
                    {
                        foreach (var message in messages)
                        {
                            var toolCalls = message?["tool_calls"]?.AsArray();
                            if (toolCalls != null)
                            {
                                foreach (var toolCall in toolCalls)
                                {
                                    var callId = toolCall?["id"]?.GetValue<string>();
                                    if (!string.IsNullOrEmpty(callId) && toolCall?["extra_content"] == null)
                                    {
                                        if (ThoughtSignatureCache.TryGetValue(callId, out var cachedExtra))
                                        {
                                            toolCall!["extra_content"] = cachedExtra.DeepClone();
                                            modified = true;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (modified)
                    {
                        var newContent = jsonDoc!.ToJsonString();
                        var mediaType = request.Content.Headers.ContentType?.MediaType ?? "application/json";
                        request.Content = new StringContent(newContent, Encoding.UTF8, mediaType);
                    }
                }
                catch
                {
                    // Fallback to original content if parsing fails
                }
            }
        }

        // 2. Forward request to upstream endpoint
        var response = await base.SendAsync(request, cancellationToken);

        // 3. Inspect incoming response: Cache extra_content from any tool_calls
        if (response.Content != null)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(responseBody) && responseBody.Contains("thought_signature"))
            {
                try
                {
                    var jsonDoc = JsonNode.Parse(responseBody);
                    var choices = jsonDoc?["choices"]?.AsArray();
                    if (choices != null)
                    {
                        foreach (var choice in choices)
                        {
                            var toolCalls = choice?["message"]?["tool_calls"]?.AsArray();
                            if (toolCalls != null)
                            {
                                foreach (var toolCall in toolCalls)
                                {
                                    var callId = toolCall?["id"]?.GetValue<string>();
                                    var extraContent = toolCall?["extra_content"];
                                    if (!string.IsNullOrEmpty(callId) && extraContent != null)
                                    {
                                        ThoughtSignatureCache[callId] = extraContent.DeepClone();
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore non-JSON or unparseable payloads
                }
            }
        }

        return response;
    }
}
