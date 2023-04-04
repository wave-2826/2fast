﻿using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Security.Cryptography;
using System.Text;

#if WINDOWS_UWP
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;
#else
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Data;
#endif

namespace Project2FA.Converters
{
    public class SVGImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var svg = new SvgImageSource();
            if (value != null)
            {
                try
                {
                    //var svgBuffer = CryptographicBuffer.ConvertStringToBinary(value.ToString(), BinaryStringEncoding.Utf8);

                    //using (var stream = svgBuffer.AsStream())
                    //{
                    //    svg.SetSourceAsync(stream.AsRandomAccessStream()).AsTask().ConfigureAwait(false);
                    //}
                    var utf8 = new UTF8Encoding();
                    var svgBuffer = utf8.GetBytes(value.ToString());

                    using (var stream = new MemoryStream(svgBuffer) { Position = 0 })
                    {
                        svg.SetSourceAsync(stream.AsRandomAccessStream()).AsTask().ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
            return svg;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}