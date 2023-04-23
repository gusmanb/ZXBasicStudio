using Avalonia;
using Avalonia.Data;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ZXBasicStudio.Classes
{
    public class ObjectTreeEventBehavior : AvaloniaObject
    {
        public static readonly AttachedProperty<IEnumerable<ObjectTreeEventHandler>?> ObjectTreeEventsProperty = AvaloniaProperty.RegisterAttached<ObjectTreeEventBehavior, Interactive, IEnumerable<ObjectTreeEventHandler>?>("ObjectTreeEvents", default(IEnumerable<ObjectTreeEventHandler>?), false, BindingMode.OneWay);

        public static void SetObjectTreeEvents(AvaloniaObject element, IEnumerable<ObjectTreeEventHandler>? events)
        {
            if (element is Interactive)
            {
                var iElement = element as Interactive;

                if (iElement != null) 
                {
                    var oldValue = iElement.GetValue(ObjectTreeEventsProperty);

                    if (oldValue != null)
                    {
                        foreach (var evtHandler in oldValue)
                            iElement.RemoveHandler(evtHandler.Event, evtHandler.Handler);
                    }

                    if (events != null)
                    {
                        foreach (var evt in events)
                            iElement.AddHandler(evt.Event, evt.Handler, handledEventsToo: true);
                    }
                }
            }

            element.SetValue(ObjectTreeEventsProperty, events);
        }

        public static IEnumerable<ObjectTreeEventHandler>? GetObjectTreeEvents(AvaloniaObject element)
        {
            return element.GetValue(ObjectTreeEventsProperty);
        }
    }

    public class ObjectTreeEventHandler 
    {
        public ObjectTreeEventHandler(RoutedEvent Event, EventHandler<RoutedEvent> Handler) 
        {
            this.Event = Event;
            this.Handler = Handler;
        }
        public RoutedEvent Event { get; set; }
        public EventHandler<RoutedEvent> Handler { get; set; }
    }
}
