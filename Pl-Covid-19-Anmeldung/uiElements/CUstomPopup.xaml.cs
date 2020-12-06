using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace projektlabor.noah.planmeldung.uiElements
{
    /// <summary>
    /// Interaction logic for Popup.xaml
    /// </summary>
    public partial class CustomPopup : UserControl
    {

        public object ButtonContent
        {
            get { return GetValue(ButtonContentProperty); }
            set { SetValue(ButtonContentProperty, value); }
        }

        public object PopupContent
        {
            get { return GetValue(PopupContentProperty); }
            set { SetValue(PopupContentProperty, value); }
        }

        public bool Transparent { get; set; } = true;

        public static readonly DependencyProperty ButtonContentProperty = DependencyProperty.Register("ButtonContent", typeof(object), typeof(CustomPopup), null);
        public static readonly DependencyProperty PopupContentProperty = DependencyProperty.Register("PopupContent", typeof(object), typeof(CustomPopup), null);


        public CustomPopup()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        /// <summary>
        /// Hides the popup
        /// </summary>
        public void HidePopup()
        {
            this.Button.IsChecked = false;
        }
    }
}
