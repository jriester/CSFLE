## Initial setup adopted from Patrick Neff (Confluent)
https://github.com/pneff93/csfle

## In AWS, create a key

![image](https://github.com/user-attachments/assets/82c858b6-12a2-4a35-b7f4-bd915dc8bb4d)

1. **Key Type**: Symmetric
2. **Key Usage**: Encrypt and decrypt
3. Leave Advanced Options at default values
4. Click **Next**
5. Define an alias, for my example I chose `james_csfle`
6. Click **Next**
7. From the Key administrators page, select an administrator for this key who will serve as it's 'owner'
   1. This is optilonal
8. Click **Next**
9. From Key users, select the user principal which you'll be utilizing for your producer / consumer
10. Click **Next**
11. Review the configurations and click **Finish** when done

## Create an access key for your user

![image](https://github.com/user-attachments/assets/893b75e9-64e6-4824-b4c5-7e0706cbb397)

1. Click **Application running outside AWS**
   1. This isn't important, pick any option you want
2. Click **Next**
3. Click **Create access key**
4. Note your **Access key** and **Secret access key**
   1. Click **Download .csv file** to keep your key and secret locally

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
    "id": 101010742,
    "version": 1,
    "metadata": {
        "properties": {
            "email": "jriester@confluent.io",
            "owner": "James Riester"
        }
    },
    "schema": "{\"type\":\"record\",\"name\":\"PersonalData\",\"namespace\":\"com.csfleExample\",\"fields\":[{\"name\":\"id\",\"type\":\"string\"},{\"name\":\"name\",\"type\":\"string\"},{\"name\":\"birthday\",\"type\":\"string\",\"confluent:tags\":[\"PII\"]},{\"name\":\"timestamp\",\"type\":[\"string\",\"null\"]}]}"
}
```

## Register the ruleset

```
curl -X POST  '`<Schema Registry bootstrap>`/subjects/jriester-csfle-demo-value/versions' -u `<Schema Registry API Key>`:`<Schema Registry API Secret>` -H 'Content-Type: application/vnd.schemaregistry.v1+json'
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
           "encrypt.kek.name": "`<AWS KMS Key Name>`",
           "encrypt.kms.key.id": "`<AWS KMS Key ARN>`",
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
                     "encrypt.kek.name": "`<AWS KMS Key Name>`",
                     "encrypt.kms.key.id": "`<AWS KMS Key ARN>`",
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
curl -s 'https://`<Schema Registry bootstrap>`/subjects/jriester-csfle-demo-value/versions/latest' -u `<Schema Registry API Key>`:`<Schema Registry API Secret>` | jq
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
             "encrypt.kek.name": "`<AWS KMS Key Name>`",
             "encrypt.kms.key.id": "`<AWS KMS Key ARN>`",
             "encrypt.kms.type": "aws-kms"
        },
        "onFailure": "ERROR,NONE",
        "disabled": false
      }
    ]
  }
}
```
