using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace VDRClient
{
    public class DeviceTypeAdaptiveTrigger : StateTriggerBase
    {
        public DeviceType PlatformType
        {
            get { return (DeviceTypeAdaptiveTrigger.DeviceType)GetValue(DeviceTypeProperty); }
            set { SetValue(DeviceTypeProperty, value); }
        }
        
        public static readonly DependencyProperty DeviceTypeProperty =
            DependencyProperty.Register("DeviceType", typeof(DeviceType), typeof(DeviceTypeAdaptiveTrigger),
            new PropertyMetadata(DeviceType.Unknown, OnDeviceTypePropertyChanged));

        private static void OnDeviceTypePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (DeviceTypeAdaptiveTrigger)d;
            var val = (DeviceType)e.NewValue;
            var qualifiers = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().QualifierValues;
            if (qualifiers.ContainsKey("DeviceFamily") && qualifiers["DeviceFamily"] == "Mobile")
                obj.SetActive(val == DeviceType.Mobile);
            if (qualifiers.ContainsKey("DeviceFamily") && qualifiers["DeviceFamily"] == "Desktop")
                obj.SetActive(val == DeviceType.Desktop);
        }
        
        public enum DeviceType
        {
            Unknown = 0, Desktop = 1, Mobile = 2,
        }
    }
}
