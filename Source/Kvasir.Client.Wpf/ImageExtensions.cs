// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageExtensions.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, September 17, 2020 1:35:48 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client.Wpf;

using System.Windows.Media;
using System.Windows.Media.Imaging;
using nGratis.AI.Kvasir.Contract;

public static class ImageExtensions
{
    public static ImageSource ToImageSource(this IImage image)
    {
        var bitmapImage = new BitmapImage();

        bitmapImage.BeginInit();
        bitmapImage.StreamSource = image.SaveData();
        bitmapImage.EndInit();

        return bitmapImage;
    }
}