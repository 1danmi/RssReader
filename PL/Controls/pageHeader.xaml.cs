using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace PL.Controls
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class pageHeader : Page
    {
        private static readonly double DEFAULT_LEFT_MARGIN = 24;

        public pageHeader()
        {
            this.InitializeComponent();
            this.Loaded += (s, a) =>
            {
                AppShell.Current.TogglePaneButtonRectChanged += Current_TogglePaneButtonSizeChanged;
                double leftMargin = AppShell.Current.TogglePaneButtonRect.Right;
                leftMargin = leftMargin > 0 ? leftMargin : DEFAULT_LEFT_MARGIN;
                TitleBar.Margin = new Thickness(leftMargin, 0, 0, 0);
            };
        }
        private void Current_TogglePaneButtonSizeChanged(AppShell sender, Rect e)
        {





            //    If there is no adjustment due to the toggle button, use the default left margin. 
            TitleBar.Margin = new Thickness(e.Right == 0 ? DEFAULT_LEFT_MARGIN : e.Right, 0, 0, 0);
        }

        public UIElement HeaderContent
        {
            get { return (UIElement)GetValue(HeaderContentProperty); }
            set { SetValue(HeaderContentProperty, value); }
        }

        public static readonly DependencyProperty HeaderContentProperty =
            DependencyProperty.Register("HeaderContent", typeof(UIElement), typeof(pageHeader), new PropertyMetadata(DependencyProperty.UnsetValue));

    }
}
