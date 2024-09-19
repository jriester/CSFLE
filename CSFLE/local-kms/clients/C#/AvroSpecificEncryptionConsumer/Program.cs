using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Encryption;
using Confluent.SchemaRegistry.Encryption.Aws;
using Confluent.SchemaRegistry.Encryption.Azure;
using Confluent.SchemaRegistry.Encryption.Gcp;
using Confluent.SchemaRegistry.Encryption.HcVault;
using Confluent.SchemaRegistry.Serdes;


namespace Confluent.Kafka.Examples.SpecificEncryptionConsumer
{
    class Program
    {
        static void Main(string[] args)
        {

            AwsKmsDriver.Register();
            FieldEncryptionExecutor.Register();

            string topicName = "<TopicName>";

            # Standard Confluent Cloud consumer configuration
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = "<Bootstrap server>>",
                SecurityProtocol = Confluent.Kafka.SecurityProtocol.SaslSsl,
                SaslMechanism = Confluent.Kafka.SaslMechanism.Plain,
                SaslUsername = "<Username>",
                SaslPassword = "<Password>",
                GroupId = "<ConsumerGroup ID"
            };

            # Schema Registry configs to fetch our schema
            var schemaRegistryConfig = new SchemaRegistryConfig
            {
                Url = "<Schema Registry Bootstrap>",
                BasicAuthUserInfo = "<Schema Registry API Key>:<Schema Registry API Secret>"
            };

            # This is optional here, leaving for visibility
            var avroDeserializerConfig = new AvroSerializerConfig
            {
                // UseLatestVersion = true,
            };

            # The value here is the output of the "openssl rand -base64 16" command we ran in setup
            avroDeserializerConfig.Set("rules.secret", "<OpenSSL output>");

            CancellationTokenSource cts = new CancellationTokenSource();

            using (var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig))
            using (var consumer =
                new ConsumerBuilder<string, User>(consumerConfig)
                    .SetValueDeserializer(new AvroDeserializer<User>(schemaRegistry, avroDeserializerConfig).AsSyncOverAsync())
                    .SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}"))
                    .Build())
            {

                consumer.Subscribe(topicName);
                Console.WriteLine($"Subscribed to {topicName}");
                try
                {
                    while (true)
                    {
                        try
                        {
                            var consumeResult = consumer.Consume(cts.Token);
                            var user = consumeResult.Message.Value;
                            Console.WriteLine($"key: {consumeResult.Message.Key}, user name: {user.name}, favorite number: {user.favorite_number}, favorite color: {user.favorite_color}, hourly_rate: {user.hourly_rate}");
                        }
                        catch (ConsumeException e)
                        {
                            Console.WriteLine($"Consume error: {e.Error.Reason}\nInner Exception:{e.InnerException}");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    consumer.Close();
                }
            }


            cts.Cancel();
        }
    }
}
