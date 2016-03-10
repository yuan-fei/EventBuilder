namespace EventBuilder.Events
{
  public class NotCorrelated:IIdentifier
  {
    private static IIdentifier sInstance=new NotCorrelated();
    private NotCorrelated()
    {
      
    }

    public static IIdentifier Instance { get { return sInstance; } }
    #region IIdentifier Members

    public string Id
    {
      get { return string.Empty; }
    }

    #endregion
  }
}
