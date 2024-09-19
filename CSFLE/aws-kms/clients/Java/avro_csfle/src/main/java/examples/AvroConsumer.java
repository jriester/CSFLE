package examples;
import org.apache.avro.generic.GenericRecord;
import org.apache.kafka.clients.consumer.*;
import org.apache.log4j.Logger;
import java.time.Duration;
import java.util.Properties;
import java.util.Arrays;


public class AvroConsumer {

    public static void main(String[] args) {
        Properties props = new Properties();
        props.put("bootstrap.servers", "<bootstrap>");
        props.put("sasl.jaas.config", "org.apache.kafka.common.security.plain.PlainLoginModule required username=\"<kafka api key>\" password=\"<kafka api secret>\";");
        props.put("security.protocol", "SASL_SSL");
        props.put("sasl.mechanism", "PLAIN");
        props.put("value.deserializer", "io.confluent.kafka.serializers.KafkaAvroDeserializer");
        props.put("key.deserializer", "org.apache.kafka.common.serialization.StringDeserializer");
        props.put("schema.registry.url", "<SR Bootstrap>");
        props.put("basic.auth.user.info", "<SR API Key>:<SR API Secret>");
        props.put("basic.auth.credentials.source", "USER_INFO");
        props.put("auto.offset.reset", "earliest");
        props.put(ConsumerConfig.GROUP_ID_CONFIG, "<consumer group>");


        final String topicName = "<topic>";
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
