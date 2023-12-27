namespace HeartRateZoneService.Domain;


public class HeartRate
{
  public HeartRate(int value, DateTime dateTime)
  {
    Value = value;
    DateTime = dateTime;
  }

  public int Value { get; }

  public DateTime DateTime { get; }
}


public class Biometrics
{
  public Biometrics(Guid deviceId, List<HeartRate> heartRates, int maxHeartRate)
  {
    DeviceId = deviceId;
    HeartRates = heartRates;
    MaxHeartRate = maxHeartRate;
  }

  public Guid DeviceId { get; }
  public List<HeartRate> HeartRates { get; }

  public int MaxHeartRate { get; }
}


