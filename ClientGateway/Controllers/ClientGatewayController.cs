using System.Diagnostics.Metrics;
using System.Net;
using ClientGateway.Domain;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using ClientGateway.Domain;
using static Confluent.Kafka.ConfigPropertyNames;

namespace ClientGateway.Controllers;

[ApiController]
[Route("[controller]")]
public class ClientGatewayController : ControllerBase
{
  private readonly ILogger<ClientGatewayController> _logger;

  private readonly IProducer<string, Biometrics> _producer;

  private const string BiometricsImportedTopicName = "BiometricsImported";

  public ClientGatewayController(IProducer<string, Biometrics> producer, ILogger<ClientGatewayController> logger)
  {
    _logger = logger;
    _producer = producer;
    logger.LogInformation("ClientGatewayController is Active.");
  }

  [HttpGet("Hello")]
  [ProducesResponseType(typeof(String), (int)HttpStatusCode.OK)]
  public String Hello()
  {
    _logger.LogInformation("Hello World");
    return "Hello World";
  }

  [HttpPost("Biometrics")]
  [ProducesResponseType(typeof(Biometrics), (int)HttpStatusCode.Accepted)]
  public async Task<AcceptedResult> RecordMeasurements(Biometrics metrics)
  {
    _logger.LogInformation("Accepted biometrics");

    var msg = new Message<string, Biometrics>
    {
      Key = metrics.DeviceId.ToString(),
      Value = metrics
    };

    var response = await _producer.ProduceAsync(BiometricsImportedTopicName, msg);

    _producer.Flush();


    return Accepted(response.Value);
  }
}



