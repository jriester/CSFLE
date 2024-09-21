## Initial setup adopted from Patrick Neff (Confluent)
https://github.com/pneff93/csfle

## Create your encryption key using the openssl command:
```
 openssl rand -base64 16
 ```

 ### Example output:
 ```
 MLA4pQssdLl494kT6FBboQ==
 ```

## Create a topic to produce your encrypted data to, we'll stick with topic defaults for this demo:
```
 kafka-topics --bootstrap-server <hostname>:<port> --create --topic jriester-csfle-local --command-config /path/to/security/file
 ```

## Export the value to a local environment variable:

 ```
export LOCAL_SECRET=MLA4pQssdLl494kT6FBboQ==
``` 

### Ensure this echoes correctly:
```
 echo $LOCAL_SECRET
 ```

### Example output:
 ```
 MLA4pQssdLl494kT6FBboQ==
 ```

## Register the schema:
```
 curl -s -u <Schema Registry API Key>:<Schema Registry API Secret> -X POST <Schema Registry endpoint>/subjects/jriester-csfle-local-value/versions \
   --header 'content-type: application/octet-stream' \
   --data '{
             "schemaType": "AVRO",
             "schema": "{  \"name\": \"PersonalData\", \"type\": \"record\", \"namespace\": \"Confluent.Kafka.Examples.AvroSpecificEncryptionLocal\", \"fields\": [{\"name\": \"id\", \"type\": \"string\"}, {\"name\": \"name\", \"type\": \"string\"},{\"name\": \"birthday\", \"type\": \"string\", \"confluent:tags\": [ \"PII\"]},{\"name\": \"timestamp\",\"type\": [\"string\", \"null\"]}]}",
             "metadata": {
             "properties": {
             "owner": "James Riester",
             "email": "jriester@confluent.io"
             }
           }
     }'
 ```

 ### Example response:
```
 {
   "id": 101010811,
   "version": 16,
   "metadata": {
     "properties": {
       "email": "jriester@confluent.io",
       "owner": "James Riester"
     }
   },
   "schema": "{\"type\":\"record\",\"name\":\"PersonalData\",\"namespace\":\"Confluent.Kafka.Examples.AvroSpecificEncryptionLocal\",\"fields\":[{\"name\":\"id\",\"type\":\"string\"},{\"name\":\"name\",\"type\":\"string\"},{\"name\":\"birthday\",\"type\":\"string\",\"confluent:tags\":[\"PII\"]},{\"name\":\"timestamp\",\"type\":[\"string\",\"null\"]}]}"
 }
 ```

## Register the rule:
```
 curl -X POST '<Schema Registry endpoint>/subjects/jriester-csfle-local-value/versions' -u <Schema Registry API Key>:<Schema Registry API Secret> -H 'Content-Type: application/vnd.schemaregistry.v1+json' \
   --data '{
         "ruleSet": {
         "domainRules": [
       {
         "name": "encryptPII",
         "kind": "TRANSFORM",
         "type": "ENCRYPT",
         "mode": "WRITEREAD",
         "tags": ["PII"],
         "params": {
            "encrypt.kek.name": "james_local",
            "encrypt.kms.key.id": "my-key",
            "encrypt.kms.type": "local-kms"
           },
         "onFailure": "ERROR,NONE"
         }
         ]
       }
     }'
```
 ### Example response:

```
 {
   "id": 101010812,
   "version": 17,
   "metadata": {
     "properties": {
       "email": "jriester@confluent.io",
       "owner": "James Riester"
     }
   },
   "ruleSet": {
     "domainRules": [
       {
         "name": "encryptPII",
         "kind": "TRANSFORM",
         "mode": "WRITEREAD",
         "type": "ENCRYPT",
         "tags": ["PII"],
         "params": {
           "encrypt.kek.name": "james_local",
           "encrypt.kms.key.id": "my-key",
           "encrypt.kms.type": "local-kms"
         },
         "onFailure": "ERROR,NONE",
         "disabled": false
       }
     ]
   },
      "schema": "{\"type\":\"record\",\"name\":\"PersonalData\",\"namespace\":\"Confluent.Kafka.Examples.AvroSpecificEncryptionLocal\",\"fields\":[{\"name\":\"id\",\"type\":\"string\"},{\"name\":\"name\",\"type\":\"string\"},{\"name\":\"birthday\",\"type\":\"string\",\"confluent:tags\":[\"PII\"]},{\"name\":\"timestamp\",\"type\":[\"string\",\"null\"]}]}"
    }
```
## Check rule exists:
```
 curl -s '<Schema Registry endpoint>/subjects/jriester-csfle-local-value/versions/latest' -u <Schema Registry API Key>:<Schema Registry API Secret> | jq
```

 ### Example response:
```
 {
   "subject": "jriester-csfle-local-value",
   "version": 15,
   "id": 101010810,
   "metadata": {
     "properties": {
       "email": "jriester@confluent.io",
       "owner": "James Riester"
     }
   },
   "schema": "{\"type\":\"record\",\"name\":\"PersonalData\",\"namespace\":\"com.csfleExample\",\"fields\":[{\"name\":\"id\",\"type\":\"string\"},{\"name\":\"name\",\"type\":\"string\"},{\"name\":\"birthday\",\"type\":\"string\",\"confluent:tags\":[\"PII\"]},{\"name\":\"timestamp\",\"type\":[\"string\",\"null\"]}]}",
   "ruleSet": {
     "domainRules": [
       {
         "name": "encryptPII",
         "kind": "TRANSFORM",
         "mode": "WRITEREAD",
         "type": "ENCRYPT",
         "tags": [
           "PII"
         ],
         "params": {
           "encrypt.kek.name": "james_local",
           "encrypt.kms.key.id": "my-key",
           "encrypt.kms.type": "local-kms"
         },
         "onFailure": "ERROR,NONE",
         "disabled": false
       }
     ]
   }
 }
 ```

## Run the avrogen tool to create a C# class based on the input schema PersonalData.avsc:
```
 avrogen -s PersonalData.avsc . --namespace "confluent.io.examples.serialization.avro:Confluent.Kafka.Examples.AvroSpecificEncryptionLocal"
 ```

## Run code
```
dotnet build
dotnet run
```

## Check CCloud UI:
```
 {
   "id": "my id",
   "name": "james",
   "birthday": "+yU+jbNfxXj07AGR7BOIW4d26s1q4+jJ5DkbIXkux91Nr2YDxj4=",
   "timestamp": {
     "string": "500"
   }
 }
 ```
