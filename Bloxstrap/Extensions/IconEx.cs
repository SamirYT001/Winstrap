using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace Bloxstrap.Extensions
{
    public static class IconEx
    {
        public static Icon GetSized(this Icon icon, int width, int height)
        {
            // === ЗАЩИТА ОТ NULL ===
            if (icon == null)
            {
                // Если иконка пустая, возвращаем стандартную иконку Bloxstrap нужного размера
                return Properties.Resources.IconBloxstrap.GetSized(width, height);
            }
            // ======================

            return new Icon(icon, new Size(width, height));
        }

        public static ImageSource GetImageSource(this Icon icon, bool handleException = true)
        {
            if (icon == null)
            {
                // Если иконка не найдена, принудительно возвращаем стандартную иконку Bloxstrap, чтобы приложение не упало
                return Properties.Resources.IconBloxstrap.GetImageSource(false);
            }
            using MemoryStream stream = new();
            icon.Save(stream);
            stream.Seek(0, SeekOrigin.Begin);

            if (handleException)
            {
                try
                {
                    return BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }
                catch (Exception ex)
                {
                    App.Logger.WriteException("IconEx::GetImageSource", ex);
                    Frontend.ShowMessageBox(string.Format(Strings.Dialog_IconLoadFailed, ex.Message));
                    return BootstrapperIcon.IconBloxstrap.GetIcon().GetImageSource(false);
                }
            }
            else
            {
                return BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            }
        }
    }
}
