using System;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;
using Newtonsoft.Json;

namespace EventBuilder.Events.Consumers
{
  public class FileConsumer : IEventConsumer
  {
    private EventLogger mEventLogger;

    public FileConsumer(int maxLogFileNumber)
    {
      mEventLogger = new EventLogger(maxLogFileNumber);
    }

    public void OnEvent(BaseEvent eventData)
    {
      string eventText = JsonConvert.SerializeObject(eventData);
      mEventLogger.Log(eventText);
    }

   private class EventLogger
   {
     public EventLogger(int maxLogFileNumber)
     {
       Logger = GetLogger(maxLogFileNumber);
     }
     
     private LogWriter Logger { get; set; }
     
     private LogWriter GetLogger(int maxLogFileNumber)
     {
       maxLogFileNumber = (maxLogFileNumber > 0) ? maxLogFileNumber : 50;
       var builder = new ConfigurationSourceBuilder();

       builder.ConfigureLogging()
         .WithOptions
           .DoNotRevertImpersonation()
         .LogToCategoryNamed("All")
         .WithOptions
           .SetAsDefaultCategory()
         .SendTo.RollingFile("Event Log File")
         .WhenRollFileExists(RollFileExistsBehavior.Increment)
         .RollAfterSize(5120)
         .CleanUpArchivedFilesWhenMoreThan(maxLogFileNumber)
         .WithHeader("")
         .FormatWith(new FormatterBuilder()
                       .TextFormatterNamed("Text Formatter")
                       .UsingTemplate("{timestamp(local:yyyy-MM-dd HH:mm:ss.fffffff)}[#{win32ThreadId}]{newline}{message}"))
         .ToFile("EventLog/events.log");

       var configSource = new DictionaryConfigurationSource();
       builder.UpdateConfigurationWithReplace(configSource);


       LogWriterFactory writerFactory = new LogWriterFactory(configSource);
       return writerFactory.Create();
     }

     public void Log(object obj)
     {
       Logger.Write(obj);
     }
   }
  }
}
