using System.Windows.Controls;

namespace projektlabor.noah.planmeldung.uiElements
{
    /// <summary>
    /// Interaction logic for LoadingAnimation.xaml
    /// </summary>
    public partial class LoadingAnimation : UserControl
    {
        public LoadingAnimation()
        {
            InitializeComponent();
            this.DataContext = this;
        }
    }
}
