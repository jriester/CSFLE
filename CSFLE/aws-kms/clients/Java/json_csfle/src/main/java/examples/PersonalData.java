package examples;

import com.fasterxml.jackson.annotation.JsonProperty;

public class PersonalData {
  @JsonProperty
  public String id;

  @JsonProperty
  public String name;

  @JsonProperty
  public String birthday;

  @JsonProperty
  public String timestamp;

  public PersonalData(String id, String name, String birthday, String timestamp) {
    this.id = id;
    this.name = name;
    this.birthday = birthday;
    this.timestamp = timestamp;
  }

  public PersonalData(String id, String name, String birthday) {
    this.id = id;
    this.name = name;
    this.birthday = birthday;
  }
}