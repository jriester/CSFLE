package examples;
import org.apache.kafka.clients.producer.*;
import org.apache.kafka.common.errors.SerializationException;
import org.apache.kafka.common.serialization.StringSerializer;
import io.confluent.kafka.serializers.KafkaAvroSerializer;
import java.util.Properties;
import org.apache.log4j.Logger;
import java.util.Scanner;

public class AvroProducer {
    private static final Logger logger = Logger.getLogger(AvroProducer.class);

    public static void main(String[] args) {
        Properties props = new Properties();

        props.put("bootstrap.servers", "<Kafka Bootstrap>");
        props.put("sasl.jaas.config", "org.apache.kafka.common.security.plain.PlainLoginModule required username=\"<Kafka API Secret>\" password=\"<Kafka API Secret>\";");
        props.put("security.protocol", "SASL_SSL");
        props.put("sasl.mechanism", "PLAIN");
        props.put("acks", "all");
        props.put("value.serializer", KafkaAvroSerializer.class.getName());
        props.put("key.serializer", StringSerializer.class.getName());
        props.put("schema.registry.basic.auth.credentials.source", "USER_INFO");
        props.put("schema.registry.url", "<Schema Registry URL>");
        props.put("basic.auth.user.info", "<Schema Registry API Key>:<Schema Registry API Secret>");
        // This configuration 
        props.put("rule.executors._default_.param.secret", "banana");
        props.put("use.latest.version", "true");
        props.put("auto.register.schemas", "false");

        Scanner kb = new Scanner(System.in);
        String topic = "<topicName>";
        while (true) {
            String input = kb.nextLine();
            if (input.equals("s")) {
                try (KafkaProducer<String, PersonalData> producer = new KafkaProducer<>(props)) {
                    PersonalData my_msg = new PersonalData("0", "Anna", "1993-08-01");
                    final ProducerRecord<String, PersonalData> record = new ProducerRecord<>(topic, my_msg);
                    producer.send(
                            record,
                            (event, ex) -> {
                                if (ex != null)
                                    ex.printStackTrace();
                                else
                                    System.out.printf("Produced event to topic %s: value = %s, partition: %d", topic, record.value(), record.partition(), '\n');
                            });
                } catch (final SerializationException e) {
                    e.printStackTrace();
                }
            }
        }
    }
}

