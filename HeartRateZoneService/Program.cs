using HeartRateZoneService;
using HeartRateZoneService.Workers;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Microsoft.Extensions.Options;
using Confluent.Kafka.SyncOverAsync;
using HeartRateZoneService.Domain;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {

      // loading the configuration setting for the consumer
      services.Configure<ConsumerConfig>(hostContext.Configuration.GetSection("Consumer"));
      // loading the configuraiton setting for the producer
      services.Configure<ProducerConfig>(hostContext.Configuration.GetSection("Producer"));
      // loading the configuraiton setting for the schema registry
      services.Configure<SchemaRegistryConfig>(hostContext.Configuration.GetSection("SchemaRegistry"));
      
      // Register an instance of a ISchemaRegistryClient :: Connecting to the Schema Registry
      services.AddSingleton<ISchemaRegistryClient>(sp => {
        var config = sp.GetRequiredService<IOptions<SchemaRegistryConfig>>();
        return new CachedSchemaRegistryClient(config.Value);
      });

      // registering an instance of an IConsumer<string, Biometrics>
      services.AddSingleton<IConsumer<string, Biometrics>>(sp => {
        var config = sp.GetRequiredService<IOptions<ConsumerConfig>>();
        return new ConsumerBuilder<string, Biometrics>(config.Value)
          .SetValueDeserializer(new JsonDeserializer<Biometrics>().AsSyncOverAsync())
          .Build();

      });

      // registering an instance of a IProducer<string, HeartRateZoneReached>
      services.AddSingleton<IProducer<string, HeartRateZoneReached>>(sp => {
        var config = sp.GetRequiredService<IOptions<ProducerConfig>>();
        var schemaReg = sp.GetRequiredService<ISchemaRegistryClient>();

        return new ProducerBuilder<string, HeartRateZoneReached>(config.Value)
        .SetValueSerializer(new JsonSerializer<HeartRateZoneReached>(schemaReg))
        .Build();
      });

      services.AddHostedService<HeartRateZoneWorker>();
    })
    .Build();

await host.RunAsync();

