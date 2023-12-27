using Confluent.Kafka;
using static Confluent.Kafka.ConfigPropertyNames;
using HeartRateZoneService.Domain;


namespace HeartRateZoneService.Workers;

public class HeartRateZoneWorker : BackgroundService
{
  private readonly ILogger<HeartRateZoneWorker> _logger;

  private const string BiometricsImportedTopicName = "BiometricsImported";
  private const string HeartRateZoneReachedTopicName = "HeartRateZoneReached";
  private readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

  private readonly IConsumer<string, Biometrics> _consumer;
  private readonly IProducer<string, HeartRateZoneReached> _producer; 

  public HeartRateZoneWorker(IConsumer<string, Biometrics> consumer, IProducer<string, HeartRateZoneReached> producer, ILogger<HeartRateZoneWorker> logger)
  {
    _logger = logger;
    _consumer = consumer;
    _producer = producer;
    logger.LogInformation("HeartRateZoneWorker is Active.");
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _producer.InitTransactions(DefaultTimeout);

    // Subscribing to BiometricsImported topic
    _consumer.Subscribe(BiometricsImportedTopicName);

    while (!stoppingToken.IsCancellationRequested)
    {
      var result = _consumer.Consume(stoppingToken);

      await HandleMessage(result.Message.Value as Biometrics, stoppingToken);


    }
    _consumer.Close();
  }
  
  protected virtual async Task HandleMessage(Biometrics metrics, CancellationToken cancellationToken)
  {
    List<Task<DeliveryResult<string, HeartRateZoneReached>>> lstOfTask = new List<Task<DeliveryResult<string, HeartRateZoneReached>>>();


    _logger.LogInformation("Message Received: " + metrics.DeviceId);

    // obtain the offset for the transaction and assign them to a variable
    IEnumerable<TopicPartitionOffset> lstOffset = _consumer.Assignment.Select(topicPartition =>
      new TopicPartitionOffset(
        topicPartition,
        _consumer.Position(topicPartition)
        )
    );

    _producer.BeginTransaction();
    _producer.SendOffsetsToTransaction(lstOffset, _consumer.ConsumerGroupMetadata, DefaultTimeout);

    try
    {

      //Process the heart rates in a loop
      //foreach (var item in metrics.HeartRates) :: i had to filter the heartrates and then iterator 
      //{
      //  var result = item.GetHeartRateZone(metrics.MaxHeartRate);
      //  if (result >= HeartRateZone.Zone1 && result <= HeartRateZone.Zone5)
      //  {
      //    var msg = new Message<string, HeartRateZoneReached>
      //    {
      //      Key = metrics.DeviceId.ToString(),
      //      Value = new HeartRateZoneReached(metrics.DeviceId, result, DateTime.Now, item.Value, metrics.MaxHeartRate)
      //    };

      //    Task<DeliveryResult<string, HeartRateZoneReached>> tsk = _producer.ProduceAsync(HeartRateZoneReachedTopicName, msg, cancellationToken);
      //    lstOfTask.Add(tsk);
      //  }
      //}

      await Task.WhenAll(metrics.HeartRates
        .Where(hr => hr.GetHeartRateZone(metrics.MaxHeartRate) != HeartRateZone.None)
        .Select(hr =>
        {
          var zone = hr.GetHeartRateZone(metrics.MaxHeartRate);

          var heartRateZoneReached = new HeartRateZoneReached(
                          metrics.DeviceId,
                          zone,
                          hr.DateTime,
                          hr.Value,
                          metrics.MaxHeartRate
                      );

          var message = new Message<String, HeartRateZoneReached>
          {
            Key = metrics.DeviceId.ToString(),
            Value = heartRateZoneReached
          };

          return _producer.ProduceAsync(HeartRateZoneReachedTopicName, message, cancellationToken);
        }));


      _producer.CommitTransaction();

    }
    catch (Exception ex)
    {
      _producer.AbortTransaction();
      throw new Exception("Transaction Failed", ex);
    }
    //finally
    //{
    //  await Task.WhenAll(lstOfTask);
    //}

  }


}
