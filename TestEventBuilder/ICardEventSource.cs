using EventBuilder.Attributes;

namespace TestEventBuilder
{
    [EventSourceType("CardEvent")]
    public interface ICardEventSource
    {
        [EventType("CardInserted", MessageTemplate = "Card inserted: SessionType = {0}, CardId = {1}", Level = EventLevel.Info, PayLoadType = typeof(CardEventPayLoad))]
        void CardInserted([PayLoadProperty("SessionType")]int sessionType, [PayLoadProperty("CardId")]string cardId);

        [EventType("CardRemoved", MessageTemplate = "Card removed", Level = EventLevel.Info)]
        void CardRemoved();
    }

    public class CardEventPayLoad
    {
        public int SessionType { get; set; }
        public string CardId { get; set; }
    }
}