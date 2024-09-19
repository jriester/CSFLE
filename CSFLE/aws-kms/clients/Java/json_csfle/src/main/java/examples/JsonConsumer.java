package examples;
import org.apache.avro.generic.GenericRecord;
import org.apache.kafka.clients.consumer.*;

import java.time.Duration;
import java.util.Properties;
import java.util.Arrays;


public class JsonConsumer {

    public static void main(String[] args) {
        Properties props = new Properties();
        props.put("bootstrap.servers", "<Bootstrap>");
        props.put("sasl.jaas.config", "org.apache.kafka.common.security.plain.PlainLoginModule required username=\"<Kafka API Key>\" password=\"<Kafka API Key>\";");
        props.put("security.protocol", "SASL_SSL");
        props.put("sasl.mechanism", "PLAIN");
        props.put("value.deserializer", "io.confluent.kafka.serializers.KafkaJsonSchemaDeserializer");
        props.put("key.deserializer", "org.apache.kafka.common.serialization.StringDeserializer");
        props.put("schema.registry.url", "<Schema Registry URL>");
        props.put("basic.auth.user.info", "<Schema Registry API Key>:<Schema Registry API Secret>");
        props.put("basic.auth.credentials.source", "USER_INFO");
        props.put("auto.offset.reset", "earliest");
        props.put(ConsumerConfig.GROUP_ID_CONFIG, "<GroupId");


        final String topicName = "<TopicName";
        try (final KafkaConsumer<String, GenericRecord> consumer = new KafkaConsumer<>(props)) {
            while (true) {
                consumer.subscribe(Arrays.asList(topicName));
                ConsumerRecords<String, GenericRecord> records = consumer.poll(Duration.ofMillis(100));
                ;
                for (ConsumerRecord<String, GenericRecord> record : records) {
                    System.out.printf("Consumed event from topic %s: value = %s", record.topic(), record.value());
                }
            }
            } catch (Exception e) {
                System.out.print("Try exception: ");

            }
        }
    }
