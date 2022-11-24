using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.UI;
using Microsoft.UI.Xaml.Markup;
using Monaco;
using Monaco.Languages;

namespace MonacoEditorTestApp.Helpers
{
    public class ColorProvider : DocumentColorProvider
    {
        private readonly WeakReference<CodeEditor> _editor;

        public ColorProvider(CodeEditor editor) // TODO: Make Internal later.
        {
            // We need the editor component in order to execute JavaScript within 
            // the WebView environment to retrieve data (even though this Monaco class is static).
            _editor = new WeakReference<CodeEditor>(editor);
        }

        //// Called whenever Monaco needs to translate a color value to the textual representation (e.g. from the onhover color picker selector)
        public IEnumerable<ColorPresentation> ProvideColorPresentations(ColorInformation colorInfo)
        {
            return new ColorPresentation[]
            {
                new ColorPresentation(colorInfo.Color.ToString()),
            }.AsEnumerable();
        }

        //// Called whenever changes to the document are made, should identify colors in text and return the actual color value as well as where it was found in the text.
        public async Task<IEnumerable<ColorInformation>> ProvideDocumentColors()
        {
            var info = new List<ColorInformation>();
            if (_editor.TryGetTarget(out CodeEditor editor))
            {
                var document = editor.GetModel();

                Debug.WriteLine("Code May Be 1234: {TargetText}", await editor.GetModel().GetValueAsync());

                // Find all the 8 long hex values we can find in the document using regex.
                var matches = await document.FindMatchesAsync("#[A-Fa-f0-9]{8}", true, true, true, null, true);

                if (matches?.Count() > 0)
                {
                    foreach (var match in matches)
                    {
                        // Generate color info for each of these matches by using the XAML converter to read it to a Color value.
                        info.Add(new ColorInformation(
                            XamlBindingHelper.ConvertValue(typeof(Color), match.Matches.First()) as Color? ??
                            Colors.Black,
                            match.Range));
                    }
                }
            }

            return info.AsEnumerable();
        }
    }
}