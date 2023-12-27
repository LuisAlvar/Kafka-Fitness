namespace ClientGateway.Domain
{
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

  public class StepCount
  {

    public StepCount(int value, DateTime dateTime)
    {
      Value = value;
      DateTime = dateTime;
    }

    public int Value { get; }

    public DateTime DateTime { get; }
  }

  public class Biometrics
  {
    public Biometrics(Guid deviceId, List<HeartRate> heartRates, List<StepCount> stepCounts, int maxHeartRate)
    {
      DeviceId = deviceId;
      HeartRates = heartRates;
      StepCounts = stepCounts;
      MaxHeartRate = maxHeartRate;
    }

    public Guid DeviceId { get; }
    public List<HeartRate> HeartRates { get; }
    public List<StepCount> StepCounts { get; }

    public int MaxHeartRate { get;  }
  }


}
