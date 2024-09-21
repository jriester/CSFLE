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
