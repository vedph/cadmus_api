using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessagingApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Cadmus.Api.Controllers;

/// <summary>
/// Test controller.
/// </summary>
#if !DEBUG
[Authorize(Roles = "admin")]
#endif
[ApiController]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;
    private readonly IMessageBuilderService _messageBuilderService;
    private readonly IMailerService _mailerService;
    private readonly IConfiguration _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestController" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="messageBuilderService">The message builder service.</param>
    /// <param name="mailerService">The mailer service.</param>
    /// <exception cref="ArgumentNullException">logger</exception>
    public TestController(ILogger<TestController> logger,
        IMessageBuilderService messageBuilderService,
        IMailerService mailerService,
        IConfiguration config)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messageBuilderService = messageBuilderService ??
            throw new ArgumentNullException(nameof(messageBuilderService));
        _mailerService = mailerService ??
            throw new ArgumentNullException(nameof(mailerService));
        _config = config ??
            throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Adds a diagnostic entry to the log.
    /// </summary>
    /// <returns>OK</returns>
    [HttpPost("api/test/log")]
    [ProducesResponseType(200)]
    public IActionResult AddLogEntry()
    {
        _logger.LogInformation("Diagnostic log entry posted at {Now} UTC " +
                               "from IP {IP}",
            DateTime.UtcNow,
            HttpContext.Connection.RemoteIpAddress);
        return Ok();
    }

    /// <summary>
    /// Raises an exception to test for logging.
    /// </summary>
    /// <exception cref="Exception">error</exception>
    [HttpGet("api/test/exception")]
    [ProducesResponseType(500)]
    public IActionResult RaiseError()
    {
        Exception exception = new("Fake exception raised for test purposes");
        _logger.LogError(exception, "Fake exception");
        throw exception;
    }

    /// <summary>
    /// Sends a test email message.
    /// </summary>
    [HttpGet("api/test/email")]
    [ProducesResponseType(200)]
    public async Task SendEmail()
    {
        string? to = _config.GetValue<string>("Mailer:TestRecipient");
        if (string.IsNullOrEmpty(to))
        {
            _logger.LogWarning("No recipient defined for test email");
            return;
        }

        _logger.LogInformation("Building test email message for {Recipient}",
            to);
        var message = _messageBuilderService.BuildMessage("test-message",
            new Dictionary<string, string>());

        if (message != null)
        {
            _logger.LogInformation("Sending test email message");
            await _mailerService.SendEmailAsync(
                to,
                "Test Recipient",
                message);
            _logger.LogInformation("Test email message sent");
        }
    }
}