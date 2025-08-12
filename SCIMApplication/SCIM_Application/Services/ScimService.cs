using System.Net.Http.Headers;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SCIM_Application.Data;
using SCIM_Application.Models;

namespace SCIM_Application.Services
{
    public class ScimService : IScimService
    {
        private readonly ApplicationDbContext _db;
        private readonly HttpClient _httpClient;
        private readonly ILogger<ScimService> _logger;
        
        public ScimService(ApplicationDbContext db, IHttpClientFactory httpClientFactory, ILogger<ScimService> logger)
        {
            _db = db;
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }

        public async Task<bool> CreateUserAsync(User user, Application application, CancellationToken cancellationToken = default)
        {
            var payload = BuildScimUser(user, application.Provider);
            return await SendAsync(HttpMethod.Post, application, application.ScimEndpoint, payload, user, cancellationToken);
        }

        public async Task<bool> UpdateUserAsync(User user, Application application, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(user.ScimId))
            {
                return await CreateUserAsync(user, application, cancellationToken);
            }

            var endpoint = CombineUrl(application.ScimEndpoint, user.ScimId);
            var payload = BuildScimUser(user, application.Provider);
            return await SendAsync(HttpMethod.Put, application, endpoint, payload, user, cancellationToken);
        }

        public async Task<bool> DeleteUserAsync(User user, Application application, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(user.ScimId)) return true;
            var endpoint = CombineUrl(application.ScimEndpoint, user.ScimId);
            return await SendAsync(HttpMethod.Delete, application, endpoint, body: null, user, cancellationToken);
        }

        private async Task<bool> SendAsync(HttpMethod method, Application app, string url, object? body, User user, CancellationToken ct)
        {
            using var request = new HttpRequestMessage(method, url);
            string? requestJson = null;
            
            // SCIM 2.0 standartlarına uygun headers
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/scim+json"));
            
            if (body != null)
            {
                requestJson = JsonConvert.SerializeObject(body);
                // SCIM 2.0 standart Content-Type
                request.Content = new StringContent(requestJson, Encoding.UTF8, "application/scim+json");
            }

            // Authorization headers
            if (!string.IsNullOrEmpty(app.BearerToken))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", app.BearerToken);
            else if (!string.IsNullOrEmpty(app.ApiKey))
            {
                request.Headers.Add("X-API-Key", app.ApiKey);
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            string? responseContent = null;
            string? errorMessage = null;
            bool success = false;

            try
            {
                var response = await _httpClient.SendAsync(request, ct);
                responseContent = await response.Content.ReadAsStringAsync(ct);
                success = response.IsSuccessStatusCode;

                if (success && (method == HttpMethod.Post || method == HttpMethod.Put))
                {
                    try
                    {
                        var doc = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        if (doc != null)
                        {
                            // SCIM 2.0 response'dan id ve externalId al
                            if (doc.id != null)
                            {
                                user.ScimId = (string)doc.id;
                            }
                            
                            // externalId varsa kaydet
                            if (doc.externalId != null)
                            {
                                user.ExternalId = (string)doc.externalId;
                            }
                            
                            await _db.SaveChangesAsync(ct);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "SCIM response parsing failed for user {UserId}", user.Id);
                    }
                }

                return success;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                _logger.LogError(ex, "SCIM request failed for user {UserId} to {Provider}", user.Id, app.Provider);
                return false;
            }
            finally
            {
                stopwatch.Stop();
                try
                {
                    var log = new ScimLog
                    {
                        UserId = user.Id,
                        ApplicationId = app.Id,
                        Operation = GetOperationName(method),
                        Status = success ? "Success" : "Failed",
                        RequestData = requestJson,
                        ResponseData = responseContent,
                        ErrorMessage = errorMessage,
                        CreatedAt = DateTime.UtcNow,
                        ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds
                    };
                    _db.ScimLogs.Add(log);
                    await _db.SaveChangesAsync(ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to log SCIM operation for user {UserId}", user.Id);
                }
            }
        }

        private static string GetOperationName(HttpMethod method)
        {
            if (method == HttpMethod.Post) return "Create";
            if (method == HttpMethod.Put) return "Replace";
            if (method.Method.Equals("PATCH", StringComparison.OrdinalIgnoreCase)) return "Update";
            if (method == HttpMethod.Delete) return "Delete";
            return method.Method;
        }

        private static string CombineUrl(string baseUrl, string segment)
        {
            if (string.IsNullOrWhiteSpace(baseUrl)) return segment;
            return baseUrl.TrimEnd('/') + "/" + segment.TrimStart('/');
        }

        private object BuildScimUser(User user, string provider)
        {
            // SCIM 2.0 standart User schema'sına uygun
            var scimUser = new
            {
                schemas = new[] { "urn:ietf:params:scim:schemas:core:2.0:User" },
                userName = user.UserName,
                name = new 
                { 
                    givenName = user.FirstName, 
                    familyName = user.LastName,
                    formatted = $"{user.FirstName} {user.LastName}"
                },
                emails = new[] 
                { 
                    new 
                    { 
                        value = user.Email, 
                        type = "work",
                        primary = true 
                    } 
                },
                active = user.IsActive,
                phoneNumbers = !string.IsNullOrEmpty(user.PhoneNumber) ? new[]
                {
                    new
                    {
                        value = user.PhoneNumber,
                        type = "work"
                    }
                } : null
            };

            // Provider'a özel ek alanlar
            switch (provider?.Trim().ToLowerInvariant())
            {
                case "azuread":
                    return new
                    {
                        schemas = new[] { "urn:ietf:params:scim:schemas:core:2.0:User" },
                        userName = user.UserName,
                        name = new 
                        { 
                            givenName = user.FirstName, 
                            familyName = user.LastName,
                            formatted = $"{user.FirstName} {user.LastName}"
                        },
                        emails = new[] 
                        { 
                            new 
                            { 
                                value = user.Email, 
                                type = "work",
                                primary = true 
                            } 
                        },
                        active = user.IsActive,
                        phoneNumbers = !string.IsNullOrEmpty(user.PhoneNumber) ? new[]
                        {
                            new
                            {
                                value = user.PhoneNumber,
                                type = "work"
                            }
                        } : null,
                        // Azure AD özel alanları
                        displayName = $"{user.FirstName} {user.LastName}",
                        userType = "Member"
                    };

                case "auth0":
                    return new
                    {
                        schemas = new[] { "urn:ietf:params:scim:schemas:core:2.0:User" },
                        userName = user.UserName,
                        name = new 
                        { 
                            givenName = user.FirstName, 
                            familyName = user.LastName,
                            formatted = $"{user.FirstName} {user.LastName}"
                        },
                        emails = new[] 
                        { 
                            new 
                            { 
                                value = user.Email, 
                                type = "work",
                                primary = true 
                            } 
                        },
                        active = user.IsActive,
                        phoneNumbers = !string.IsNullOrEmpty(user.PhoneNumber) ? new[]
                        {
                            new
                            {
                                value = user.PhoneNumber,
                                type = "work"
                            }
                        } : null,
                        // Auth0 özel alanları
                        nickname = user.UserName
                    };

                case "googleworkspace":
                    return new
                    {
                        schemas = new[] { "urn:ietf:params:scim:schemas:core:2.0:User" },
                        userName = user.UserName,
                        name = new 
                        { 
                            givenName = user.FirstName, 
                            familyName = user.LastName,
                            formatted = $"{user.FirstName} {user.LastName}"
                        },
                        emails = new[] 
                        { 
                            new 
                            { 
                                value = user.Email, 
                                type = "work",
                                primary = true 
                            } 
                        },
                        active = user.IsActive,
                        phoneNumbers = !string.IsNullOrEmpty(user.PhoneNumber) ? new[]
                        {
                            new
                            {
                                value = user.PhoneNumber,
                                type = "work"
                            }
                        } : null,
                        // Google Workspace özel alanları
                        displayName = $"{user.FirstName} {user.LastName}"
                    };

                default:
                    return scimUser;
            }
        }
    }
}
