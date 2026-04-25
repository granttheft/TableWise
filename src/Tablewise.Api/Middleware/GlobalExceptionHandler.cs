using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Sentry;
using Tablewise.Domain.Exceptions;

namespace Tablewise.Api.Middleware;

/// <summary>
/// Global exception handler. Tüm unhandled exception'ları yakalar ve ProblemDetails formatında döner.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    /// <summary>
    /// GlobalExceptionHandler constructor.
    /// </summary>
    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var correlationId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        var (statusCode, problemDetails) = exception switch
        {
            ValidationException validationEx => HandleValidationException(validationEx, correlationId),
            NotFoundException notFoundEx => HandleNotFoundException(notFoundEx, correlationId),
            UnauthorizedException unauthorizedEx => HandleUnauthorizedException(unauthorizedEx, correlationId),
            ForbiddenException forbiddenEx => HandleForbiddenException(forbiddenEx, correlationId),
            TenantIsolationException tenantEx => HandleTenantIsolationException(tenantEx, correlationId, httpContext),
            PlanLimitExceededException planEx => HandlePlanLimitException(planEx, correlationId),
            BusinessRuleException businessEx => HandleBusinessRuleException(businessEx, correlationId),
            _ => HandleUnknownException(exception, correlationId, httpContext)
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        };

        await httpContext.Response.WriteAsJsonAsync(problemDetails, options, cancellationToken)
            .ConfigureAwait(false);

        return true;
    }

    /// <summary>
    /// Validation exception → 422 Unprocessable Entity.
    /// </summary>
    private (int, ProblemDetails) HandleValidationException(
        ValidationException exception,
        string correlationId)
    {
        _logger.LogWarning("Validation hatası: {Errors}", exception.Errors);

        var problemDetails = new ValidationProblemDetails(exception.Errors)
        {
            Type = "https://tools.ietf.org/html/rfc4918#section-11.2",
            Title = "Doğrulama hatası",
            Status = StatusCodes.Status422UnprocessableEntity,
            Detail = "Bir veya daha fazla doğrulama hatası oluştu.",
            Instance = correlationId
        };

        problemDetails.Extensions["traceId"] = correlationId;

        return (StatusCodes.Status422UnprocessableEntity, problemDetails);
    }

    /// <summary>
    /// NotFoundException → 404 Not Found.
    /// </summary>
    private (int, ProblemDetails) HandleNotFoundException(
        NotFoundException exception,
        string correlationId)
    {
        _logger.LogWarning(
            "Entity bulunamadı: {EntityName}, ID: {EntityId}",
            exception.EntityName,
            exception.EntityId);

        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "Kayıt bulunamadı",
            Status = StatusCodes.Status404NotFound,
            Detail = exception.Message,
            Instance = correlationId
        };

        problemDetails.Extensions["traceId"] = correlationId;

        return (StatusCodes.Status404NotFound, problemDetails);
    }

    /// <summary>
    /// UnauthorizedException → 401 Unauthorized.
    /// </summary>
    private (int, ProblemDetails) HandleUnauthorizedException(
        UnauthorizedException exception,
        string correlationId)
    {
        _logger.LogWarning("Yetkisiz erişim denemesi: {Message}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
            Title = "Kimlik doğrulama gerekli",
            Status = StatusCodes.Status401Unauthorized,
            Detail = exception.Message,
            Instance = correlationId
        };

        problemDetails.Extensions["traceId"] = correlationId;

        return (StatusCodes.Status401Unauthorized, problemDetails);
    }

    /// <summary>
    /// ForbiddenException → 403 Forbidden.
    /// </summary>
    private (int, ProblemDetails) HandleForbiddenException(
        ForbiddenException exception,
        string correlationId)
    {
        _logger.LogWarning(
            "Erişim reddedildi: {Message}, RequiredPermission: {Permission}",
            exception.Message,
            exception.RequiredPermission);

        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            Title = "Erişim reddedildi",
            Status = StatusCodes.Status403Forbidden,
            Detail = exception.Message,
            Instance = correlationId
        };

        problemDetails.Extensions["traceId"] = correlationId;

        if (!string.IsNullOrEmpty(exception.RequiredPermission))
        {
            problemDetails.Extensions["requiredPermission"] = exception.RequiredPermission;
        }

        return (StatusCodes.Status403Forbidden, problemDetails);
    }

    /// <summary>
    /// TenantIsolationException → 403 Forbidden + Sentry alert.
    /// Bu ciddi bir güvenlik ihlali olabilir.
    /// </summary>
    private (int, ProblemDetails) HandleTenantIsolationException(
        TenantIsolationException exception,
        string correlationId,
        HttpContext httpContext)
    {
        // Bu kritik bir güvenlik olayı - Sentry'ye gönder
        _logger.LogError(
            exception,
            "TENANT ISOLATION VIOLATION! UserId: {UserId}, RequestedTenantId: {RequestedTenantId}, CurrentTenantId: {CurrentTenantId}, Path: {Path}",
            httpContext.User.FindFirst("sub")?.Value,
            exception.RequestedTenantId,
            exception.CurrentTenantId,
            httpContext.Request.Path);

        SentrySdk.CaptureException(exception, scope =>
        {
            scope.SetTag("security_event", "tenant_isolation_violation");
            scope.SetExtra("path", httpContext.Request.Path.ToString());
            scope.SetExtra("method", httpContext.Request.Method);
            scope.SetExtra("requestedTenantId", exception.RequestedTenantId.ToString());
            scope.SetExtra("currentTenantId", exception.CurrentTenantId?.ToString() ?? "null");
            scope.Level = SentryLevel.Error;
        });

        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            Title = "Erişim reddedildi",
            Status = StatusCodes.Status403Forbidden,
            Detail = "Bu kaynağa erişim yetkiniz bulunmuyor.",
            Instance = correlationId
        };

        problemDetails.Extensions["traceId"] = correlationId;

        return (StatusCodes.Status403Forbidden, problemDetails);
    }

    /// <summary>
    /// PlanLimitExceededException → 403 Forbidden + upgrade URL.
    /// </summary>
    private (int, ProblemDetails) HandlePlanLimitException(
        PlanLimitExceededException exception,
        string correlationId)
    {
        _logger.LogInformation(
            "Plan limiti aşıldı: {LimitType}, CurrentLimit: {CurrentLimit}",
            exception.LimitType,
            exception.CurrentLimit);

        var problemDetails = new ProblemDetails
        {
            Type = "https://tablewise.com.tr/errors/plan-limit-exceeded",
            Title = "Plan limiti aşıldı",
            Status = StatusCodes.Status403Forbidden,
            Detail = exception.Message,
            Instance = correlationId
        };

        problemDetails.Extensions["traceId"] = correlationId;
        problemDetails.Extensions["limitType"] = exception.LimitType;
        problemDetails.Extensions["currentLimit"] = exception.CurrentLimit;
        problemDetails.Extensions["upgradeUrl"] = exception.UpgradeUrl;

        return (StatusCodes.Status403Forbidden, problemDetails);
    }

    /// <summary>
    /// BusinessRuleException → 400 Bad Request veya 409 Conflict.
    /// </summary>
    private (int, ProblemDetails) HandleBusinessRuleException(
        BusinessRuleException exception,
        string correlationId)
    {
        _logger.LogWarning(
            "İş kuralı ihlali: {RuleName}, Message: {Message}",
            exception.RuleName,
            exception.Message);

        var statusCode = exception.RuleName switch
        {
            "EMAIL_ALREADY_EXISTS" => StatusCodes.Status409Conflict,
            "SLOT_NOT_AVAILABLE" => StatusCodes.Status409Conflict,
            "ACCOUNT_LOCKED" => StatusCodes.Status429TooManyRequests,
            _ => StatusCodes.Status400BadRequest
        };

        var problemDetails = new ProblemDetails
        {
            Type = statusCode == StatusCodes.Status409Conflict
                ? "https://tools.ietf.org/html/rfc7231#section-6.5.8"
                : "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "İş kuralı ihlali",
            Status = statusCode,
            Detail = exception.Message,
            Instance = correlationId
        };

        problemDetails.Extensions["traceId"] = correlationId;

        if (!string.IsNullOrEmpty(exception.RuleName))
        {
            problemDetails.Extensions["errorCode"] = exception.RuleName;
        }

        return (statusCode, problemDetails);
    }

    /// <summary>
    /// Diğer tüm exception'lar → 500 Internal Server Error.
    /// </summary>
    private (int, ProblemDetails) HandleUnknownException(
        Exception exception,
        string correlationId,
        HttpContext httpContext)
    {
        _logger.LogError(
            exception,
            "Beklenmeyen hata: {Message}, Path: {Path}",
            exception.Message,
            httpContext.Request.Path);

        // Sentry'ye gönder
        SentrySdk.CaptureException(exception, scope =>
        {
            scope.SetExtra("path", httpContext.Request.Path.ToString());
            scope.SetExtra("method", httpContext.Request.Method);
            scope.SetExtra("correlationId", correlationId);
        });

        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "Sunucu hatası",
            Status = StatusCodes.Status500InternalServerError,
            Instance = correlationId
        };

        problemDetails.Extensions["traceId"] = correlationId;

        // Development'ta detaylı hata göster
        if (_environment.IsDevelopment())
        {
            problemDetails.Detail = exception.ToString();
        }
        else
        {
            problemDetails.Detail = "Beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
        }

        return (StatusCodes.Status500InternalServerError, problemDetails);
    }
}
