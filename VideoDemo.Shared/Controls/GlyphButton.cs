using Xamarin.Forms;

namespace VideoDemo.Controls
{
    public class GlyphButton : Button
    {
        public GlyphButton()
        {
            FontFamily = "Font Awesome 5 Free";
        }

        public GlyphType Glyph
        {
            get { return (GlyphType)GetValue(GlyphProperty); }
            set { SetValue(GlyphProperty, value); }
        }

        private static readonly BindableProperty GlyphProperty =
            BindableProperty.Create(nameof(Glyph), typeof(GlyphType), typeof(GlyphButton), GlyphType.None, propertyChanged: OnGlyphChange);

        private static void OnGlyphChange(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is GlyphButton button)
            {
                string code = string.Empty;
                switch ((GlyphType)newValue)
                {
                    case GlyphType.Close:
                        code = "\uf00d";
                        break;

                    case GlyphType.Play:
                        code = "\uf04b";
                        break;
                }

                button.Text = code;
            }
        }

        public enum GlyphType
        {
            None,
            Close,
            Play
        }
    }
}