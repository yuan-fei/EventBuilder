using System;

namespace EventBuilder.Events.Filters
{
  public class BaseFilter:IFilter
  {
    private readonly Func<BaseEvent, bool> mFilter;

    public BaseFilter(Func<BaseEvent,bool> filter)
    {
      mFilter = filter;
    }


    #region IFilter Members

    public bool Filter(BaseEvent eventData)
    {
      return mFilter(eventData);
    }

    #endregion
  }
}
