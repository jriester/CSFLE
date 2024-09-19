## In AWS, create a key I called mine ```james_csfle```

## Give your AWS IAM user principal user key usage permissions

## Create an access key for your IAM user principal
## Created an access key for jriester

## Register the schema:
```
curl -s -u <Schema Registry API Key>:<Schema Registry API Secret> -X POST <Schema Registry bootstrap>/subjects/jriester-csfle-demo-value/versions \
  --header 'content-type: application/octet-stream' \
  --data '{
            "schemaType": "AVRO",
            "schema": "{  \"name\": \"PersonalData\", \"type\": \"record\", \"namespace\": \"com.csfleExample\", \"fields\": [{\"name\": \"id\", \"type\": \"string\"}, {\"name\": \"name\", \"type\": \"string\"},{\"name\": \"birthday\", \"type\": \"string\", \"confluent:tags\": [ \"PII\"]},{\"name\": \"timestamp\",\"type\": [\"string\", \"null\"]}]}",
            "metadata": {
            "properties": {
            "owner": "James Riester",
            "email": "jriester@confluent.io"
            }
          }
    }'
  ```

  ### Example response
  ```
{
    "id": 101010742,
    "version": 1,
    "metadata": {
        "properties": {
            "email": "jriester@confluent.io",
            "owner": "James Riester"
        }
    },
    "schema": "{\"type\":\"record\",\"name\":\"PersonalData\",\"namespace\":\"com.csfleExample\",\"fields\":[{\"name\":\"id\",\"type\":\"string\"},{\"name\":\"name\",\"type\":\"string\"},{\"name\":\"birthday\",\"type\":\"string\",\"confluent:tags\":[\"PII\"]},{\"name\":\"timestamp\",\"type\":[\"string\",\"null\"]}]}"
}```

## Register the RuleSet
```
 curl -X POST  '<Schema Registry bootstrap>/subjects/jriester-csfle-demo-value/versions' -u <Schema Registry API Key>:<Schema Registry API Secret> -H 'Content-Type: application/vnd.schemaregistry.v1+json' \
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
           "encrypt.kek.name": "<AWS KMS Key Name>",
           "encrypt.kms.key.id": "<AWS KMS Key ARN>",
           "encrypt.kms.type": "aws-kms"
          },
        "onFailure": "ERROR,NONE"
        }
        ]
      }
    }'
   ```

   ### Example response
   ```
{
    "id": 101010743,
    "version": 2,
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
                "tags": [
                    "PII"
                ],
                "params": {
                     "encrypt.kek.name": "<AWS KMS Key Name>",
                     "encrypt.kms.key.id": "<AWS KMS Key ARN>",
                     "encrypt.kms.type": "aws-kms"
                },
                "onFailure": "ERROR,NONE",
                "disabled": false
            }
        ]
    },
    "schema": "{\"type\":\"record\",\"name\":\"PersonalData\",\"namespace\":\"com.csfleExample\",\"fields\":[{\"name\":\"id\",\"type\":\"string\"},{\"name\":\"name\",\"type\":\"string\"},{\"name\":\"birthday\",\"type\":\"string\",\"confluent:tags\":[\"PII\"]},{\"name\":\"timestamp\",\"type\":[\"string\",\"null\"]}]}"
}
```

## Check rule exists
```
curl -s 'https://<Schema Registry bootstrap>/subjects/jriester-csfle-demo-value/versions/latest' -u <Schema Registry API Key>:<Schema Registry API Secret> | jq
```
  
### Example response
```  
{
  "subject": "jriester-csfle-demo-value",
  "version": 2,
  "id": 101010743,
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
             "encrypt.kek.name": "<AWS KMS Key Name>",
             "encrypt.kms.key.id": "<AWS KMS Key ARN>",
             "encrypt.kms.type": "aws-kms"
        },
        "onFailure": "ERROR,NONE",
        "disabled": false
      }
    ]
  }
}
```

