using System;
using System.Collections.Generic;
using System.Globalization;
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


namespace Confluent.Kafka.Examples.AvroSpecificEncryptionLocal
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            # Register the driver for the local KMS system, without this your messages will not be encrypted
            LocalKmsDriver.Register();
            FieldEncryptionExecutor.Register();

            string topicName = "<topicName>";

            # Standard Confluent Cloud producer configurations
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = "<Bootstrap Server>",
                SecurityProtocol = Confluent.Kafka.SecurityProtocol.SaslSsl,
                SaslMechanism = Confluent.Kafka.SaslMechanism.Plain,
                SaslUsername = "<Kafka API Key>",
                SaslPassword = "<Kafka API Secret>"
            };
            # Schema Registry config to fetch our rules
            var schemaRegistryConfig = new SchemaRegistryConfig
            {
                Url = "<Schema Registry Bootstrap>",
                BasicAuthUserInfo = "<Schema Registry API Key>:<Schema Registry API Secret>"
            };

            # Make sure to set UseLatestVersion to true to ensure you're getting the most recent schema with your new rules
            var avroSerializerConfig = new AvroSerializerConfig
            {
                AutoRegisterSchemas = false,
                UseLatestVersion = true,
            };

            # The value here is the output of the "openssl rand -base64 16" command we ran in setup
            avroSerializerConfig.Set("rules.secret", "<OpenSSL output>");

            using (var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig))
            using (var producer = new ProducerBuilder<string, PersonalData>(producerConfig)
            .SetValueSerializer(new AvroSerializer<PersonalData>(schemaRegistry, avroSerializerConfig))
            .SetLogHandler((_, msg) =>
            {
                # Optional logging here
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(@"csfle_local_dotnet.log", true))
                {
                    sw.WriteLine($"{DateTime.Now.ToString("HH:mm:ss tt")} {msg.Level} {msg.Facility} {msg.Name} {msg.Message}");
                }
            }).Build())
            {
                var cancelled = false;
                Console.CancelKeyPress += (_, e) =>
                {
                    e.Cancel = true; // prevent the process from terminating.
                    cancelled = true;
                };

                while (!cancelled) {
                    Console.WriteLine($"{producer.Name} producing on {topicName}");

                    # Sample message, you can change the data here but keep the data types
                    PersonalData pd = new PersonalData { id = "my id", name = "james", birthday = "01/01/1978", timestamp = "500" };
                    try
                    {
                        # You don't need to produce a key here, I do it for fun
                        var deliveryReport = await producer.ProduceAsync(topicName, new Message<string, PersonalData> { Key = "<YOUR KEY HERE>", Value = pd });
                        Console.WriteLine($"produced to: {deliveryReport.TopicPartitionOffset}");

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception:", e);
                    }

                    }
            }
        }
    }
}