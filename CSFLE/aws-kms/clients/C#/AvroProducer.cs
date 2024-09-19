using System;
using System.Collections.Generic;
using System.Threading;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Encryption;
using Confluent.SchemaRegistry.Encryption.Aws;
using Confluent.SchemaRegistry.Serdes;


namespace Confluent.Kafka.Examples.AvroSpecificEncryption
{
    class Program
    {
        static void Main(string[] args)
        {
    
            AwsKmsDriver.Register();
        
            FieldEncryptionExecutor.Register();

            string topicName = "<topicName";
            string kekName = "<name of AWS kek>";
            string kmsType = "aws-kms"; // one of aws-kms, azure-kms, gcp-kms, hcvault
            string kmsKeyId = "<aws ARN>";
            string subjectName = topicName + "-value";

            var producerConfig = new ProducerConfig {
                BootstrapServers = "",
                SecurityProtocol = ""
                SaslMechanism = "",
                SaslUsername = "<Kafka API Key",
                SaslPassword = "<Kafka API Secret>"
            };

             var schemaRegistryConfig = new SchemaRegistryConfig {
                Url = "<Schema Registry URL>",
                BasicAuthUserInfo = "<Schema Registry API Key>:<Schema Registry API Secret>"
            };

            // Ensure your serializer has UseLatestVersion set to true to ensure you're serializing using the most recent schema version which has the DEK RuleSet registerer
            // This isn't mandatory but it's the easiest way to ensure you're using the correct schema assuming the last version you registered was a version which contains your Encryption rules
            var avroSerializerConfig = new AvroSerializerConfig
            {
                AutoRegisterSchemas = false,
                UseLatestVersion = true,
            };

            
            avroSerializerConfig.Set("rules.secret.access.key", "<AWS access key>");
            avroSerializerConfig.Set("rules.access.key.id", "<AWS key id>");

            // Create your ruleSet
            RuleSet ruleSet = new RuleSet(new List<Rule>(),
                new List<Rule>
                {
                    new Rule("encryptPII", RuleKind.Transform, RuleMode.WriteRead, "ENCRYPT", new HashSet<string>
                    {
                        "PII"
                    }, new Dictionary<string, string>
                    {
                        ["encrypt.kek.name"] = kekName,
                        ["encrypt.kms.type"] = kmsType,
                        ["encrypt.kms.key.id"] = kmsKeyId,
                    }, null, null, "ERROR,NONE", false)
                }
            );
            Schema schema = new Schema(User._SCHEMA.ToString(), null, SchemaType.Avro, null, ruleSet);

            CancellationTokenSource cts = new CancellationTokenSource();
        
            using (var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig))
            using (var producer =
                new ProducerBuilder<string, User>(producerConfig)
                    .SetValueSerializer(new AvroSerializer<User>(schemaRegistry, avroSerializerConfig))
                    .Build())
            {

              // Register your schema which contains your ruleset, if you've already registered the RuleSet via the UI / API you can skip this 
              schemaRegistry.RegisterSchemaAsync(subjectName, schema, true);
                    
                Console.WriteLine($"{producer.Name} producing on {topicName}. Enter user names, q to exit.");

                int i = 1;
                string text;
                while ((text = Console.ReadLine()) != "q")
                {
                    User user = new User { name = text, favorite_color = "green", favorite_number = ++i, hourly_rate = new Avro.AvroDecimal(67.99) };
                    producer.ProduceAsync(topicName, new Message<string, User> { Key = text, Value = user });
                    
                }
            }

            cts.Cancel();
        }
    }
}
