namespace HeartRateZoneService.Domain;

public static class HeartRateExtensions
{
  public static HeartRateZone GetHeartRateZone(this HeartRate hr, int maxHeartRate)
  {
    
    if (hr.Value < (.5 * maxHeartRate))
    {
      return HeartRateZone.None;
    }
    else if ((.50 * maxHeartRate) <= hr.Value && hr.Value <= (.59 * maxHeartRate))
    {
      return HeartRateZone.Zone1;
    }
    else if ((.60 * maxHeartRate) <= hr.Value && hr.Value <= (.69 * maxHeartRate))
    {
      return HeartRateZone.Zone2;
    }
    else if ((.70 * maxHeartRate) <= hr.Value && hr.Value <= (.79 * maxHeartRate))
    {
      return HeartRateZone.Zone3;
    }
    else if ((.80 * maxHeartRate) <= hr.Value && hr.Value <= (.89 * maxHeartRate))
    {
      return HeartRateZone.Zone4;
    }
    else if ((.90 * maxHeartRate) <= hr.Value )
    {
      return HeartRateZone.Zone5;
    }

    return HeartRateZone.None;
  }


}